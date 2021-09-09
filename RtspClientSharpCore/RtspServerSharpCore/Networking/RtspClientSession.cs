using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RtspServerSharpCore.Networking
{
    class RtspClientSession : IDisposable
    {
        private readonly Guid _id;
        private readonly IOPipeLine _ioPipeline;

        private string _sessionId;

        public RtspClientSession(IOPipeLine stream)
        {
            _id = Guid.NewGuid();
            _ioPipeline = stream;
        }

        public string Id => _id.ToString();

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
