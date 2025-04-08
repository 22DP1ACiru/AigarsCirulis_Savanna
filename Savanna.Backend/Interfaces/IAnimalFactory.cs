using Savanna.Backend.Models;
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

        /// <summary>
        /// Creates an animal using specified type in specified position.
        /// </summary>
        /// <param name="animalTypeName">Animal type name in string.</param>
        /// <param name="initialPosition"></param>
        /// <returns></returns>
        IAnimal CreateAnimalByType(string animalTypeName, Position initialPosition);
    }
}