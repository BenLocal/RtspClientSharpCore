using RtspClientSharpCore.Codecs;
using System.Collections.Generic;

namespace RtspClientSharpCore.Sdp
{
    public class RtspMediaTrackInfo : RtspTrackInfo
    {
        public CodecInfo Codec { get; }
        public int SamplesFrequency { get; }

        public RtspMediaTrackInfo(string trackType, string trackName, CodecInfo codec, int samplesFrequency, List<string> sdpLines)
            : base(trackType, trackName, sdpLines)
        {
            Codec = codec;
            SamplesFrequency = samplesFrequency;
        }
    }
}