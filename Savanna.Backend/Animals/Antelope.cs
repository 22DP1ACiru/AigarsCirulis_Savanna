namespace Savanna.Backend.Animals
{
    using System.Collections.Generic;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;
    using Savanna.Backend.Strategy;

    public class Antelope : AnimalBase, IHerbivore
    {
        private static readonly ConfigurationService _configService = ConfigurationService.Instance;
        private readonly IGrazingStrategy _grazingStrategy;
        private readonly IFleeingStrategy _fleeingStrategy;

        public override char Symbol => _configService.GetAnimalConfig("Antelope").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Antelope").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Antelope").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Antelope").MaxHealth;
        public override int PowerLevel => _configService.GetAnimalConfig("Antelope").PowerLevel;
        public double GrazingThresholdPercentage => _configService.GetAnimalConfig("Antelope").GrazingThresholdPercentage ?? 0.5;

        public Antelope(Position position) : base(position)
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

            // Determine if the antelope should graze based on its health and surroundings
            if (_grazingStrategy.ShouldGraze(this, this, visibleAnimals))
            {
                Graze();
                return; // Grazing takes the whole turn
            }

            // Try to flee from predators
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
            return new Antelope(position);
        }
    }
}   