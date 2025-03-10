using Savanna.Backend.Animals;
using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Backend.Strategy;

namespace Savanna.Plugins.Zebra
{
    public class Zebra : AnimalBase, IHerbivore
    {
        private readonly IGrazingStrategy _grazingStrategy;
        private readonly IFleeingStrategy _fleeingStrategy;

        public override char Symbol => _configService.GetAnimalConfig("Zebra").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Zebra").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Zebra").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Zebra").MaxHealth;
        public override int PowerLevel => _configService.GetAnimalConfig("Zebra").PowerLevel;
        public double GrazingThresholdPercentage => _configService.GetAnimalConfig("Zebra").GrazingThresholdPercentage ?? 0.8;

        public Zebra(Position position) : base(position)
        {
            _grazingStrategy = new HealthBasedGrazingStrategy();
            _fleeingStrategy = new PowerLevelFleeingStrategy();
        }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            var visibleAnimals = LookAround(animals);

            // Check for potential reproduction
            CheckNearbyAnimalsForBirth(animals);

            // Use grazing strategy
            if (_grazingStrategy.ShouldGraze(this, this, visibleAnimals))
            {
                Graze();
                return;
            }

            // Try to flee from predators (using the fleeing strategy)
            bool fled = _fleeingStrategy.TryFlee(this, visibleAnimals);

            // If no fleeing occurred, move randomly
            if (!fled)
            {
                Move(DirectionExtensions.GetRandomDirection());
            }
        }

        public void Graze()
        {
            RestoreHealth();
        }

        public void RestoreHealth()
        {
            Health = MaxHealth;
        }

        protected override IAnimal Birth(Position position)
        {
            return new Zebra(position);
        }
    }
}
