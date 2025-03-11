namespace Savanna.Backend.Strategy
{
    using Savanna.Backend.Animals;
    using Savanna.Backend.Interfaces;

    public class HealthBasedGrazingStrategy : IGrazingStrategy
    {
        public bool ShouldGraze(IHerbivore herbivore, IAnimal herbivoreAsAnimal, List<IAnimal> visibleAnimals)
        {
            if (!herbivoreAsAnimal.IsAlive)
                return false;

            // If it's a herbivore with AnimalBase capabilities, check its health
            if (herbivoreAsAnimal is AnimalBase animalBase)
            {
                double healthThreshold = animalBase.MaxHealth * herbivore.GrazingThresholdPercentage;

                // Look for predators
                var predators = visibleAnimals
                    .Where(a => a.IsAlive && a.PowerLevel > herbivoreAsAnimal.PowerLevel)
                    .Any();

                // Graze if health is low and no predators nearby
                return animalBase.Health < healthThreshold && !predators;
            }

            return false;
        }
    }
}