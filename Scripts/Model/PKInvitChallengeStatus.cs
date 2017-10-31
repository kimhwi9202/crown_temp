using UnityEngine;
using System.Collections;

namespace PK.InviteChallengeStatus
{
    public class SendData
    {
        public int challenge_id { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, int id)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.challenge_id = id;
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