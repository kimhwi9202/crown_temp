using UnityEngine;
using System.Collections;

namespace PK.SendGiftChallenge
{
    public class SendData
    {
        public string type { get; set; }
        public string[] to { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set;  }
        public SEND(string cmd, SendData data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }




    public class REData
    {
        public string type { get; set; }
        public string[] to { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}