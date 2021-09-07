using RtspClientSharpCore.Rtsp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RtspServerSharpCore.Networking
{
    class RtspStreamContext : IDisposable
    {
        private IOPipeLine _ioPipeline = null;
        private RtspResponseMessageFactory rtspResponseMessageFactory;
        internal RtspClientSession _clientSession = null;

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
                    break;

            }

            if (response != null)
            {
                byte[] a = ASCIIEncoding.UTF8.GetBytes(response.ToString());
                await _ioPipeline.SendRawData(a);
            }
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
