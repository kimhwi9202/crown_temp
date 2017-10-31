using UnityEngine;
using System.Collections;

namespace PK.News
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
        public string image_url { get; set; }
        public string type { get; set; }
        public string popup_value { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData[] data { get; set; }
    }
}
