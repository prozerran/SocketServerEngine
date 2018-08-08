
using System.Net.Sockets;

namespace SocketEngine
{
    /// <summary>
    /// Base class for the Socket Listeners.
    /// </summary>
    public abstract class ISocketListener
    {
        protected bool _running = false;

        protected string _deviceModel = "";

        public abstract void Start(int port);
 
        public abstract void Stop();

        public abstract bool OnGetDeviceId(byte[] data, out long devid);

        public abstract bool IsClientClosing(byte[] data);

        public abstract bool OnRead(byte[] data, int len, NetworkStream ns);

        protected abstract void HandleDeviceThread(object obj);

        protected abstract bool OnGetDeviceId(byte[] data, int start_idx, int length, out long devid);
    }
}
