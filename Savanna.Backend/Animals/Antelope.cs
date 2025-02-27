namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class Antelope : AnimalBase
    {
        public override char Symbol => AnimalConstants.AntelopeSymbol;
        public override int VisionRange => AnimalConstants.AntelopeVisionRange;
        public override int MovementSpeed => AnimalConstants.AntelopeMovementSpeed;
        public override double MaxHealth => AnimalConstants.AntelopeMaxHealth;

        // Health threshold when the antelope considers grazing instead of moving
        private double _grazingThreshold => MaxHealth * 0.5; // Graze when below 50% health

        public Antelope(Position position) : base(position) { }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            var visibleAnimals = LookAround(animals);
            var predators = visibleAnimals.Where(a => a.Symbol == AnimalConstants.LionSymbol).ToList();

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

        protected override void Birth()
        {
            // Request a new Antelope from the GameEngine
            GameEngineMediator.Instance.RequestAnimalCreation(new Antelope(Position));
        }
    }
}