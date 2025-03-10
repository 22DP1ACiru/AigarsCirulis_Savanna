namespace Savanna.Frontend.Managers
{
    using System;
    using Savanna.Backend.Animals;
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
        private readonly Dictionary<ConsoleKey, IAnimalPlugin> _pluginKeyMappings = new Dictionary<ConsoleKey, IAnimalPlugin>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InputManager"/> class.
        /// </summary>
        /// <param name="gameEngine">The game engine instance.</param>
        public InputManager(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));

            RegisterDefaultKeyActions();
            InitializePluginKeyMappings();
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

        private void InitializePluginKeyMappings()
        {
            var pluginManager = PluginManager.Instance;
            foreach (var plugin in pluginManager.RegisteredPlugins.Values)
            {
                var config = plugin.GetAnimalConfig();
                if (Enum.TryParse(config.Symbol.ToString(), out ConsoleKey key))
                {
                    _pluginKeyMappings[key] = plugin;
                }
            }
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

            var key = Console.ReadKey(true).Key;

            // Check if we have a registered action for this key
            if (_keyActions.TryGetValue(key, out var action))
            {
                action();
                return true;
            }

            // Check if this is a plugin animal key
            if (_pluginKeyMappings.TryGetValue(key, out var plugin))
            {
                _gameEngine.AddAnimal(plugin.CreateAnimal(new Position(0, 0)));
                return true;
            }

            // Handle escape key for exiting
            if (key == ConsoleKey.Escape)
            {
                return false;
            }

            return true;
        }
    }
}