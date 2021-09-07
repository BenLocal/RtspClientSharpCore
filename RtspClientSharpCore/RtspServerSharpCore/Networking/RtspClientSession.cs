using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspServerSharpCore.Networking
{
    class RtspClientSession : IDisposable
    {
        private IOPipeLine _ioPipeline = null;

        public RtspClientSession(IOPipeLine stream)
        {
            _ioPipeline = stream;
        }

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
