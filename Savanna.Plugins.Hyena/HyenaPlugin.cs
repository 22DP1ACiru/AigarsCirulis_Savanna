using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using System.Reflection;
using System.Text.Json;

namespace Savanna.Plugins.Hyena
{
    public class HyenaPlugin : IAnimalPlugin
    {
        public string AnimalType => typeof(Hyena).Name;

        public IAnimal CreateAnimal(Position position)
        {
            return new Hyena(position);
        }

        public AnimalTypeConfig GetAnimalConfig()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{assembly.GetName().Name}.config.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Plugin config not found.");
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonSerializer.Deserialize<AnimalTypeConfig>(reader.ReadToEnd());
                }
            }
        }
    }
}
