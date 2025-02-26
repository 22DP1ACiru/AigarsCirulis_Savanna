namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Models;

    public interface IAnimal
    {
        Position Position { get; }
        char Symbol { get; }
        int VisionRange { get; }
        int MovementSpeed { get; }
        bool IsAlive { get; }

        void Move(Direction direction);
        List<IAnimal> LookAround(List<IAnimal> animals);
        void Act(List<IAnimal> animals);
    }
}