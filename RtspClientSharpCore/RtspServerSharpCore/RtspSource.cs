using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;
using RtspClientSharpCore.Sdp;
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

        public IEnumerable<RtspTrackInfo> TrackInfos { get; private set; }

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
            _client.RtspTrackReceived += OnRtspTrackReceived;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            try
            {
                await _client.ConnectAsync(_cancellationTokenSource.Token);
                await _client.ReceiveAsync(_cancellationTokenSource.Token);
                IsReady = true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (RtspClientSharpCore.Rtsp.RtspClientException)
            {
                IsReady = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReStartAsync()
        {
            if (IsReady)
            {
                return true;
            }

            return await StartAsync();
        }

        public Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            _client.FrameReceived -= OnFrameReceived;
            _client.RtspTrackReceived -= OnRtspTrackReceived;
            _client.Dispose();
            return Task.CompletedTask;
        }

        private void OnFrameReceived(object state, RawFrame frame)
        { 
            
        }

        private void OnRtspTrackReceived(object state, IEnumerable<RtspTrackInfo> tracks)
        {
            TrackInfos = tracks;
        }
    }
}
