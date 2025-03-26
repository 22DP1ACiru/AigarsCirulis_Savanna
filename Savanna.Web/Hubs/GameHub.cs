﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Savanna.Web.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGameSaveService _gameSaveService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IGameSessionService sessionService,
            IGameSaveService gameSaveService,
            UserManager<ApplicationUser> userManager,
            ILogger<GameHub> logger)
        {
            _sessionService = sessionService;
            _gameSaveService = gameSaveService;
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

            try
            {
                var grid = ConvertToJaggedArray(session.Engine.GetDisplayGrid());
                await Clients.Caller.SendAsync("ReceiveUpdate", grid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending initial grid state for Session {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Client calls this to add a new animal to the game.
        /// </summary>
        public async Task AddAnimal(string sessionId, string animalType)
        {
            var (user, session, isAuthorized) = await AuthorizeSessionAction(sessionId);
            if (!isAuthorized) return;

            _logger.LogInformation("User {UserId} adding animal '{AnimalType}' to session {SessionId}", user.Id, animalType, sessionId);

            IAnimal newAnimal = null;
            try
            {
                if (animalType == nameof(Antelope))
                {
                    newAnimal = new Antelope(new Position(0, 0));
                }
                else if (animalType == nameof(Lion))
                {
                    newAnimal = new Lion(new Position(0, 0));
                }

                if (newAnimal != null)
                {
                    session.Engine.AddAnimal(newAnimal);
                    _logger.LogDebug("Animal '{AnimalType}' added successfully to session {SessionId}", animalType, sessionId);
                }
                else
                {
                    _logger.LogWarning("Could not create animal of type '{AnimalType}' for session {SessionId}", animalType, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding animal '{AnimalType}' to session {SessionId}", animalType, sessionId);
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
                await _gameSaveService.SaveGameState(sessionId, saveName, user.Id);
                await Clients.Caller.SendAsync("SaveStatus", $"Game '{saveName}' saved successfully!", true);
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
                session.InitializeNewGame();
                var grid = ConvertToJaggedArray(session.Engine.GetDisplayGrid());

                await Clients.Group(sessionId).SendAsync("ReceiveUpdate", grid);
                _logger.LogDebug("Reset completed and update sent for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting game session {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", $"Failed to reset game: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "Client {ConnectionId} disconnected with error.", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client {ConnectionId} disconnected.", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Helper to convert char[,] to char[][] (Jagged Array for JS)
        private char[][] ConvertToJaggedArray(char[,] multiArray)
        {
            if (multiArray == null || multiArray.Rank != 2 || multiArray.Length == 0)
            {
                _logger.LogWarning("ConvertToJaggedArray received null or invalid array.");
                return Array.Empty<char[]>();
            }

            int width = multiArray.GetLength(0);
            int height = multiArray.GetLength(1);
            char[][] jaggedArray = new char[height][]; // Outer array represents rows (Y)

            for (int y = 0; y < height; y++) // Iterate rows (Y)
            {
                jaggedArray[y] = new char[width]; // Inner array represents columns (X)
                for (int x = 0; x < width; x++) // Iterate columns (X)
                {
                    jaggedArray[y][x] = multiArray[x, y]; // Access multiArray as [x, y]
                }
            }
            return jaggedArray;
        }
    }
}