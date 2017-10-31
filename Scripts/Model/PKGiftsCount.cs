using UnityEngine;
using System.Collections;

namespace PK.GiftsCount
{
    /// <summary>
    /// 로비 gifts_count 요청커맨드 
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
        public int gift_count { get; set; }
    }

    /// <summary>
    /// gifts_count 응답패킷
    /// </summary>
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}