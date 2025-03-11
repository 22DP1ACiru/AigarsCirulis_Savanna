using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Plugins.Hyena;
using Savanna.Plugins.Zebra;

namespace Savanna.Tests
{
    [TestClass]
    public class ZebraTests
    {
        [TestInitialize]
        public void Setup()
        {
            var configService = ConfigurationService.Instance;
            configService.RegisterPluginConfig("Hyena", new HyenaPlugin().GetAnimalConfig());
            configService.RegisterPluginConfig("Zebra", new ZebraPlugin().GetAnimalConfig());

            // Create and register a mock game grid
            var mockGrid = new GameGrid(new List<IAnimal>());
            GameGridMediator.Instance.RegisterGameGrid(mockGrid);
        }

        [TestMethod]
        public void Act_ShouldMoveAwayFromPredator_WhenPredatorIsNearby()
        {
            // Arrange
            var zebra = new Zebra(new Position(5, 5));
            var lion = new Lion(new Position(6, 5)); // Lion is to the right of the zebra
            var animals = new List<IAnimal> { zebra, lion };

            // Act
            zebra.Act(animals);

            // Assert
            Assert.IsTrue(zebra.Position.X < 5); // Zebra should move left to flee from lion
        }

        [TestMethod]
        public void Act_ShouldGraze_WhenHealthIsLow()
        {
            // Arrange
            var zebra = new Zebra(new Position(5, 5));

            // Reduce health by moving randomly a few times
            for (int i = 0; i < 5; i++)
            {
                zebra.Move(DirectionExtensions.GetRandomDirection());
            }

            double initialHealth = zebra.Health;
            var animals = new List<IAnimal> { zebra };

            // Act
            zebra.Act(animals);

            // Assert
            Assert.IsTrue(zebra.Health > initialHealth); // Health should increase after grazing
        }

        [TestMethod]
        public void Act_ShouldMoveRandomly_WhenNoPredatorsAndHealthIsGood()
        {
            // Arrange
            var zebra = new Zebra(new Position(5, 5));
            var initialPosition = new Position(zebra.Position.X, zebra.Position.Y);
            var animals = new List<IAnimal> { zebra };

            // Act
            zebra.Act(animals);

            // Assert
            // The zebra should have moved from its initial position
            Assert.AreNotEqual(initialPosition, zebra.Position);
        }
    }
}