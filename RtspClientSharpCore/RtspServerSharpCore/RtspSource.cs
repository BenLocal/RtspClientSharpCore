using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtspServerSharpCore
{
    public class RtspSource
    {
        public string AppName { get; private set; }

        public string StreamName { get; private set; }

        /// <summary>
        /// 拉流地址
        /// </summary>
        public string Url { get; private set; }

        public bool IsReady { get; private set; }


        private readonly RtspClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public RtspSource(string appName, string name, string url)
        {
            AppName = appName;
            StreamName = name;
            Url = url;

            var connectionParameters = new ConnectionParameters(new Uri(url));
            _client = new RtspClient(connectionParameters);

            _client.FrameReceived += OnFrameReceived;
        }

        public async Task<bool> StartAsync()
        {
            await _client.ConnectAsync(_cancellationTokenSource.Token);
            await _client.ReceiveAsync(_cancellationTokenSource.Token);
            IsReady = true;
            // TODO
            return true;
        }

        //public Task<bool> StopAsync()
        //{
        //    _client.Dispose();
        //}

        private void OnFrameReceived(object state, RawFrame frame)
        { 
            
        }
    }
}
