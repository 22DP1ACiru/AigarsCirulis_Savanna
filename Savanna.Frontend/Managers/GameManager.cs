namespace Savanna.Frontend.Managers
{
    using System;
    using System.Threading;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;

    public class GameManager
    {
        private readonly IGameEngine _gameEngine;
        private readonly ConsoleManager _consoleManager;
        private readonly InputManager _inputManager;
        private bool _isRunning;

        public GameManager(IGameEngine gameEngine, ConsoleManager consoleManager, InputManager inputManager)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            _consoleManager = consoleManager ?? throw new ArgumentNullException(nameof(consoleManager));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
        }

        public void Initialize()
        {
            _gameEngine.Initialize();
            _consoleManager.Initialize();

            // Add initial animals
            _gameEngine.AddAnimal(AnimalConstants.AntelopeSymbol);
            _gameEngine.AddAnimal(AnimalConstants.AntelopeSymbol);
            _gameEngine.AddAnimal(AnimalConstants.LionSymbol);
        }

        public void Run()
        {
            _isRunning = true;

            while (_isRunning)
            {
                // Process input
                _isRunning = _inputManager.ProcessInput();

                // Update game state
                _gameEngine.Update();

                // Render the game
                _consoleManager.Render(_gameEngine.GetDisplayGrid());

                // Wait for next frame
                Thread.Sleep(GameConstants.GameTickDelayMs);
            }
        }
    }
}