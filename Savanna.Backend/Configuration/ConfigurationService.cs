namespace Savanna.Backend.Configuration
{
    using System;
    using System.IO;
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
            if (_config.Animals.TryGetValue(animalType, out var animalConfig))
            {
                return animalConfig;
            }

            // Return default config if specific animal type not found
            return new AnimalTypeConfig
            {
                VisionRange = _config.DefaultVisionRange,
                MovementSpeed = 1,
                Symbol = '?'
            };
        }
    }
}