using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RtspClientSharpCore.Rtsp
{
    public class RtspResponseMessageFactory
    {
        public RtspResponseMessage CreateOptionsResponse(RtspRequestMessage rtspRequest)
        {
            var headers = new NameValueCollection()
            {
                { "Public", "OPTIONS,DESCRIBE,ANNOUNCE,SETUP,PLAY,PAUSE,TEARDOWN,GET_PARAMETER,SET_PARAMETER,REDIRECT"}
            };

            var rtspRequestMessage = new RtspResponseMessage(RtspStatusCode.Ok, rtspRequest.ProtocolVersion,
                rtspRequest.CSeq, headers);
            return rtspRequestMessage;
        }

        public RtspResponseMessage CreateDescribeResponse(RtspRequestMessage rtspRequest,
            NameValueCollection headers, ArraySegment<byte> body)
        {
            var rtspRequestMessage = new RtspResponseMessage(RtspStatusCode.Ok, rtspRequest.ProtocolVersion,
                rtspRequest.CSeq, headers);

            rtspRequestMessage.ResponseBody = body;
            return rtspRequestMessage;
        }

        public RtspResponseMessage CreateInvalidResponse(RtspRequestMessage rtspRequest, RtspStatusCode invalidCode)
        {
            var rtspRequestMessage = new RtspResponseMessage(invalidCode, rtspRequest.ProtocolVersion,
                rtspRequest.CSeq, null);
            return rtspRequestMessage;
        }
    }
}
