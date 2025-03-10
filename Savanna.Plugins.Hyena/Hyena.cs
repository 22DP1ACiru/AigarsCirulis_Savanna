using Savanna.Backend.Animals;
using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Backend.Strategy;

namespace Savanna.Plugins.Hyena
{
    public class Hyena : AnimalBase, ICarnivore
    {
        private static readonly ConfigurationService _configService = ConfigurationService.Instance;
        private readonly IHuntingStrategy _huntingStrategy;
        private readonly IFleeingStrategy _fleeingStrategy;

        public override char Symbol => _configService.GetAnimalConfig("Hyena").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Hyena").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Hyena").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Hyena").MaxHealth;
        public override int PowerLevel => _configService.GetAnimalConfig("Hyena").PowerLevel;
        public int DigestionTime => _configService.GetAnimalConfig("Hyena").DigestionTime ?? 3;
        public int DigestionTimeRemaining { get; set; } = 0;

        public Hyena(Position position) : base(position)
        {
            _huntingStrategy = new PowerLevelHuntingStrategy();
            _fleeingStrategy = new PowerLevelFleeingStrategy();
        }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            // Check for potential reproduction
            CheckNearbyAnimalsForBirth(animals);

            var visibleAnimals = LookAround(animals);

            // If there are lions, try to flee
            if (_fleeingStrategy.TryFlee(this, visibleAnimals))
            {
                return; // Fleeing spends turn action
            }

            // If no lions, use the hunting strategy
            bool actionTaken = _huntingStrategy.TryHunt(this, this, visibleAnimals);

            // If hunting didn't handle the turn, move randomly
            if (!actionTaken)
            {
                Move(DirectionExtensions.GetRandomDirection());
            }
        }

        public void Hunt(IAnimal prey)
        {
            if (prey is IKillable killable)
            {
                killable.Kill();
                DigestionTimeRemaining = DigestionTime;
                RestoreHealth();
            }
        }

        public void RestoreHealth()
        {
            Health = MaxHealth;
        }

        protected override IAnimal Birth(Position position)
        {
            return new Hyena(position);
        }
    }
}