using UnityEngine;
using System.Collections;


namespace PK.GetDailyWheelShop
{
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }




    public class REDataPacks
    {
        public int coins { get; set; }
        public string product_url { get; set; }
        public int regular_price { get; set; }
        public string level_bonus_percentage { get; set; }
        public int active { get; set; }
        public int free_percentage { get; set; }
        public string title { get; set; }
        public int level_up_bonus { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public long code { get; set; }
        public int regular_coins { get; set; }
        public int price { get; set; }
        public string tag { get; set; }
        public int sale_percentage { get; set; }
    }
    public class REData
    {
        public bool on_sale { get; set; }
        public string pm_kind { get; set; }
        public string pm_type { get; set; }
        public int user_level { get; set; }
        public int sale_type { get; set; }
        public int sale_limit_time { get; set; }
        public REDataPacks[] packs { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
