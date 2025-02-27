namespace Savanna.Backend.Animals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class Lion : AnimalBase
    {
        public override char Symbol => AnimalConstants.LionSymbol;
        public override int VisionRange => AnimalConstants.LionVisionRange;
        public override int MovementSpeed => AnimalConstants.LionMovementSpeed;
        public override double MaxHealth => AnimalConstants.LionMaxHealth;

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
            var prey = visibleAnimals.Where(a => a.Symbol == AnimalConstants.AntelopeSymbol && a.IsAlive).ToList();

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
                        _digestionTimeRemaining = AnimalConstants.LionDigestionTime;

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