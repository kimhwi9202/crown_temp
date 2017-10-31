using UnityEngine;
using System.Collections;

using xLIB;

/// <summary>
/// PlayerPrefs 헬퍼클래스
/// </summary>
public class PlayerPrefHelper {


    #region PlayerPrefs key define
    const string PREF_KEY_LOGIN_TYPE = "_loginType";
    const string PREF_KEY_USER_ID = "_userID";

    const string PREF_KEY_FBLOGIN_INFO = "facebook_login_info";
    const string PREF_KEY_FBLOGIN_ACCESS_TOKEN = "facebook_access_token";

    const string PREF_KEY_FBFRIENDS_INFO = "facebook_Friends_info";
    const string PREF_KEY_FBINVITABLEFRIENDS_INFO = "facebook_InvitableFriends_info";

    const string PREF_KEY_FIRST_LOBBY = "firstLobby";

    const string PREF_KEY_SETTING_SOUND = "setting_sound";
    const string PREF_KEY_SETTING_NOTI = "setting_notification";
    #endregion  // PlayerPrefs key define


    #region eLoginType 
    /// <summary>
    /// 로그인타입 반환 (none, facebook, guest)
    /// PlayerPrefs에 저장된 값을 반환한다.
    /// </summary>
    /// <returns></returns>
    static public eLoginType GetLoginType()
    {
        //eLoginType loginType = (eLoginType)System.Enum.Parse(typeof(eLoginType), PlayerPrefs.GetString(PREF_KEY_LOGIN_TYPE, eLoginType.none.ToString()));
        eLoginType loginType = (eLoginType)System.Enum.Parse(typeof(eLoginType), xEncryptPlayerPrefs.GetString(PREF_KEY_LOGIN_TYPE, eLoginType.none.ToString()));
        return loginType;
    }

    /// <summary>
    /// 로그인타입 저장
    /// </summary>
    /// <param name="loginType">로그인타입  (none, facebook, guest)</param>
    static public void SetLoginType(eLoginType loginType)
    {
        //PlayerPrefs.SetString(PREF_KEY_LOGIN_TYPE, loginType.ToString());
        xEncryptPlayerPrefs.SetString(PREF_KEY_LOGIN_TYPE, loginType.ToString());
    }
    #endregion // eLoginType 

    #region User ID

    /// <summary>
    /// 사용자 고유아이디 반환 (로그인용)
    /// </summary>
    /// <returns>사용자아이디</returns>
    static public string GetUserID()
    {
        //return PlayerPrefs.GetString(PREF_KEY_USER_ID, "");
        return xEncryptPlayerPrefs.GetString(PREF_KEY_USER_ID, "");
    }

    /// <summary>
    /// 사용자 고유아이디 저장 (로그인용 : facebook => 페이스북 사용자아이디, guest => 단말기 고유키)
    /// </summary>
    /// <param name="userID">사용자 아이디</param>
    static public void SetUserID(string userID)
    {
        //PlayerPrefs.SetString(PREF_KEY_USER_ID, userID);
        xEncryptPlayerPrefs.SetString(PREF_KEY_USER_ID, userID);
    }

    #endregion // User ID

    #region Facebook info
#if UNITY_EDITOR
    /// <summary>
    /// 페이스북 로그인 사용자 json파일정보 (개발시 직접 로그인이 필요해서 만듬) 
    /// 개발버젼에서만 사용할것
    /// </summary>
    static public void SetFBLoginResult(string result)
    {
        xEncryptPlayerPrefs.SetString(PREF_KEY_FBLOGIN_INFO, result);
    }
    static public string GetFBLoginResult()
    {
        return xEncryptPlayerPrefs.GetString(PREF_KEY_FBLOGIN_INFO, "");
    }
    static public void SetFBFriendsResult(string result)
    {
        xEncryptPlayerPrefs.SetString(PREF_KEY_FBFRIENDS_INFO, result);
    }
    static public string GetFBFriendsResult()
    {
        return xEncryptPlayerPrefs.GetString(PREF_KEY_FBFRIENDS_INFO, "");
    }
    static public void SetFBInvitableFriendsResult(string result)
    {
        xEncryptPlayerPrefs.SetString(PREF_KEY_FBINVITABLEFRIENDS_INFO, result);
    }
    static public string GetFBInvitableFriendsResult()
    {
        return xEncryptPlayerPrefs.GetString(PREF_KEY_FBINVITABLEFRIENDS_INFO, "");
    }
#endif
    /// <summary>
    /// 페이스북 access_token 저장
    /// </summary>
    /// <param name="strAccessToken">The string access token.</param>
    static public void SetFbAccessToken(string strAccessToken)
    {
        //PlayerPrefs.SetString(PREF_KEY_FBLOGIN_ACCESS_TOKEN, strAccessToken);
        xEncryptPlayerPrefs.SetString(PREF_KEY_FBLOGIN_ACCESS_TOKEN, strAccessToken);
    }

