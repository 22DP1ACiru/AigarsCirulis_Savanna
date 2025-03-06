using Savanna.Backend.Interfaces;

public interface IFleeingStrategy
{
    bool TryFlee(IAnimal animal, List<IAnimal> visibleAnimals);
}