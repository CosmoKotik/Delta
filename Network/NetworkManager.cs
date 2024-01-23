using Delta.Core;
using Delta.Enums;
using Delta.Network.Packets.Handshake;
using Delta.Network.Packets.Login;
using Delta.Network.Packets.Status;
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
        private CancellationTokenSource _listenerCancellationTokenSource = new CancellationTokenSource();

        private bool _isAvailable = true;

        private ProxyManager _proxyManager;

        private States _currentState = States.Handshake;

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
                            var sock = await listener.AcceptAsync(token);
                            //new Thread(() => { HandleClient(sock); }).Start();
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            /*await Task.Run(() =>
                            {
                                HandleClient(sock, cancellationTokenSource);
                            }, cancellationTokenSource.Token);
*/
                            Logger.Log($"{sock.RemoteEndPoint} is pinging server");

                            await Task.Factory.StartNew(() =>
                            {
                                new ClientHandler(_proxyManager, this).HandleClient(sock, cancellationTokenSource).GetAwaiter();
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
    }
}
