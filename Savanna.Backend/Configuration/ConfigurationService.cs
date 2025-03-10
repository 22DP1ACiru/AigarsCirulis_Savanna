namespace Savanna.Backend.Configuration
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.Json;

    public class ConfigurationService
    {
        private static ConfigurationService _instance;
        private static readonly object _lock = new object();

        public static ConfigurationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private const string ConfigFileName = "AnimalConfig.json";
        private AnimalConfiguration _config;
        private Dictionary<string, AnimalTypeConfig> _pluginConfigs = new Dictionary<string, AnimalTypeConfig>();

        private ConfigurationService()
        {
            LoadConfiguration();
        }

        public AnimalConfiguration AnimalConfig => _config;

        private void LoadConfiguration()
        {
            try
            {
                string jsonContent = File.ReadAllText(ConfigFileName);
                _config = JsonSerializer.Deserialize<AnimalConfiguration>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load configuration: {ex.Message}");
            }
        }

        // Helper method to get animal-specific config with fallback to default values
        public AnimalTypeConfig GetAnimalConfig(string animalType)
        {
            // First check plugin configs
            if (_pluginConfigs.TryGetValue(animalType, out var pluginConfig))
            {
                return pluginConfig;
            }

            // Then check built-in animal configs
            if (_config.Animals.TryGetValue(animalType, out var animalConfig))
            {
                return animalConfig;
            }

            // Return default config if specific animal type not found
            return new AnimalTypeConfig
            {
                VisionRange = _config.DefaultVisionRange,
                MovementSpeed = 1,
                Symbol = '?',
                MaxHealth = 10.0,
                PowerLevel = 0
            };
        }

        /// <summary>
        /// Registers a plugin animal configuration.
        /// </summary>
        /// <param name="animalType">The type name of the animal.</param>
        /// <param name="config">The configuration for the animal.</param>
        public void RegisterPluginConfig(string animalType, AnimalTypeConfig config)
        {
            // Add or update the plugin configuration
            _pluginConfigs[animalType] = config;
        }

        /// <summary>
        /// Loads a plugin configuration from an embedded resource in the plugin assembly.
        /// </summary>
        /// <param name="pluginAssembly">The assembly containing the plugin.</param>
        /// <param name="configResourceName">The resource name of the configuration file.</param>
        /// <returns>True if the configuration was loaded successfully; otherwise, false.</returns>
        public bool LoadPluginConfigFromResource(Assembly pluginAssembly, string configResourceName)
        {
            try
            {
                using (Stream stream = pluginAssembly.GetManifestResourceStream(configResourceName))
                {
                    if (stream == null)
                    {
                        Console.WriteLine($"Resource not found: {configResourceName}");
                        return false;
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        var pluginConfig = JsonSerializer.Deserialize<AnimalConfiguration>(jsonContent);

                        // Register each animal from the plugin configuration
                        foreach (var animal in pluginConfig.Animals)
                        {
                            RegisterPluginConfig(animal.Key, animal.Value);
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin configuration: {ex.Message}");
                return false;
            }
        }
    }
}