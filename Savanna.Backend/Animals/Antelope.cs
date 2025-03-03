namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class Antelope : AnimalBase
    {
        private static readonly ConfigurationService _configService = ConfigurationService.Instance;

        public override char Symbol => _configService.GetAnimalConfig("Antelope").Symbol;
        public override int VisionRange => _configService.GetAnimalConfig("Antelope").VisionRange;
        public override int MovementSpeed => _configService.GetAnimalConfig("Antelope").MovementSpeed;
        public override double MaxHealth => _configService.GetAnimalConfig("Antelope").MaxHealth;
        public double GrazingThresholdPercentage => _configService.GetAnimalConfig("Antelope").GrazingThresholdPercentage ?? 0.5;

        // Health threshold when the antelope considers grazing instead of moving
        private double _grazingThreshold => MaxHealth * GrazingThresholdPercentage; // Graze when below 50% health

        public Antelope(Position position) : base(position) { }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            var visibleAnimals = LookAround(animals);
            var predators = visibleAnimals.Where(a => a.Symbol == _configService.GetAnimalConfig("Lion").Symbol).ToList();

            // Check for potential reproduction
            CheckNearbyAnimalsForBirth(animals);

            // If health is low and no predators are nearby, graze
            if (Health < _grazingThreshold && !predators.Any())
            {
                Graze();
                return; // Skip movement this turn since it is spent grazing
            }

            if (predators.Any())
            {
                // Run away from the closest predator
                var closestPredator = predators.OrderBy(p => p.Position.DistanceTo(Position)).First();

                // Calculate escape direction (away from predator)
                int dx = Position.X - closestPredator.Position.X;
                int dy = Position.Y - closestPredator.Position.Y;

                Direction escapeDirection;

                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    escapeDirection = dx > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    escapeDirection = dy > 0 ? Direction.Down : Direction.Up;
                }

                Move(escapeDirection);
            }
            else
            {
                // Move randomly if no predators are visible
                Move(DirectionExtensions.GetRandomDirection());
            }
        }

        private void Graze()
        {
            // Åntelope eats grass and regains health
            Health = MaxHealth;
        }

        protected override IAnimal Birth(Position position)
        {
            return new Antelope(position);
        }
    }
}   