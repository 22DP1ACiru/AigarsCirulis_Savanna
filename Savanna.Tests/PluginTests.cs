using Moq;
using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using Savanna.Backend.Plugins;
using System.Text.Json;

namespace Savanna.Tests
{
    [TestClass]
    public class PluginTests
    {
        private Mock<IPluginLoader> _mockPluginLoader;
        private ConfigurationService _configService;
        private PluginManager _pluginManager;
        private AnimalTypeConfig _zebraConfig;
        private AnimalTypeConfig _hyenaConfig;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary AnimalConfig.json file for testing
            var animalConfig = new AnimalConfiguration
            {
                DefaultVisionRange = 4,
                HealthDrainPerMove = 0.5,
                ReproductionProximityCounter = 3,
                ReproductionRange = 1.5,
                Animals = new Dictionary<string, AnimalTypeConfig>
                {
                    ["Lion"] = new AnimalTypeConfig
                    {
                        VisionRange = 5,
                        MovementSpeed = 2,
                        Symbol = 'L',
                        MaxHealth = 15.0,
                        PowerLevel = 2,
                        DigestionTime = 2
                    }
                }
            };

            string configJson = JsonSerializer.Serialize(animalConfig);
            File.WriteAllText("AnimalConfig.json", configJson);

            // Initialize ConfigurationService singleton instance
            _configService = ConfigurationService.Instance;

            // Set up Zebra config
            _zebraConfig = new AnimalTypeConfig
            {
                VisionRange = 5,
                MovementSpeed = 3,
                Symbol = 'Z',
                MaxHealth = 13.0,
                PowerLevel = 0,
                GrazingThresholdPercentage = 0.8
            };

            // Set up Hyena config
            _hyenaConfig = new AnimalTypeConfig
            {
                VisionRange = 4,
                MovementSpeed = 2,
                Symbol = 'H',
                MaxHealth = 13.0,
                PowerLevel = 1,
                DigestionTime = 3
            };

            // Create mock plugin loader
            _mockPluginLoader = new Mock<IPluginLoader>();

            // Create PluginManager with mock loader
            _pluginManager = new PluginManager(_mockPluginLoader.Object, _configService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test config file
            if (File.Exists("AnimalConfig.json"))
            {
                File.Delete("AnimalConfig.json");
            }
        }

        [TestMethod]
        public void PluginLoader_LoadPlugins_ReturnsEmptyList_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var loader = new PluginLoader();
            string nonExistentDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // Act
            var result = loader.LoadPlugins(nonExistentDirectory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            Assert.IsTrue(Directory.Exists(nonExistentDirectory));

            // Cleanup
            Directory.Delete(nonExistentDirectory);
        }

        [TestMethod]
        public void PluginManager_CreateAnimalFromPlugin_ReturnsAnimal_WhenPluginIsRegistered()
        {
            // Arrange
            var zebraPluginMock = new Mock<IAnimalPlugin>();
            zebraPluginMock.Setup(p => p.AnimalType).Returns("Zebra");
            zebraPluginMock.Setup(p => p.GetAnimalConfig()).Returns(_zebraConfig);

            var animalMock = new Mock<IAnimal>();
            animalMock.Setup(a => a.Symbol).Returns('Z');

            zebraPluginMock.Setup(p => p.CreateAnimal(It.IsAny<Position>())).Returns(animalMock.Object);

            var plugins = new List<IAnimalPlugin> { zebraPluginMock.Object };
            _mockPluginLoader.Setup(l => l.LoadPlugins(It.IsAny<string>())).Returns(plugins);

            // Act
            _pluginManager.Initialize();
            var animal = _pluginManager.CreateAnimalFromPlugin('Z', new Position(0, 0));

            // Assert
            Assert.IsNotNull(animal);
            Assert.AreEqual('Z', animal.Symbol);
            zebraPluginMock.Verify(p => p.CreateAnimal(It.IsAny<Position>()), Times.Once);
        }

        [TestMethod]
        public void PluginManager_CreateAnimalFromPlugin_ReturnsNull_WhenPluginIsNotRegistered()
        {
            // Arrange
            _mockPluginLoader.Setup(l => l.LoadPlugins(It.IsAny<string>())).Returns(new List<IAnimalPlugin>());

            // Act
            _pluginManager.Initialize();
            var animal = _pluginManager.CreateAnimalFromPlugin('X', new Position(0, 0));

            // Assert
            Assert.IsNull(animal);
        }

