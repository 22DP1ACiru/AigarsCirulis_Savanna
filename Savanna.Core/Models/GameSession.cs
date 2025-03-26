using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;

namespace Savanna.Core.Models
{
    public class GameSession
    {
        public string SessionId { get; }
        public GameEngine Engine { get; }
        public string OwnerUserId { get; set; } // Tracks who owns/created this session
        public char[,] Grid => Engine.GetDisplayGrid();

        public GameSession(string sessionId, IAnimalFactory animalFactory)
        {
            SessionId = sessionId;
            Engine = new GameEngine(animalFactory);
        }

        public void InitializeNewGame()
        {
            Engine.Initialize();

            Engine.AddAnimal(new Antelope(new Position(0, 0)));
            Engine.AddAnimal(new Antelope(new Position(0, 0)));
            Engine.AddAnimal(new Lion(new Position(0, 0)));
        }
    }
}
