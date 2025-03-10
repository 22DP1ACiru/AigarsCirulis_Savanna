namespace Savanna.Backend.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;

    /// <summary>
    /// Loads plugins from DLL files.
    /// </summary>
    public class PluginLoader : IPluginLoader
    {
        /// <summary>
        /// Loads all available animal plugins from the specified directory.
        /// </summary>
        /// <param name="directory">The directory containing plugin DLLs.</param>
        /// <returns>A list of loaded animal plugins.</returns>
        public List<IAnimalPlugin> LoadPlugins(string directory)
        {
            List<IAnimalPlugin> plugins = new List<IAnimalPlugin>();

            // Create plugins directory if it doesn't exist
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Created plugins directory: {directory}");
                return plugins; // Return empty list since no plugins exist yet
            }

            // Get all DLL files in the plugins directory
            string[] pluginFiles = Directory.GetFiles(directory, "*.dll");

            foreach (string pluginFile in pluginFiles)
            {
                try
                {
                    // Load the assembly
                    Assembly assembly = LoadPluginAssembly(pluginFile);
                    if (assembly == null) continue;

                    // Find all types that implement IAnimalPlugin
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (typeof(IAnimalPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            // Create an instance of the plugin
                            if (Activator.CreateInstance(type) is IAnimalPlugin plugin)
                            {
                                plugins.Add(plugin);
                                Console.WriteLine($"Loaded plugin: {plugin.AnimalType} from {Path.GetFileName(pluginFile)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue loading other plugins
                    Console.WriteLine($"Error loading plugin {pluginFile}: {ex.Message}");
                }
            }

            return plugins;
        }

        /// <summary>
        /// Loads a plugin assembly from a file safely.
        /// </summary>
        /// <param name="pluginPath">The file path to the plugin DLL.</param>
        /// <returns>The loaded assembly or null if loading failed.</returns>
        private Assembly LoadPluginAssembly(string pluginPath)
        {
            try
            {
                // Load the assembly into a separate context to avoid locking the file
                return Assembly.LoadFrom(pluginPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {pluginPath}: {ex.Message}");
                return null;
            }
        }
    }
}