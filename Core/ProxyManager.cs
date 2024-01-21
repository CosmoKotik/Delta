using Delta.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Core
{
    internal class ProxyManager
    {
        public string DeltaVersion { get { return _deltaVersion; } }
        private string _deltaVersion = "Delta 1.0.0";

        public int MaxPlayers { get { return _maxPlayers; } }
        private int _maxPlayers = 100;

        public int CompressionThreshold { get { return _compressionThreshold; } }
        private int _compressionThreshold = 256;

        public int ConnectionTimeout { get { return _connectionTimeout; } }
        private int _connectionTimeout = 10000;

        public int ReadTimeout { get { return _readTimeout; } }
        private int _readTimeout = 3000;

        public string Motd { get { return _motd; } }
        private string _motd = "Delta proxy";

        public string Bind { get { return _bind; } }
        private string _bind = "127.0.0.1:56505";

        public bool OnlineMode { get { return _onlineMode; } }
        private bool _onlineMode = true;

        public bool CheckForUpdates { get { return _checkForUpdates; } }
        private bool _checkForUpdates = true;

        public bool AllowManualGC { get { return _allowManualGC; } }
        private bool _allowManualGC = true;

        public int GCMemoryActivationThreshold { get { return _GCMemoryActivationThreshold; } }
        private int _GCMemoryActivationThreshold = 100;

        /*public int CompressionLevel { get { return _compressionLevel; } }
        private int _compressionLevel = -1;*/

        public void Start()
        {
            if (this.CheckForUpdates)
            {
                //Check for update code
                Logger.Log("Checking for updates...");
                Logger.Log("No updates");   //temporary lol
            }

            if (AllowManualGC)
            {
                new Thread(() => ManualGCCleaner()).Start();
            }

            Logger.Log($"Starting delta version: {DeltaVersion}");

            NetworkManager nm = new NetworkManager(this);
            nm.Init(Bind);
        }

        private void ManualGCCleaner()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                long memory = GC.GetTotalMemory(false);
                double memoryMB = memory / 1000000;

                if (memoryMB > _GCMemoryActivationThreshold)
                {
                    Logger.Warn("Running manual garbage collector.");

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                Thread.Sleep(5000);
            }
        }
    }
}
