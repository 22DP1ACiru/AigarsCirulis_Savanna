using Savanna.Backend.Configuration;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;
using System.Reflection;
using System.Text.Json;

namespace Savanna.Plugins.Zebra
{
    public class ZebraPlugin : IAnimalPlugin
    {
        public string AnimalType => "Zebra";

        public IAnimal CreateAnimal(Position position)
        {
            return new Zebra(position);
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
