using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using System.Collections.Concurrent;

namespace Savanna.Core.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly ConcurrentDictionary<string, GameSession> _sessions = new();

        public GameSession GetOrCreateSession(string sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => new GameSession(id));
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public void UpdateAllSessions()
        {
            foreach (var session in _sessions.Values)
            {
                session.Engine.Update();
            }
        }

        public IEnumerable<string> GetActiveSessionIds()
        {
            return _sessions.Keys.ToList();
        }
    }
}