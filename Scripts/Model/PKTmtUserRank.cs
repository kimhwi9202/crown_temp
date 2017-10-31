using UnityEngine;
using System.Collections;

namespace PK.TmtUserRank
{
    #region Receive Struct
    public class REData
    {
        public int rank_coins { get; set; }
        public int rank { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
    #endregion



    #region Send Struct 
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
    #endregion
}