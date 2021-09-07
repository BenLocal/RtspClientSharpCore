using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RtspServerSharpCore.Networking
{
    class WriteState
    {
        public byte[] Buffer;
        public int Length;
        public TaskCompletionSource<int> TaskSource = null;
    }
}
