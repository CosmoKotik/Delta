using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network.Packets.Login
{
    internal class L_LoginStartPacket
    {
        public string Name;
        public bool HasUUID;
        public string UUID;

        private ClientHandler _clientHandler;

        public L_LoginStartPacket(ClientHandler nm)
        {
            _clientHandler = nm;
        }

        public object Read(byte[] bytes)
        {
            BufferManager bm = new BufferManager();
            bm.SetBytes(bytes);

            this.Name = bm.GetString();

            return this;
        }
    }
}
