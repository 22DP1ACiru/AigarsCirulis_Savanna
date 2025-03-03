using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;

namespace Savanna.Tests
{
    [TestClass]
    public class AntelopeTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Create and register a mock game grid
            var mockGrid = new GameGrid(new List<IAnimal>());
            GameGridMediator.Instance.RegisterGameGrid(mockGrid);
        }

        [TestMethod]
        public void Act_ShouldMoveAwayFromPredator_WhenPredatorIsNearby()
        {
            // Arrange
            var antelope = new Antelope(new Position(5, 5));
            var lion = new Lion(new Position(6, 5)); // Lion is to the right of the antelope
            var animals = new List<IAnimal> { antelope, lion };

            // Act
            antelope.Act(animals);

            // Assert
            Assert.IsTrue(antelope.Position.X < 5); // Antelope should move left to flee
        }
    }

}
