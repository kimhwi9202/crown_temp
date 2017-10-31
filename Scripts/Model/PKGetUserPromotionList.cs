using UnityEngine;
using System.Collections;


namespace PK.GetUserPromotionList
{
    //get_user_promotion_list: 유저가 소유한 모든 프로모션 쿠폰 리스트를 요청합니다.
    //- data: 항상 null
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }


    public class REDataList
    {
        public string pro_name { get; set; }
        public string pro_code { get; set; }
        public long end_time { get; set; }
    }

    public class REData
    {
        public REDataList[] data { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
