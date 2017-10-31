using UnityEngine;
using System.Collections;

namespace PK.SendGiftChallengeStatus
{
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
        public int remain { get; set; }
        public int status { get; set; }
        public int index { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}