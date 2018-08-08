using System;
using System.Threading;
using System.Configuration;
using SocketCommon;

namespace SocketEngine
{
    public sealed class SocketEngine
    {
        private static readonly SocketEngine _instance = new SocketEngine();
        private ISocketListener pSockObj = null;
        /// <summary>
        /// Status checker if engine is running.
        /// </summary>
        private bool _running = false;
        private Thread _thread = null;

        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as beforefieldinit.
        /// </summary>
        static SocketEngine() {}

        private SocketEngine() { }

        public static SocketEngine Instance { get { return _instance; } }
        /// <summary>
        /// Function that creates the thread for 
        /// the actual start of the process. See <see cref="SocketEngine.Start"/>.
        /// The function that gets called by <c>SocketService.OnStart</c>
        /// of the Service Controller.
        /// </summary>
        public void StartAsyn()
        {
            if (!_running)
            {
                _thread = new Thread(Start);
                _thread.Name = "SocketEngineThread";
                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private void Start()
        {
            int nPort = 0;

            try
            {
                string sModel = ConfigurationManager.AppSettings["model"];
                string sPort = ConfigurationManager.AppSettings["port"];

                if (Int32.TryParse(sPort, out nPort))
                {
                    // Socket factory to determine which listener we want to open/read
                    switch (sModel)
                    {
                        case DeviceModels.Device:
                            pSockObj = Listener_Device.Instance;
                            break;

                        default:
                            pSockObj = null;
                            break;
                    }

                    // start listening service on the particular device models
                    if (pSockObj != null)
                    {
                        _running = true;
                        pSockObj.Start(nPort);      // blocking call!
                    }
                    else
                    {
                        LogManager.LogErrorLine("Unable to find Socket Instance");
                    }
                }
                else
                {
                    LogManager.LogErrorLine("Unable to read port number");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogErrorLine("SocketEngine.Start - Exception: {0}", ex.Message);
            }
        }

        public void Stop()
        {
            if (pSockObj != null)
            {
                pSockObj.Stop();
                pSockObj = null;
                _running = false;
            }
        }
    }
}
