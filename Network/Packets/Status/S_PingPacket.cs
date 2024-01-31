using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network.Packets.Status
{
    internal class S_PingPacket
    {
        private ClientHandler _clientHandler;

        public S_PingPacket(ClientHandler nm) 
        {
            _clientHandler = nm;
        }

        public void Handle(byte[] bytes)
        {
            BufferManager bm = new BufferManager();
            bm.SetBytes(bytes);

            bm.SetPacketId(0x00);

            _clientHandler.SendToClient(bytes, false);
        }
    }
}
