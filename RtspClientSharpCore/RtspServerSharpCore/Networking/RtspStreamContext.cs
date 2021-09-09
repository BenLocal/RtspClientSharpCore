using RtspClientSharpCore.Rtsp;
using System;
using System.Buffers;
using System.Collections.Generic;
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

            return rtspResponseMessageFactory.CreateInvalidResponse(request, RtspStatusCode.NotFound);
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
