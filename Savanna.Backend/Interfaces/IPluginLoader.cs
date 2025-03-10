namespace Savanna.Backend.Interfaces
{
    /// <summary>
    /// Interface for plugin loading functionality.
    /// </summary>
    public interface IPluginLoader
    {
        /// <summary>
        /// Loads all available animal plugins from the specified directory.
        /// </summary>
        /// <param name="directory">The directory containing plugin DLLs.</param>
        /// <returns>A list of loaded animal plugins.</returns>
        List<IAnimalPlugin> LoadPlugins(string directory);
    }
}