    /// <summary>
    /// 페이스북 access_token 반환
    /// </summary>
    /// <returns></returns>
    static public string GetFbAccessToken()
    {
        //return PlayerPrefs.GetString(PREF_KEY_FBLOGIN_ACCESS_TOKEN, "");
        return xEncryptPlayerPrefs.GetString(PREF_KEY_FBLOGIN_ACCESS_TOKEN, "");
    }
    #endregion  // Facebook info

    /// <summary>
    /// 사용자가 처음으로 로비에 진입한 것인지 여부 설정
    /// </summary>
    /// <param name="bFirst">if set to <c>true</c> [b first].</param>
    static public void SetFirstLobby(bool bFirst)
    {
        //PlayerPrefs.SetInt(PREF_KEY_FIRST_LOBBY, (bFirst == true) ? 1 : 0);
        xEncryptPlayerPrefs.SetInt(PREF_KEY_FIRST_LOBBY, (bFirst == true) ? 1 : 0);
    }

    /// <summary>
    /// 사용자가 처음으로 로비에 진입한 것인지 여부 반환
    /// </summary>
    /// <returns></returns>
    static public bool IsFirstLobby()
    {
        //return PlayerPrefs.GetInt(PREF_KEY_FIRST_LOBBY, 1) == 1 ? true : false;
        return xEncryptPlayerPrefs.GetInt(PREF_KEY_FIRST_LOBBY, 1) == 1 ? true : false;
    }

    #region Settings 설정
    //const string PREF_KEY_SETTING_SOUND = "setting_sound";
    //const string PREF_KEY_SETTING_NOTI = "setting_notification";
    /// <summary>
    /// Settings 에서 사운드 On/Off 설정
    /// </summary>
    /// <param name="bSet">if set to <c>true</c> [b set].</param>
    static public void SetSoundOn(bool bSet)
    {
        //PlayerPrefs.SetInt(PREF_KEY_SETTING_SOUND, (bSet) ? 1 : 0);
        xEncryptPlayerPrefs.SetInt(PREF_KEY_SETTING_SOUND, (bSet) ? 1 : 0);
    }

    /// <summary>
    /// Settings 에서 사운드 On/Off 반환
    /// </summary>
    /// <returns></returns>
    static public bool GetSoundOn()
    {
        //return PlayerPrefs.GetInt(PREF_KEY_SETTING_SOUND, 1) == 1 ? true : false;
        return xEncryptPlayerPrefs.GetInt(PREF_KEY_SETTING_SOUND, 1) == 1 ? true : false;
    }

    /// <summary>
    /// Settings에서 Notification On/Off 설정
    /// </summary>
    /// <param name="bSet">if set to <c>true</c> [b set].</param>
    static public void SetNofiOn(bool bSet)
    {
        //PlayerPrefs.SetInt(PREF_KEY_SETTING_NOTI, (bSet) ? 1 : 0);
        xEncryptPlayerPrefs.SetInt(PREF_KEY_SETTING_NOTI, (bSet) ? 1 : 0);
    }

    /// <summary>
    /// Settings에서 Notification On/Off 반환
    /// </summary>
    /// <returns></returns>
    static public bool GetNofiOn()
    {
        //return PlayerPrefs.GetInt(PREF_KEY_SETTING_NOTI) == 1 ? true : false;
        return xEncryptPlayerPrefs.GetInt(PREF_KEY_SETTING_NOTI) == 1 ? true : false;
    }


    #endregion  // Settings 설정
}
