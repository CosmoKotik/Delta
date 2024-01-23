using Delta.Tools.Chat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Delta.Network.Packets.Status
{
    internal class S_StatusResponsePacket
    {
        private ClientHandler _clientHandler;

        public S_StatusResponsePacket(ClientHandler nm)
        {
            _clientHandler = nm;
        }

        public void Write()
        {
            BufferManager bm = new BufferManager();
            bm.SetPacketId(0x00);

            StatusResponse sr = new StatusResponse()
            {
                version = new StatusResponse.Version()
                {
                    name = "DeltaProxy",
                    protocol = _clientHandler.ProtocolVersion
                },
                players = new StatusResponse.Players() 
                {
                    max = _clientHandler.ProxyManager.MaxPlayers,
                    online = _clientHandler.ProxyManager.CurrentOnline,
                    sample = new StatusResponse.Sample[1] { new StatusResponse.Sample() { name = "Server is proxied by Delta", id = new Guid().ToString() } }
                },
                description = new Chat(_clientHandler.ProxyManager.Motd),
                favicon = _clientHandler.ProxyManager.FaviconBase64,
                enforcesSecureChat = _clientHandler.ProxyManager.EnforceSecureChat,
                previewsChat = false
            };

            bm.AddString(JsonConvert.SerializeObject(sr));

            _clientHandler.SendToClient(bm.GetBytes());
        }
    }

    public class StatusResponse
    {
        public class Version 
        {
            public string name { get; set; }
            public int protocol { get; set; }
        }
        public Version version { get; set; }
        public class Sample
        { 
            public string name { get; set; }
            public string id { get; set; }
        }
        public class Players
        {
            public int max { get; set; }
            public int online { get; set; }
            public Sample[] sample { get; set; }
        }
        public Players players { get; set; }
        public Chat description { get; set; }
        public string favicon { get; set; }
        public bool enforcesSecureChat { get; set; }
        public bool previewsChat { get; set; }
    }
}
