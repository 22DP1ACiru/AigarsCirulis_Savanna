namespace Savanna.Backend.Animals
{
    using System.Collections.Generic;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;
    using Savanna.Backend.Strategy;
    using Savanna.Backend.Models.State;


    public class Lion : AnimalBase, ICarnivore  
    {
        private readonly IHuntingStrategy _huntingStrategy;

        public override char Symbol => _configService.GetAnimalConfig("Lion").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Lion").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Lion").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Lion").MaxHealth;
        public override int PowerLevel => _configService.GetAnimalConfig("Lion").PowerLevel;
        public int DigestionTime => _configService.GetAnimalConfig("Lion").DigestionTime ?? 2;
        public int DigestionTimeRemaining { get; set; } = 0;

        public Lion(Position position) : base(position)
        {
            _huntingStrategy = new PowerLevelHuntingStrategy();
        }

        public override AnimalStateDto GetState()
        {
            var state = base.GetState(); // Get common state from base
            state.DigestionTimeRemaining = this.DigestionTimeRemaining;
            return state;
        }

        public override void LoadState(AnimalStateDto state)
        {
            base.LoadState(state); // Restore common state using base method

            this.DigestionTimeRemaining = state.DigestionTimeRemaining ?? 0;
        }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            // Check for potential reproduction
            CheckNearbyAnimalsForBirth(animals);

            var visibleAnimals = LookAround(animals);

            // Use the hunting strategy
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
            return new Lion(position);
        }
    }
}