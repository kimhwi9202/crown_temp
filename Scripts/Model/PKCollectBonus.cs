using UnityEngine;
using System.Collections;

namespace PK.CollectBonus
{
    public class SendData
    {
        public long uid { get; set; }
    }
    public class SEND
    {
        public SendData data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, long uid)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.uid = uid;
        }
    }



    public class REData
    {
        public string status { get; set; }
        public long old_balance { get; set; }
        public long balance { get; set; }
        public long coins { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
