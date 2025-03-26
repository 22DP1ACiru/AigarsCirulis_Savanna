using Savanna.Backend.Models.State;

namespace Savanna.Backend.Interfaces
{
    public interface IAnimalFactory
    {
        /// <summary>
        /// Creates an animal instance from its persisted state DTO.
        /// </summary>
        /// <param name="state">The state DTO to restore from.</param>
        /// <returns>A new IAnimal instance with its state loaded, or null if creation fails.</returns>
        IAnimal CreateAnimalFromState(AnimalStateDto state);
    }
}