        [TestMethod]
        public void PluginManager_Initialize_RegistersPluginConfigs()
        {
            // Arrange
            var zebraPluginMock = new Mock<IAnimalPlugin>();
            zebraPluginMock.Setup(p => p.AnimalType).Returns("Zebra");
            zebraPluginMock.Setup(p => p.GetAnimalConfig()).Returns(_zebraConfig);

            var hyenaPluginMock = new Mock<IAnimalPlugin>();
            hyenaPluginMock.Setup(p => p.AnimalType).Returns("Hyena");
            hyenaPluginMock.Setup(p => p.GetAnimalConfig()).Returns(_hyenaConfig);

            var plugins = new List<IAnimalPlugin> { zebraPluginMock.Object, hyenaPluginMock.Object };
            _mockPluginLoader.Setup(l => l.LoadPlugins(It.IsAny<string>())).Returns(plugins);

            // Act
            _pluginManager.Initialize();

            // Assert
            var registeredPlugins = _pluginManager.RegisteredPlugins;
            Assert.AreEqual(2, registeredPlugins.Count);
            Assert.IsTrue(registeredPlugins.ContainsKey('Z'));
            Assert.IsTrue(registeredPlugins.ContainsKey('H'));

            // Verify the configs were registered
            var zebraConfig = _configService.GetAnimalConfig("Zebra");
            var hyenaConfig = _configService.GetAnimalConfig("Hyena");

            Assert.AreEqual(5, zebraConfig.VisionRange);
            Assert.AreEqual(3, zebraConfig.MovementSpeed);
            Assert.AreEqual('Z', zebraConfig.Symbol);

            Assert.AreEqual(4, hyenaConfig.VisionRange);
            Assert.AreEqual(2, hyenaConfig.MovementSpeed);
            Assert.AreEqual('H', hyenaConfig.Symbol);
        }

        [TestMethod]
        public void PluginManager_Initialize_HandlesSymbolCollision()
        {
            // Arrange
            // First plugin with symbol 'Z'
            var zebraPluginMock = new Mock<IAnimalPlugin>();
            zebraPluginMock.Setup(p => p.AnimalType).Returns("Zebra");
            zebraPluginMock.Setup(p => p.GetAnimalConfig()).Returns(_zebraConfig);

            // Second plugin also trying to use symbol 'Z' - should be skipped
            var hyenaConfigWithCollision = new AnimalTypeConfig
            {
                VisionRange = 4,
                MovementSpeed = 2,
                Symbol = 'Z', // Same symbol as zebra
                MaxHealth = 13.0,
                PowerLevel = 1,
                DigestionTime = 3
            };

            var hyenaPluginMock = new Mock<IAnimalPlugin>();
            hyenaPluginMock.Setup(p => p.AnimalType).Returns("Hyena");
            hyenaPluginMock.Setup(p => p.GetAnimalConfig()).Returns(hyenaConfigWithCollision);

            var plugins = new List<IAnimalPlugin> { zebraPluginMock.Object, hyenaPluginMock.Object };
            _mockPluginLoader.Setup(l => l.LoadPlugins(It.IsAny<string>())).Returns(plugins);

            // Act
            _pluginManager.Initialize();

            // Assert
            var registeredPlugins = _pluginManager.RegisteredPlugins;
            Assert.AreEqual(1, registeredPlugins.Count);
            Assert.IsTrue(registeredPlugins.ContainsKey('Z'));
            Assert.AreEqual("Zebra", registeredPlugins['Z'].AnimalType);
        }

        [TestMethod]
        public void ConfigurationService_GetAnimalConfig_ReturnsPluginConfig_WhenRegistered()
        {
            // Arrange
            _configService.RegisterPluginConfig("Zebra", _zebraConfig);

            // Act
            var config = _configService.GetAnimalConfig("Zebra");

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(5, config.VisionRange);
            Assert.AreEqual(3, config.MovementSpeed);
            Assert.AreEqual('Z', config.Symbol);
            Assert.AreEqual(13.0, config.MaxHealth);
            Assert.AreEqual(0, config.PowerLevel);
            Assert.AreEqual(0.8, config.GrazingThresholdPercentage);
        }

        [TestMethod]
        public void ConfigurationService_GetAnimalConfig_ReturnsBuiltInConfig_WhenFound()
        {
            // Arrange - Lion is in the built-in config

            // Act
            var config = _configService.GetAnimalConfig("Lion");

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(5, config.VisionRange);
            Assert.AreEqual(2, config.MovementSpeed);
            Assert.AreEqual('L', config.Symbol);
            Assert.AreEqual(15.0, config.MaxHealth);
            Assert.AreEqual(2, config.PowerLevel);
            Assert.AreEqual(2, config.DigestionTime);
        }

        [TestMethod]
        public void ConfigurationService_GetAnimalConfig_ReturnsDefaultConfig_WhenTypeNotFound()
        {
            // Arrange - "Unknown" animal type is not configured

            // Act
            var config = _configService.GetAnimalConfig("Unknown");

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(4, config.VisionRange); // Default from config
            Assert.AreEqual(1, config.MovementSpeed); // Default
            Assert.AreEqual('?', config.Symbol); // Default
            Assert.AreEqual(10.0, config.MaxHealth); // Default
            Assert.AreEqual(0, config.PowerLevel); // Default
        }

        [TestMethod]
        public void AnimalPluginConfig_Deserialization_DeserializesCorrectly()
        {
            // Arrange
            string zebraConfigJson = @"{
              ""VisionRange"": 5,
              ""MovementSpeed"": 3,
              ""Symbol"": ""Z"",
              ""MaxHealth"": 13.0,
              ""PowerLevel"": 0,
              ""GrazingThresholdPercentage"": 0.8
            }";

            // Act
            AnimalTypeConfig config = JsonSerializer.Deserialize<AnimalTypeConfig>(zebraConfigJson);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(5, config.VisionRange);
            Assert.AreEqual(3, config.MovementSpeed);
            Assert.AreEqual('Z', config.Symbol);
            Assert.AreEqual(13.0, config.MaxHealth);
            Assert.AreEqual(0, config.PowerLevel);
            Assert.AreEqual(0.8, config.GrazingThresholdPercentage);
        }
    }
}