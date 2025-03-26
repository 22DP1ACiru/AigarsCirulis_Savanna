using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Backend.Interfaces;
using System.Collections.Concurrent;

namespace Savanna.Core.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly ConcurrentDictionary<string, GameSession> _sessions = new();
        private readonly IAnimalFactory _animalFactory;

        public GameSessionService(IAnimalFactory animalFactory)
        {
            _animalFactory = animalFactory ?? throw new ArgumentNullException(nameof(animalFactory));
        }

        public GameSession GetOrCreateSession(string sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => new GameSession(id, _animalFactory));
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public void UpdateAllSessions()
        {
            // Consider parallel update? Be careful with Engine thread-safety if so.
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