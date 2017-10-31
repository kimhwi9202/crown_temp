using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 'spin','coin','share_b','share_m','share_j','invite','system','free_spin','promotion','invitee_first','invitee_levelup'
public class InBoxDataHelper : MonoBehaviour {

    public const string GIFT_SYSTEM = "system";
    public const string GIFT_COIN = "coin";
    public const string GIFT_SPIN = "spin";
    public const string GIFT_INVITE = "invite";
    public const string GIFT_FREESPIN = "free_spin";
    public const string GIFT_SHARE_J = "share_j";
    public const string GIFT_SHARE_M = "share_m";
    public const string GIFT_SHARE_B = "share_b";
    public const string GIFT_PROMOTION = "promotion";
    public const string GIFT_INVITE_FIRST = "invitee_first";
    public const string GIFT_INVITE_LEVELUP = "invitee_levelup";

    static Dictionary<string, string> _iconSpriteName = new Dictionary<string, string> {
        { GIFT_SYSTEM, "i_ibox_icon_coins"},
        { GIFT_COIN, "i_ibox_icon_coins" },
        { GIFT_SPIN, "i_ibox_icon_spin" },
        { GIFT_INVITE, "i_ibox_icon_nancy" },
        { GIFT_FREESPIN, "i_ibox_icon_spin" },
        { GIFT_SHARE_J, "i_ibox_icon_nancy" },
        { GIFT_SHARE_M, "i_ibox_icon_nancy" },
        { GIFT_SHARE_B, "i_ibox_icon_nancy" },
        { GIFT_PROMOTION, "i_ibox_icon_coins" },
        { GIFT_INVITE_FIRST, "i_ibox_icon_coins" },
        { GIFT_INVITE_LEVELUP, "i_ibox_icon_coins" }
    };


    #region 맴버함수
    /// <summary>
    /// 인박스 Notice 리스트 항목의 아이콘 이미지 이름 반환
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static Sprite GetInboxIconSprite(string type)
    {
        string iconName = "";
        try
        {
            iconName = _iconSpriteName[type];
        }
        catch (System.Exception)
        {
            iconName = _iconSpriteName["system"];
        }
        return DB.Icon.GetInboxSprite(iconName);
    }

    public static string GetMessageString(string type, string name, string message)
    {
        string ret = "";
        switch (type)
        {
            case GIFT_SHARE_J:
                ret = string.Format("Succeeded in 10 clicking!");
                break;
            case GIFT_SHARE_M:
                ret = string.Format("Succeeded in 5 clicking!");
                break;
            case GIFT_SHARE_B:
                ret = string.Format("Succeeded in 5 clicking!");
                break;
            case GIFT_INVITE:
                ret = string.Format("{0} accepted your invite!", name);
                break;
            default:
                ret = message;
                break;
        }

        return ret;
    }

    /// <summary>
    /// 잔류시간 스트링 생성
    /// </summary>
    /// <param name="totalHour">The total hour.</param>
    public static string GetLeftHour(int totalHour)
    {
        int day = totalHour / 24;
        int hour = totalHour % 24;
        string leftlHour = "";
        if (day > 0)
            leftlHour = string.Format("{0} DAY(S) {1} HOUR(S)", day, hour);
        else
            leftlHour = string.Format("{0} HOUR(S)", hour);
        return leftlHour;
    }
    /// <summary>
    /// 잔류시간 스트링 생성
    /// </summary>
    /// <param name="totalHour">The total hour.</param>
    public static string GetPromotionDay(int totalHour)
    {
        int day = totalHour / 24;
        int hour = totalHour % 24;
        string leftlHour = "";
        if (day > 0)
            leftlHour = string.Format("{0} DAY(S)", day);
        else
            leftlHour = "";
        return leftlHour;
    }
    public static string GetPromotionHour(int totalHour)
    {
        int day = totalHour / 24;
        int hour = totalHour % 24;
        string leftlHour = "";
        leftlHour = string.Format("{0} HOUR(S)", hour);
        return leftlHour;
    }

    /// <summary>
    /// 보상내용 스트링 반환
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="amount">The amount.</param>
    /// <returns></returns>
    public static string GetAmountString(string type, long amount)
    {
        string ret = "";
        switch (type)
        {
            case GIFT_SYSTEM:
                ret = string.Format("Sent you {0} Coins.", amount.ToString("#,#0"));
                break;
            case GIFT_COIN:
                ret = string.Format("Sent you {0} Coins.", amount.ToString("#,#0"));
                break;
            case GIFT_SPIN:
                ret = string.Format("Sent you {0} Bonus Spins.", amount.ToString("#,#0"));
                break;
            case GIFT_INVITE:
                ret = string.Format("Invitation rewards {0} Coins.", amount.ToString("#,#0"));
                break;
            case GIFT_FREESPIN:
                ret = string.Format("Sent you Free Spins!");
                break;
            case GIFT_SHARE_J:
                ret = string.Format("Jackpot rewards {0} coins.", amount.ToString("#,#0"));
                break;
            case GIFT_SHARE_M:
                ret = string.Format("Mega Win rewards {0} coins.", amount.ToString("#,#0"));
                break;
            case GIFT_SHARE_B:
                ret = string.Format("Big Win rewards {0} coins.", amount.ToString("#,#0"));
                break;
            default:
                ret = string.Format("Sent you {0} Coins.", amount.ToString("#,#0"));
                break;
        }
        return ret;
    }

    #endregion
}
