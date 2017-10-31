using UnityEngine;
using System.Collections;

namespace PK.GamesInfo
{
    public class SEND
    {
        public long data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, long id)
        {
            this.cmd = cmd;
            this.data = id;
        }
    }


    public class REData
    {
        public int game_id { get; set; }
        public string status { get; set; }	// active , coming_soon , maintenance
        public string loading_image { get; set; }
        public string tag { get; set; }
        public string icon { get; set; }
        public long jackpot_pool { get; set; }
        public string name { get; set; }
        public int tournament { get; set; }
        public int min_bet { get; set; }
        public int max_bet { get; set; }
        public string connection_url { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData[] data { get; set; }
    }
}