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
    public class HyenaTests
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
        public void Act_ShouldMoveAwayFromLion_WhenLionIsNearby()
        {
            // Arrange
            var hyena = new Hyena(new Position(5, 5));
            var lion = new Lion(new Position(6, 5)); // Lion is to the right of the hyena
            var animals = new List<IAnimal> { hyena, lion };

            // Act
            hyena.Act(animals);

            // Assert
            Assert.IsTrue(hyena.Position.X < 5); // Hyena should move left to flee from lion
        }

        [TestMethod]
        public void Act_ShouldHuntZebra_WhenZebraIsNearby()
        {
            // Arrange
            var hyena = new Hyena(new Position(5, 5));
            var zebra = new Zebra(new Position(8, 5)); // Zebra is to the right of the hyena
            var animals = new List<IAnimal> { hyena, zebra };

            // Act
            hyena.Act(animals);

            // Assert
            Assert.IsTrue(hyena.Position.X > 5); // Hyena should move right to hunt the antelope
        }

        [TestMethod]
        public void Act_ShouldKillPrey_WhenHyenaMovesToPreyPosition()
        {
            // Arrange
            var hyena = new Hyena(new Position(5, 5));
            var zebra = new Zebra(new Position(7, 5)); // Zebra is two cells to the right (within movement range)
            var animals = new List<IAnimal> { hyena, zebra };

            // Act
            hyena.Act(animals);

            // Assert
            Assert.IsFalse(zebra.IsAlive); // Zebra should be killed
        }

        [TestMethod]
        public void Act_ShouldRestoreHealth_AfterKillingPrey()
        {
            // Arrange
            var hyena = new Hyena(new Position(5, 5));

            // Reduce hyena's health by moving randomly a few times
            for (int i = 0; i < 5; i++)
            {
                hyena.Move(DirectionExtensions.GetRandomDirection());
            }

            // Store the reduced health value for comparison
            double reducedHealth = hyena.Health;

            var zebra = new Zebra(new Position(hyena.Position.X + 2, hyena.Position.Y)); // Zebra is two cells to the right
            var animals = new List<IAnimal> { hyena, zebra };

            // Act
            hyena.Act(animals);

            // Assert
            Assert.IsTrue(hyena.Health > reducedHealth); // Health should be restored after killing prey
        }

        [TestMethod]
        public void Act_ShouldNotMove_WhileDigesting()
        {
            // Arrange
            var hyena = new Hyena(new Position(5, 5));
            var zebra = new Zebra(new Position(7, 5)); // Zebra is two cells to the right
            var animals = new List<IAnimal> { hyena, zebra };

            // Act - First action should kill the zebra and start digestion
            hyena.Act(animals);

            // Record position after first action
            Position positionAfterKill = new Position(hyena.Position.X, hyena.Position.Y);

            // Create a new list without the dead zebra
            animals = animals.Where(a => a.IsAlive).ToList();

            // Act again - Hyena should be digesting and not move
            hyena.Act(animals);

            // Assert
            Assert.AreEqual(positionAfterKill.X, hyena.Position.X);
            Assert.AreEqual(positionAfterKill.Y, hyena.Position.Y);
        }
    }
}