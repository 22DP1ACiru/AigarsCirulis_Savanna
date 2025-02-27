namespace Savanna.Frontend.Managers
{
    using System;
    using Savanna.Backend.Animals;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Manages user input.
    /// </summary>
    public class InputManager
    {
        private readonly IGameEngine _gameEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine instance.</param>
        public InputManager(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
        }

        /// <summary>
        /// Processes user input and performs corresponding actions.
        /// </summary>
        /// <returns>True if the game should continue running; false if the game should exit.</returns>
        public bool ProcessInput()
        {
            if (!Console.KeyAvailable)
                return true;

            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.A:
                    _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
                    break;

                case ConsoleKey.L:
                    _gameEngine.AddAnimal(new Lion(new Position(0, 0)));
                    break;

                case ConsoleKey.Escape:
                    return false;
            }

            return true;
        }
    }
}