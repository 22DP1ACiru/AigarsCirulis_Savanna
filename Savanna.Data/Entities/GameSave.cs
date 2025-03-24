using Savanna.Data.Entities;

namespace Savanna.Data.Entities
{
    public class GameSave
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SaveName { get; set; }
        public string GameState { get; set; } // Serialized JSON
        public int Iteration { get; set; }
        public DateTime SaveDate { get; set; }
        public ApplicationUser User { get; set; }
    }
}