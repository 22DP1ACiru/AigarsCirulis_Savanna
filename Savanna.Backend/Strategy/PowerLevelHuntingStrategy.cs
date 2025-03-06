using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;

public class PowerLevelHuntingStrategy : IHuntingStrategy
{
    public bool TryHunt(ICarnivore hunter, IAnimal hunterAsAnimal, List<IAnimal> visibleAnimals)
    {
        if (!hunterAsAnimal.IsAlive)
            return false;

        if (hunter.DigestionTimeRemaining > 0)
        {
            hunter.DigestionTimeRemaining--;
            return true; // Handled the turn by digesting
        }

        // Look for prey with lower power level
        var prey = visibleAnimals
            .Where(a => a.IsAlive && a.PowerLevel < hunterAsAnimal.PowerLevel)
            .OrderBy(p => p.Position.DistanceTo(hunterAsAnimal.Position))
            .FirstOrDefault();

        if (prey != null)
        {
            // Calculate chase direction (towards prey)
            Direction chaseDirection = CalculateDirectionTowards(hunterAsAnimal.Position, prey.Position);
            hunterAsAnimal.Move(chaseDirection);

            // Check if the hunter caught the prey
            if (hunterAsAnimal.Position.Equals(prey.Position))
            {
                hunter.Hunt(prey);
                return true;
            }
            return true; // Handled the turn by chasing
        }

        return false; // Didn't handle the turn, animal should do something else
    }

    private Direction CalculateDirectionTowards(Position from, Position to)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;

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