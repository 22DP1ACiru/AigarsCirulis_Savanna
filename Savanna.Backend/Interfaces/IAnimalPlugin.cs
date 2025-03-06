namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Models;

    public interface IAnimalPlugin
    {
        // Unique identifier for the plugin
        string AnimalType { get; }

        // Factory method to create a new instance
        IAnimal CreateAnimal(Position position);
    }
}