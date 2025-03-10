namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Models;

    /// <summary>
    /// Represents an animal in the savanna simulation.
    /// </summary>
    public interface IAnimal
    {
        /// <summary>
        /// Gets the current position of the animal in the game grid.
        /// </summary>
        Position Position { get; set; }

        /// <summary>
        /// Gets the character symbol used to represent this animal on the display grid.
        /// </summary>
        char Symbol { get; }

        /// <summary>
        /// Gets the vision range of the animal in grid cells.
        /// </summary>
        int VisionRange { get; }

        /// <summary>
        /// Gets the movement speed of the animal in cells per turn.
        /// </summary>
        int MovementSpeed { get; }

        /// <summary>
        /// Gets the power level of the animal which defines its behaviour with other animals and status as predator or prey.
        /// </summary>
        int PowerLevel { get; }

        /// <summary>
        /// Gets a value indicating whether the animal is alive.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Moves the animal in the specified direction.
        /// </summary>
        /// <param name="direction">The direction in which to move.</param>
        void Move(Direction direction);

        /// <summary>
        /// Identifies other animals within the vision range of this animal.
        /// </summary>
        /// <param name="animals">The list of all animals in the simulation.</param>
        /// <returns>A list of animals visible to this animal.</returns>
        List<IAnimal> LookAround(List<IAnimal> animals);

        /// <summary>
        /// Determines and performs the animal's action for the current turn.
        /// </summary>
        /// <param name="animals">The list of all animals in the simulation.</param>
        void Act(List<IAnimal> animals);
    }
}