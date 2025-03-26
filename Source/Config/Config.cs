using Newtonsoft.Json;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    internal static class Config
    {
        private static Thread _worker;

        private static bool _running = false;

        public static bool Ready { get; private set; } = false;

        public static ConfigStructure ActiveConfig { get; set; } = new ConfigStructure();

        static Config()
        {
            if(!ReadConfig())
            {
                WriteConfig();
            }

            _worker = new Thread(() => Worker())
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _running = true;
            _worker.Start();
        }

        public static bool ReadConfig()
        {
            try{
                ActiveConfig = JsonConvert.DeserializeObject<ConfigStructure>(File.ReadAllText("./config.json"));

                if(ActiveConfig == null)
                {
                    Program.Log("Error reading config file. Generating default config.");
                    return false;
                }
                else
                {
                    Ready = true;
                    return true;
                }
            }
            catch
            {
                Program.Log("Error reading config file. Generating default config.");
                return false;
            }
        }

        public static void WriteConfig()
        {
            // Write config to file "./config.json"
            try{
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(ActiveConfig, Formatting.Indented));
                ActiveConfig = JsonConvert.DeserializeObject<ConfigStructure>(File.ReadAllText("./config.json"));
                Ready = true;
            }
            catch
            {
                Program.Log("Error writing config file.");
            }
        }

        public static void Worker()
        {
            // Writes the config file every 30 seconds
            while(_running){
                Thread.Sleep(30000);
                Program.Log("Writing config file.");
                WriteConfig();
            }
        }
    
        public static bool GetBoolean(string key)
        {
            // return a reference to the boolean value in the config which can be modified
            return (bool)ActiveConfig.GetType().GetProperty(key).GetValue(ActiveConfig);
        }

        public static void SetBoolean(string key, bool value)
        {
            // Set the boolean value in the config
            ActiveConfig.GetType().GetProperty(key).SetValue(ActiveConfig, value);
            Program.Log("Config value " + key + " set to " + value);
        }
        
        public static float GetFloat(string key)
        {
            // return a reference to the float value in the config which can be modified
            return (float)ActiveConfig.GetType().GetProperty(key).GetValue(ActiveConfig);
        }

        public static void SetFloat(string key, float value)
        {
            // Set the float value in the config
            ActiveConfig.GetType().GetProperty(key).SetValue(ActiveConfig, value);
            Program.Log("Config value " + key + " set to " + value);
        }    
    }

    public class ConfigStructure()
    {
        [JsonProperty("ShowPOIs")]
        public bool ShowPOIs { get; set; } = false;

        [JsonProperty("ShowShrines")]
        public bool ShowShrines { get; set; } = false;

        [JsonProperty("ShowPortals")]
        public bool ShowPortals { get; set; } = false;

        [JsonProperty("ShowChests")]
        public bool ShowChests{ get; set; } = false;

        [JsonProperty("ShowMimics")]
        public bool ShowMimics { get; set; } = false;

        [JsonProperty("ShowBosses")]
        public bool ShowBosses { get; set; } = false;

        [JsonProperty("ShowLevers")]
        public bool ShowLevers { get; set; } = false;

        [JsonProperty("Show Ore")]
        public bool ShowOre { get; set; } = false;

        [JsonProperty("ShowSpecialItems")]
        public bool ShowSpecialItems { get; set; } = false;
        
        [JsonProperty("ShowAimlines")]
        public bool ShowAimlines { get; set; } = false;

        [JsonProperty("AimlineLength")]
        public float AimlineLength { get; set; } = 50f;

        [JsonProperty("ShowTeamLines ")]
        public bool ShowTeamLines { get; set; } = true;

        [JsonProperty("ShowHeightIndicators")]
        public bool ShowHeightIndicators { get; set; } = true;

        [JsonProperty("UIScale")]
        public float UIScale { get; set; } = 1.5f;
    }
}