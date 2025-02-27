namespace Savanna.Frontend
{
    using Savanna.Backend;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Interfaces;
    using Savanna.Frontend.Managers;

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize configuration service to load animal config file that contains all Animal specific constants
            var configService = ConfigurationService.Instance;

            // Create dependencies
            IGameEngine gameEngine = new GameEngine();
            ConsoleManager consoleManager = new ConsoleManager();
            InputManager inputManager = new InputManager(gameEngine);

            // Create and run the game manager
            GameManager gameManager = new GameManager(gameEngine, consoleManager, inputManager);
            gameManager.Initialize();
            gameManager.Run();
        }
    }
}