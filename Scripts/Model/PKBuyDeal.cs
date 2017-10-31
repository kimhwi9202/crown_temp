using UnityEngine;
using System.Collections;


namespace PK.BuyDeal
{
    //buy_deal: deal 상품 구매시 purchase call 이후에 서버에 보냅니다.
    //- data: 항상 null
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }

    //- last_offer: 오늘 마지막 offer에 대해서는 구매이후 alert창의 메시지가 다르게 뜨도록 되어있습니다.
    //(현재는 마지막 offer는 24시간이내에 2번째 구매를 의미)
    //0 => false, 1 => true
    public class REData
    {
        public int last_offer { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
