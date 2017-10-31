using UnityEngine;
using System.Collections;


namespace PK.AppFriends
{
    public class SEND
    {
        public string[] data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, string[] data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }




    public class REData
    {
        public string last_sent { get; set; }
        public bool giftable { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string picture { get; set; }
        public string id { get; set; }
        public long balance { get; set; }
        public int user_level { get; set; }
        public string uid { get; set; }
        public int ranking { get; set; }    
        public bool me { get; set; }        

        public string GetName()
        {
            if (first_name.Length > 0)
                return first_name;
            else if (last_name.Length > 0)
                return last_name;
            else
                return "Unknown";
        }
    }

    public class RECEIVE : PacketData
    {
        public REData[] data { get; set; }
    }
}