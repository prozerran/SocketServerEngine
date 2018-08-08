using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketCommon;

namespace SocketEngine
{
    /// <summary>
    /// Device socket related variables.
    /// </summary>
    public struct stDeviceSocket
    {
        public long deviceid { get; set; }
        public Thread thread { get; set; }
        public TcpClient tcpClient { get; set; }
    }
    /// <summary>
    /// Implements base class <see cref="ISocketListener"/> for Tcp.
    /// </summary>
    public abstract class TcpSocketListener : ISocketListener
    {
        protected TcpListener m_server { get; set; }
        /// <summary>
        /// Number of connections.
        /// </summary>
        protected volatile int m_nConnections = 0;
        /// <summary>
        /// Socket dictionary.
        /// </summary>
        protected Dictionary<long, stDeviceSocket> m_socket_conn = new Dictionary<long, stDeviceSocket>();
        /// <summary>
        /// Object use for lock statement.
        /// </summary>
        protected readonly object _cs_mutex = new object();

        public override void Start(int port)
        {
            this.m_server = new TcpListener(IPAddress.Any, port);
            this.m_server.Start();
            _running = true;

            LogManager.LogInfoLine("Server start: {0}", this.m_server.LocalEndpoint.ToString());                    

            // start listening to socket port and accept incoming device messages
            while (_running)
            {
                try
                {
                    TcpClient client = this.m_server.AcceptTcpClient();
                    Thread t = new Thread(HandleDeviceThread);      // possibly may need to decrease stack size
                    t.IsBackground = true;
                    t.Start(client);
                }
                catch (SocketException ex)
                {
                    if (!_running)
                        break;

                    LogManager.LogErrorLine("SocketException Start: {0}", ex.Message);
                }
                catch (Exception ex)
                {
                    LogManager.LogErrorLine("Exception Start: {0}", ex.Message);
                }
            }

            // listening at port ended, socket server stopped running
            if (!_running)
            {
                LogManager.LogInfoLine("Server stop: {0}", this.m_server.LocalEndpoint.ToString());
            }
        }

        public override void Stop()
        {
            _running = false;
            this.m_server.Stop();
            LogManager.LogInfoLine("Server stop.");
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
            byte[] data = new byte[SocketParserConstants.MAX_SIZE_READ_PACKET_BUFFER];
            TcpClient remclient = (TcpClient)obj;
            NetworkStream netstream = remclient.GetStream();
            remclient.ReceiveTimeout = SocketParserConstants.READ_PACKET_TIMEOUT_SHORT;
            LogManager.LogInfoLine("Client Connected. Active Connections = {0}", ++m_nConnections);

            // main loop to service the client
            while (netstream.CanRead)
            {
                try
                {
                    // clear buffer and read from network packet [blocking call]
                    Array.Clear(data, 0, data.Length);
                    int recv = netstream.Read(data, 0, data.Length);

                    // close socket if nothing was received
                    if (recv <= 0)
                    {
                        LogManager.LogInfoLine("Socket Closed.");
                        break;
                    }                    

                    // now that we have received initial data from device, let's increase max time-out
                    remclient.ReceiveTimeout = SocketParserConstants.READ_PACKET_TIMEOUT_LONG;

                    // handle data packet, log any and all data sent to server
                    if (this.OnRead(data, recv, netstream) == false || recv > SocketParserConstants.MAX_SIZE_DATAGRAM)
                    {
                        continue;
                    }

                    /*
                    // check if client issued a closing request, if so, close client socket and thread
                    if (IsClientClosing(data))
                    {
                        LogManager.LogInfoLine("Client Closed.");
                        break;
                    }
                     * */

                    // we'll need to replace the socket/close the old thread
                    long devid = 0;
                    if (this.OnGetDeviceId(data, out devid))
                    {
                        stDeviceSocket stobj = new stDeviceSocket
                        {
                            deviceid = devid,
                            thread = Thread.CurrentThread,
                            tcpClient = remclient
                        };

                        // replace socket, and insert data into tracker log
                        string strerr = string.Empty;
                        lock (_cs_mutex)
                        {
                            this.ReplaceSocket(stobj);

                            if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                                continue;

                            if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                                continue;

                            if (SocketSQL.Instance.InsertToDatabase(_deviceModel, data, recv, out strerr))
                                continue;
                        }
                        // end critical section

                        // 3x tries and we failed to insert into TrackerLog, lets log it
                        LogManager.LogErrorLine("MySqlInsertError: {0}", strerr);
                    }
                }
                catch (IOException)
                {
                    LogManager.LogInfoLine("Socket replaced or client timed-out.");
                    break;
                }
                catch (SocketException ex)
                {
                    LogManager.LogErrorLine("HandleDeviceThread - SocketException: {0}", ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    LogManager.LogErrorLine("HandleDeviceThread - Exception: {0}", ex.Message);
                    break;
                }
            }
            netstream.Close();
            remclient.Close();
            LogManager.LogInfoLine("Client Disconnected. Active Connections = {0}", --m_nConnections);
        }
        /// <summary>
        /// Replaces the old socket from <see cref="m_socket_conn"/>
        /// with the current socket or adds it, if it does not exist.
        /// </summary>
        /// <param name="stNew">The current socket</param>
        protected void ReplaceSocket(stDeviceSocket stNew)
        {
            try
            {
                if (!m_socket_conn.ContainsKey(stNew.deviceid))
                {
                    // device not exist, lets add it
                    m_socket_conn.Add(stNew.deviceid, stNew);
                }
                else
                {
                    // device exist, find the old thread and kill it, and replace it
                    stDeviceSocket stOld = m_socket_conn[stNew.deviceid];

                    // do nothing if device is comming from same thread
                    if (stNew.thread.ManagedThreadId == stOld.thread.ManagedThreadId)
                        return;

                    // explicitly close the client socket
                    if (stOld.thread.IsAlive && stOld.tcpClient.Connected)
                    {
                        stOld.tcpClient.Close();
                    }

                    // replace with current thread
                    m_socket_conn[stNew.deviceid] = stNew;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogErrorLine("Exception ReplaceSocket: {0}", ex.Message);
            }
        }        
    }
}
