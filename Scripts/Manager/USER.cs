using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Newtonsoft.Json;
using System;
using xLIB;

public class STFBPicture
{
    public string url;
    public Texture2D pic;
}

/// <seealso cref="xLIB.Singleton{USER}" />
public class USER : Singleton<USER>
{
    private Texture2D _txtProfilePhoto = null;

    public bool IsGuestLogin = false;
    private eSaleType _SaleType = eSaleType.normal;
    public eSaleType SaleType { get { return _SaleType; } set { _SaleType = value; } }

    public PK.Login.RECEIVE _PKLogin = null;
    public PK.UserInfo.RECEIVE _PKUserInfo = null;
    public PK.GamesInfo.RECEIVE _PKGamesInfo = null;
    public PK.GiftsCount.RECEIVE _PKGiftsCount = null;
    public PK.ListGifts.RECEIVE _PKListGifts = null;
    public PK.AppFriends.RECEIVE _PKAppFriends = null;
    public PK.SendGiftChallengeItems.RECEIVE _PKSendGiftChallengeItems = null;
    public PK.InviteChallengeCheck.RECEIVE _PKInvitChallengeCheck = null;
    public PK.InviteChallengeStatus.RECEIVE _PKInvitChallengeStatus = null;
    // coins , deal
    public PK.CheckDeal.RECEIVE _PKCheckDeal = null;
    public PK.GetPurchaseItems.RECEIVE _PKGetPurchaseItems = null;
    public PK.GetUserPromotionList.RECEIVE _PKGetUserPromotionList = null;
    // collect bounus
    public PK.CollectBonus.RECEIVE _PKCollectBonus = null;
    public PK.BonusInfo.RECEIVE _PKBonusInfo = null;
    // pig bank
    //public PK.PurchaseVault.RECEIVE _PKPurchaseVault = null;
    //public PK.VaultInfo.RECEIVE _PKVaultInfo = null;
    //public PK.GetVaultShop.RECEIVE _PKGetVaultShop = null;
    // News 
    public PK.News.RECEIVE _PKNews = null;


    public bool bFirstPayDayin3Day = false;  // 가입후 3일 이내인가?

    public string _TempGuestID = "";

    /// <summary>
    /// Friends 데이터 (balance로 sorted)
    /// </summary>
    public List<PK.AppFriends.REData> _AppFriendsList = new List<PK.AppFriends.REData>();
    /// <summary>
    /// 페북 프로필 사진이 필요한 모든 이미지를 관리
    /// </summary>
    private static Dictionary<string, Texture2D> _dicFBPicture = new Dictionary<string, Texture2D>();
    /// <summary>
    /// 
    /// </summary>
    private static Dictionary<int, Texture2D> _dicGameIcons = new Dictionary<int, Texture2D>();

    /// <summary>
    /// 사용자의 정보 갱신용 콜백 정의
    /// </summary>
    public Action onUpdatePhoto;
    public Action onUpdateUserInfo;
    public Action onUpdateInbox;


    public int oldUserLevel = 0;


    // Use this for initialization
    public void Initialize() { }
    /// <summary>
    /// 게스트유저에서 페이스북 유저로 전환되었다.. 기존 데이터 정리
    /// </summary>
    public void ResetReConnect()
    {
        SaleType = eSaleType.normal;
        _PKLogin = null;
        _PKUserInfo = null;
        _PKGamesInfo = null;
        _PKGiftsCount = null;
        _PKListGifts = null;
        _PKAppFriends = null;
        _PKSendGiftChallengeItems = null;
        _PKInvitChallengeCheck = null;
        _PKInvitChallengeStatus = null;
        // coins , deal
        _PKCheckDeal = null;
        _PKGetPurchaseItems = null;
        _PKGetUserPromotionList = null;
        // collect bounus
        _PKCollectBonus = null;
        _PKBonusInfo = null;
        // pig bank
        //_PKPurchaseVault = null;
        //_PKVaultInfo = null;
        //_PKGetVaultShop = null;
        // News 
        _PKNews = null;
        oldUserLevel = 0;
    }

