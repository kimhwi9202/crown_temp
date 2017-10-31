using UnityEngine;
using System.Collections;

namespace PK.GuestUserUpdate
{
    public class SendData
    {
        public string version { get; set; }
        public string platform { get; set; }
        public string device { get; set; }
        public string maid { get; set; }
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
}