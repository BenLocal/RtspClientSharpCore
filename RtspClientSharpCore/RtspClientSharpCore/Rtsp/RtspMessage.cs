﻿using System;
using System.Collections.Specialized;

namespace RtspClientSharpCore.Rtsp
{
    public abstract class RtspMessage
    {
        public uint CSeq { get; protected set; }
        public Version ProtocolVersion { get; }
        public NameValueCollection Headers { get; }

        protected RtspMessage(uint cSeq, Version protocolVersion)
        {
            CSeq = cSeq;
            ProtocolVersion = protocolVersion;
            Headers = new NameValueCollection();
        }

        protected RtspMessage(uint cSeq, Version protocolVersion, NameValueCollection headers)
        {
            CSeq = cSeq;
            ProtocolVersion = protocolVersion;
            Headers = headers;
        }
    }
}