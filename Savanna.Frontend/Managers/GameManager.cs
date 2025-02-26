namespace Savanna.Frontend.Managers
{
    using System;
    using System.Threading;
    using Savanna.Backend.Animals;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Manages the game loop and coordinates between the game engine, console, and input.
    /// </summary>
    public class GameManager
    {
        private readonly IGameEngine _gameEngine;
        private readonly ConsoleManager _consoleManager;
        private readonly InputManager _inputManager;
        private bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine instance.</param>
        /// <param name="consoleManager">The console manager instance.</param>
        /// <param name="inputManager">The input manager instance.</param>
        public GameManager(IGameEngine gameEngine, ConsoleManager consoleManager, InputManager inputManager)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            _consoleManager = consoleManager ?? throw new ArgumentNullException(nameof(consoleManager));
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
        }

        /// <summary>
        /// Initializes the game by setting up the game engine and console.
        /// </summary>
        public void Initialize()
        {
            _gameEngine.Initialize();
            _consoleManager.Initialize();

            // Add initial animals
            _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
            _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
            _gameEngine.AddAnimal(new Lion(new Position(0, 0)));
        }

        /// <summary>
        /// Starts and runs the game loop.
        /// </summary>
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