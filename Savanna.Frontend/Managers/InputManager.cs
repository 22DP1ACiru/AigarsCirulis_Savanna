namespace Savanna.Frontend.Managers
{
    using System;
    using Savanna.Backend.Animals;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;
    using Savanna.Backend.Plugins;

    /// <summary>
    /// Manages user input.
    /// </summary>
    public class InputManager
    {
        private readonly IGameEngine _gameEngine;
        private readonly Dictionary<ConsoleKey, Action> _keyActions = new Dictionary<ConsoleKey, Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InputManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine instance.</param>
        public InputManager(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));

            RegisterDefaultKeyActions();
        }

        /// <summary>
        /// Processes user input and performs corresponding actions.
        /// </summary>
        /// <returns>True if the game should continue running; false if the game should exit.</returns>
        public bool ProcessInput()
        {
            var pluginManager = PluginManager.Instance;

            if (!Console.KeyAvailable)
                return true;

            var key = Console.ReadKey(true);

            // Check if we have a registered action for this key
            if (_keyActions.TryGetValue(key.Key, out var action))
            {
                action();
                return true;
            }

            // Check if this is a plugin animal key
            char keyChar = char.ToUpper(key.KeyChar);
            if (pluginManager.RegisteredPlugins.TryGetValue(keyChar, out var plugin))
            {
                _gameEngine.AddAnimal(plugin.CreateAnimal(new Position(0, 0)));
                return true;
            }

            // Handle escape key for exiting
            if (key.Key == ConsoleKey.Escape)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the default key actions for built-in animals.
        /// </summary>
        private void RegisterDefaultKeyActions()
        {
            // Register built-in animal keys
            _keyActions[ConsoleKey.A] = () => _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
            _keyActions[ConsoleKey.L] = () => _gameEngine.AddAnimal(new Lion(new Position(0, 0)));
        }
    }
}