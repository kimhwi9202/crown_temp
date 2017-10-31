using UnityEngine;
using System.Collections;


namespace PK.DailySpin
{
    public class SendData
    {
        public int multiple { get; set; }
    }

    public class SEND
    {
        public SendData data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, int multiple)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.multiple = multiple;
        }
    }



    public class REData
    {
        public bool success { get; set; }
        public int friendsBonus { get; set; }
        public int wheelIndex { get; set; }
        public int friendsCount { get; set; }
        public int next_daily_spin_second { get; set; }
        public int levelBonus { get; set; }
        public int spinBonus { get; set; }
        public int userLevel { get; set; }
        public long balance { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}