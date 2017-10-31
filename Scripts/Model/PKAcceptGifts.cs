using UnityEngine;
using System.Collections;

namespace PK.AcceptGifts
{
    public class SEND
    {
        public string cmd { get; set; }
        public long[] data { get; set; }
        public SEND(string cmd, long[] giftId)
        {
            this.cmd = cmd;
            this.data = giftId;
        }
    }


    public class REData
    {
        public string status { get; set; }
        public string inBoxType { get; set; }
        public long balance { get; set; }
        public long old_balance { get; set; }
        public int increase_coins { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}