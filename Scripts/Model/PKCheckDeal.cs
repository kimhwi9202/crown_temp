using UnityEngine;
using System.Collections;

namespace PK.CheckDeal
{
    //- request: check_deal 호출한 곳 파악을 위해 서버로 전달, 서버도 동일한 값 반환
    //- out_of_coins: out of coins 가 발생했을 때 coinshop을 호출한 건지 판단하는 flag, 0 => false, 1=> true
    public class SendData
    {
        public string request { get; set; }
        public int out_of_coins { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, string request, int coins)
        {
            this.cmd = cmd;
            data = new SendData();
            data.request = request;
            data.out_of_coins = coins;
        }
    }



    //- request: check_deal 호출한 곳 파악을 위해 서버로 전달, 서버도 동일한 값 반환
    //- remaining: deal을 offer 할수있는 경우, 남은시간을 보냅니다.deal을 offer할 수 없는 경우(횟수 초과, 구매 유무, 시간 초과 등등) null값을 반환합니다.
    //- deal_kind: user에게 띄워줘야하는 deal type("first", "exclusive", "exclusive_vip")
    //sale_type: 0 => normal
    //sale_type: 1 => 2x sale
    //sale_type: 2 => 3x sale
    //sale_type: 3 => flash sale

    public class REData
    {
        public string request { get; set; }
        public int remaining { get; set; }
        public string deal_kind { get; set; }
        public int sale_type { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}