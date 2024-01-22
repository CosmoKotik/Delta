using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tommy;

namespace Delta.Core.Config
{
    internal static class ConfigManager
    {
        public static Dictionary<string, object> Configs;

        public static Configs Load(ProxyManager pm)
        {
            string path = @"config.toml";

            if (!File.Exists(path)) 
            {
                TomlTable table = new TomlTable()
                {
                    ["Info"] =
                    {
                        ["Version"] = "1.0",
                        ["CheckForUpdates"] = pm.CheckForUpdates,
                    },
                    ["Main"] =
                    {
                        ["MaxPlayers"] = pm.MaxPlayers,
                        ["Motd"] = pm.Motd,
                        ["Bind"] = pm.Bind,
                        ["OnlineMode"] = pm.OnlineMode,
                    },
                    ["Connection"] =
                    {
                        ["CompressionThreshold"] = pm.CompressionThreshold,
                        ["ConnectionTimeout"] = pm.ConnectionTimeout,
                        ["ReadTimeout"] = pm.ReadTimeout,
                    },
                    ["GC"] =
                    {
                        ["AllowManualGC"] = pm.AllowManualGC,
                        ["GCMemoryActivationThreshold"] = pm.GCMemoryActivationThreshold,
                    }
                };

                using (StreamWriter writer = File.CreateText(path))
                {
                    table.WriteTo(writer);
                    // Remember to flush the data if needed!
                    writer.Flush();
                }

                return null;
            }

            using (StreamReader reader = File.OpenText(path))
            {
                var model = TOML.Parse(reader);

                Configs config = new Configs()
                {
                    ConfigVersion = (string)model["Info"]["Version"],
                    MaxPlayers = (int)model["Main"]["MaxPlayers"],
                    CompressionThreshold = (int)model["Connection"]["CompressionThreshold"],
                    ConnectionTimeout = (int)model["Connection"]["ConnectionTimeout"],
                    ReadTimeout = (int)model["Connection"]["ReadTimeout"],
                    GCMemoryActivationThreshold = (int)model["GC"]["GCMemoryActivationThreshold"],
                    Motd = (string)model["Main"]["Motd"],
                    Bind = (string)model["Main"]["Bind"],
                    OnlineMode = (bool)model["Main"]["OnlineMode"],
                    CheckForUpdates = (bool)model["Info"]["CheckForUpdates"],
                    AllowManualGC = (bool)model["GC"]["AllowManualGC"],
                };

                return config;
            }
        }
    }
}
