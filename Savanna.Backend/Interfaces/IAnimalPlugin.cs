namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Models;

    public interface IAnimalPlugin
    {
        /// <summary>
        /// Gets the type name of the animal this plugin provides.
        /// </summary>
        string AnimalType { get; }

        /// <summary>
        /// Creates a new instance of the animal at the specified position.
        /// </summary>
        /// <param name="position">The position where the animal should be created.</param>
        /// <returns>A new animal instance.</returns>
        IAnimal CreateAnimal(Position position);

        /// <summary>
        /// Gets the configuration for this animal type.
        /// </summary>
        /// <returns>Configuration settings for this animal type.</returns>
        AnimalTypeConfig GetAnimalConfig();
    }
}