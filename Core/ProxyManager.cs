using Delta.Core.Config;
using Delta.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Delta.Core
{
    internal class ProxyManager
    {
        #region Editable configs
        public string DeltaVersion { get { return _deltaVersion; } }
        private string _deltaVersion = "Delta 1.0.0";

        public string ConfigVersion { get { return _configVersion; } }
        private string _configVersion = "1.0";

        public int MaxPlayers { get { return _maxPlayers; } }
        private int _maxPlayers = 100;

        public int CompressionThreshold { get { return _compressionThreshold; } }
        private int _compressionThreshold = 256;

        public int ConnectionTimeout { get { return _connectionTimeout; } }
        private int _connectionTimeout = 10000;

        public int ReadTimeout { get { return _readTimeout; } }
        private int _readTimeout = 30000;

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

        public bool AllowProxy { get { return _allowProxy; } }
        private bool _allowProxy = true;
        public string DeltaAddress { get { return _deltaAddress; } }
        private string _deltaAddress = "default";
        
        public bool EnforceSecureChat { get { return _enforceSecureChat; } }
        private bool _enforceSecureChat = true;

        /*public int CompressionLevel { get { return _compressionLevel; } }
        private int _compressionLevel = -1;*/
        #endregion

        #region Server configs
        public int CurrentOnline { get { return _currentOnline; } }
        private int _currentOnline = 0;
        public string FaviconBase64 { get { return _faviconBase64; } }
        private string _faviconBase64 = "";
        #endregion

        public void Start()
        {
            Logger.Log("Loading config...");
            try
            {
                Configs conf = ConfigManager.Load(this);
                if (conf != null)
                {
                    _configVersion = conf.ConfigVersion;
                    _maxPlayers = conf.MaxPlayers;
                    _compressionThreshold = conf.CompressionThreshold;
                    _connectionTimeout = conf.ConnectionTimeout;
                    _readTimeout = conf.ReadTimeout;
                    _motd = conf.Motd;
                    _bind = conf.Bind;
                    _onlineMode = conf.OnlineMode;
                    _checkForUpdates = conf.CheckForUpdates;
                    _allowManualGC = conf.AllowManualGC;
                    _GCMemoryActivationThreshold = conf.GCMemoryActivationThreshold;
                    _allowProxy = conf.AllowProxy;
                    _enforceSecureChat = conf.EnforceSecureChat;

                    if (conf.DeltaAddress.Equals("default"))
                        _deltaAddress = _bind;

                    Logger.Log("Config loaded successfully.");
                }
                else
                    Logger.Log("Config generated successfully.");
            }
            catch (Exception e) { Logger.Error($"Failed to load config: {e}"); return; }

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

            if (File.Exists("favicon.png"))
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes("favicon.png");
                _faviconBase64 = Convert.ToBase64String(imageBytes);
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

        public void UpdateCurrentOnline(int value)
        {
            _currentOnline = value;
        }
    }
}
