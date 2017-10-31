using UnityEngine;
using System.Collections;

namespace PK.WinCast
{
    public class SendData
    {
        public string winMultiply { get; set; }
        public long userID { get; set; }
        public string userName { get; set; }
        public string gameName { get; set; }
        public long winID { get; set; }
        public string win { get; set; }
        public string winType { get; set; }
        public string pictureURL { get; set; }
        public int gameID { get; set; }
    }

    // [브로드캐스트 서버] 클라에서 50배 이상 당첨시 보내는 패킷
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, SendData data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }



    public class REData
    {
        public string winMultiply { get; set; }
        public long userID { get; set; }
        public string userName { get; set; }
        public string gameName { get; set; }
        public long winID { get; set; }
        public string win { get; set; }
        public string winType { get; set; }
        public string pictureURL { get; set; }
        public int gameID { get; set; }
    }

    // [브로드캐스트 서버] 비동기 받는 정보 ( 잭팟 유저 )
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}




namespace PK.GetBroadcastReward
{
    public class SendData
    {
        public string win_type { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, string win_type)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.win_type = win_type;
        }
    }


    public class REData
    {
        public long balance { get; set; }
        public long amount { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}


namespace PK.WinLike
{
    public class SendData
    {
        public long userID { get; set; }
        public long winID { get; set; }
    }

    // 비동기로 받은 wincast 에서 like[브로드캐스트 서버] 클릭시 보낸다 
    // 보낼때 get_broadcast_reward [로비서버] 같이 전송하고 끝
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, long winID, long userID)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.winID = winID;
            this.data.userID = userID;
        }
    }

    public class REData
    {
        public long win_id { get; set; }
        public string url { get; set; }
        public string first_name { get; set; }
    }
    // [브로드캐스트 서버] 비동기 받는다
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}

namespace PK.BroadCastConnect
{
    public class SendData
    {
        public long userID { get; set; }
        public string firstName{ get; set; }
        public string pictureURL { get; set; }
    }

    // 비동기로 받은 wincast 에서 like[브로드캐스트 서버] 클릭시 보낸다 
    // 보낼때 get_broadcast_reward [로비서버] 같이 전송하고 끝
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, long userID, string firstName, string pictureURL)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.userID = userID;
            this.data.firstName = firstName;
            this.data.pictureURL = pictureURL;
        }
    }
}