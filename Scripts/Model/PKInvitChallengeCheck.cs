using UnityEngine;
using System.Collections;

namespace PK.InviteChallengeCheck
{
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }


    public class REDataItem
    {
        public int invite_challenge_day { get; set; }
        public string reward_kind { get; set; }
        public int reward_amount { get; set; }
        public int invite_challenge_id { get; set; }
        public string bonus_kind { get; set; }
        public int id { get; set; }
        public int bonus_amount { get; set; }
        public int max_reward_count { get; set; }
    }
    public class REData
    {
        public int event_period { get; set; }
        public int event_remain_time { get; set; }
        public int id { get; set; }
        public REDataItem[] item { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}