using UnityEngine;
using System.Collections;

namespace PK.GetPurchaseItems
{
    //- pro_code: promotion code
    //- tag: 유저에게 보여줄 아이템 타입(non_sale, 2x_sale, 3x_sale, promotion, first, exclusive, exclusive_vip)
    public class SendData
    {
        public string pro_code { get; set; }
        public string tag { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, string tag, string promotion_code)
        {
            this.cmd = cmd;
            data = new SendData();
            data.pro_code = promotion_code;
            data.tag = tag;
        }
    }

    
    
    
    //- request: check_deal 호출한 곳 파악을 위해 서버로 전달, 서버도 동일한 값 반환
    //- remaining: deal을 offer 할수있는 경우, 남은시간을 보냅니다.deal을 offer할 수 없는 경우(횟수 초과, 구매 유무, 시간 초과 등등) null값을 반환합니다.
    //- deal_kind: user에게 띄워줘야하는 deal type("first", "exclusive", "exclusive_vip")
    public class REDataPacks
    {
        public int id { get; set; }
        public string tag { get; set; }
        public int sale_percentage { get; set; }
        public long regular_coins { get; set; }
        public int free_percentage { get; set; }
        public long coins { get; set; }
        public double price { get; set; }
        public string product_url { get; set; }
        public long level_up_bonus { get; set; }
        public string level_bonus_percentage { get; set; }
        public long regular_price { get; set; }
        public long code { get; set; }
    }

    public class REData
    {
        public string tag { get; set; }
        public int sale_type { get; set; }
        public long sale_limit_time { get; set; }
        public REDataPacks[] packs { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}