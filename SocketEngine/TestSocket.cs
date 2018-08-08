
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using SocketCommon;
using NUnit.Framework;

namespace SocketEngine
{
    /// <summary>
    /// Used for testing.
    /// </summary>
    [TestFixture]
    public class TestSocket
    {
        // declare any objects here

        [TestFixtureSetUp]
        public void Init()          // executes once
        {
            SocketSQL.Instance.SetConnectionString("mysql_db_connection_string");
        }

        [TestFixtureTearDown]
        public void Dispose()       // executes once
        {
            // for some reason, dispose seem to be called before data is inserted into DB
            //SocketSQL.Instance.Dispose();
        }

        [SetUp]
        public void Setup()         // executes for each test case        
        {}

        [TearDown]
        public void TearDown()      // executes for each test case
        {}

        protected void StartSocket(ISocketListener pSock, int nPort)
        {
            // start an instance of socket server locally
            Thread t = new Thread(() => pSock.Start(nPort));
            t.IsBackground = true;
            t.Start();

            Thread.Sleep(200);  // add some lead time to let socket server started
        }

        protected void StopSocket(ISocketListener pSock) 
        { 
            pSock.Stop();
            Thread.Sleep(800);  // add some lead time to let socket server stopped
        }

        protected int SendDataToSocket(byte[] strData, int nPort)
        {
            using (Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // connect to an instance of the socket server locally
                soc.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), nPort));
                return soc.Send(strData);
            }
        }

        protected int SendDataToSocketUdp(byte[] strData, int nPort)
        {
            using (Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // connect to an instance of the socket server locally
                soc.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), nPort));
                return soc.Send(strData);
            }
        }

        [Test]
        public void Test_Insert_Device()
        {
            string strerr = string.Empty;

            string strData = "DEVICE_PROTOCOL_STRING";
            byte[] data = Encoding.ASCII.GetBytes(strData);
            SocketSQL.Instance.InsertToDatabase(DeviceModels.Device, data, data.Length, out strerr);
        }

        [Test]
        public void Test_Socket_Device()
        {
            long devid = 0;

            StartSocket(Listener_Device.Instance, 0);
            string strData = "DEVICE_PROTOCOL_STRING";
            int sent = SendDataToSocket(Encoding.UTF8.GetBytes(strData), 0);
            Assert.AreEqual(strData.Length, sent);
            StopSocket(Listener_Device.Instance);

            byte[] data = Encoding.ASCII.GetBytes(strData);
            bool ret = Listener_Device.Instance.OnRead(data, data.Length, null);
            Assert.IsTrue(ret);

            ret = Listener_Device.Instance.OnGetDeviceId(data, out devid);
            Assert.IsTrue(ret);
            Assert.AreEqual(0, devid);
        }
    }
}
