using UnityEngine;
using System.Collections;

namespace PK.TmtNowConfig
{
    //===========================================

    #region Receive Struct
    public class REDataSubData
    {
        public int t_id { get; set; }   // 사용안함
        public int end_limit_time { get; set; } // 게임시작까지 남은시간 ( 초)
        public int game_time { get; set; }  // 게임 플레이시간 ( 단위 분 )
        public int user_cnt { get; set; }
        public int start_limit_time { get; set; }   // 게임 진입시 시작하기까지 남은 시간 (초)
        public int wait_time { get; set; }          // 게임 진입시 게임 플레이 시간 ( 분 )
        public int last_account_rank { get; set; }
        public int tournament { get; set; }
        public int multiple_after { get; set; }
        public int tmt_id { get; set; }
        public int multiple_now { get; set; }
    }
    public class REData
    {
        public REDataSubData[] subData { get; set; }
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
