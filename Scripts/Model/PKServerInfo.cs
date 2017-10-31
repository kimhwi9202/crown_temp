using UnityEngine;
using System.Collections;

namespace PK.ServerInfo
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
        public string server_name { get; set; }
        public string server_address { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData[] data { get; set; }
    }
}
