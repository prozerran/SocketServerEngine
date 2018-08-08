
using System.Text;
using System.Net.Sockets;
using SocketCommon;

namespace SocketEngine
{
    public sealed class Listener_Device : TcpSocketListener
    {
        private static readonly Listener_Device _instance = new Listener_Device();
        public static Listener_Device Instance { get { return _instance; } }
        static Listener_Device() { }

        private Listener_Device()
        {
            _deviceModel = DeviceModels.Device;
        }

        public override bool OnGetDeviceId(byte[] data, out long devid)
        {
            return OnGetDeviceId(data, 0, 0, out devid);
        }

        public override bool OnRead(byte[] data, int len, NetworkStream ns)
        {
            string str = Encoding.ASCII.GetString(data, 0, len);
            LogManager.LogInfoLine("{0}", str);

            if (str.StartsWith("+"))
                return true;

            return false;
        }
    }
}
