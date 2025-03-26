using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace DMAW_DND
{
    public class Map
    {
        /// <summary>
        /// Name of map (Ex: Customs)
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// 'MapConfig' class instance
        /// </summary>
        public readonly MapConfig ConfigFile;
        /// <summary>
        /// File path to Map .JSON Config
        /// </summary>
        public readonly string ConfigFilePath;

        public readonly string[] LevelNames;

        public Map(string name, MapConfig config, string configPath, string[] levelNames)
        {
            Name = name;
            ConfigFile = config;
            ConfigFilePath = configPath;
            LevelNames = levelNames;
        }
    }

    public class MapConfig
    {
        [JsonIgnore]
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        [JsonPropertyName("gameName")]
        public string[] GameName { get; set; }
        //public string GameName { get; set; }
        /// <summary>
        /// Bitmap 'X' Coordinate of map 'Origin Location' (where Unity X is 0).
        /// </summary>
        [JsonPropertyName("x")]
        public float X { get; set; }
        /// <summary>
        /// Bitmap 'Y' Coordinate of map 'Origin Location' (where Unity Y is 0).
        /// </summary>
        [JsonPropertyName("y")]
        public float Y { get; set; }
        /// <summary>
        /// Unused.
        /// </summary>
        [JsonPropertyName("z")]
        public float Z { get; set; }
        /// <summary>
        /// Arbitrary scale value to align map scale between the Bitmap and Game Coordinates.
        /// </summary>
        [JsonPropertyName("scale")]
        public float Scale { get; set; }
        /// <summary>
        /// * This List contains the path to the map file(s), and a minimum height (Z) value.
        /// * Each tuple consists of Item1: (float)MIN_HEIGHT, Item2: (string>MAP_PATH
        /// * This list will be iterated backwards, and if the current player height (Z) is above the float
        /// value, then that map layer will be drawn. This will allow having different bitmaps at different
        /// heights.
        /// * If using only a single map (no layers), set the float value to something very low like -100.
        /// </summary>

        // array with minHeight and Filename properties 
        [JsonPropertyName("mapLayers")]
        public List<MapLayer> mapLayers { get; set; }

        public class MapLayer
        {
            [JsonPropertyName("minHeight")]
            public float minHeight { get; set; }
            [JsonPropertyName("filename")]
            public string filename { get; set; }
        }

        //public List<Tuple<float, string>> mapLayers { get; set; }

        /// <summary>
        /// Loads map.json config file.
        /// </summary>
        /// <param name="file">Map Config .JSON file (ex: customs.json)</param>
        /// <returns></returns>
        public static MapConfig LoadFromFile(string file)
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<MapConfig>(json);
        }

        /// <summary>
        /// Saves map.json config file.
        /// </summary>
        /// <param name="file">Map Config .JSON file (ex: customs.json)</param>
        /// <returns></returns>
        public void Save(Map map)
        {
            var json = JsonSerializer.Serialize<MapConfig>(this, _jsonOptions);
            File.WriteAllText(map.ConfigFilePath, json);
        }
    }

    public struct MapPosition
    {
        public float X = 0;
        public float Y = 0;
        public float Height = 0;

        public MapPosition()
        {
            this.X = 0f;
            this.Y = 0f;
            this.Height = 0f;
        }
    }
}
