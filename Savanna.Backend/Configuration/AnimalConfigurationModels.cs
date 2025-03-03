﻿namespace Savanna.Backend.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public class AnimalConfiguration
    {
        public int DefaultVisionRange { get; set; } = 4;
        public Dictionary<string, AnimalTypeConfig> Animals { get; set; } = new Dictionary<string, AnimalTypeConfig>();
    }

    public class AnimalTypeConfig
    {
        public int VisionRange { get; set; }
        public int MovementSpeed { get; set; }
        public char Symbol { get; set; }
        public int? DigestionTime { get; set; }
    }
}