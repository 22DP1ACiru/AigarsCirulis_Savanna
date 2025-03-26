namespace Savanna.Frontend
{
    using Savanna.Backend;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Factories;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Plugins;
    using Savanna.Frontend.Managers;

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize configuration service to load animal config file that contains all Animal specific constants
            var configService = ConfigurationService.Instance;

            // Initialize plugin system
            PluginManager.Instance.Initialize();

            // Create dependencies
            IAnimalFactory animalFactory = new AnimalFactory();
            IGameEngine gameEngine = new GameEngine(animalFactory);
            ConsoleManager consoleManager = new ConsoleManager();
            InputManager inputManager = new InputManager(gameEngine);


            // Create and run the game manager
            GameManager gameManager = new GameManager(gameEngine, consoleManager, inputManager);
            gameManager.Initialize();
            gameManager.Run();
        }
    }
}