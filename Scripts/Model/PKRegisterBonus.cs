using UnityEngine;
using System.Collections;

/// <summary>
/// ex) https://apps.facebook.com/slotica_slots_test/?share_id=C7JSK0X7rm&fb_ref=share_mega_win
/// 게임시상 big, maga, jackpot 시에  facebook share 링크 정보를 얻는 방법이다.
/// </summary>
namespace PK.RegisterBonus
{
    public class SendData
    {
        public int game_id { get; set; }
        public int win_id { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, int gameId, int winId)
        {
            this.cmd = cmd;
            this.data = new SendData();
            this.data.game_id = gameId;
            this.data.win_id = winId;
        }
    }


    public class REData
    {
        public string bonus_id { get; set; }
        public string game_id { get; set; }
        public long bonus { get; set; }
        public string bonus_code { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
