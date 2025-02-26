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

        public Antelope(Position position) : base(position) { }

        public override void Act(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            var visibleAnimals = LookAround(animals);
            var predators = visibleAnimals.Where(a => a.Symbol == AnimalConstants.LionSymbol).ToList();

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
    }
}