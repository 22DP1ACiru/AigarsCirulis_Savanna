using Savanna.Backend.Interfaces;

public interface IHuntingStrategy
{
    bool TryHunt(ICarnivore hunter, IAnimal hunterAsAnimal, List<IAnimal> visibleAnimals);
}