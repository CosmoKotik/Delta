using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network.Packets.Handshake
{
    internal class HandshakePacket
    {
        public int ProtocolVersion;
        public string ServerAddress;
        public short ServerPort;
        public int NextState;

        private ClientHandler _clientHandler;

        public HandshakePacket(ClientHandler nm)
        {
            _clientHandler = nm;
        }

        public object Read(byte[] bytes)
        {
            BufferManager bm = new BufferManager();
            bm.SetBytes(bytes);

            this.ProtocolVersion = bm.ReadVarInt();
            this.ServerAddress = bm.GetString();
            this.ServerPort = bm.GetShort();
            this.NextState = bm.ReadVarInt();

            return this;
        }
    }
}
