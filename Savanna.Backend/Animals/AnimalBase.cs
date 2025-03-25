namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using Savanna.Backend.Configuration;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public abstract class AnimalBase : IAnimal, IKillable
    {
        protected readonly Random Random = new Random();
        private readonly Dictionary<IAnimal, int> _proximityCounter = new Dictionary<IAnimal, int>();
        protected static readonly ConfigurationService _configService = ConfigurationService.Instance;

        public Position Position { get; set; }
        public abstract char Symbol { get; }
        public virtual int VisionRange => _configService.AnimalConfig.DefaultVisionRange;
        public abstract int MovementSpeed { get; }

        public bool IsAlive { get; protected set; } = true;
        public double Health { get; protected set; }
        public abstract double MaxHealth { get; }
        public abstract int PowerLevel { get; }
        public virtual double HealthDrainPerMove => _configService.AnimalConfig.HealthDrainPerMove;

        public int ReproductionProximityCounter => _configService.AnimalConfig.ReproductionProximityCounter;
        public double ReproductionRange => _configService.AnimalConfig.ReproductionRange;

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

                if (GameGridMediator.Instance.IsPositionValid(newPosition))
                {
                    // Check if the target cell is occupied
                    var animalAtPosition = GameGridMediator.Instance.GetAnimalAtPosition(newPosition);

                    if (animalAtPosition == null)
                    {
                        Position = newPosition;
                    }
                    else if (animalAtPosition.GetType() != this.GetType())
                    {
                        // Target cell is occupied by an animal of a different type, allow the move
                        Position = newPosition;
                    }
                    else
                    {
                        // Target cell is occupied by an animal of the same type
                        // Block the move
                        break;
                    }

                    // Drain health when moving
                    Health -= HealthDrainPerMove;

                    // Check if animal should die from lack of health
                    if (Health <= 0)
                    {
                        Kill();
                    }
                }
                else
                {
                    // Cannot move in this direction
                    break;
                }
            }
        }

        public virtual List<IAnimal> LookAround(List<IAnimal> animals)
        {
            List<IAnimal> visibleAnimals = new List<IAnimal>();

            // Iterate over a copy to prevent modification during enumeration
            foreach (var animal in animals.ToList())
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

        protected virtual void CheckNearbyAnimalsForBirth(List<IAnimal> animals)
        {
            if (!IsAlive) return;

            // Only check for animals of the same type (using symbol as identifier)
            var nearbyAnimals = animals
                .Where(a => a != this && a.IsAlive && a.Symbol == this.Symbol)
                .Where(a => Position.DistanceTo(a.Position) <= ReproductionRange)
                .ToList();

            foreach (var animal in nearbyAnimals)
            {
                // Initialize counter if this is the first encounter
                if (!_proximityCounter.ContainsKey(animal))
                {
                    _proximityCounter[animal] = 1;
                }
                else
                {
                    _proximityCounter[animal]++;

                    // Check if the animals have been together for specified consecutive turns
                    if (_proximityCounter[animal] >= ReproductionProximityCounter)
                    {
                        // Only one animal initiates reproduction to avoid duplicate offspring
                        if (GetHashCode() < animal.GetHashCode())
                        {
                            CreateOffspring();
                        }

                        _proximityCounter[animal] = 0;
                    }
                }
            }

            // Reset counters for animals no longer nearby or not alive
            var keysToRemove = _proximityCounter.Keys
                .Where(a => !nearbyAnimals.Contains(a) || !a.IsAlive)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _proximityCounter.Remove(key);
            }
        }

        protected virtual void CreateOffspring()
        {
            // Find an empty adjacent position
            Position birthPosition = GameGridMediator.Instance.FindEmptyAdjacentPosition(Position);

            if (birthPosition != null)
            {
                // Create offspring at the empty adjacent position
                IAnimal offspring = Birth(birthPosition);
                GameEngineMediator.Instance.RequestAnimalCreation(offspring);
            }
        }

        protected abstract IAnimal Birth(Position position);
    }
}