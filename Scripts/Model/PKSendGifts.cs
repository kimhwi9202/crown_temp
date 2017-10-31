﻿using UnityEngine;
using System.Collections;

namespace PK.SendGifts
{
    public class REData
    {
        public string status { get; set; }
        public string type { get; set; }
        public string[] to { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }



    public class SendData
    {
        public string type { get; set; }
        public string[] to { get; set; }
    }

    public class SEND
    {
        public SendData data { get; set; }
        public string cmd { get; set; }
        public SEND(string cmd, SendData data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }
}