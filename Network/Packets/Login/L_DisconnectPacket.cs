using Delta.Tools.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network.Packets.Login
{
    internal class L_DisconnectPacket
    {
        private ClientHandler _clientHandler;

        public L_DisconnectPacket(ClientHandler nm)
        {
            _clientHandler = nm;
        }

        public void Write(string Reason)
        {
            BufferManager bm = new BufferManager();
            bm.SetPacketId(0x00);
            bm.AddString(ChatComponent.Build(new Chat(Reason)));

            _clientHandler.SendToClient(bm.GetBytes());
        }
    }
}
