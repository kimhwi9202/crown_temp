using UnityEngine;
using System.Collections;

namespace PK.TmtNowAccount
{
    public class REDataRank
    {
        public string middle_name { get; set; }
        public string picture { get; set; }
        public int earned_total { get; set; }
        public int rank { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public long user_id { get; set; }
    }
    public class REDataAccount
    {
        public int rank { get; set; }
        public int accumulate_per { get; set; }
    }

    public class REData
    {
        public REDataRank[] rank { get; set; }
        public REDataAccount[] account { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }



    public class SendData
    {
        public int tmt_id { get; set; }
        public int game_id { get; set; }
    }
    public class SEND
    {
        public SendData data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, int tmt_id, int game_id)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.tmt_id = tmt_id;
            this.data.game_id = game_id;
        }
    }
}