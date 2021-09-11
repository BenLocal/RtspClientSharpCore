using RtspClientSharpCore.Rtsp;
using RtspClientSharpCore.Sdp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace RtspServerSharpCore.Networking
{
    class RtspStreamContext : IDisposable
    {
        private IOPipeLine _ioPipeline = null;
        private RtspResponseMessageFactory rtspResponseMessageFactory;
        internal RtspClientSession _clientSession = null;

        private string _appName;
        private string _streamName;

        public RtspStreamContext(IOPipeLine stream)
        {
            _ioPipeline = stream;

            rtspResponseMessageFactory = new RtspResponseMessageFactory();

            _clientSession = new RtspClientSession(stream);
        }

        public async Task ProcessRequest(ReadOnlySequence<byte> buffer)
        {
            var request = RtspRequestMessage.Parse(buffer);
            RtspResponseMessage response = null;

            switch (request.Method)
            {
                case RtspMethod.OPTIONS:
                    response = rtspResponseMessageFactory.CreateOptionsResponse(request);
                    break;
                case RtspMethod.DESCRIBE:
                    response = HandleDescribe(request);
                    break;
                case RtspMethod.SETUP:
                    response = HandleSetup(request);
                    break;

            }

            if (response != null)
            {
                byte[] data = ASCIIEncoding.UTF8.GetBytes(response.ToString());
                await _ioPipeline.SendRawData(data);
            }
        }

        private RtspResponseMessage HandleDescribe(RtspRequestMessage request)
        {
            var (_appName, _streamName) = ParseUrlPath(request.ConnectionUri.AbsolutePath);
            var source = RtspManager.Current.FindSourceByAppNameAndStream(_appName, _streamName);
            if (source == null)
            {
                // TODO return error
                return rtspResponseMessageFactory.CreateInvalidResponse(request, RtspStatusCode.NotFound);
            }

            if (AuthenticateRequest(request).Equals(false))
            {
                // TODO
                return rtspResponseMessageFactory.CreateInvalidResponse(request, RtspStatusCode.ConnectionAuthorizationRequired);
            }

            if (!source.IsReady)
            {
                // TODO return error
                return rtspResponseMessageFactory.CreateInvalidResponse(request, RtspStatusCode.NotFound);
            }

            // set header
            var header = new NameValueCollection();
            // 
            header.Set(HttpHeaders.ContentType, "application/sdp");
            //Don't cache this SDP
            header.Set(HttpHeaders.CacheControl, "no-cache");
            header.Set(HttpHeaders.ContentBase, request.ConnectionUri.ToString());

            // body TODO
            StringBuilder sdp = new StringBuilder();
            sdp.Append("v=0\n");
            sdp.Append("o=user 123 0 IN IP4 0.0.0.0\n");
            sdp.Append($"s={_streamName}\n");

            foreach (var track in source.TrackInfos)
            {
                if (track is RtspMediaTrackInfo info && track.SdpLines.Any())
                {
                    track.SdpLines.ForEach(x => sdp.Append(x));
                }
            }

            byte[] sdp_bytes = Encoding.ASCII.GetBytes(sdp.ToString());

            header.Set(HttpHeaders.ContentLength, sdp_bytes.Length.ToString());
            return rtspResponseMessageFactory.CreateDescribeResponse(request, header, sdp_bytes);
        }

        private RtspResponseMessage HandleSetup(RtspRequestMessage request)
        {
            var transport = request.Headers.Get("Transport");
            if (transport == null)
            {
                return rtspResponseMessageFactory.CreateInvalidResponse(request, RtspStatusCode.UnsupportedTransport);
            }
            RtspTransport firstTransport = GetTransports(transport)[0];

            RtspTransport transport_reply = new RtspTransport();
            // Convert to Hex, padded to 8 characters
            transport_reply.SSrc = 0x4321FADE.ToString("X8");
            if (firstTransport.LowerTransport == RtspTransport.LowerTransportType.TCP)
            {
                // RTP over RTSP mode
                transport_reply.LowerTransport = RtspTransport.LowerTransportType.TCP;
                transport_reply.Interleaved = new PortCouple(firstTransport.Interleaved.First, firstTransport.Interleaved.Second);
            }
        }

        private RtspTransport[] GetTransports(string transport)
        {
            string[] items = transport.Split(',');
            return Array.ConvertAll<string, RtspTransport>(items,
                new Converter<string, RtspTransport>(RtspTransport.Parse));
        }

        private bool AuthenticateRequest(RtspRequestMessage request)
        {
            // TODO
            return true;
        }

        private (string, string) ParseUrlPath(string absolutePath)
        {
            var index = absolutePath.LastIndexOf("/");
            if (index == -1)
            {
                return (string.Empty, string.Empty);
            }
            else if (index == 0 && absolutePath == "/")
            {
                return (string.Empty, string.Empty);
            }
            else if (index == 0 && absolutePath != "/")
            {
                return (string.Empty, absolutePath.Substring(1));
            }
            else
            {
                return (absolutePath.Substring(1, absolutePath.Length - index), absolutePath.Substring(index + 1));
            }
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
