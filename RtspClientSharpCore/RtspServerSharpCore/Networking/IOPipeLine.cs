using RtspClientSharpCore.Rtsp;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace RtspServerSharpCore.Networking
{
    class IOPipeLine : IDisposable
    {
        private SemaphoreSlim _writerSignal = new SemaphoreSlim(0);

        private Socket _socket;
        private ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        private readonly int _resumeWriterThreshole;
        private ConcurrentQueue<WriteState> _writerQueue = new ConcurrentQueue<WriteState>();

        private readonly RtspStreamContext _rtspStreamContext;

        public IOPipeLine(Socket socket, int resumeWriterThreshole = 65535)
        {
            _socket = socket;
            _resumeWriterThreshole = resumeWriterThreshole;

            _rtspStreamContext = new RtspStreamContext(this);
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            var d = PipeOptions.Default;
            var opt = new PipeOptions(
                d.Pool,
                d.ReaderScheduler,
                d.WriterScheduler,
                _resumeWriterThreshole,
                d.ResumeWriterThreshold,
                d.MinimumSegmentSize,
                d.UseSynchronizationContext);
            var pipe = new Pipe(opt);
            var t1 = Producer(_socket, pipe.Writer, ct);
            var t2 = Consumer(pipe.Reader, ct);
            var t3 = Writer(ct);
            ct.Register(() =>
            {
                _rtspStreamContext?.Dispose();
            });

            var tcs = new TaskCompletionSource<int>();
            Action<Task> setException = t =>
            {
                tcs.TrySetException(t.Exception.InnerException);
            };
            Action<Task> setCanceled = _ =>
            {
                tcs.TrySetCanceled();
            };
            Action<Task> setResult = t =>
            {
                tcs.TrySetResult(1);
            };


            t1.ContinueWith(setException, TaskContinuationOptions.OnlyOnFaulted);
            t2.ContinueWith(setException, TaskContinuationOptions.OnlyOnFaulted);
            t3.ContinueWith(setException, TaskContinuationOptions.OnlyOnFaulted);
            t1.ContinueWith(setCanceled, TaskContinuationOptions.OnlyOnCanceled);
            t2.ContinueWith(setCanceled, TaskContinuationOptions.OnlyOnCanceled);
            t3.ContinueWith(setCanceled, TaskContinuationOptions.OnlyOnCanceled);
            t1.ContinueWith(setResult, TaskContinuationOptions.OnlyOnRanToCompletion);
            t2.ContinueWith(setResult, TaskContinuationOptions.OnlyOnRanToCompletion);
            t3.ContinueWith(setResult, TaskContinuationOptions.OnlyOnRanToCompletion);
            return tcs.Task;
        }

        #region Sender

        internal async Task SendRawData(ReadOnlyMemory<byte> data)
        {
            var tcs = new TaskCompletionSource<int>();
            var buffer = _arrayPool.Rent(data.Length);
            data.CopyTo(buffer);

            _writerQueue.Enqueue(new WriteState()
            {
                Buffer = buffer,
                TaskSource = tcs,
                Length = data.Length
            });
            _writerSignal.Release();
            await tcs.Task;
        }

        private async Task Writer(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && !disposedValue)
            {
                await _writerSignal.WaitAsync(ct);
                if (_writerQueue.TryDequeue(out var data))
                {
                    Debug.Assert(data != null);
                    Debug.Assert(_socket != null);
                    Debug.Assert((data.Buffer[0] & 0x3F) < 10);
                    await _socket.SendAsync(data.Buffer.AsMemory(0, data.Length), SocketFlags.None, ct);
                    _arrayPool.Return(data.Buffer);
                    data.TaskSource?.SetResult(1);
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }
        #endregion

        #region Receiver
        private async Task Producer(Socket s, PipeWriter writer, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested && !disposedValue)
            {
                // TODO
                var memory = writer.GetMemory(1536);
                var bytesRead = await s.ReceiveAsync(memory, SocketFlags.None);
                if (bytesRead == 0)
                {
                    break;
                }
                writer.Advance(bytesRead);
                var result = await writer.FlushAsync(ct);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }

            writer.Complete();
        }

        private async Task Consumer(PipeReader reader, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested && !disposedValue)
            {
                var result = await reader.ReadAsync(ct);
                await _rtspStreamContext.ProcessRequest(result.Buffer);
                reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }

        internal void Disconnect()
        {
            _socket.Close();
            Dispose();
        }
        #endregion



        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _rtspStreamContext?.Dispose();
                    _socket.Dispose();
                    _writerSignal.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
