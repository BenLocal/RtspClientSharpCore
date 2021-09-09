using RtspServerSharpCore.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspServerSharpCore
{
    class RtspManager
    {
        /// <summary>
        /// TODO lazy
        /// </summary>
        public static RtspManager Current => new RtspManager();

        private readonly ConcurrentDictionary<string, RtspSource> _rtspSources;
        private readonly ConcurrentDictionary<string, RtspClientSession> _rtspClientSessions;

        private RtspManager()
        {
            _rtspSources = new ConcurrentDictionary<string, RtspSource>();
            _rtspClientSessions = new ConcurrentDictionary<string, RtspClientSession>();
        }

        internal bool TryAddSession(RtspClientSession session)
        {
            return _rtspClientSessions.TryAdd(session.Id, session);
        }

        internal bool TryRemoveSession(RtspClientSession session)
        {
            return _rtspClientSessions.TryRemove(session.Id, out var _);
        }

        /// <summary>
        /// 添加播放源
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool TryAddSource(RtspSource stream)
        {
            bool result = _rtspSources.TryAdd(stream.Name, stream);
            if (result)
            {
                // 开始执行
                Task.Run(async () =>
                {
                    await stream.StartAsync();
                });
            }

            return result;
        }

        public RtspSource FindSourceByAppNameAndStream(string appName, string streamName)
{
            return _rtspSources.Values.FirstOrDefault(x =>
                appName.Equals(x.AppName, StringComparison.InvariantCultureIgnoreCase) &&
                streamName.Equals(x.StreamName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
