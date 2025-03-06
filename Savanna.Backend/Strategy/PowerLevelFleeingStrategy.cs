namespace Savanna.Backend.Strategy
{
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class PowerLevelFleeingStrategy : IFleeingStrategy
    {
        public bool TryFlee(IAnimal animal, List<IAnimal> visibleAnimals)
        {
            // Look for predators with higher power level
            var predators = visibleAnimals
                .Where(a => a.IsAlive && a.PowerLevel > animal.PowerLevel)
                .ToList();

            if (predators.Any())
            {
                // Run away from the closest predator
                var closestPredator = predators
                    .OrderBy(p => p.Position.DistanceTo(animal.Position))
                    .First();

                Direction escapeDirection = CalculateDirectionAwayFrom(animal.Position, closestPredator.Position);
                animal.Move(escapeDirection);
                return true; // Handled the turn by fleeing
            }

            return false; // No predators to flee from
        }

        private Direction CalculateDirectionAwayFrom(Position from, Position away)
        {
            int dx = from.X - away.X;
            int dy = from.Y - away.Y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                return dx > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return dy > 0 ? Direction.Down : Direction.Up;
            }
        }
    }
}