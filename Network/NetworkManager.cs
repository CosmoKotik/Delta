using Delta.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Network
{
    internal class NetworkManager
    {
        public int BufferSize { get; set; } = 1048576;

        private IPEndPoint _bind;

        private Socket _listener;
        private Socket _server;
        private Socket _client;
        private CancellationTokenSource _listenerCancellationTokenSource = new CancellationTokenSource();

        private bool _isAvailable = true;

        private ProxyManager _proxyManager;

        public NetworkManager(ProxyManager pm)
        {
            _proxyManager = pm;
        }

        public void Init(string bind)
        {
            string[] bindSplit = bind.Split(":");
            _bind = new IPEndPoint(IPAddress.Parse(bindSplit[0]), int.Parse(bindSplit[1]));

            Logger.Log($"Server binded on {bind}");

            new Thread(() => { Listen(); }).Start();
        }

        private async void Listen()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            using (Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                _listener = listener;
                _listenerCancellationTokenSource = new CancellationTokenSource();
                try
                {
                    listener.Bind(_bind);
                    listener.Listen(17493);

                    //ListenerStatus(0, new EventArgs());

                    CancellationToken token = _listenerCancellationTokenSource.Token;
                    while (_isAvailable)
                    {
                        try
                        {
                            Logger.Log("dasdasd");

                            var sock = await listener.AcceptAsync(token);
                            //new Thread(() => { HandleClient(sock); }).Start();
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            /*await Task.Run(() =>
                            {
                                HandleClient(sock, cancellationTokenSource);
                            }, cancellationTokenSource.Token);
*/
                            await Task.Factory.StartNew(() =>
                            {
                                HandleClient(sock, cancellationTokenSource).GetAwaiter();
                            }, cancellationTokenSource.Token).ContinueWith(task =>
                            {
                                if (!task.IsCompleted || task.IsFaulted)
                                {
                                    Logger.Error("Task couldn't stop, resulting increased memory.");
                                    cancellationTokenSource.Cancel();
                                }
                            }, cancellationTokenSource.Token);
                        }
                        catch { }
                    }

                    //ListenerStatus(2, new EventArgs());

                    //listener.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e) { }
                finally
                {
                    listener.Close();
                    listener.Dispose();
                }
            }

            Thread.CurrentThread.Join();
        }

        private async Task HandleClient(Socket sock, CancellationTokenSource cancellationTokenSource)
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
                    //await sender.ConnectAsync("mc.hypixel.net", 25565);

                    while (!sender.Connected)
                        Task.Delay(1).Wait();

                    BufferManager bm = new BufferManager();

                    var buffer = new byte[BufferSize];
                    byte[] bytes = new byte[1024];

                    bool isCancelling = false;

                    long lastPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

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

                                bm.SetBytes(bytes);
                                int packetSize = bytes.Length - (bm.GetVarIntOffset());
                                int packetID = bm.GetPacketId(bm.GetVarIntOffset());

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

                                bm.SetBytes(bytes);
                                int packetSize = bytes.Length - (bm.GetVarIntOffset());
                                int packetID = bm.GetPacketId(bm.GetVarIntOffset());

                                //await SendToServer(buffer);
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
                            }

                        }

                        if (lastPacketTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - _proxyManager.ReadTimeout)
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
    }
}
