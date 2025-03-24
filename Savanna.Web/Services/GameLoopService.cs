using Microsoft.AspNetCore.SignalR;
using Savanna.Core.Interfaces;
using Savanna.Web.Hubs;

namespace Savanna.Web.Services
{
    public class GameLoopService : BackgroundService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IHubContext<GameHub> _hubContext;

        public GameLoopService(
            IGameSessionService sessionService,
            IHubContext<GameHub> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Update all game sessions
                _sessionService.UpdateAllSessions();

                // Push updates to all active sessions
                foreach (var sessionId in _sessionService.GetActiveSessionIds())
                {
                    var session = _sessionService.GetOrCreateSession(sessionId);
                    var grid = ConvertToJaggedArray(session.Engine.GetDisplayGrid());
                    await _hubContext.Clients.Group(sessionId)
                        .SendAsync("ReceiveUpdate", grid);
                }

                await Task.Delay(500, stoppingToken);
            }
        }

        // Add the conversion helper
        private char[][] ConvertToJaggedArray(char[,] grid)
        {
            int rows = grid.GetLength(1);
            int cols = grid.GetLength(0);
            char[][] jagged = new char[rows][];

            for (int y = 0; y < rows; y++)
            {
                jagged[y] = new char[cols];
                for (int x = 0; x < cols; x++)
                {
                    jagged[y][x] = grid[x, y];
                }
            }
            return jagged;
        }
    }
}