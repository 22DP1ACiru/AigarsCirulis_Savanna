using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Savanna.Tests
{
    [TestClass]
    public class LionTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Create and register a mock game grid
            var mockGrid = new GameGrid(new List<IAnimal>());
            GameGridMediator.Instance.RegisterGameGrid(mockGrid);
        }

        [TestMethod]
        public void Act_ShouldMoveTowardsPrey_WhenPreyIsVisible()
        {
            // Arrange
            var lion = new Lion(new Position(5, 5));
            var antelope = new Antelope(new Position(8, 5)); // Antelope is to the right of the lion
            var animals = new List<IAnimal> { lion, antelope };

            // Act
            lion.Act(animals);

            // Assert
            Assert.IsTrue(lion.Position.X > 5); // Lion should move right to chase the antelope
        }

        [TestMethod]
        public void Act_ShouldKillPrey_WhenMovesPutsLionOnPreyPosition()
        {
            // Arrange
            var lion = new Lion(new Position(5, 5));
            var antelope = new Antelope(new Position(7, 5)); // Antelope is one cell to the right
            var animals = new List<IAnimal> { lion, antelope };

            // Act
            lion.Act(animals);

            // Assert
            Assert.IsFalse(antelope.IsAlive); // Antelope should be killed
        }

        [TestMethod]
        public void Act_ShouldRestoreHealth_AfterKillingPrey()
        {
            // Arrange
            var lion = new Lion(new Position(5, 5));

            // Reduce lion's health by moving randomly a few times
            for (int i = 0; i < 5; i++)
            {
                lion.Move(DirectionExtensions.GetRandomDirection());
            }

            // Store the reduced health value for comparison
            double reducedHealth = lion.Health;

            var antelope = new Antelope(new Position(lion.Position.X + 2, lion.Position.Y)); // Antelope is two cells to the right
            var animals = new List<IAnimal> { lion, antelope };

            // Act
            lion.Act(animals);

            // Assert
            Assert.IsTrue(lion.Health > reducedHealth); // Health should be restored after killing prey
        }

        [TestMethod]
        public void Act_ShouldNotMove_WhileDigesting()
        {
            // Arrange
            var lion = new Lion(new Position(5, 5));
            var antelope = new Antelope(new Position(7, 5)); // Antelope is two cells to the right
            var animals = new List<IAnimal> { lion, antelope };

            // Act - First action should kill the first antelope and start digestion
            lion.Act(animals);

            // Record position after first action
            Position positionAfterKill = new Position(lion.Position.X, lion.Position.Y);

            // Act again - Lion should be digesting and not move
            lion.Act(animals);

            // Assert
            Assert.AreEqual(positionAfterKill.X, lion.Position.X);
            Assert.AreEqual(positionAfterKill.Y, lion.Position.Y);
        }
    }
}