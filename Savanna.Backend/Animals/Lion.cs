namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class Lion : AnimalBase
    {
        private static readonly ConfigurationService _configService = ConfigurationService.Instance;

        public override char Symbol => _configService.GetAnimalConfig("Lion").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Lion").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Lion").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Lion").MaxHealth;
        public int DigestionTime => _configService.GetAnimalConfig("Lion").DigestionTime ?? 2;

        private int _digestionTimeRemaining = 0;

        public Lion(Position position) : base(position) { }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            // Check for potential reproduction
            CheckNearbyAnimalsForBirth(animals);

            if (_digestionTimeRemaining > 0)
            {
                _digestionTimeRemaining--;
                return; // Lion is digesting and can't move
            }

            var visibleAnimals = LookAround(animals);
            var prey = visibleAnimals.Where(a => a.Symbol == _configService.GetAnimalConfig("Antelope").Symbol && a.IsAlive).ToList();

            if (prey.Any())
            {
                // Chase the closest prey
                var closestPrey = prey.OrderBy(p => p.Position.DistanceTo(Position)).First();

                // Calculate chase direction (towards prey)
                int dx = closestPrey.Position.X - Position.X;
                int dy = closestPrey.Position.Y - Position.Y;

                Direction chaseDirection;

                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    chaseDirection = dx > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    chaseDirection = dy > 0 ? Direction.Down : Direction.Up;
                }

                Move(chaseDirection);

                // Check if the lion caught the prey
                if (Position.Equals(closestPrey.Position))
                {
                    // Lion caught the prey - need to use the interface method instead of direct access
                    // Change the prey's state to not alive through the interface
                    var antelopeAsIAnimal = closestPrey;

                    if (antelopeAsIAnimal is IKillable killable)
                    {
                        killable.Kill();
                        _digestionTimeRemaining = DigestionTime;

                        // Lion regains health from eating the prey
                        Health = MaxHealth;
                    }
                }
            }
            else
            {
                // Move randomly if no prey is visible
                Move(DirectionExtensions.GetRandomDirection());
            }
        }

        protected override IAnimal Birth(Position position)
        {
            return new Lion(position);
        }
    }
}