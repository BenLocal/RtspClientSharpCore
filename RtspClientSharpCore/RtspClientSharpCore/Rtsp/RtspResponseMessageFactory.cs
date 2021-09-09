using System;
using System.Collections.Generic;
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
            var headers = new System.Collections.Specialized.NameValueCollection()
            {
                { "Public", "OPTIONS,DESCRIBE,ANNOUNCE,SETUP,PLAY,PAUSE,TEARDOWN,GET_PARAMETER,SET_PARAMETER,REDIRECT"}
            };

            var rtspRequestMessage = new RtspResponseMessage(RtspStatusCode.Ok, rtspRequest.ProtocolVersion,
                rtspRequest.CSeq, headers);
            return rtspRequestMessage;
        }

        public RtspResponseMessage CreateDescribeResponse(RtspRequestMessage rtspRequest)
        {
            var headers = new System.Collections.Specialized.NameValueCollection()
            {
                { "Public", "OPTIONS,DESCRIBE,ANNOUNCE,SETUP,PLAY,PAUSE,TEARDOWN,GET_PARAMETER,SET_PARAMETER,REDIRECT"}
            };

            var rtspRequestMessage = new RtspResponseMessage(RtspStatusCode.Ok, rtspRequest.ProtocolVersion,
                rtspRequest.CSeq, headers);
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
