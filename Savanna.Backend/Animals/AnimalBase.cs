namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public abstract class AnimalBase : IAnimal, IKillable
    {
        protected readonly Random Random = new Random();
        private readonly Dictionary<IAnimal, int> _proximityCounter = new Dictionary<IAnimal, int>();

        public Position Position { get; set; }
        public abstract char Symbol { get; }
        public virtual int VisionRange => AnimalConstants.DefaultVisionRange;
        public abstract int MovementSpeed { get; }

        public bool IsAlive { get; protected set; } = true;
        public double Health { get; protected set; }
        public abstract double MaxHealth { get; }
        public virtual double HealthDrainPerMove => AnimalConstants.HealthDrainPerMove;

        protected AnimalBase(Position position)
        {
            Position = position;
            Health = MaxHealth;
        }

        public virtual void Move(Direction direction)
        {
            Position offset = direction.GetOffset();

            // Move according to the animal's speed
            for (int i = 0; i < MovementSpeed && IsAlive; i++)
            {
                Position newPosition = new Position(
                    Position.X + offset.X,
                    Position.Y + offset.Y
                );

                // Check if new position is within grid bounds
                if (newPosition.X >= 0 && newPosition.X < Constants.GameConstants.GridWidth &&
                    newPosition.Y >= 0 && newPosition.Y < Constants.GameConstants.GridHeight)
                {
                    Position = newPosition;

                    // Drain health when moving
                    Health -= HealthDrainPerMove;

                    // Check if animal should die from lack of health
                    if (Health <= 0)
                    {
                        Kill();
                    }
                }
            }
        }

        public virtual List<IAnimal> LookAround(List<IAnimal> animals)
        {
            List<IAnimal> visibleAnimals = new List<IAnimal>();

            foreach (var animal in animals)
            {
                if (animal == this) continue;

                double distance = Position.DistanceTo(animal.Position);
                if (distance <= VisionRange)
                {
                    visibleAnimals.Add(animal);
                }
            }

            return visibleAnimals;
        }

        public abstract void Act(List<IAnimal> animals);

        public void Kill()
        {
            IsAlive = false;
        }
    }
}