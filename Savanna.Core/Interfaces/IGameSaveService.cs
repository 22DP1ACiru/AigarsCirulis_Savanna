using Savanna.Core.Models.DTOs;

namespace Savanna.Core.Interfaces
{
    public interface IGameSaveService
    {
        /// <summary>
        /// Saves the current state of a game session. 
        /// </summary>
        Task<string> SaveGameState(string sessionId, string saveName, string userId);

        /// <summary>
        /// Gets metadata for all saves belonging to a user.\
        /// </summary>
        Task<IEnumerable<GameSaveDto>> GetUserSaves(string userId);

        /// <summary>
        /// Loads a game state into a new session and returns the new session ID.
        /// </summary>
        /// <returns>The new SessionId if load successful, otherwise null.</returns>
        Task<string> LoadGameState(int saveId, string userId);

        /// <summary>
        /// Deletes a specific game save belonging to a user.
        /// </summary>
        Task DeleteSave(int saveId, string userId);
    }
}