using Savanna.Core.Models;

namespace Savanna.Core.Interfaces 
{ 
    public interface IGameSessionService
    {
        GameSession GetOrCreateSession(string sessionId);
        void RemoveSession(string sessionId);
        void UpdateAllSessions();
        IEnumerable<string> GetActiveSessionIds();
    }
}