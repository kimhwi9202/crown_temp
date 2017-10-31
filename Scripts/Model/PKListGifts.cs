using UnityEngine;
using System.Collections;


namespace PK.ListGifts
{
    /// <summary>
    /// 로비 list_gifts 요청커맨드
    /// </summary>
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }



    public class REData
    {
        public long gift_id { get; set; }
        public long sender_uid { get; set; }
        public string request_id { get; set; }
        public string name { get; set; }
        public string picture { get; set; }

        public string type { get; set; }
        public long amount { get; set; }
        public string message { get; set; }
        public int limit_hour { get; set; }
    }

    /// <summary>
    /// list_gifts 응답패킷
    /// </summary>
    public class RECEIVE : PacketData
    {
        public REData[] data { get; set; }
    }
}