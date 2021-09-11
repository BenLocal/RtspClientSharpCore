using System.Collections.Generic;

namespace RtspClientSharpCore.Sdp
{
    public abstract class RtspTrackInfo
    {
        public string TrackName { get; }

        public List<string> SdpLines { get; }

        private readonly string _trackType;

        public TrackType TrackType
        {
            get
            {
                if (_trackType == null)
                {
                    return TrackType.Unknow;
                }

                if (_trackType.ToLower().Contains("video"))
                {
                    return TrackType.Video;
                }
                else if (_trackType.ToLower().Contains("audio"))
                {
                    return TrackType.Audio;
                }

                return TrackType.Unknow;
            }

        }

        protected RtspTrackInfo(string trackType, string trackName, List<string> sdpLines)
        {
            _trackType = trackType;
            TrackName = trackName;
            SdpLines = sdpLines;
        }
    }

    public enum TrackType
    {
        Unknow,
        Video,
        Audio,
    }
}