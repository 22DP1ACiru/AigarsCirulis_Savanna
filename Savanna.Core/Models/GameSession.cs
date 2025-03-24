using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Models;

namespace Savanna.Core.Models
{
    public class GameSession
    {
        public string SessionId { get; }
        public GameEngine Engine { get; }
        public char[,] Grid => Engine.GetDisplayGrid();

        public GameSession(string sessionId)
        {
            SessionId = sessionId;
            Engine = new GameEngine();
            InitializeGame();
        }

        private void InitializeGame()
        {
            Engine.Initialize();
            Engine.AddAnimal(new Antelope(new Position(0, 0)));
            Engine.AddAnimal(new Lion(new Position(5, 5)));
        }
    }
}
