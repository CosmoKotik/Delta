using Delta.Core;
using Delta.Enums;
using Delta.Network.Packets.Handshake;
using Delta.Network.Packets.Login;
using Delta.Network.Packets.Status;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network
{
    internal class ClientHandler
    {
        public ProxyManager ProxyManager;
        public NetworkManager NetworkManager;
        public int ProtocolVersion { get; set; }

        public int BufferSize { get; set; } = 1048576;
        private Socket _server;
        private Socket _client;
      
        private bool _isAvailable = true;

        private States _currentState = States.Handshake;

        public ClientHandler(ProxyManager pm, NetworkManager nm)
        { 
            this.ProxyManager = pm;
            this.NetworkManager = nm;
            Test();
        }

        public void Test()
        {
            byte[] bytes = new byte[] { 2, 0, 0 };
            ParseAllBytePacket(bytes);
        }

        public async Task HandleClient(Socket sock, CancellationTokenSource cancellationTokenSource)
        {
            using (Socket sender = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                _server = sender;

                sender.NoDelay = true;
                sender.ReceiveTimeout = 1000;
                sender.SendTimeout = 1000;
                using (sock)
                {
                    _client = sock;

                    sock.ReceiveTimeout = 1000;
                    sock.SendTimeout = 1000;
                    sock.NoDelay = true;

                    BufferManager bm = new BufferManager();

                    var buffer = new byte[BufferSize];
                    byte[] bytes = new byte[1024];

                    bool isCancelling = false;

                    long lastPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    bool isStatus = true;
                    while (isStatus)
                    {
                        using (var receivedTask = sock.ReceiveAsync(bytes, SocketFlags.None))
                        {
                            if (receivedTask.Result > 20)
                            {
                                isStatus = false;
                                break;
                            }

                            byte[] correcetdBytes = new byte[receivedTask.Result];
                            Array.Copy(bytes, correcetdBytes, receivedTask.Result);
                            //Console.WriteLine("zalupa: {0}", BitConverter.ToString(correcetdBytes).Replace("-", " ") + "   " + correcetdBytes.Length);
                            //bm.SetBytes(correcetdBytes);
                            //int packetSize = bm.GetPacketSize() - 1;
                            //int packetID = bm.GetPacketId();

                            //buffer = new byte[packetSize];
                            //Array.Copy(bm.GetBytes(), buffer, packetSize);

                            //isStatus = HandleHandshake(buffer, correcetdBytes, packetID);
                            ParseAllBytePacket(correcetdBytes, false);

                            //if (bytes.Length > packetSize)
                            //{
                            //    try
                            //    {
                            //        bm.RemoveRangeByte(packetSize);
                            //        packetSize = bm.GetPacketSize() - 1;
                            //        packetID = bm.GetPacketId();

                            //        buffer = new byte[packetSize];
                            //        Array.Copy(bm.GetBytes(), buffer, packetSize);
                            //        //Console.WriteLine("Received: {0}", BitConverter.ToString(buffer).Replace("-", " ") + "   " + buffer.Length);
                            //        isStatus = HandleHandshake(buffer, correcetdBytes, packetID);
                            //    }
                            //    catch { }
                            //}
                        }

                        await Task.Delay(1);
                    }

                    try
                    {
                        await sender.ConnectAsync("10.0.0.17", 43801);
                    }
                    catch
                    {
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        sender.Dispose();

                        sock.Shutdown(SocketShutdown.Both);
                        sock.Close();
                        sock.Dispose();
                    }

                    while (!sender.Connected)
                        Task.Delay(1).Wait();

                    //Send the previous packet
                    await sender.SendAsync(bytes, SocketFlags.None);

                    while (sock.Connected && sender.Connected && _isAvailable && !isCancelling)
                    {
                        if (sock.Available > 0)
                        {
                            lastPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            //var received = await sock.ReceiveAsync(buffer, SocketFlags.None);
                            using (var receivedTask = sock.ReceiveAsync(buffer, SocketFlags.None))
                            {
                                int received = receivedTask.Result;

                                bytes = new byte[received];
                                Array.Copy(buffer, bytes, received);

                                //HandleClientBytes(bytes);
                                ParseAllBytePacket(bytes, false);
                                //await SendToClient(buffer);
                                using (CancellationTokenSource source = new CancellationTokenSource())
                                {
                                    CancellationToken token = source.Token;

                                    /*try
                                    {
                                        int BytesSent = await sender.SendAsync(bytes, SocketFlags.None, token);
                                    }
                                    catch (SocketException e) { }
                                    finally { source.Cancel(); }*/
                                    int BytesSent = await sender.SendAsync(bytes, SocketFlags.None, token);
                                }
                            }

                        }

                        if (sender.Available > 0)
                        {
                            lastPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            using (var receivedTask = sender.ReceiveAsync(buffer, SocketFlags.None))
                            {
                                int received = receivedTask.Result;

                                bytes = new byte[received];
                                Array.Copy(buffer, bytes, received);

                                //await SendToServer(buffer);
                                ParseAllBytePacket(bytes, true);
                                using (CancellationTokenSource source = new CancellationTokenSource())
                                {
                                    CancellationToken token = source.Token;

                                    /*try
                                    {
                                        int BytesSent = await sock.SendAsync(bytes, SocketFlags.None, token);
                                    }
                                    catch (SocketException e) { }
                                    finally { source.Cancel(); }*/
                                    int BytesSent = await sock.SendAsync(bytes, SocketFlags.None, token);
                                }

                                //HandleServerBytes(bytes);
                            }

                        }

                        if (lastPacketTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - ProxyManager.ReadTimeout)
                            isCancelling = true;

                        await Task.Delay(1);
                    }

                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                    sock.Dispose();
                }

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                sender.Dispose();
            }

            //Thread.CurrentThread.Join();

            cancellationTokenSource.Cancel();

            /*GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();*/
        }

        private BufferManager _bufferManager = new BufferManager();
        private bool HandleHandshake(byte[] bytes, byte[] buffer, int packetID)
        {
            //Returns cancellation state

            switch (packetID)
            {
                case 0x00:
                    if (_currentState.Equals(States.Handshake))
                    {
                        HandshakePacket handshake = (HandshakePacket)new HandshakePacket(this).Read(bytes);

                        this.ProtocolVersion = handshake.ProtocolVersion;

                        if (!ProxyManager.AllowProxy && !handshake.ServerAddress.Equals(ProxyManager.DeltaAddress))
                            new L_DisconnectPacket(this).Write("§cProxy is not allowed");

                        _currentState = (States)handshake.NextState;
                        if (handshake.NextState != (int)States.Status)
                            return false;

                        return true;
                    }
                    else if (_currentState.Equals(States.Status))
                    {
                        new S_StatusResponsePacket(this).Write();
                        return true;
                    }
                    break;
                case 0x01:
                    if (_currentState.Equals(States.Status))
                    {
                        new S_PingPacket(this).Handle(buffer);
                        return true;
                    }
                    break;
                default:
                    return false;
            }
           
            return false;
        }


        private void ParseAllBytePacket(byte[] bytes, bool isServer = false)
        {
            BufferManager bm = new BufferManager();
            bm.SetBytes(bytes);

            int packetSizeLength = bm.GetVarIntOffset();
            int packetLength = bm.GetPacketSize() - 1;
            int packetId = bm.GetPacketId();
            int totalOffsetLength = packetLength + packetSizeLength + 1;

            byte[] bytesSeciton = new byte[packetLength];
            Array.Copy(bytes, bytesSeciton, packetLength);
            while (totalOffsetLength <= bytes.Length)
            {
                if (isServer)
                    HandleServerBytes(bytesSeciton, packetId);
                else
                    HandleClientBytes(bytesSeciton, packetId);

                if (totalOffsetLength != bytes.Length)
                    bm.RemoveRangeByte(packetLength);

                packetSizeLength = bm.GetVarIntOffset();
                packetLength = bm.GetPacketSize() - 1;
                packetId = bm.GetPacketId();

                bytesSeciton = new byte[packetLength];
                Array.Copy(bytes, bytesSeciton, packetLength);

                totalOffsetLength += packetLength + packetSizeLength + 1;
            }
        }

        
        private void HandleServerBytes(byte[] buffer, int packetId)
        {
            BufferManager bm = new BufferManager();

            bm.SetBytes(buffer);
            /*int packetSize = buffer.Length - (bm.GetVarIntOffset());
            int packetID = bm.GetPacketId(bm.GetVarIntOffset());*/
            int packetSize = bm.GetPacketSize() - 1;
            int packetID = bm.GetPacketId();

            byte[] bytes = bm.GetBytes();

            switch (_currentState)
            { 
                case States.Handshake:
                    switch (packetID)
                    {
                        case 0x02:
                            _currentState = States.Play;
                            break;
                    }
                    break;
            }
        }

        private void HandleClientBytes(byte[] buffer, int packetId)
        {
            BufferManager bm = _bufferManager;

            bm.SetBytes(buffer);
            /*int packetSize = buffer.Length - (bm.GetVarIntOffset());
            int packetID = bm.GetPacketId(bm.GetVarIntOffset());*/
            int packetSize = bm.GetPacketSize() - 1;
            int packetID = bm.GetPacketId();

            byte[] bytes = bm.GetBytes();

            switch (_currentState)
            {
                case States.Handshake:
                    switch (packetID)
                    {
                        case 0x00:
                            _currentState = States.Login;
                            break;
                    }
                    break;
                case States.Login:
                    switch (packetID)
                    {
                        case 0x00:
                            L_LoginStartPacket loginPacket = (L_LoginStartPacket)new L_LoginStartPacket(this).Read(bytes);

                            Logger.Log($"{loginPacket.Name} is joining the game.");
                            break;
                    }
                    break;
            }
        }

        public void SendToClient(byte[] bytes, bool includeSize = true)
        {
            BufferManager bm = new BufferManager();
            if (includeSize)
                bm.AddVarInt(bytes.Length);
            bm.InsertBytes(bytes);

            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                CancellationToken token = source.Token;

                _client.SendAsync(bm.GetBytes(), SocketFlags.None, token).ConfigureAwait(false).GetAwaiter().GetResult();
                //Console.WriteLine("sended: {0}", BitConverter.ToString(bm.GetBytes()).Replace("-", " ") + "   " + bm.GetBytes().Length);
            }
        }

        public void SendToServer(byte[] bytes)
        {
            BufferManager bm = new BufferManager();
            bm.AddVarInt(bytes.Length);
            bm.InsertBytes(bytes);

            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                CancellationToken token = source.Token;

                _server.SendAsync(bm.GetBytes(), SocketFlags.None, token).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
