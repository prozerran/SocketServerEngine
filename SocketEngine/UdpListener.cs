using System;
using System.Net;
using System.Net.Sockets;
using SocketCommon;

namespace SocketEngine
{
    /// <summary>
    /// Implements base class <see cref="ISocketListener"/> for Udp.
    /// </summary>
    public abstract class UdpSocketListener : ISocketListener
    {
        protected Socket m_server { get; set; }
        protected byte[] m_recvBuf;
        /// <summary>
        /// Creates a socket instance.
        /// <para><c>new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);</c></para>
        /// </summary>
        public UdpSocketListener()
        {
            m_server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);    
        }

        public override void Start(int port)
        {       
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            LogManager.LogInfoLine("Server start: {0}", ipEndPoint.ToString());

            try
            {
                m_recvBuf = new byte[SocketParserConstants.MAX_SIZE_READ_PACKET_BUFFER];
                EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);

                // bind to server
                m_server.Bind(ipEndPoint);

                // receive data from client
                m_server.BeginReceiveFrom(m_recvBuf, 0, m_recvBuf.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, m_server);
            }
            catch (SocketException ex)
            {
                LogManager.LogErrorLine("SocketException Start: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogErrorLine("Exception Start: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Establishes the connection to client for Udp. 
        /// Listens, reads and saves data.
        /// </summary>
        /// <param name="iar"></param>
        private void DoReceiveFrom(IAsyncResult iar)
        {
            try
            {
                Socket recvSock = (Socket)iar.AsyncState;
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                int recv = recvSock.EndReceiveFrom(iar, ref clientEP);

                byte[] data = new byte[recv];
                Array.Copy(m_recvBuf, data, recv);

                EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                m_server.BeginReceiveFrom(m_recvBuf, 0, m_recvBuf.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, m_server);

                if (recv <= SocketParserConstants.MAX_SIZE_DATAGRAM)
                {
                    if (this.OnRead(data, recv, null))
                    {
                        string strerr = string.Empty;

                        if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                            return;

                        if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                            return;

                        if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                            return;

                        // 3x tries and we failed to insert into TrackerLog, lets log it
                        LogManager.LogErrorLine("MySqlInsertError: {0}", strerr);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                LogManager.LogInfoLine("Server Closing.");
            }
            catch (SocketException ex)
            {
                LogManager.LogErrorLine("SocketException DoReceiveFrom: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogErrorLine("Exception DoReceiveFrom: {0}", ex.Message);
            }
        }

        public override void Stop()
        {
            this.m_server.Close();
            LogManager.LogInfoLine("Server stop: {0}", this.m_server.LocalEndPoint.ToString());
        }

        public override bool OnRead(byte[] data, int len, NetworkStream ns)
        {
            throw new NotImplementedException();
        }

        public override bool OnGetDeviceId(byte[] data, out long devid)
        {
            throw new NotImplementedException();
        }

        protected override bool OnGetDeviceId(byte[] data, int start_idx, int length, out long devid)
        {
            devid = 0;
            string str = data.SubBytesAsString(start_idx, length);

            if (!String.IsNullOrEmpty(str))
            {
                if (Int64.TryParse(str, out devid))
                    return true;
            }
            return false;
        }

        public override bool IsClientClosing(byte[] data)
        {
            return false;
        }

        protected override void HandleDeviceThread(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
