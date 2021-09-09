using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RtspServerSharpCore.Networking;
using System.Collections.Concurrent;

namespace RtspServerSharpCore
{
    public class RtspServer
    {
        private readonly Socket _listener;
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
       

        public bool Started { get; private set; } = false;

        internal RtspServer(IPEndPoint endPoint)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.NoDelay = true;
            _listener.Bind(endPoint);
            _listener.Listen(128);
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            if (Started)
            {
                throw new InvalidOperationException("already started");
            }

            Started = true;
            var ret = new TaskCompletionSource<int>();
            var t = new Thread(o =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            _allDone.Reset();
                            _listener.BeginAccept(new AsyncCallback(ar =>
                            {
                                AcceptCallback(ar, ct);
                            }), _listener);
                            while (!_allDone.WaitOne(1))
                            {
                                ct.ThrowIfCancellationRequested();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    ret.SetResult(1);
                }
            });

            t.Start();
            return ret.Task;
        }

        private async void AcceptCallback(IAsyncResult ar, CancellationToken ct)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket client = listener.EndAccept(ar);
            client.NoDelay = true;
            // Signal the main thread to continue.
            _allDone.Set();
            IOPipeLine pipe = null;
            try
            {
                pipe = new IOPipeLine(client);
                RtspManager.Current.TryAddSession(pipe.CurrentSession);
                await pipe.StartAsync(ct);
            }
            catch (TimeoutException)
            {
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Message: {1}", e.GetType().ToString(), e.Message);
                Console.WriteLine(e.StackTrace);
                client.Close();
            }
            finally
            {
                RtspManager.Current.TryRemoveSession(pipe?.CurrentSession);
                pipe?.Dispose();
            }
        }
    }
}
