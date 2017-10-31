using UnityEngine;
using System.Collections;

namespace PK.InviteChallengeParticipate
{
    public class SendData
    {
        public int challenge_id { get; set; }
        public int item_id { get; set; }
        public string request_id { get; set; }
        public int count { get; set; }
        public string[] to { get; set; }
    }
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
        public int reward_count { get; set; }
        public string bonus_kind { get; set; }
        public long bonus_amount { get; set; }
        public long user_amount { get; set; }
        public string reward_kind { get; set; }
        public long reward_amount { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}