using Savanna.Core.Interfaces;
using Savanna.Core.Models.DTOs;
using Savanna.Data.Interfaces;
using Savanna.Data.Entities;
using Savanna.Backend.Models.State;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Savanna.Core.Services
{
    public class GameSaveService : IGameSaveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameSessionService _sessionService;
        private readonly ILogger<GameSaveService> _logger; // Logger is optional

        public GameSaveService(
            IUnitOfWork unitOfWork,
            IGameSessionService sessionService,
            ILogger<GameSaveService> logger = null) // Make logger optional if not always provided via DI
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _logger = logger;
        }

        /// <summary>
        /// Gets metadata for all saves belonging to a user, ordered by most recent first.
        /// </summary>
        public async Task<IEnumerable<GameSaveDto>> GetUserSaves(string userId)
        {
            _logger?.LogInformation("Fetching game saves for User ID: {UserId}", userId);
            try
            {
                var saves = await _unitOfWork.GameSaves.FindAsync(s => s.UserId == userId);

                return saves
                    .OrderByDescending(s => s.SaveDate)
                    .Select(s => new GameSaveDto(s.Id, s.SaveName, s.Iteration, s.SaveDate));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching game saves for User ID: {UserId}", userId);
                return Enumerable.Empty<GameSaveDto>();
            }
        }

        /// <summary>
        /// Saves the current state of a given game session to the database.
        /// </summary>
        public async Task SaveGameState(string sessionId, string saveName, string userId)
        {
            _logger?.LogInformation("Attempting to save game state for Session: {SessionId}, User: {UserId}, Name: {SaveName}", sessionId, userId, saveName);
            try
            {
                var session = _sessionService.GetOrCreateSession(sessionId);

                // Validate session exists and belongs to the user requesting the save
                if (session == null || session.OwnerUserId != userId)
                {
                    _logger?.LogWarning("Save unauthorized or session not found. Session: {SessionId}, User: {UserId}", sessionId, userId);
                    throw new UnauthorizedAccessException("User does not own this session or session not found.");
                }

                GameStateDto gameState = session.Engine.GetGameState();

                string serializedState = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = false });

                // Create the entity to save
                var gameSave = new GameSave
                {
                    UserId = userId,
                    SaveName = saveName,
                    GameState = serializedState,
                    Iteration = gameState.IterationCount,
                    SaveDate = DateTime.UtcNow
                };

                _unitOfWork.GameSaves.Add(gameSave);

                // Persist changes to the database asynchronously
                int recordsAffected = await _unitOfWork.CompleteAsync();
                _logger?.LogInformation("Game state saved successfully. {RecordsAffected} records affected. Session: {SessionId}, User: {UserId}, Save ID: {SaveId}", recordsAffected, sessionId, userId, gameSave.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving game state for Session: {SessionId}, User: {UserId}", sessionId, userId);
                throw;
            }
        }

        /// <summary>
        /// Loads a saved game state into a brand new session.
        /// </summary>
        /// <returns>The SessionId of the newly created session containing the loaded state, or null if loading fails.</returns>
        public async Task<string> LoadGameState(int saveId, string userId)
        {
            _logger?.LogInformation("Attempting to load game state for Save ID: {SaveId}, User: {UserId}", saveId, userId);
            try
            {
                var gameSave = await GetAndValidateUserSaveAsync(saveId, userId);

                if (gameSave == null)
                {
                    return null;
                }

                GameStateDto gameState = DeserializeGameState(gameSave.GameState);
                if (gameState == null)
                {
                    _logger?.LogError("Load failed. Could not deserialize game state for Save ID: {SaveId}", saveId);
                    return null;
                }

                // Create a new  unique session ID for the loaded game
                string newSessionId = Guid.NewGuid().ToString("N");

                // Get or create the new GameSession instance using the service
                var newSession = _sessionService.GetOrCreateSession(newSessionId);

                // Assign the owner to the new session
                newSession.OwnerUserId = userId;

                newSession.Engine.LoadGameState(gameState);

                _logger?.LogInformation("Game state loaded successfully from Save ID: {SaveId} into New Session: {NewSessionId} for User: {UserId}", saveId, newSessionId, userId);

                // Return the ID of the new session
                return newSessionId;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading game state for Save ID: {SaveId}, User: {UserId}", saveId, userId);
                return null;
            }
        }

        /// <summary>
        /// Deletes a specific game save if it exists and belongs to the user.
        /// </summary>
        public async Task DeleteSave(int saveId, string userId)
        {
            _logger?.LogInformation("Attempting to delete Save ID: {SaveId} for User: {UserId}", saveId, userId);
            try
            {
                var gameSave = await GetAndValidateUserSaveAsync(saveId, userId);

                if (gameSave != null)
                {
                    _unitOfWork.GameSaves.Remove(gameSave);

                    // Persist the deletion
                    int recordsAffected = await _unitOfWork.CompleteAsync();
                    _logger?.LogInformation("Game save deleted successfully. {RecordsAffected} records affected. Save ID: {SaveId}, User: {UserId}", recordsAffected, saveId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting Save ID: {SaveId} for User: {UserId}", saveId, userId);
                throw;
            }
        }

        private GameStateDto DeserializeGameState(string serializedData)
        {
            if (string.IsNullOrWhiteSpace(serializedData))
            {
                _logger?.LogWarning("Attempted to deserialize null or empty game state data.");
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<GameStateDto>(serializedData);
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogError(jsonEx, "JSON Deserialization failed during game state loading.");
                return null;
            }
        }

        /// <summary>
        /// Fetches a GameSave by ID and validates if it exists and belongs to the specified user.
        /// </summary>
        /// <param name="saveId">The ID of the GameSave to fetch.</param>
        /// <param name="userId">The expected owner's User ID.</param>
        /// <returns>The GameSave entity if found and validated, otherwise null.</returns>
        private async Task<GameSave> GetAndValidateUserSaveAsync(int saveId, string userId)
        {
            // Fetch the entity asynchronously
            var gameSave = await _unitOfWork.GameSaves.GetByIdAsync(saveId);

            // Validate existence
            if (gameSave == null)
            {
                _logger?.LogWarning("Validation failed. Save not found. Save ID: {SaveId}, User: {UserId}", saveId, userId);
                return null;
            }

            // Validate ownership
            if (gameSave.UserId != userId)
            {
                _logger?.LogWarning("Validation failed. User mismatch. Save ID: {SaveId}, User: {UserId}", saveId, userId);
                return null;
            }

            _logger?.LogDebug("Save validation successful for Save ID: {SaveId}, User: {UserId}", saveId, userId);
            return gameSave;
        }
    }
}