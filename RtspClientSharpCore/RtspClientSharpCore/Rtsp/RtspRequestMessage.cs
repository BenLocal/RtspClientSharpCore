using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace RtspClientSharpCore.Rtsp
{
    public class RtspRequestMessage : RtspMessage
    {
        private readonly Func<uint> _cSeqProvider;

        public RtspMethod Method { get; }
        public Uri ConnectionUri { get; }
        public string UserAgent { get; }

        public RtspRequestMessage(RtspMethod method, Uri connectionUri, Version protocolVersion, Func<uint> cSeqProvider,
            string userAgent, string session)
            : base(cSeqProvider(), protocolVersion)
        {
            Method = method;
            ConnectionUri = connectionUri;
            _cSeqProvider = cSeqProvider;
            UserAgent = userAgent;

            if (!string.IsNullOrEmpty(session))
                Headers.Add("Session", session);
        }

        public static RtspRequestMessage Parse(ReadOnlySequence<byte> byteSegment)
        {
            using var headersStream = new MemoryStream(byteSegment.ToArray(), false);
            using var headersReader = new StreamReader(headersStream);

            string startLine = headersReader.ReadLine();

            if (startLine == null)
                throw new RtspParseResponseException("Empty response");

            string[] tokens = GetFirstLineTokens(startLine);
            RtspMethod rtspMethod = ParseRtspMethod(tokens[0]);
            Uri uri = ParseUri(tokens[1]);
            Version protocolVersion = ParseProtocolVersion(tokens[2]);

            NameValueCollection headers = HeadersParser.ParseHeaders(headersReader);

            uint cSeq = 0;
            string cseqValue = headers.Get("CSEQ");

            if (cseqValue != null)
                uint.TryParse(cseqValue, out cSeq);

            var session = headers.Get("SESSION");
            return new RtspRequestMessage(rtspMethod, uri, protocolVersion, () => cSeq, null, session);
        }


        public void UpdateSequenceNumber()
        {
            CSeq = _cSeqProvider();
        }

        public override string ToString()
        {
            var queryBuilder = new StringBuilder(512);

            queryBuilder.AppendFormat("{0} {1} RTSP/{2}\r\n", Method, ConnectionUri, ProtocolVersion.ToString(2));
            queryBuilder.AppendFormat("CSeq: {0}\r\n", CSeq);

            if (!string.IsNullOrEmpty(UserAgent))
                queryBuilder.AppendFormat("User-Agent: {0}\r\n", UserAgent);

            foreach (string headerName in Headers.AllKeys)
                queryBuilder.AppendFormat("{0}: {1}\r\n", headerName, Headers[headerName]);

            queryBuilder.Append("\r\n");

            return queryBuilder.ToString();
        }

        private static Uri ParseUri(string url)
        {
            return new Uri(url);
        }

        private static string[] GetFirstLineTokens(string startLine)
        {
            string[] tokens = startLine.Split(' ');

            if (tokens.Length == 0)
                throw new RtspParseResponseException("Missing method");
            if (tokens.Length == 1)
                throw new RtspParseResponseException("Missing URI");
            if (tokens.Length == 2)
                throw new RtspParseResponseException("Missing protocol version");

            return tokens;
        }

        private static Version ParseProtocolVersion(string protocolNameVersion)
        {
            int slashPos = protocolNameVersion.IndexOf('/');

            if (slashPos == -1)
                throw new RtspParseResponseException($"Invalid protocol name/version format: {protocolNameVersion}");

            string version = protocolNameVersion.Substring(slashPos + 1);
            if (!Version.TryParse(version, out Version protocolVersion))
                throw new RtspParseResponseException($"Invalid RTSP protocol version: {version}");

            return protocolVersion;
        }

        private static RtspMethod ParseRtspMethod(string method)
        {
            return (RtspMethod)Enum.Parse(typeof(RtspMethod), method.Trim(), true);
        }
    }
}