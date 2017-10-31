using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;


namespace xLIB
{
    ///<summary>
    ///네트워크 소켓 클래스 ( 동기, 비동기 패킷을 처리한다 )
    ///</summary>
    public class NetSocket : NetBase
    {
        #region ApiTokenKey
        public string APIKEY_CMD = "cmd";
        public string APIKEY_RESULT = "success";
        public string APIKEY_DATA = "data";
        #endregion

        public class eEventIDs
        {
            public const string SocketConnect = "SocketConnect";
            public const string SocketDisconnected = "SocketDisconnected";
            public const string SocketStateChanged = "SocketStateChanged";
            public const string SocketError = "SocketError";
            public const string SocketData = "SocketData";
        }

        public struct STPacketQ
        {
            public string id;
            public int time;
            public Action<string> timeoutCallback;
            public Action<string, string> responseCallback;
            public STPacketQ(string id, Action<string, string> response, Action<string> timeout, int time)
            {
                this.id = id;
                this.time = time;
                this.responseCallback = response;
                this.timeoutCallback = timeout;
            }
        }

        protected bool bAutoConnect;
        protected Timer autoConnectTimer;
        protected int AutoConnectInterval = 15 * 1000;
        protected Schedule _ReceiveSchedule = new Schedule();
        protected int _timeOutSocket = 30;
        protected List<STPacketQ> _packetQueue = new List<STPacketQ>();
        protected IPEndPoint _address = null;
        protected Action<string> _receivePacket = null;
        protected Action<string,string> _socketEvent = null;

#if UNITY_EDITOR || LOCAL_DEBUG
        public bool logShow = false;
#endif
        #region Constructor
        public NetSocket(bool log=false) : base()
        {
#if UNITY_EDITOR || LOCAL_DEBUG
            logShow = log;
#endif
            Connected += new EventHandler<EventNetSocketConnected>(OnSocketConnected);
            Disconnected += new EventHandler<EventNetSocketDisconnected>(OnSocketDisconnected);
            StateChanged += new EventHandler<EventNetSockStateChanged>(OnSocketStateChanged);
            ErrorReceived += new EventHandler<EventNetSockErrorReceived>(OnSocketError);
            DataArrived += new EventHandler<EventNetSockDataArrival>(OnSocketDataArrived);
            autoConnectTimer = new Timer(new TimerCallback(AutoConnectTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
            _ReceiveSchedule.SetCallback_HandleMessage(ReceiveParserCommand);
        }
        #endregion

        #region 내부함수
        ///<summary>
        ///서버접속 정보를 ip주소와 port정보로 나누어 반환
        ///</summary>
        private void GetServerIpPortInfo(string serverAddr, out string ipAddress, out int port)
        {
            string[] serverInfo = serverAddr.Split(':');
            ipAddress = serverInfo[0];
            port = int.Parse(serverInfo[1]);
        }
        private string DomainToIPAddress(string domainName, bool favorIpV6 = false)
        {
            var favoredFamily = favorIpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            IPAddress[] ips = Dns.GetHostAddresses(domainName);
            foreach (IPAddress ipAddr in ips)
            {
                if (ipAddr.AddressFamily == favoredFamily)
                {
                    return ipAddr.ToString();
                }
            }
            return null;
        }
        private void AutoConnectTimerCallback(object sender)        {}
        protected void OnSocketConnected(object sender, EventNetSocketConnected e)
        {
            if (bAutoConnect)
            {
                this.autoConnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            _ReceiveSchedule.AddMessage(eEventIDs.SocketConnect);
        }
        protected void OnSocketDisconnected(object sender, EventNetSocketDisconnected e)
        {
            if (bAutoConnect)
            {
                if (e.Reason == "ping error" || e.Reason == "Connect Timer")
                {
                    this.autoConnectTimer.Change(0, this.AutoConnectInterval);
                }
            }
            _ReceiveSchedule.AddMessage(eEventIDs.SocketDisconnected);
        }
        protected void OnSocketStateChanged(object sender, EventNetSockStateChanged e)
        {
            Debug.Log("<Color=#FF00FF> NET::Callback_EventSocket > new state : " + e.NewState.ToString() + " , prev state : " + e.PrevState.ToString() + "</Color>");
            _ReceiveSchedule.AddMessage(eEventIDs.SocketStateChanged);
        }
        private void OnSocketError(object sender, EventNetSockErrorReceived e)
        {
            if (e.Exception.GetType() == typeof(System.Net.Sockets.SocketException))
            {
                System.Net.Sockets.SocketException s = (System.Net.Sockets.SocketException)e.Exception;
                Debug.Log("OnSocketError > System.Net.Sockets Exception " + e.Function + " - " + s.SocketErrorCode.ToString() + " => " + s.ToString());
            }
            else
            {
                Debug.Log("OnSocketError > ERROR : " + e.Function.ToString() + " => " + e.Exception.ToString());
            }
            _ReceiveSchedule.AddMessage(eEventIDs.SocketError);
        }
        /// <summary>
        /// 서버에서 받은 소켓 데이터 Called when [socket data arrived].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnSocketDataArrived(object sender, EventNetSockDataArrival e)
        {
            string msg = Encoding.UTF8.GetString(e.Data);
            msg = msg.Substring(0, msg.Length - 1);
            // 패킷정보를 스케줄 처리하는 이유는 메인쓰레드 접근 규칙 오류 해결을 위해서다
            // 손쉬운 콜백처리시 콜백에서 유니티 함수 사용시 메인쓰레드 접근 오류난다.
            _ReceiveSchedule.AddMessage(eEventIDs.SocketData, "msg", msg);
        }
        /// <summary>
        /// 전송패킷의 응답 채크및 콜백 처리후에 삭제처리
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        private bool PopRemoveQueue(string msg)
        {
            bool rqPacket = false;
            xLitJson.JsonData data = xLitJson.JsonMapper.ToObject(msg);
            string packetId = (string)data[APIKEY_CMD];
            for (int i = 0; i < _packetQueue.Count; i++)
            {
                if (_packetQueue[i].id.Equals(packetId))
                {
                    _packetQueue[i].responseCallback(packetId, msg);
                    _packetQueue.RemoveAt(i);
                    rqPacket = true;
                    break;
                }
            }
            return rqPacket;
        }
        /// <summary>
        /// 현재시간을 초단위로 환산하여 반환
        /// </summary>
        /// <returns></returns>
        private int GetCurTime()
        {
            DateTime dateTime = DateTime.Now;
            return dateTime.Hour * 60 * 60 + dateTime.Minute * 60 + dateTime.Second;
        }
        /// <summary>
        /// 소켓통신 타임아웃 체크
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void TimeoutCallback()
        {
            for (int i = 0; i < _packetQueue.Count; i++)
            {
                int curTime = GetCurTime();
                if (_packetQueue[i].time + _timeOutSocket < curTime)
                {
                    _packetQueue[i].timeoutCallback(_packetQueue[i].id);
                    _packetQueue.RemoveAt(i);
                }
            }
        }
        private void ReceiveParserCommand(Hashtable has)
        {
            string _id = has["id"].ToString();
            switch (_id)
            {
                case eEventIDs.SocketConnect: 
                case eEventIDs.SocketDisconnected: 
                case eEventIDs.SocketStateChanged: 
                case eEventIDs.SocketError:
                    if (_socketEvent != null)
                    {
                        _socketEvent(_id, "");
                    }
                    break;

                case eEventIDs.SocketData:
                    {
                        string msg = has["msg"].ToString();
#if UNITY_EDITOR || LOCAL_DEBUG
                        if (logShow)
                        {
                            xLitJson.JsonData data = xLitJson.JsonMapper.ToObject(msg);
                            string id = (string)data[APIKEY_CMD];
                            Debug.Log(_address.Port + "<Color=#30C0FF>ReceiveData >> id: " + id + " > packet </Color> " + msg);
                        }
#endif
                        if(!PopRemoveQueue(msg))
                        {
                            if (_receivePacket != null) _receivePacket(msg);
                        }
                    }
                    break;

            }
            _ReceiveSchedule.remove(_id);

        }

        #endregion


        #region Interface Funcs
        /// <summary>
        /// 소켓 이벤트 콜백
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void SetCallback_EventSocket(Action<string, string> callback) { _socketEvent = callback; }
        /// <summary>
        /// 받은 패킷 콜백 ( Send 응답패킷 외의 서버 패킷 정보다 )
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void SetCallback_ReceivePacket(Action<string> callback) { _receivePacket = callback; }
        /// <summary>
        /// 패킷 해더 분석용 키값이다.
        /// </summary>
        /// <param name="key_cmd">The key command.</param>
        /// <param name="key_result">The key result.</param>
        /// <param name="key_data">The key data.</param>
        public void InitApiTokenKey(string key_cmd, string key_result, string key_data)
        {
            APIKEY_CMD = key_cmd;
            APIKEY_RESULT = key_result;
            APIKEY_DATA = key_data;
        }
        /// <summary>
        /// 패킷 전송 ( 응답 패킷은 response 콜백사용 , 응답 시간 기다리려면 timeout 콜백사용 )
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="id">The identifier.전송패킷과 응답 패킷의 아이디는 같아야 한다.</param>
        /// <param name="response">The response. null 이면 응답을 기다리지 않는다.</param>
        /// <param name="timeout">The timeout.null 이면 응답시간을 채크하지 않는다.</param>
        public void SendData(string packet, string id, Action<string, string> response, Action<string> timeout)
        {
#if UNITY_EDITOR || LOCAL_DEBUG
            if (logShow) Debug.Log(_address.Port + "<Color=#ffc000>SendData >> id: " + id + " > packet: </Color> " + packet);
#endif
            if (string.IsNullOrEmpty(packet) || base.State != eSocketState.Connected)   {   return;    }

            try
            {
                //byte[] rawData = Encoding.ASCII.GetBytes(packet);
                byte[] rawData = Encoding.UTF8.GetBytes(packet);
                if (rawData.Length > 0)
                {
                    if(response != null) _packetQueue.Add(new STPacketQ(id, response, timeout, GetCurTime()));
                    base.Send(rawData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("SendData >> System.Exception = " + e.ToString());
            }
        }

        public void Connect(string serverURL)
        {
            string ip;
            int port;
            Utils.GetServerIpPortInfo(serverURL, out ip, out port);
            Connect(ip, port);
        }

        public bool Connect(string ip, int port)
        {
			bool bIPv6Support = true;
			//Debug.Log("IP.::" + ip + " port :: " + port);
            try {
				//Debug.Log(">>>>>>>>> start Connect <<<<<<<<<");
				/*
                _address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port);
				Debug.Log("first::" + _address.ToString());
				*/
				string serverIP = Utils.DomainToIPAddress(ip, true);
				if (string.IsNullOrEmpty(serverIP))
					bIPv6Support = false;
				//Debug.Log("Try IPv6 String : " + serverIP);
				_address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(serverIP), port);
				//Debug.Log("Try IPv6 : " + _address.ToString());

            }//
            catch {
				bIPv6Support = false;
				//Debug.Log("Src : " + e.Source);
				//Debug.Log("Msg : " +b e.Message);

				/*
                string serverIP = Utils.DomainToIPAddress(ip,true);
                _address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(serverIP), port);
				Debug.Log("second::" + _address.ToString());
				*/
				string serverIP = Utils.DomainToIPAddress(ip, false);
				//Debug.Log("Try IPv4 String : " + serverIP);
				_address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(serverIP), port);
				//Debug.Log("Try IPv4 : " + _address.ToString());
            }
            if (_address == null) {
                Debug.Log(">>>>>>>>> Faild! Connect <<<<<<<<<");
                return false;
            }

			Connect(_address, false, bIPv6Support);
            return true;
        }
        public void Connect(IPEndPoint endPoint, bool autoConnect = false, bool bIPv6Support = false)
        {
            _address = endPoint;
			base.Connect(_address, bIPv6Support);
            bAutoConnect = autoConnect;
        }

        public void Disconnect()
        {
            base.Close(NetConfig.CLOSE_NORMAL);
        }

        /// <summary>
        /// 소켓사용시 꼭 업데이트 호출해줘야 정상적인 작동이 가능( 업데이트 갱신타임은 응답 패킷 처리 시간에 상응한다 )
        /// </summary>
        public void Update()
        {
            // 스케줄러가 클래스 형이라서 업데이트는 여기서 직접해줘야 한다.
            _ReceiveSchedule.UpdateMessage();  
            TimeoutCallback();
        }

        /// <summary>
        /// 패킷 타임아웃 시간 정의 ( 초단위 )
        /// </summary>
        /// <param name="timeOutSec">The time out sec.</param>
        public void SetTimoutTime(int timeOutSec)
        {
            _timeOutSocket = timeOutSec;
        }

        /// <summary>
        /// 패킷 아이디 얻는 유틸리티 함수
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns></returns>
        public string ParserPacketID(string packet)
        {
            if (string.IsNullOrEmpty(packet) || base.State != eSocketState.Connected) { return ""; }
            xLitJson.JsonData data = xLitJson.JsonMapper.ToObject(packet);
            return (string)data[APIKEY_CMD];
        }

        public bool IsConnected() { return base.State == eSocketState.Connected ? true : false; }
        public bool IsClosed() { return base.State == eSocketState.Closed? true : false; }
    }
    #endregion //Interface Funcs
}