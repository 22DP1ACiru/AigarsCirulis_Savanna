using Savanna.Backend.Configuration;
using Savanna.Backend.Plugins;
using Savanna.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Savanna.Backend.Interfaces;

namespace Savanna.Core.Services
{
    public class DisplayConfigService : IDisplayConfigService
    {
        private readonly ILogger<DisplayConfigService> _logger;
        private readonly ConfigurationService _configService = ConfigurationService.Instance;
        private readonly PluginManager _pluginManager = PluginManager.Instance;

        public DisplayConfigService(ILogger<DisplayConfigService> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the current symbol-to-icon mapping.
        /// </summary>
        /// <returns>A dictionary where Key=Symbol (char), Value=Icon (string).</returns>
        public Dictionary<char, string> GetSymbolIconMap()
        {
            _logger?.LogInformation("Building display configuration map.");
            var displayMap = new Dictionary<char, string>();
            
            // Add built-ins
            if (_configService.AnimalConfig?.Animals != null)
            {
                foreach (var kvp in _configService.AnimalConfig.Animals)
                {
                    var config = kvp.Value;
                    // Check symbol and non-empty icon
                    if (config.Symbol != default(char) && !string.IsNullOrEmpty(config.Icon))
                    {
                        displayMap[config.Symbol] = config.Icon;
                    }
                }
            }
            
            // Add plugins
            foreach (var kvp in _pluginManager.RegisteredPlugins)
            {
                char symbol = kvp.Key;
                IAnimalPlugin plugin = kvp.Value;

                // Get the config associated with this specific plugin instance
                // (which should be the same one registered in ConfigurationService)
                var pluginConfig = plugin.GetAnimalConfig();

                // Ensure it has an icon and hasn't already been added
                if (pluginConfig != null && !string.IsNullOrEmpty(pluginConfig.Icon) && !displayMap.ContainsKey(symbol))
                {
                    displayMap[symbol] = pluginConfig.Icon;
                }
            }
            _logger?.LogDebug("Returning display config map with {Count} entries.", displayMap.Count);
            return displayMap;
        }
    }
}