    public void UpdateAllUserInfo()
    {
        if (onUpdatePhoto != null) onUpdatePhoto();
        if (onUpdateUserInfo != null) onUpdateUserInfo();
    }
    public void LoadUserPhoto()
    {
        StartCoroutine(coLoadUserPhoto(GetUserInfo().GetUserPhotoURL()));
    }
    IEnumerator coLoadUserPhoto(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            CurProfileTexture = www.texture;
            if (onUpdatePhoto != null) onUpdatePhoto();
        }
    }
    /// <summary>
    /// Gets the user level.
    /// </summary>
    public void GetUserLevel(out int level, out float levelPercent)
    {
        level = _PKUserInfo.data.user_level;
        levelPercent = _PKUserInfo.data.xpPercent * 0.01f;
    }
    /// <summary>
    /// 사용자레벨 업데이트
    /// </summary>
    public void SetUserLevel(int level, int levelPercent)
    {
        _PKUserInfo.data.user_level = level;
        _PKUserInfo.data.xpPercent = levelPercent;
    }

    public PK.GamesInfo.REData GetGameListInfo(int gameId)
    {
        for (int i = 0; i < _PKGamesInfo.data.Length; i++)
        {
            if (_PKGamesInfo.data[i].game_id == gameId)
            {
                return _PKGamesInfo.data[i];
            }
        }
        return null;
    }


    public Texture2D CurProfileTexture { get { return _txtProfilePhoto; } set { _txtProfilePhoto = value; } }

    public PK.UserInfo.RECEIVE GetUserInfo() { return _PKUserInfo; }


    /// <summary>
    /// 나의 정보 세팅
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void SetPKUserInfo(string msg)
    {
        _PKUserInfo = JsonConvert.DeserializeObject<PK.UserInfo.RECEIVE>(msg);
        _PKUserInfo.ConvertIdToString();
        SaleType = (eSaleType)_PKUserInfo.data.sale_type;
        oldUserLevel = _PKUserInfo.data.user_level;

        System.DateTime reg_date = System.Convert.ToDateTime(_PKUserInfo.data.reg_date);
        long tick = System.DateTime.Now.Ticks - reg_date.Ticks;
        if (tick > 0)
        {
            System.DateTime n_day = new System.DateTime(tick);
            bFirstPayDayin3Day = n_day.Day > 3 ? false : true;
        }
        else bFirstPayDayin3Day = false;

        Debug.Log("========= SaleType : " + SaleType + " =====================");
    }
    // Inbox list 
    public void SetPKListGifts(string msg)
    {
        _PKListGifts = JsonConvert.DeserializeObject<PK.ListGifts.RECEIVE>(msg);
        if (onUpdateInbox != null) onUpdateInbox();
    }

    public void PKReciveSetAppFriends(string msg)
    {
        _PKAppFriends = JsonConvert.DeserializeObject<PK.AppFriends.RECEIVE>(msg);
        for (int i = 0; i < _PKAppFriends.data.Length; i++)
        {
            _PKAppFriends.data[i].me = false;
            _AppFriendsList.Add(_PKAppFriends.data[i]);
        }

        // Add my information
        PK.AppFriends.REData myInfo = new PK.AppFriends.REData();
        myInfo.balance = _PKUserInfo.GetBalance();
        myInfo.giftable = false;
        myInfo.first_name = _PKUserInfo.GetFirstName();
        myInfo.last_name = _PKUserInfo.GetLastName();
        myInfo.picture = _PKUserInfo.GetUserPhotoURL();
        myInfo.id = _PKUserInfo.data.id.ToString();
        myInfo.uid = _PKUserInfo.data.uid.ToString();
        myInfo.me = true;
        _AppFriendsList.Add(myInfo);

        _AppFriendsList.Sort((x, y) => y.balance.CompareTo(x.balance));
        for (int i = 0; i < _AppFriendsList.Count; i++)
        {
            _AppFriendsList[i].ranking = i + 1;
        }
    }

    #region 게임 아이콘 및 페이스북 사진 이미지 다운로드
    public delegate void LoadPictureCallback(Texture2D texture);
    public void AddGameIconPicture(int gameId, string url, LoadPictureCallback callback)
    {
        //Debug.Log("AddGameIconPicture id=" + gameId + "url=" + url);

        // 로드한 이미지가 있는지 확인.
        if (_dicGameIcons.ContainsKey(gameId))
        {
            Texture2D txt;
            _dicGameIcons.TryGetValue(gameId, out txt);
            if (txt != null)
            {
                if (callback != null) callback(txt);
                return;
            }
        }
        // 이미지 다운로드 등록
        StartCoroutine(LoadPictureEnumerator(url, pic =>
        {
            if (pic != null) _dicGameIcons.Add(gameId, pic);
            if (callback != null) callback(pic);
        }));
    }
    public void AddFacebookPicture(string url, LoadPictureCallback callback)
    {
        // 로드한 이미지가 있는지 확인.
        if (_dicFBPicture.ContainsKey(url))
        {
            Texture2D txt;
            _dicFBPicture.TryGetValue(url, out txt);
            if (txt != null)
            {
                if (callback != null) callback(txt);
                return;
            }
        }
        // 이미지 다운로드 등록
        // We don't have this players image yet, request it now
        //LoadPictureAPI(Util.GetPictureURL(id, 128, 128), pictureTexture =>    // url 요청후 처리.
        StartCoroutine(LoadPictureEnumerator(url, pic =>                // 기존 url정보가 있는경우.
        {
            if (pic != null) _dicFBPicture.Add(url, pic);
            if (callback != null) callback(pic);
        }));
    }

    IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback)
    {
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) callback(null);
        else callback(www.texture);
    }
    #endregion


    /// <summary>
    /// Sets the pk check deal.
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void SetPKCheckDeal(string msg)
    {
        _PKCheckDeal = JsonConvert.DeserializeObject<PK.CheckDeal.RECEIVE>(msg);
        //if (onUpdateCheckDeal != null) onUpdateCheckDeal();
        Debug.Log("=========================================================");
        Debug.Log("RQCheckDeal => request: " + _PKCheckDeal.data.request +
            ", remaining: " + _PKCheckDeal.data.remaining +
            ", deal_kind: " + _PKCheckDeal.data.deal_kind +
            ", sale_type: " + _PKCheckDeal.data.sale_type);
        Debug.Log("=========================================================");
#if UNITY_EDITOR
        // 결재 테스트 ( 강제 세팅 )
        //_PKCheckDeal.data.deal_kind = eDealKind.first.ToString();
#endif
    }

    /// <summary>
    /// coins , deal
    /// </summary>
    public bool IsDealKind(eDealKind kind)
    {
        switch (kind)
        {
            case eDealKind.first:
                return _PKUserInfo.data.total_purchase == 0 ? true : false;
                //if (_PKCheckDeal.data.deal_kind == eDealKind.first.ToString()) return true;
                //else return false;
            case eDealKind.exclusive_1:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_1.ToString()) return true;
                else return false;
            case eDealKind.exclusive_2:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_2.ToString()) return true;
                else return false;
            case eDealKind.exclusive_3:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_3.ToString()) return true;
                else return false;
            case eDealKind.exclusive_4:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_4.ToString()) return true;
                else return false;
            case eDealKind.exclusive_5:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_5.ToString()) return true;
                else return false;
            case eDealKind.exclusive_vip:
                if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_vip.ToString()) return true;
                else return false;
        }
        return false;
    }

    public eDealKind GetDealKind()
    {
        return _PKUserInfo.data.total_purchase == 0 ? eDealKind.first : eDealKind.none;
        /*
        if (_PKCheckDeal.data.deal_kind == eDealKind.first.ToString()) return eDealKind.first;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_1.ToString()) return eDealKind.exclusive_1;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_2.ToString()) return eDealKind.exclusive_2;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_3.ToString()) return eDealKind.exclusive_3;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_4.ToString()) return eDealKind.exclusive_4;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_5.ToString()) return eDealKind.exclusive_5;
        else if (_PKCheckDeal.data.deal_kind == eDealKind.exclusive_vip.ToString()) return eDealKind.exclusive_vip;
        return eDealKind.none;
        */
    }

    public eSaleType GetSaleType() { return _SaleType; }

    public string GetBuyCoinsTag()
    {
        switch (_SaleType)
        {
            case eSaleType.normal: return "non_sale";
            case eSaleType.x2: return "2x_sale";
            case eSaleType.x3: return "3x_sale";
            case eSaleType.flash: return "2x_sale";
        }
        return "non_sale";
    }

    // pig bank
    //public void SetPKPurchaseVault(string msg)
    //{
    //    _PKPurchaseVault = JsonConvert.DeserializeObject<PK.PurchaseVault.RECEIVE>(msg);
    //    _PKUserInfo.Balance = _PKPurchaseVault.data.balance;
    //    if (onUpdateUserInfo != null) onUpdateUserInfo();
    //}
}
