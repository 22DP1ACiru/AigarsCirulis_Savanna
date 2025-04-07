using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Data.Entities;
using Savanna.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Savanna.Backend.Configuration;
using Savanna.Backend.Plugins;
using Savanna.Web.DTOs;

namespace Savanna.Web.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGameSaveService _gameSaveService;
        private readonly IDisplayConfigService _displayConfigService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IGameSessionService sessionService,
            IGameSaveService gameSaveService,
            IDisplayConfigService displayConfigService,
            UserManager<ApplicationUser> userManager,
            ILogger<GameHub> logger)
        {
            _sessionService = sessionService;
            _gameSaveService = gameSaveService;
            _displayConfigService = displayConfigService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<(ApplicationUser User, GameSession Session, bool IsAuthorized)> AuthorizeSessionAction(string sessionId)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            var session = _sessionService.GetOrCreateSession(sessionId);

            // Check if user exists and session owner matches
            bool isAuthorized = user != null && session != null && session.OwnerUserId == user.Id;

            if (!isAuthorized)
            {
                _logger.LogWarning("Unauthorized action attempt. User: {UserId}, Session: {SessionId}, Actual Owner: {OwnerId}",
                    user?.Id, sessionId, session?.OwnerUserId);
            }

            return (user, session, isAuthorized);
        }

        /// <summary>
        /// Client calls this to get the current symbol-to-icon mapping.
        /// </summary>
        /// <returns>A dictionary where Key=Symbol (char), Value=Icon (string).</returns>
        public Dictionary<char, string> GetDisplayConfig() { return _displayConfigService.GetSymbolIconMap(); }

        /// <summary>
        /// Client calls this to join a specific game session group.
        /// </summary>
        public async Task JoinSession(string sessionId)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized)
            {
                _logger.LogWarning("JoinSession denied for User: {UserId}, Session: {SessionId}", user?.Id, sessionId);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            _logger.LogInformation("Client {ConnectionId} joined session group {SessionId}", Context.ConnectionId, sessionId);

            // Send the initial state immediately upon joining
            try
            {
                var payload = PayloadUtils.CreateWebPayload(session, _logger);
                await Clients.Caller.SendAsync("ReceiveUpdate", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending initial state for Session {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Client calls this to add a new animal to the game.
        /// </summary>
        public async Task AddAnimal(string sessionId, string animalTypeName)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized) return;

            _logger.LogInformation("User {UserId} adding animal '{AnimalType}' to session {SessionId}", user.Id, animalTypeName, sessionId);

            IAnimal newAnimal = null;
            try
            {
                bool wasAdded = session.Engine.AddAnimalByName(animalTypeName);

                if (!wasAdded)
                {
                    // Engine returned false, meaning factory didn't create the animal OR
                    // AddAnimal failed (likely grid full on placement attempt).
                    _logger.LogWarning("[GameHub] Engine failed to add animal '{AnimalType}' for session {SessionId}.", animalTypeName, sessionId);
                    // Send specific feedback to the user
                    await Clients.Caller.SendAsync("Error", $"Failed to add '{animalTypeName}'. Unknown type or grid may be full.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding animal '{AnimalType}' to session {SessionId}", animalTypeName, sessionId);
                await Clients.Caller.SendAsync("Error", $"Failed to add animal: {ex.Message}");
            }
        }

        /// <summary>
        /// Client calls this to save the current game state.
        /// </summary>
        public async Task SaveGame(string sessionId, string saveName)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized)
            {
                await Clients.Caller.SendAsync("SaveStatus", "Error: Not authorized to save this game.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(saveName))
            {
                await Clients.Caller.SendAsync("SaveStatus", "Error: Save name cannot be empty.", false);
                return;
            }

            try
            {
                // Call the core service to handle saving
                string finalSaveName = await _gameSaveService.SaveGameState(sessionId, saveName, user.Id);
                await Clients.Caller.SendAsync("SaveStatus", $"Game '{finalSaveName}' saved successfully!", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game via Hub for Session {SessionId}, User {UserId}", sessionId, user.Id);
                await Clients.Caller.SendAsync("SaveStatus", $"An error occurred while saving: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Client calls this to reset the game state to defaults.
        /// </summary>
        public async Task ResetGame(string sessionId)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized) return;

            _logger.LogInformation("User {UserId} resetting game session {SessionId}", user.Id, sessionId);

            try
            {
                session.InitializeNewGame(); // Initialize the game engine

                var payload = PayloadUtils.CreateWebPayload(session, _logger);
                await Clients.Group(sessionId).SendAsync("ReceiveUpdate", payload);

                _logger.LogDebug("Reset completed and update sent for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting game session {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", $"Failed to reset game: {ex.Message}");
            }
        }

        public async Task<object> GetAnimalStats(string sessionId, Guid animalId)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized) return null;

            _logger.LogDebug("Request for animal stats. Session: {SessionId}");

            try
            {
                IAnimal animal = session.Engine.GetAnimalById(animalId);

                if (animal != null)
                {
                    var stats = new
                    {
                        TypeName = animal.AnimalTypeName,
                        Age = animal.Age,
                        Health = animal.Health,
                        Offspring = animal.OffspringCount,
                        PositionX = animal.Position.X,
                        PositionY = animal.Position.Y
                    };
                    _logger.LogDebug("Found animal: {TypeName}, returning stats.", stats.TypeName);
                    return stats;
                }
                else
                {
                    _logger.LogDebug("No animal found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting animal stats for Session {SessionId}", sessionId);
                // Don't send exception details to client, just indicate failure
                return new { Error = "Failed to retrieve animal stats." };
            }
        }

        public List<AvailableAnimalDto> GetAvailableAnimalTypes()
        {
            _logger.LogInformation("[GameHub] Client requested available animal types.");
            var configService = ConfigurationService.Instance; // For built-in config
            var pluginManager = PluginManager.Instance;     // For plugin list
            var animalList = new List<AvailableAnimalDto>();
            var addedSymbols = new HashSet<char>(); // To handle potential symbol overlap

            // Add built-in animals defined in AnimalConfig.json
            if (configService.AnimalConfig?.Animals != null)
            {
                foreach (var kvp in configService.AnimalConfig.Animals)
                {
                    string typeName = kvp.Key;
                    var config = kvp.Value;
                    if (config.Symbol != default(char))
                    {
                        _logger.LogDebug("[GameHub] Found built-in type: {TypeName} ({Symbol})", typeName, config.Symbol);
                        animalList.Add(new AvailableAnimalDto
                        {
                            TypeName = typeName,
                            Symbol = config.Symbol,
                            Icon = config.Icon,
                            IsPlugin = false 
                        });
                        addedSymbols.Add(config.Symbol);
                    }
                }
            }

            // Add registered plugin animals from PluginManager
            foreach (var kvp in pluginManager.RegisteredPlugins) // Key is Symbol, Value is IAnimalPlugin
            {
                char symbol = kvp.Key;
                IAnimalPlugin plugin = kvp.Value;

                // Check if this symbol was already added (e.g., a built-in with the same symbol)
                // Note: PluginManager's current logic prevents loading plugins with duplicate symbols,
                // so this check might be redundant but safe.
                if (addedSymbols.Contains(symbol))
                {
                    _logger.LogDebug("[GameHub] Skipping plugin {PluginType} for display because symbol {Symbol} was already added.", plugin.AnimalType, symbol);
                    continue;
                }

                var config = plugin.GetAnimalConfig(); // Get config directly from plugin instance
                if (config != null)
                {
                    _logger.LogDebug("[GameHub] Found plugin type: {TypeName} ({Symbol})", plugin.AnimalType, config.Symbol);
                    animalList.Add(new AvailableAnimalDto
                    {
                        TypeName = plugin.AnimalType, // Get name from plugin interface
                        Symbol = config.Symbol,
                        Icon = config.Icon,
                        IsPlugin = true
                    });
                    // No need to add to addedSymbols here because PluginManager already enforced uniqueness on Symbol key
                }
            }

            _logger.LogInformation("[GameHub] Returning {Count} available animal types.", animalList.Count);

            return animalList.OrderBy(a => a.TypeName).ToList();
        }

    }
}   