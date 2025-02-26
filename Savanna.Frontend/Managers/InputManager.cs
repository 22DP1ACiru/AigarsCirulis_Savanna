namespace Savanna.Frontend.Managers
{
    using System;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;

    public class InputManager
    {
        private readonly IGameEngine _gameEngine;

        public InputManager(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
        }

        public bool ProcessInput()
        {
            if (!Console.KeyAvailable)
                return true;

            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.A:
                    _gameEngine.AddAnimal(AnimalConstants.AntelopeSymbol);
                    break;

                case ConsoleKey.L:
                    _gameEngine.AddAnimal(AnimalConstants.LionSymbol);
                    break;

                case ConsoleKey.Escape:
                    return false;
            }

            return true;
        }
    }
}