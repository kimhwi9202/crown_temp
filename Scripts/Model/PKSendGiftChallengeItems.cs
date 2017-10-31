using UnityEngine;
using System.Collections;

namespace PK.SendGiftChallengeItems
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
        public int event_id { get; set; }
        public int phase { get; set; }
        public int reward { get; set; }
        public string kind { get; set; }
        public int id { get; set; }
        public int max_reward_count { get; set; }
    }

    public class REData
    {
        public REDataItem[] item { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}