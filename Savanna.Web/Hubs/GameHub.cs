using Microsoft.AspNetCore.SignalR;
using Savanna.Backend.Animals;
using Savanna.Backend.Models;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Web.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameSessionService _sessionService;

        public GameHub(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task AddAnimal(string sessionId, string animalType)
        {
            var session = _sessionService.GetOrCreateSession(sessionId);
            switch (animalType)
            {
                case "Antelope":
                    session.Engine.AddAnimal(new Antelope(new Position(0, 0)));
                    break;
                case "Lion":
                    session.Engine.AddAnimal(new Lion(new Position(0, 0)));
                    break;
            }
        }

        // Helper method to convert char[,] to char[][]
        private char[][] ConvertToJaggedArray(char[,] multiArray)
        {
            int rows = multiArray.GetLength(0);
            int cols = multiArray.GetLength(1);
            char[][] jaggedArray = new char[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new char[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = multiArray[i, j];
                }
            }
            return jaggedArray;
        }

        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            var session = _sessionService.GetOrCreateSession(sessionId);

            char[][] serializableGrid = ConvertToJaggedArray(session.Engine.GetDisplayGrid());
            await Clients.Caller.SendAsync("ReceiveUpdate", serializableGrid);
        }
    }
}