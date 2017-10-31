using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace xLIB
{
    #region Enums
	public enum eSocketState {
		Closed,
		Closing,
		Connected,
		Connecting,
		Listening,
	}
    #endregion


    public class NetConfig
    {
        #region socket colse reason
        public static readonly string CLOSE_NORMAL = "Normal";
        public static readonly string CLOSE_ASYNC_CONNECT_SOCKET_MISMATCHED = "Async Connect Socket mismatched";
        public static readonly string CLOSE_ASYNC_RECEIVE_SOCKET_MISMATCHED = "Async Receive Socket mismatched";
        public static readonly string CLOSE_CONNECT_EXCEPTION = "Connect Exception";
        public static readonly string CLOSE_CONNECT_TIMER = "Connect Timer";
        public static readonly string CLOSE_CONNECT_TIMER_EXCEPTION = "Connect Timer Exception";
        public static readonly string CLOSE_NO_BYTES_RECEIVED = "No Bytes Received";
        public static readonly string CLOSE_REMOTE_SOCKET_CLOSED = "Remote Socket Closed";
        public static readonly string CLOSE_SOCKET_SEND_EXCEPTION = "Socket Send Exception";
        public static readonly string CLOSE_SOCKET_CONNECT_EXCEPTION = "Socket Connect Exception";
        public static readonly string CLOSE_SOCKET_RECEIVE_EXCEPTION = "Socket Receive Exception";
        public static readonly string CLOSE_SOCKET_PING_TIMER_EXCEPTION = "Socket Ping Timer Exception";
        public static readonly string CLOSE_SOCKET_API_TIMEOUT = "API command Timeout";
        #endregion
    }


    #region Event Args
    public class EventNetSocketConnected : EventArgs {
		public IPAddress SourceIP;
		public EventNetSocketConnected(IPAddress ip) {
			this.SourceIP = ip;
		}
	}

	public class EventNetSocketDisconnected : EventArgs {
		public string Reason;
		public EventNetSocketDisconnected(string reason) {
			this.Reason = reason;
		}
	}

    public class EventNetSockStateChanged : EventArgs {
		public eSocketState NewState;
		public eSocketState PrevState;
		public EventNetSockStateChanged(eSocketState newState, eSocketState prevState) {
			this.NewState = newState;
			this.PrevState = prevState;
		}
	}

	public class EventNetSockDataArrival : EventArgs {
		public byte[] Data;
		public EventNetSockDataArrival(byte[] data) {
			this.Data = data;
		}
	}

	public class EventNetSockErrorReceived : EventArgs {
		public string Function;
		public Exception Exception;
		public EventNetSockErrorReceived(string function, Exception ex) {
			this.Function = function;
			this.Exception = ex;
		}
	}

	public class EventNetSockConnectionRequest : EventArgs {
		public Socket Client;
		public EventNetSockConnectionRequest(Socket client) {
			this.Client = client;
		}
	}
	#endregion
    
    /// <summary>
    /// 
    /// </summary>
    public abstract class NetBase {
        
        #region Fields
        /// <summary>Current socket state</summary>
        protected eSocketState _netState = eSocketState.Closed;
        /// <summary>The socket object, obviously</summary>
        protected Socket _socket;
        /// <summary>Keep track of when data is being sent</summary>
        protected bool _isSending = false;
        /// <summary>Queue of objects to be sent out</summary>
        protected Queue<byte[]> _sendBuffer = new Queue<byte[]>();
        /// <summary>Store incoming bytes to be processed</summary>
        protected byte[] _byteBuffer = new byte[8192];
        /// <summary>Position of the bom header in the _rxBuffer</summary>
        protected int _rxHeaderIndex = -1;
        /// <summary>Position of the end in the _rxBuffer</summary>
        protected int _rxFooterIndex = -1;
        /// <summary>Expected length of the message from the bom header</summary>
        protected int _rxBodyLen = -1;
        /// <summary>Buffer of received data</summary>
        protected MemoryStream _rxBuffer = new MemoryStream();
        /// <summary>Beginning of message indicator</summary>
        protected ArraySegment<byte> _bomBytes = new ArraySegment<byte>(new byte[] { 1, 2, 1, 255 });
        /// <summary>Ending of message indicator</summary>
        protected ArraySegment<byte> _endBytes = new ArraySegment<byte>(new byte[] {0});
        /// <summary>TCP inactivity before sending keep-alive packet (ms)</summary>
        protected uint KeepAliveInactivity = 500;
        /// <summary>Interval to send keep-alive packet if acknowledgement was not received (ms)</summary>
        protected uint KeepAliveInterval = 100;
        /// <summary>Threaded timer checks if socket is busted</summary>
        protected Timer _connectionTimer;
        /// <summary>Interval for socket checks (ms)</summary>
        protected int ConnectionCheckInterval = 2*1000;
        #endregion
        
        #region Public Properties
        /// <summary>Current state of the socket</summary>
        public eSocketState State { get { return this._netState; } }

        /// <summary>Port the socket control is listening on.</summary>
        public int LocalPort
        {
            get
            {
                try
                {
                    return ((IPEndPoint)this._socket.LocalEndPoint).Port;
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// <summary>IP address enumeration for local computer</summary>
        public static string[] LocalIP
        {
            get
            {
                IPHostEntry h = Dns.GetHostEntry(Dns.GetHostName());
                List<string> s = new List<string>(h.AddressList.Length);
                foreach (IPAddress i in h.AddressList)
                    s.Add(i.ToString());
                return s.ToArray();
            }
        }
        #endregion

        #region Events
        /// <summary>Socket is connected</summary>
        public event EventHandler<EventNetSocketConnected> Connected;
        /// <summary>Socket connection closed</summary>
        public event EventHandler<EventNetSocketDisconnected> Disconnected;
        /// <summary>Socket state has changed</summary>
        /// <remarks>This has the ability to fire very rapidly during connection / disconnection.</remarks>
        public event EventHandler<EventNetSockStateChanged> StateChanged;
        /// <summary>Recived a new object</summary>
        public event EventHandler<EventNetSockDataArrival> DataArrived;
        /// <summary>An error has occurred</summary>
        public event EventHandler<EventNetSockErrorReceived> ErrorReceived;
        #endregion
        
        #region Constructor
        /// <summary>Base constructor sets up buffer and timer</summary>
        public NetBase() {
            this._connectionTimer = new Timer(
                new TimerCallback(this.connectedTimerCallback),
                null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        #region Connect
		/// <summary>Connect to the server specified by Host and Port</summary>
       	/// <param name="endPoint"></param> 
		public void Connect(IPEndPoint endPoint, bool bIPv6Support) {
			if (this._netState == eSocketState.Connected)
				return; // already connecting to something

			try
			{
				if (this._netState != eSocketState.Closed)
					throw new Exception("Cannot connect socket is " + this._netState.ToString());

				this.OnChangeState(eSocketState.Connecting);

				if (this._socket == null)
				{
					if (bIPv6Support)
						this._socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
					else
						this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					
				}
                
				this._socket.BeginConnect(endPoint, new AsyncCallback(this.ConnectCallback), this._socket);
			}
			catch (Exception ex)
			{
				this.OnErrorReceived("Connect", ex);
				this.Close(NetConfig.CLOSE_CONNECT_EXCEPTION);
			}
		}

		/// <summary>Callback for BeginConnect</summary>
		/// <param name="ar"></param>
		private void ConnectCallback(IAsyncResult ar) {
			try
			{
				//Debug.Log("Connect CallBack Called!");
				Socket sock = (Socket)ar.AsyncState;
				sock.EndConnect(ar);

				if (this._socket != sock) {
					this.Close(NetConfig.CLOSE_ASYNC_CONNECT_SOCKET_MISMATCHED);
					return;
				}

				if (this._netState != eSocketState.Connecting)
					throw new Exception("Cannot connect socket is " + this._netState.ToString());

				this._socket.ReceiveBufferSize = this._byteBuffer.Length;
				this._socket.SendBufferSize = this._byteBuffer.Length;
				// this.SetKeepAlive(); // -> not work
				this.OnChangeState(eSocketState.Connected);
				this.OnConnected(this._socket);
				this.Receive();
			}
			catch (Exception ex)
			{
				//Debug.Log("Connect CallBack Exception : " + ex.Message);
				this.Close(NetConfig.CLOSE_SOCKET_CONNECT_EXCEPTION);
				this.OnErrorReceived("Socket Connect", ex);
			}
		}
		#endregion
        
        #region Send
        /// <summary>Send data</summary>
        /// <param name="bytes">Bytes to send</param>
        public void Send(byte[] data) {
            try
            {
                if (data == null)
                    throw new NullReferenceException("data cannot be null");
                else if (data.Length == 0)
                    throw new NullReferenceException("data cannot be empty");
                else
                {
                    lock (this._sendBuffer)
                    {
                        this._sendBuffer.Enqueue(data);
                    }

                    if (!this._isSending)
                    {
                        this._isSending = true;
                        this.SendNextQueued();
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Send", ex);
            }
        }

        /// <summary>Send data for real</summary>
        private void SendNextQueued() {
            try
            {
                // List<ArraySegment<byte>> send = new List<ArraySegment<byte>>(3);
                List<ArraySegment<byte>> send = new List<ArraySegment<byte>>(1);
                int length = 0;
                lock (this._sendBuffer)
                {
                    if (this._sendBuffer.Count == 0)
                    {
                        this._isSending = false;
                        return; // nothing more to send
                    }

                    byte[] data = this._sendBuffer.Dequeue();
                    
                    // case 1> packet protocol
                    // send.Add(this._bomBytes);
                    // send.Add(new ArraySegment<byte>(BitConverter.GetBytes(data.Length)));
                    // send.Add(new ArraySegment<byte>(data));
                    // length = this._bomBytes.Count + sizeof(int) + data.Length;

                    // case 2> pure string + _endBytes
                    send.Add(new ArraySegment<byte>(data));
                    send.Add(this._endBytes);
                    //length = this._endBytes.Count + data.Length;
                }
                this._socket.BeginSend(send, SocketFlags.None, new AsyncCallback(this.SendCallback), this._socket);
            }
            catch (Exception ex)
            {
                Debug.Log("## NetBase : SendNextQueued > Error = " + ex.ToString());
                this.OnErrorReceived("Sending", ex);
            }
        }

        /// <summary>Callback for BeginSend</summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar) {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                //int didSend = sock.EndSend(ar);

                if (this._socket != sock) {
                    this.Close(NetConfig.CLOSE_ASYNC_CONNECT_SOCKET_MISMATCHED);
                    return;
                }

                this.SendNextQueued();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    this.Close(NetConfig.CLOSE_REMOTE_SOCKET_CLOSED);
                else
                    throw;
            }
            catch (Exception ex)
            {
                this.Close(NetConfig.CLOSE_SOCKET_SEND_EXCEPTION);
                this.OnErrorReceived("Socket Send", ex);
            }
        }
        #endregion

        #region Close
        /// <summary>Disconnect the socket</summary>
        /// <param name="reason"></param>
        public void Close(string reason) {
            try
            {
                if (this._netState == eSocketState.Closing || this._netState == eSocketState.Closed)
                    return; // already closing/closed

                this.OnChangeState(eSocketState.Closing);

                if (this._socket != null)
                {
                    this._socket.Close();
                    this._socket = null;
                }
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Close", ex);
            }

            try
            {
                if (this._rxBuffer.Length > 0)
                {
                    if (this._rxHeaderIndex > -1 && this._rxBodyLen > -1)
                    {
                        // start of message - length of header
                        int msgbytes = (int)this._rxBuffer.Length - this._rxHeaderIndex - this._bomBytes.Count - sizeof(int);
                        this.OnErrorReceived("Close Buffer", new Exception("Incomplete Message (" + msgbytes.ToString() + " of " + this._rxBodyLen.ToString() + " bytes received)"));
                    }
                    else
                    {
                        this.OnErrorReceived("Close Buffer", new Exception("Unprocessed data " + this._rxBuffer.Length.ToString() + " bytes"));
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Close Buffer", ex);
            }

            try
            {
                lock (this._rxBuffer)
                {
                    this._rxBuffer.SetLength(0);
                }
                lock (this._sendBuffer)
                {
                    this._sendBuffer.Clear();
                    this._isSending = false;
                }
                
                this.OnChangeState(eSocketState.Closed);
                if (this.Disconnected != null)
                {
                    Debug.Log("### NetBase : Close , Disconnected >>>>>>>>>>>>>");
                    this.Disconnected(this, new EventNetSocketDisconnected(reason));
                }
                    
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Close Cleanup", ex);
            }
        }
        #endregion

        #region Receive
        /// <summary>Receive data asynchronously</summary>
        protected void Receive() {
            try
            {
                this._socket.BeginReceive(this._byteBuffer, 0, this._byteBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback2), this._socket);
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Receive", ex);
            }
        }

        /// <summary>Callback for BeginReceive</summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar) {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                int size = sock.EndReceive(ar);

                if (this._socket != sock)
                {
                    this.Close(NetConfig.CLOSE_ASYNC_RECEIVE_SOCKET_MISMATCHED);
                    return;
                }

                if (size < 1)
                {
                    this.Close(NetConfig.CLOSE_NO_BYTES_RECEIVED);
                    return;
                }

                lock (this._rxBuffer)
                {
                    // put at the end for safe writing
                    this._rxBuffer.Position = this._rxBuffer.Length;
                    this._rxBuffer.Write(this._byteBuffer, 0, size);

                    bool more = false;
                    do
                    {
                        // search for header if not found yet
                        if (this._rxHeaderIndex < 0)
                        {
                            this._rxBuffer.Position = 0; // rewind to search
                            this._rxHeaderIndex = this.IndexOfBytesInStream(this._rxBuffer, this._bomBytes.Array);
                        }

                        // have the header
                        if (this._rxHeaderIndex > -1)
                        {
                            // read the body length from header
                            if (this._rxBodyLen < 0 && this._rxBuffer.Length - this._rxHeaderIndex - this._bomBytes.Count >= 4)
                            {
                                this._rxBuffer.Position = this._rxHeaderIndex + this._bomBytes.Count; // start reading after _bomBytes
                                this._rxBuffer.Read(this._byteBuffer, 0, 4); // read message length
                                this._rxBodyLen = BitConverter.ToInt32(this._byteBuffer, 0);
                            }

                            // we have the message
                            if (this._rxBodyLen > -1 && (this._rxBuffer.Length - this._rxHeaderIndex - this._bomBytes.Count - 4) >= this._rxBodyLen)
                            {
                                try
                                {
                                    this._rxBuffer.Position = this._rxHeaderIndex + this._bomBytes.Count + sizeof(int);
                                    byte[] data = new byte[this._rxBodyLen];
                                    this._rxBuffer.Read(data, 0, data.Length);
                                    if (this.DataArrived != null)
                                        this.DataArrived(this, new EventNetSockDataArrival(data));
                                }
                                catch (Exception ex)
                                {
                                    this.OnErrorReceived("Receiving", ex);
                                }

                                if (this._rxBuffer.Position == this._rxBuffer.Length)
                                {
                                    // no bytes left
                                    // just resize buffer
                                    this._rxBuffer.SetLength(0);
                                    this._rxBuffer.Capacity = this._byteBuffer.Length;
                                    more = false;
                                }
                                else
                                {
                                    // leftover bytes after current message
                                    // copy these bytes to the beginning of the _rxBuffer
                                    this.CopyBack();
                                    more = true;
                                }

                                // reset header info
                                this._rxHeaderIndex = -1;
                                this._rxBodyLen = -1;
                            }
                            else if (this._rxHeaderIndex > 0)
                            {
                                // remove bytes from before the header
                                this._rxBuffer.Position = this._rxHeaderIndex;
                                this.CopyBack();
                                this._rxHeaderIndex = 0;
                                more = false;
                            }
                            else
                                more = false;
                        }
                    } while (more);
                }
                this._socket.BeginReceive(this._byteBuffer, 0, this._byteBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback2), this._socket);
            }
            catch (ObjectDisposedException e)
            {
                return; // socket disposed, let it die quietly
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    this.Close(NetConfig.CLOSE_REMOTE_SOCKET_CLOSED);
                else
                    throw;
            }
            catch (Exception ex)
            {
                this.Close(NetConfig.CLOSE_SOCKET_RECEIVE_EXCEPTION);
                this.OnErrorReceived("Socket Receive", ex);
            }
        }

        /// <summary>Callback for BeginReceive</summary>
        /// <remarks>Receive data pure data only.
        ///          no boombyte, no packet size
        /// </remarks>
        private void ReceiveCallback2(IAsyncResult ar) {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                int size = sock.EndReceive(ar);

                if (this._socket != sock)
                {
                    this.Close(NetConfig.CLOSE_ASYNC_RECEIVE_SOCKET_MISMATCHED);
                    return;
                }

                if (size < 1)
                {
                    this.Close(NetConfig.CLOSE_NO_BYTES_RECEIVED);
                    return;
                }

                lock (this._rxBuffer)
                {
                    // put at the end for safe writing
                    this._rxBuffer.Position = this._rxBuffer.Length;
                    this._rxBuffer.Write(this._byteBuffer, 0, size);

                    bool more = false;
                    do
                    {
                        // search for header if not found yet
                        if (this._rxFooterIndex < 0)
                        {
                            this._rxBuffer.Position = 0; // rewind to search
                            this._rxFooterIndex = this.IndexOfBytesInStream(this._rxBuffer, this._endBytes.Array);
                        }
                        
                        if(this._rxFooterIndex > -1)
                        {
                            try
                            {
                                this._rxBuffer.Position = 0;
                                byte[] data = new byte[this._rxFooterIndex + this._endBytes.Count];
                                this._rxBuffer.Read(data, 0, data.Length);    
                                if(this.DataArrived != null)
                                    this.DataArrived(this, new EventNetSockDataArrival(data));
                            }
                            catch (System.Exception ex)
                            {
                                this.OnErrorReceived("Receiving", ex);
                            }
                            
                            if(_rxBuffer.Length == this._rxFooterIndex  + this._endBytes.Count)
                            {
                                // no bytes left
                                // just resize buffer
                                this._rxBuffer.SetLength(0);
                                this._rxBuffer.Capacity = this._byteBuffer.Length;
                                more = false;
                            }
                            else
                            {
                                // leftover bytes after current message
                                // copy these bytes to the beginning of the _rxBuffer
                                this.CopyBack();
                                more = true;
                            }
                            // reset footer info
                            this._rxFooterIndex = -1;
                            this._rxBodyLen = -1;
                        }
                        else{
                            more = false;
                        }
                    } while (more);
                }
                this._socket.BeginReceive(this._byteBuffer, 0, this._byteBuffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback2), this._socket);
            }
            catch (ObjectDisposedException e)
            {
                return; // socket disposed, let it die quietly
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    this.Close(NetConfig.CLOSE_REMOTE_SOCKET_CLOSED);
                else
                    throw;
            }
            catch (Exception ex)
            {
                this.Close(NetConfig.CLOSE_SOCKET_RECEIVE_EXCEPTION);
                this.OnErrorReceived("Socket Receive", ex);
            }
        }

        /// <summary>
        /// Copies the stuff after the current position, back to the start of the stream,
        /// resizes the stream to only include the new content, and
        /// limits the capacity to length + another buffer.
        /// </summary>
        private void CopyBack() {
            int count;
            long readPos = this._rxBuffer.Position;
            long writePos = 0;
            do
            {
                count = this._rxBuffer.Read(this._byteBuffer, 0, this._byteBuffer.Length);
                readPos = this._rxBuffer.Position;
                this._rxBuffer.Position = writePos;
                this._rxBuffer.Write(this._byteBuffer, 0, count);
                writePos = this._rxBuffer.Position;
                this._rxBuffer.Position = readPos;
            }
            while (count > 0);
            this._rxBuffer.SetLength(writePos);
            this._rxBuffer.Capacity = (int)this._rxBuffer.Length + this._byteBuffer.Length;
        }

        /// <summary>Find first position the specified byte within the stream, or -1 if not found</summary>
        /// <param name="ms"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        private int IndexOfByteInStream(MemoryStream ms, byte find) {
            int b;
            do
            {
                b = ms.ReadByte();
            } while(b > -1 && b != find);

            if (b == -1)
                return -1;
            else
                return (int)ms.Position - 1; // position is +1 byte after the byte we want
        }

        /// <summary>Find first position the specified bytes within the stream, or -1 if not found</summary>
        /// <param name="ms"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        private int IndexOfBytesInStream(MemoryStream ms, byte[] find) {
            int index;
            do
            {
                index = this.IndexOfByteInStream(ms, find[0]);

                if (index > -1)
                {
                    bool found = true;
                    for (int i = 1; i < find.Length; i++)
                    {
                        if(find[i] != ms.ReadByte())
                        {
                            found = false;
                            ms.Position = index + 1;
                            break;
                        }
                    }
                    if (found)
                        return index;
                }
            } while(index > -1);
            return -1;
        }
        #endregion

        #region OnEvents
        protected void OnErrorReceived(string function, Exception ex) {
            if (this.ErrorReceived != null)
                this.ErrorReceived(this, new EventNetSockErrorReceived(function, ex));
        }

        protected void OnConnected(Socket sock) {
            if (this.Connected != null)
                this.Connected(this, new EventNetSocketConnected(((IPEndPoint)sock.RemoteEndPoint).Address));
        }

        protected void OnChangeState(eSocketState newState) {
            eSocketState prev = this._netState;
            this._netState = newState;
            if (this.StateChanged != null)
                this.StateChanged(this, new EventNetSockStateChanged(this._netState, prev));

            if (this._netState == eSocketState.Connected)
            {
                this._connectionTimer.Change(0, this.ConnectionCheckInterval);
            }
                
            else if (this._netState == eSocketState.Closed)
            {
                this._connectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
                
        }
        #endregion

        #region Keep-alives
        /*
            * Note about usage of keep-alives
            * The TCP protocol does not successfully detect "abnormal" socket disconnects at both
            * the client and server end. These are disconnects due to a computer crash, cable 
            * disconnect, or other failure. The keep-alive mechanism built into the TCP socket can
            * detect these disconnects by essentially sending null data packets (header only) and
            * waiting for acks.
            */

        /// <summary>Structure for settings keep-alive bytes</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct tcp_keepalive {
            /// <summary>1 = on, 0 = off</summary>
            public uint onoff;
            /// <summary>TCP inactivity before sending keep-alive packet (ms)</summary>
            public uint keepalivetime;
            /// <summary>Interval to send keep-alive packet if acknowledgement was not received (ms)</summary>
            public uint keepaliveinterval;
        }


        /// <summary>Set up the socket to use TCP keep alive messages</summary>
        protected void SetKeepAlive() {
            try
            {
                tcp_keepalive sioKeepAliveVals = new tcp_keepalive();
                sioKeepAliveVals.onoff = (uint)1; // 1 to enable 0 to disable
                sioKeepAliveVals.keepalivetime = this.KeepAliveInactivity;
                sioKeepAliveVals.keepaliveinterval = this.KeepAliveInterval;

                IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(sioKeepAliveVals));
                Marshal.StructureToPtr(sioKeepAliveVals, p, true);
                byte[] inBytes = new byte[Marshal.SizeOf(sioKeepAliveVals)];
                Marshal.Copy(p, inBytes, 0, inBytes.Length);
                Marshal.FreeHGlobal(p);

                byte[] outBytes = BitConverter.GetBytes(0);
                this._socket.IOControl(IOControlCode.KeepAliveValues, inBytes, outBytes);
                this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("Keep Alive", ex);
            }
        }

        #endregion

        #region Connection Sanity Check
        private void connectedTimerCallback(object sender) {
            // Checks if socket is busted
            try
            {
                if (this._netState == eSocketState.Connected &&
                    (this._socket == null || !this._socket.Connected))
                {
                    this.Close(NetConfig.CLOSE_CONNECT_TIMER);    
                }
            }
            catch (Exception ex)
            {
                this.OnErrorReceived("ConnectTimer", ex);
                this.Close(NetConfig.CLOSE_CONNECT_TIMER_EXCEPTION);
            }
            
            // Checks if network is busted 
            // if(pingCheckCount++ > pingCheckInterval)
            // {
            //     SendPing();
            // }
        }
        #endregion
    }   
}

