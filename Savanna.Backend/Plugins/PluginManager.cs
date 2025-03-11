namespace Savanna.Backend.Plugins
{
    using System;
    using System.Collections.Generic;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Manages the loading and registration of animal plugins.
    /// </summary>
    public class PluginManager
    {
        private static PluginManager _instance;
        private static readonly object _lockObject = new object();

        private readonly IPluginLoader _pluginLoader;
        private readonly Dictionary<char, IAnimalPlugin> _registeredPlugins = new Dictionary<char, IAnimalPlugin>();
        private readonly ConfigurationService _configService;

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginManager(new PluginLoader(), ConfigurationService.Instance);
                        }
                    }
                }
                return _instance;
            }
        }

        public PluginManager(IPluginLoader pluginLoader, ConfigurationService configService)
        {
            _pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <summary>
        /// Initializes the plugin system by loading all available plugins.
        /// </summary>
        public void Initialize()
        {
            // Load plugins from the plugins directory
            LoadPlugins();
        }

        /// <summary>
        /// Gets a list of all registered animal plugins.
        /// </summary>
        public IReadOnlyDictionary<char, IAnimalPlugin> RegisteredPlugins => _registeredPlugins;

        /// <summary>
        /// Creates an animal instance from a registered plugin.
        /// </summary>
        /// <param name="symbol">The symbol of the animal plugin to create.</param>
        /// <param name="position">The position where the animal should be created.</param>
        /// <returns>A new animal instance if the plugin is registered; otherwise, null.</returns>
        public IAnimal CreateAnimalFromPlugin(char symbol, Position position)
        {
            if (_registeredPlugins.TryGetValue(symbol, out var plugin))
            {
                return plugin.CreateAnimal(position);
            }

            return null;
        }

        /// <summary>
        /// Loads and registers all available plugins.
        /// </summary>
        private void LoadPlugins()
        {
            try
            {
                var plugins = _pluginLoader.LoadPlugins(PluginConstants.PluginsDirectory);

                foreach (var plugin in plugins)
                {
                    var config = plugin.GetAnimalConfig();
                    char symbol = config.Symbol;

                    // Register plugin if the symbol is not already used
                    if (!_registeredPlugins.ContainsKey(symbol))
                    {
                        _configService.RegisterPluginConfig(plugin.AnimalType, config);
                        _registeredPlugins.Add(symbol, plugin);
                        Console.WriteLine($"Registered plugin: {plugin.AnimalType} with symbol '{symbol}'");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Symbol '{symbol}' is already used by another plugin or built-in animal. Skipping {plugin.AnimalType}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugins: {ex.Message}");
            }
        }
    }
}