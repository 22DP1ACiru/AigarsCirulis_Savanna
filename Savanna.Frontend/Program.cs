namespace Savanna.Frontend
{
    using Savanna.Backend;
    using Savanna.Backend.Interfaces;
    using Savanna.Frontend.Managers;

    class Program
    {
        static void Main(string[] args)
        {
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