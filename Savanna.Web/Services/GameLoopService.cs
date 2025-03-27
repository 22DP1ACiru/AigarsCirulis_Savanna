using Microsoft.AspNetCore.SignalR;
using Savanna.Core.Interfaces;
using Savanna.Web.Hubs;
using Savanna.Web.Utils;

namespace Savanna.Web.Services
{
    public class GameLoopService : BackgroundService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<GameLoopService> _logger;
        private readonly TimeSpan _tickDelay = TimeSpan.FromMilliseconds(500);

        public GameLoopService(
            IGameSessionService sessionService,
            IHubContext<GameHub> hubContext,
            ILogger<GameLoopService> logger)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameLoopService starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var loopStart = DateTime.UtcNow; // Optional: for timing diagnostics
                try
                {
                    // Update all game sessions
                    _sessionService.UpdateAllSessions();

                    // Push updates to all active sessions
                    // Get IDs first to avoid issues if collection modified during iteration
                    var activeSessionIds = _sessionService.GetActiveSessionIds().ToList();

                    foreach (var sessionId in activeSessionIds)
                    {
                        var session = _sessionService.GetOrCreateSession(sessionId);

                        if (session != null && !string.IsNullOrEmpty(session.OwnerUserId))
                        {
                            try
                            {
                                var grid = session.Engine.GetDisplayGrid().ToJaggedArray();
                                var iterationCount = session.Engine.IterationCount;
                                var livingAnimalCount = session.Engine.LivingAnimalCount;

                                var updatePayload = new
                                {
                                    Grid = grid,
                                    IterationCount = iterationCount,
                                    LivingAnimalCount = livingAnimalCount
                                };

                                await _hubContext.Clients.Group(sessionId)
                                    .SendAsync("ReceiveUpdate", updatePayload, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error getting grid or sending update for session {SessionId}", sessionId);
                            }
                        }
                        else if (session != null && string.IsNullOrEmpty(session.OwnerUserId))
                        {
                            _logger.LogWarning("Session {SessionId} found without owner during update loop. Skipping broadcast.", sessionId);
                        }

                    }

                    // Calculate time spent and delay accordingly
                    var loopDuration = DateTime.UtcNow - loopStart;
                    var delay = _tickDelay - loopDuration;
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("Game loop duration ({LoopDuration}ms) exceeded target tick delay ({TickDelay}ms). Running next tick immediately.",
                            loopDuration.TotalMilliseconds, _tickDelay.TotalMilliseconds);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("GameLoopService stopping due to cancellation request.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in GameLoopService execution loop. The loop will continue after a delay.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

            }
            _logger.LogInformation("GameLoopService stopped.");
        }
    }
}