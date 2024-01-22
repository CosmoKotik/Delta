using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Core.Config
{
    public class Configs
    {
        public string DeltaVersion;
        public string ConfigVersion;
        public string Motd;
        public string Bind;

        public int MaxPlayers;
        public int CompressionThreshold;
        public int ConnectionTimeout;
        public int ReadTimeout;
        public int GCMemoryActivationThreshold;

        public bool OnlineMode;
        public bool CheckForUpdates;
        public bool AllowManualGC;
    }
}
