using UnityEngine;
using System.Collections;

namespace PK.Login
{
    public class SendDataScreen
    {
        public int width { get; set; }
        public int height { get; set; }
        public SendDataScreen(int nWidth, int nHeight)
        {
            width = nWidth;
            height = nHeight;
        }
    }
    public class SendData
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string ver { get; set; }
        public string gender { get; set; }
        public string picture { get; set; }
        public string last_name { get; set; }
        public string url { get; set; }
        public string signed_request { get; set; }
        public string locale { get; set; }
        public int timezone { get; set; }
        public string name { get; set; }
        public SendDataScreen screen { get; set; }

        public SendData(string strId, string strEmail, string strFirstName, string strGender,
                                string strPicture, string strLastName, string strLocale, int nTimeZone, string strName)
        {
            id = strId;
            email = strEmail;
            first_name = strFirstName;
            ver = xLIB.CONFIG.GetCurrentConfigVersion().GetVersion();
            gender = strGender;
            picture = strPicture;
            last_name = strLastName;
            url = xLIB.CONFIG.GetConfigSystemInfoToString();
            signed_request = "mobile";
            locale = strLocale;
            timezone = nTimeZone;
            name = strName;
            screen = new SendDataScreen(0, 0);
        }
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
        public REDataOptions options { get; set; }
        public int liked { get; set; }
    }
    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }
}
