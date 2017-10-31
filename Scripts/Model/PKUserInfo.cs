using UnityEngine;
using System.Collections;

namespace PK.UserInfo
{
    public class SEND
    {
        public string cmd { get; set; }
        public SEND(string cmd)
        {
            this.cmd = cmd;
        }
    }


    public class REDataOptions
    {
        public string LOBBY_BG { get; set; }
        public string NANCY_GIFT_FACE { get; set; }
        public string SOUNDS_SWF { get; set; }
        public string SPECAIL_OFFER_SALE3_IMAGE { get; set; }
        public string URL_GAME { get; set; }
        public string RESOURCE_URL { get; set; }
        public string RESETTLEMENT { get; set; }
        public string INVITE_REWARD { get; set; }
        public string SPECAIL_OFFER_SALE2_IMGAE { get; set; }
        public string SPECAIL_OFFER_IMAGE { get; set; }
        public string URL_IMAGE { get; set; }
    }
    public class REData
    {
        public REDataOptions options;

        public long id;
        public long balance;
        public int user_level;
        public string name;
        public string first_name;
        public string middle_name;
        public string last_name;
        public string email;
        public string gender;
        public string picture;

        public long total_purchase;
        public string locale;
        public int timezone;

        public int event_flag;
        public long losses;
        public int xpPercent;
        public double wins;
        public long welcome_back_coins;
        public int flag;
        public int hof_popup;
        public int last_weekly_purchase_enable;
        public int sale_limit_time;
        public int welcome_back_day;
        public int num_lobby_spins;
        public int game_collect_time;
        public string last_bonus_time;
        public int last_special_offer_flag;
        public int attendance_check;
        public long uid;
        public int sound_on;
        public string next_luck_boll_time;
        public int game_collect_all_enable;
        public int sale_type;    // ?
        public int attendance_check_second;
        public int bgm_on;
        public int t_step;
        public int daily_spin_enable;
        public int next_daily_spin_second;
        public int no_ad;
        public string reg_date;
        public int liked;
        public int auto_share;
        public string userID;
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }

        public long GetId()
        {
            return data.id;
        }

        public void ConvertIdToString()
        {
            data.userID = data.id.ToString();
        }

        public string GetIdString()
        {
            return data.userID;
        }

        public long GetBalance()
        {
            return data.balance;
        }

        public long Balance { get { return data.balance; } set { data.balance = value; } }

        public int GetUserLevel()
        {
            return data.user_level;
        }

        public int GetUserLevelPercent()
        {
            return data.xpPercent;
        }

        public int GetCollectAllEnabled()
        {
            return data.game_collect_all_enable;
        }

        public string GetUserPhotoURL()
        {
            return data.picture;
        }

        public long GetFacebookID()
        {
            return data.uid;
        }

        public string GetGender()
        {
            return data.gender;
        }

        public string GetEmail()
        {
            return data.email;
        }

        public string GetFirstName()
        {
            return data.first_name;
        }

        public string GetMiddleName()
        {
            return data.middle_name;
        }

        public string GetLastName()
        {
            return data.last_name;
        }

        public string GetName()
        {
            return data.name;
        }

        public string GetImageDownURL()
        {
            return data.options.URL_IMAGE;
        }
    }
}