using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using xLIB;
using UnityEngine.Purchasing.Security;

/// <summary>
/// global define packet ids
/// </summary>
public class PKID
{
    public const string None = "None";
    public const string ReceivePacket = "ReceivePacket";
    public const string DelayWaitTime = "DelayWaitTime";
    public const string LobbyConnect = "LobbyConnect";
    public const string LobbyDisconnect = "LobbyDisconnect";

    public const string SCENE_LOAD_LOBBY = "SceneLoadLobby";

    public const string LobbyLogin = "login";
    public const string GuestUserUpdate = "guest_user_update";
    public const string UserInfo = "user_info";
    public const string ServerInfo = "mobile_Server_info";
    public const string GamesInfo = "mobile_games_info";    // 게임 리스트 정보
    public const string GiftsCount = "gifts_count";  // 인박스 카운터
    public const string ListGifts = "list_gifts";    // 
    public const string CollectGameBonus = "collect_game_bonus"; 
    public const string AcceptGifts = "accept_gifts";
    public const string AppFriends = "app_friends";
    public const string SendGifts = "send_gifts";
    // tournament
    public const string TournamentNowConfig = "tournament_now_config";
    public const string TournamentNowRank = "tournament_now_rank";
    public const string TournamentUserRank = "tournament_user_rank";
    public const string TournamentBeforeMyHistory = "tournament_before_my_history";
    public const string TournamentNowAccount = "tournament_now_account";
    // daily
    public const string ReqDailySpin = "reqDailySpin";
    public const string GetDailyWheelShop = "get_wheel_shop";
    public const string WheelPurchase = "wheel_purchase";
    public const string WheelPurchaseAndroid = "purchase_wheel_google";
    public const string WheelPurchaseIOS = "purchase_wheel_apple";
    // coins , deal shop
    public const string CheckDeal = "check_deal";
    public const string BuyDeal = "buy_deal";
    public const string GetPurchaseItems = "get_purchase_items";
    public const string GetUserPromotionList = "get_user_promotion_list";
    // puchase
    public const string Purchase = "purchase";
    public const string PurchaseAndroid = "do_purchase_google";
    public const string PurchaseIOS = "do_purchase_apple";
    // collect bounus 
    public const string CollectBonus = "mobile_collect_bonus";
    public const string BonusInfo = "mobile_bonus_info";
    // pig bank
    //public const string VaultInfo = "mobile_vault_info";
    //public const string GetVaultShop = "get_vault_shop";
    //public const string PurchaseVault = "mobile_purchase_vault";
    //public const string PurchaseVaultAndroid = "purchase_vault_google";
    //public const string PurchaseVaultIOS = "purchase_vault_apple";
    // wincast
    // news
    public const string News = "mobile_news_info";

    // broadcast packet
    public const string SendWinCast = "wincast";
    public const string SendWinLike = "winlike";
    public const string BroadCastConnect = "connect";
    public const string ReceiveWinCast = "wincast";
    public const string ReceiveWinLike = "winlike";
    public const string GetBroadcastReward = "get_broadcast_reward";

    // guest login
    public const string GuestUserJoin = "guest_user_join";
    public const string GuestToFacebook = "guest_to_facebook";

    public const string CheckFBLikeUS = "check_liked";

    // send gift challenge
    public const string SendGiftChallenge = "send_gift_challenge";
    public const string SendGiftChallengeItems = "send_gift_challenge_items";
    public const string SendGiftChallengeStatus = "send_gift_challenge_status";

    // invite challenge
    public const string InvitationChallengeCheck = "invitation_challenge_check";
    public const string InvitationChallengeParticipate = "invitation_challenge_participate";
    public const string InvitationChallengeStatus = "invitation_challenge_status";

    // RegisterBonus 
    public const string RegisterBonus = "register_bonus";
}

/// <summary>
/// 로비 네트워크 매니져
/// </summary>
public class NET : SingletonSchedule<NET>
{
    #region ApiTokenKey
    public const string APIKEY_CMD = "cmd";
    public const string APIKEY_RESULT = "success";
    public const string APIKEY_DATA = "data";
    #endregion

#if LOCAL_DEBUG
    protected NetSocket _NetSocket = new NetSocket(true);
    protected NetSocket _BCNetSocket = new NetSocket(true);
#else
    protected NetSocket _NetSocket = new NetSocket();
    protected NetSocket _BCNetSocket = new NetSocket();
#endif

    bool _bSelfDisconnect = false;  // 직접 접속 끊기 처리여부 플러그
    bool _IsReConnect = false;  // 재접속시 컨넥트 가능 여부 판단.

    string _currentId = "";
    public override void virAwake()
    {
        SetCallback_HandleMessage(ParserCommand);

        _NetSocket.SetCallback_EventSocket(Callback_EventSocket);

        _BCNetSocket.SetCallback_EventSocket(BC_Callback_EventSocket);
        _BCNetSocket.SetCallback_ReceivePacket(BC_Callback_ReceivePacket);
    }

    public void Initialize()
    {
        _NetSocket.InitApiTokenKey(APIKEY_CMD, APIKEY_RESULT, APIKEY_DATA);
        _NetSocket.SetTimoutTime(15);
        _BCNetSocket.InitApiTokenKey(APIKEY_CMD, APIKEY_RESULT, APIKEY_DATA);
    }



#region Protocol 
    /// <summary>
    /// 서버 접속 정보 요청
    /// </summary>
    public void SendRQLobbyLogin(Action<string,string> response, Action<string> timeout)
    {
        if(USER.I.IsGuestLogin)
        {
            PK.Login.SendData loginItem = new PK.Login.SendData(USER.I._TempGuestID, "", "guest", "", "",  "", "",  0, "guest");
            PK.Login.SEND command = new PK.Login.SEND(PKID.LobbyLogin, loginItem);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.LobbyLogin, response, timeout);
            if (!string.IsNullOrEmpty(USER.I._TempGuestID)) { PlayerPrefHelper.SetUserID(USER.I._TempGuestID); USER.I._TempGuestID = ""; }
        }
        else  // facebook login
        {
            FBLoginVO facebookLoginInfo = Main.FB.GetFbLoginInfo();
            PK.Login.SendData loginItem = new PK.Login.SendData(facebookLoginInfo.id, facebookLoginInfo.email,
                                                facebookLoginInfo.first_name, facebookLoginInfo.gender, facebookLoginInfo.picUrl,
                                                facebookLoginInfo.last_name, facebookLoginInfo.locale, facebookLoginInfo.timezone,
                                                facebookLoginInfo.name);

            PK.Login.SEND command = new PK.Login.SEND(PKID.LobbyLogin, loginItem);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.LobbyLogin, response, timeout);
        }
    }

    public void SendGuestUserUpdate()
    {
        // 디바이스 사용자 광고ID 얻기
        Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) =>
        {
            //Debug.Log("advertisingId " + advertisingId + " " + trackingEnabled + " " + error);
            PK.GuestUserUpdate.SendData data = new PK.GuestUserUpdate.SendData();

            data.version = xLIB.CONFIG.GetBuildVersion();
            data.platform = xLIB.CONFIG.CurrentPlatform.ToString();
            data.device = SystemInfo.deviceModel;
            data.maid = advertisingId;

            PK.GuestUserUpdate.SEND command = new PK.GuestUserUpdate.SEND(PKID.GuestUserUpdate, data);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), null, null, null);
        });
    }

    /// <summary>
    /// 유저정보 요청
    /// </summary>
    public void SendReqLobbyUserInfo(Action<string, string> response, Action<string> timeout)
    {
        PK.UserInfo.SEND command = new PK.UserInfo.SEND(PKID.UserInfo);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.UserInfo, response, timeout);
    }
    public void SendReqServerInfo(Action<string, string> response, Action<string> timeout)
    {
        PK.ServerInfo.SEND command = new PK.ServerInfo.SEND(PKID.ServerInfo);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.ServerInfo, response, timeout);
    }
    /// <summary>
    /// 로비에서의 게임 리스트 정보
    /// </summary>
    public void SendReqGamesInfo(Action<string,string> response, Action<string> timeout, long userId)
    {
        PK.GamesInfo.SEND command = new PK.GamesInfo.SEND(PKID.GamesInfo, userId);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GamesInfo, response, timeout);
    }
    /// <summary>
    /// InBox 카운터 
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="timeout">The timeout.</param>
    public void SendReqGiftsCount(Action<string,string> response, Action<string> timeout)
    {
        PK.GiftsCount.SEND command = new PK.GiftsCount.SEND(PKID.GiftsCount);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GiftsCount, response, timeout);
    }
    /// <summary>
    /// InBox 리스트 
    /// </summary>
    public void SendReqListGifts(Action<string,string> response, Action<string> timeout)
    {
        PK.ListGifts.SEND command = new PK.ListGifts.SEND(PKID.ListGifts);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.ListGifts, response, timeout);
    }
    /// <summary>
    /// InBox 받은 선물및 기타 아이템 받기
    /// </summary>
    public void SendReqAcceptGifts(Action<string,string> response, Action<string> timeout, long[] gift_ids)
    {
        PK.AcceptGifts.SEND command = new PK.AcceptGifts.SEND(PKID.AcceptGifts, gift_ids);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.AcceptGifts, response, timeout);
    }
    /// <summary>
    /// 친구 리스트 요청
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="timeout">The timeout.</param>
    public void SendReqAppFriends(Action<string,string> response, Action<string> timeout)
    {
        if(Main.FB._AppFriendsIDs.Count <= 0)
        {
            response(null, null);
            return;
        }
        PK.AppFriends.SEND command = new PK.AppFriends.SEND(PKID.AppFriends, Main.FB._AppFriendsIDs.ToArray());
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.AppFriends, response, timeout);
    }
    /// <summary>
    /// 친구에서 선물 기부
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="timeout">The timeout.</param>
    /// <param name="friendIDs">The friend i ds.</param>
    public void SendReqSendGifts(Action<string,string> response, Action<string> timeout, List<string> friendIDs)
    {
        PK.SendGifts.SendData data = new PK.SendGifts.SendData();
        data.type = "coin";
        data.to = friendIDs.ToArray();
        PK.SendGifts.SEND command = new PK.SendGifts.SEND(PKID.SendGifts, data);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.SendGifts, response, timeout);
    }

    /// <summary>
    /// DailySpin 결과 요청
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="timeout">The timeout.</param>
    public void SendReqDailySpin(Action<string,string> response, Action<string> timeout, int multiple)
    {
        PK.DailySpin.SEND command = new PK.DailySpin.SEND(PKID.ReqDailySpin, multiple);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.ReqDailySpin, response, timeout);
    }
    public void SendReqGetDailyWheelShop(Action<string,string> response, Action<string> timeout)
    {
        PK.GetDailyWheelShop.SEND command = new PK.GetDailyWheelShop.SEND(PKID.GetDailyWheelShop);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GetDailyWheelShop, response, timeout);
    }



    /// <summary>
    /// 토너먼트 현재상태 정보 요청
    /// </summary>
    public void SendReqTmtNowConfig(Action<string,string> response, Action<string> timeout, int tmt_id, int game_id)
    {
        PK.TmtNowConfig.SEND command = new PK.TmtNowConfig.SEND(PKID.TournamentNowConfig, tmt_id, game_id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.TournamentNowConfig, response, timeout);
    }
    public void SendReqTmtNowRank(Action<string,string> response, Action<string> timeout, int tmt_id, int game_id)
    {
        PK.TmtNowRank.SEND command = new PK.TmtNowRank.SEND(PKID.TournamentNowRank, tmt_id, game_id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.TournamentNowRank, response, timeout);
    }
    public void SendReqTmtUserRank(Action<string,string> response, Action<string> timeout, int tmt_id, int game_id)
    {
        PK.TmtUserRank.SEND command = new PK.TmtUserRank.SEND(PKID.TournamentUserRank, tmt_id, game_id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.TournamentUserRank, response, timeout);
    }
    public void SendReqTmtBeforeMyHistory(Action<string, string> response, Action<string> timeout, int tmt_id, int game_id)
    {
        PK.TmtBeforeMyHistory.SEND command = new PK.TmtBeforeMyHistory.SEND(PKID.TournamentBeforeMyHistory, tmt_id, game_id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.TournamentBeforeMyHistory, response, timeout);
    }
    public void SendReqTmtNowAccount(Action<string, string> response, Action<string> timeout, int tmt_id, int game_id)
    {
        PK.TmtNowAccount.SEND command = new PK.TmtNowAccount.SEND(PKID.TournamentNowAccount, tmt_id, game_id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.TournamentNowAccount, response, timeout);
    }

    /// <summary>
    /// coins shop , deal 
    /// </summary>
    public void SendReqCheckDeal(Action<string,string> response, Action<string> timeout, string request, int coins)
    {
        PK.CheckDeal.SEND command = new PK.CheckDeal.SEND(PKID.CheckDeal, request, coins);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.CheckDeal, response, timeout);
    }
    public void SendReqBuyDeal(Action<string,string> response, Action<string> timeout)
    {
        PK.BuyDeal.SEND command = new PK.BuyDeal.SEND(PKID.BuyDeal);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.BuyDeal, response, timeout);
    }
    public void SendReqGetPurchaseItems(Action<string,string> response, Action<string> timeout, string tag, string promotion_code)
    {
        PK.GetPurchaseItems.SEND command = new PK.GetPurchaseItems.SEND(PKID.GetPurchaseItems, tag, promotion_code);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GetPurchaseItems, response, timeout);
    }
    public void SendReqGetUserPromotionList(Action<string,string> response, Action<string> timeout)
    {
        PK.GetUserPromotionList.SEND command = new PK.GetUserPromotionList.SEND(PKID.GetUserPromotionList);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GetUserPromotionList, response, timeout);
    }


    /// <summary> 
    /// collect bonus
    /// </summary>
    public void SendReqCollectBonus(Action<string,string> response, Action<string> timeout)
    {
        PK.CollectBonus.SEND command = new PK.CollectBonus.SEND(PKID.CollectBonus, USER.I.GetUserInfo().GetId());
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.CollectBonus, response, timeout);
    }
    public void SendReqBonusInfo(Action<string,string> response, Action<string> timeout)
    {
        PK.BonusInfo.SEND command = new PK.BonusInfo.SEND(PKID.BonusInfo, USER.I.GetUserInfo().GetId());
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.BonusInfo, response, timeout);
    }

    /// <summary> 
    /// pig bank
    /// </summary>
    //public void SendReqVaultInfo(Action<string,string> response, Action<string> timeout)
    //{
    //    PK.VaultInfo.SEND command = new PK.VaultInfo.SEND(PKID.VaultInfo, USER.I.GetUserInfo().GetId());
    //    _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.VaultInfo, response, timeout);
    //}
    //public void SendReqGetVaultShop(Action<string, string> response, Action<string> timeout)
    //{
    //    PK.GetVaultShop.SEND command = new PK.GetVaultShop.SEND(PKID.GetVaultShop);
    //    _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GetVaultShop, response, timeout);
    //}


    /// <summary> 
    /// 브로드 캐스팅 Like 클리식 같이 보내면서 보상을 받아서 처리
    /// </summary>
    public void SendReqGetBroadCastReward(Action<string,string> response, Action<string> timeout, string win_type)
    {
        PK.GetBroadcastReward.SEND command = new PK.GetBroadcastReward.SEND(PKID.GetBroadcastReward, win_type);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GetBroadcastReward, response, timeout);
    }



    /// <summary> 
    /// News ( 첫 로그인 화면에서 띄워야 할 팝업창 정보 )
    /// </summary>
    public void SendReqNews(Action<string,string> response, Action<string> timeout)
    {
        PK.News.SEND command = new PK.News.SEND(PKID.News);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.News, response, timeout);
    }


    /// <summary> 
    /// 게스트 로그인
    /// </summary>
    public void SendReqGuestUserJoin(Action<string,string> response, Action<string> timeout)
    {
        CmdGuestUserJoin command = new CmdGuestUserJoin(PKID.GuestUserJoin);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GuestUserJoin, response, timeout);
    }
    public void SendReqGuestToFacebook(Action<string,string> response, Action<string> timeout, long guestId, long facebookId)
    {
        CmdGuestToFacebook command = new CmdGuestToFacebook(PKID.GuestToFacebook, guestId, facebookId);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.GuestToFacebook, response, timeout);
    }


    public void SendCheckFBLikeUS()
    {
        PKCheckFBLikeUS command = new PKCheckFBLikeUS(PKID.CheckFBLikeUS);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), null, null, null);
    }






    /// <summary>
    /// SendGift 페북 친구 선물보내기
    /// </summary>
    public void SendReqSendGiftChallenge(Action<string, string> response, Action<string> timeout, List<string> listIDs)
    {
        PK.SendGiftChallenge.SendData data = new PK.SendGiftChallenge.SendData();
        data.type = "coin";
        data.to = listIDs.ToArray();
        PK.SendGiftChallenge.SEND command = new PK.SendGiftChallenge.SEND(PKID.SendGiftChallenge, data);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.SendGiftChallenge, response, timeout);
    }
    public void SendReqSendGiftChallengeStatus(Action<string, string> response, Action<string> timeout)
    {
        PK.SendGiftChallengeStatus.SEND command = new PK.SendGiftChallengeStatus.SEND(PKID.SendGiftChallengeStatus);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.SendGiftChallengeStatus, response, timeout);
    }
    public void SendReqSendGiftChallengeItems(Action<string, string> response, Action<string> timeout)
    {
        PK.SendGiftChallengeItems.SEND command = new PK.SendGiftChallengeItems.SEND(PKID.SendGiftChallengeItems);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.SendGiftChallengeItems, response, timeout);
    }



    /// <summary>
    /// Invite 친구 초대
    /// </summary>
    public void SendReqInvitationChallengeCheck(Action<string, string> response, Action<string> timeout)
    {
        PK.InviteChallengeCheck.SEND command = new PK.InviteChallengeCheck.SEND(PKID.InvitationChallengeCheck);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.InvitationChallengeCheck, response, timeout);
    }

    public void SendReqInvitationChallengeStatus(Action<string, string> response, Action<string> timeout)
    {
        PK.InviteChallengeStatus.SEND command = new PK.InviteChallengeStatus.SEND(PKID.InvitationChallengeStatus, USER.I._PKInvitChallengeCheck.data.id);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.InvitationChallengeStatus, response, timeout);
    }
    public void SendReqInvitationChallengeParticipate(Action<string, string> response, Action<string> timeout, List<string> to)
    {
        PK.InviteChallengeParticipate.SendData data = new PK.InviteChallengeParticipate.SendData();
        data.challenge_id = USER.I._PKInvitChallengeCheck.data.id;
        data.item_id = USER.I._PKInvitChallengeCheck.data.item[USER.I._PKInvitChallengeStatus.data.index].id;
        data.request_id = "";
        data.count = to.Count;
        data.to = to.ToArray();
        PK.InviteChallengeParticipate.SEND command = new PK.InviteChallengeParticipate.SEND(PKID.InvitationChallengeParticipate, data);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.InvitationChallengeParticipate, response, timeout);
    }



    /// <summary>
    /// big,maga,jackpot fb share 시 링크 정보얻기
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="timeout">The timeout.</param>
    public void SendReqRegisterBonus(Action<string, string> response, Action<string> timeout, int gameId, int winId)
    {
        PK.RegisterBonus.SEND command = new PK.RegisterBonus.SEND(PKID.RegisterBonus, gameId, winId);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.RegisterBonus, response, timeout);
    }







    //public void TestSendReqPurchaseVault(Action<string, string> response, Action<string> timeout)
    //{
    //    PK.PurchaseVault.SendData data = new PK.PurchaseVault.SendData();
    //    //data.uid = USER.I.GetUserInfo().GetId();
    //    data.status = "completed";
    //    data.quantity = "1";
    //    data.currency = "USD";
    //    data.amount = "1.00";
    //    data.purchase_type = "vault";
    //    //data.product_url = productUrl;
    //    int prID = UnityEngine.Random.Range(1, 10000000);
    //    data.payment_id = prID;
    //    data.signed_request = "test";

    //    PK.PurchaseVault.SEND command = new PK.PurchaseVault.SEND(PKID.PurchaseVault, data);
    //    _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.PurchaseVault, response, timeout);
    //}

    //public void SendReqPurchaseVault(Action<string, string> response, Action<string> timeout, string productUrl,
    //    GooglePlayReceipt google, AppleInAppPurchaseReceipt apple)
    //{
    //    if (CONFIG.IsRunningAndroid())
    //    {
    //        PK.PurchaseVault.SendDataAndroid data = new PK.PurchaseVault.SendDataAndroid();
    //        data.status = "completed";
    //        data.currency = "USD";
    //        data.amount = 1.0;
    //        data.quantity = "1";
    //        data.purchase_type = "coin";
    //        data.product_url = productUrl;
    //        data.packageName = google.packageName;
    //        data.productId = google.productID;
    //        data.transactionID = google.transactionID;
    //        data.purchaseTime = (double)google.purchaseDate.Ticks;
    //        data.purchaseState = (int)google.purchaseState;
    //        data.purchaseToken = google.purchaseToken;

    //        PK.PurchaseVault.SENDAndroid command = new PK.PurchaseVault.SENDAndroid(PKID.PurchaseVaultAndroid, data);
    //        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.PurchaseVaultAndroid, response, timeout);
    //    }
    //    else if (CONFIG.IsRunningiOS())
    //    {
    //        PK.PurchaseVault.SendDataIOS data = new PK.PurchaseVault.SendDataIOS();
    //        data.status = "completed";
    //        data.currency = "USD";
    //        data.quantity = "1";
    //        data.product_url = productUrl;
    //        data.purchase_type = "coin";
    //        data.amount = 1.0;
    //        data.receiptdata = apple.originalTransactionIdentifier;

    //        PK.PurchaseVault.SENDIOS command = new PK.PurchaseVault.SENDIOS(PKID.PurchaseVaultIOS, data);
    //        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.PurchaseVaultIOS, response, timeout);
    //    }
    //}


    public void TestSendReqWheelPurchase(Action<string, string> response, Action<string> timeout, string productUrl)
    {
        PK.WheelPurchase.SendData data = new PK.WheelPurchase.SendData();
        data.status = "completed";
        data.quantity = "1";
        data.currency = "USD";
        data.amount = "1.00";
        data.purchase_type = "coin";
        data.product_url = productUrl;
        int prID = UnityEngine.Random.Range(1, 10000000);
        data.payment_id = prID;
        data.signed_request = "test";

        PK.WheelPurchase.SEND command = new PK.WheelPurchase.SEND(PKID.WheelPurchase, data);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.WheelPurchase, response, timeout);
    }
    public void SendReqWheelPurchase(Action<string, string> response, Action<string> timeout, string productUrl,
        GooglePlayReceipt google, AppleInAppPurchaseReceipt apple)
    {
        if (CONFIG.IsRunningAndroid())
        {
            PK.WheelPurchase.SendDataAndroid data = new PK.WheelPurchase.SendDataAndroid();
            data.status = "completed";
            data.currency = "USD";
            data.quantity = "1";
            data.amount = 1.0;
            data.purchase_type = "coin";
            data.product_url = productUrl;
            data.packageName = google.packageName;
            data.productId = google.productID;
            data.transactionID = google.transactionID;
            data.purchaseTime = (double)google.purchaseDate.Ticks;
            data.purchaseState = (int)google.purchaseState;
            data.purchaseToken = google.purchaseToken;

            PK.WheelPurchase.SENDAndroid command = new PK.WheelPurchase.SENDAndroid(PKID.WheelPurchaseAndroid, data);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.WheelPurchaseAndroid, response, timeout);
        }
        else if (CONFIG.IsRunningiOS())
        {
            PK.WheelPurchase.SendDataIOS data = new PK.WheelPurchase.SendDataIOS();
            data.status = "completed";
            data.currency = "USD";
            data.quantity = "1";
            data.product_url = productUrl;
            data.purchase_type = "coin";
            data.amount = 1.0;
            data.purchaseToken = apple.originalTransactionIdentifier;

            PK.WheelPurchase.SENDIOS command = new PK.WheelPurchase.SENDIOS(PKID.WheelPurchaseIOS, data);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.WheelPurchaseIOS, response, timeout);
        }
    }

    /// <summary>
    /// purchase product 
    /// </summary>
    public void TestSendReqPurchase(Action<string, string> response, Action<string> timeout, string productUrl)
    {
        PK.Purchase.SendData data = new PK.Purchase.SendData();
        data.status = "completed";
        data.quantity = "1";
        data.currency = "USD";
        data.amount = "1.00";
        data.purchase_type = "coin";
        data.product_url = productUrl;
        int prID = UnityEngine.Random.Range(1, 10000000);
        data.payment_id = prID;
        data.signed_request = "test";
        PK.Purchase.SEND command = new PK.Purchase.SEND(PKID.Purchase, data);
        _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.Purchase, response, timeout);
    }

    public void SendReqPurchase(Action<string, string> response, Action<string> timeout, string productUrl,
        GooglePlayReceipt google, AppleInAppPurchaseReceipt apple)
    {
        if (CONFIG.IsRunningAndroid())
        {
            PK.Purchase.SendDataAndroid data = new PK.Purchase.SendDataAndroid();
            data.status = "completed";
            data.currency = "USD";
            data.quantity = "1";
            data.amount = 1.0;
            data.purchase_type = "coin";
            data.product_url = productUrl;
            data.packageName = google.packageName;
            data.productId = google.productID;
            data.transactionID = google.transactionID;
            data.purchaseTime = (double)google.purchaseDate.Ticks;
            data.purchaseState = (int)google.purchaseState;
            data.purchaseToken = google.purchaseToken;

            PK.Purchase.SENDAndroid command = new PK.Purchase.SENDAndroid(PKID.PurchaseAndroid, data);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.PurchaseAndroid, response, timeout);
        }
        else if (CONFIG.IsRunningiOS())
        {
            PK.Purchase.SendDataIOS data = new PK.Purchase.SendDataIOS();
            data.status = "completed";
            data.currency = "USD";
            data.quantity = "1";
            data.product_url = productUrl;
            data.purchase_type = "coin";
            data.amount = 1.0;
            data.purchaseToken = apple.originalTransactionIdentifier;

            PK.Purchase.SENDIOS command = new PK.Purchase.SENDIOS(PKID.PurchaseIOS, data);
            _NetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.PurchaseIOS, response, timeout);
        }
    }



#endregion //Protocol 

    public void OnSendReqTimerout(string id)
    {
        if (xLIB.CONFIG.CurrentNetworkState != xLIB.eNetworkState.Connect) return;

        Debug.Log("<Color=#FF00FF> NET::OnSendReqTimerout > id : " + id + "</Color>");

        if(_currentId == PKID.LobbyLogin) Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.ACCOUNT_LOGIN, "false");
        else if(_currentId == PKID.UserInfo) Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.USER_DATA_LOADING, "false");


        UI.SetWaitLoading(false);
        UI.I.ShowMsgBox("Would you like to reconnect the network ?", (_id,args)=> {
            if (args[0].ToString() == "ok")
            {
                UI.SetWaitLoading(true);
                NET.I.removeAll();
                SCENE.I.AddMessage(SCENEIDs.GameToLobby);
                SCENE.I.AddMessage(SCENEIDs.ReConnectServer);
            }
        });
    }

    void Callback_EventSocket(string id, string msg)
    {
        Debug.Log("<Color=#FF00FF> NET::Callback_EventSocket > id : " + id + "</Color>");

        switch (id)
        {
            case NetSocket.eEventIDs.SocketConnect:
                I.remove(PKID.LobbyConnect);
                // 서버 재접속 테스트 OK 떨어짐 재접속 처리 할것.
                if(_IsReConnect == true)
                {
                    SCENE.I.AddMessage(SCENEIDs.NetReConnect);
                }
                _IsReConnect = false;
                CONFIG.CurrentNetworkState = eNetworkState.Connect;
                break;
            case NetSocket.eEventIDs.SocketDisconnected:
                CONFIG.CurrentNetworkState = eNetworkState.Disconnect;
                if (_bSelfDisconnect == true)
                {
                    I.remove(PKID.LobbyDisconnect);
                }
                else // 네트워크 변경으로 끊긴 경우다..재로그인 처리
                {
                    SCENE.I.AddMessage(SCENEIDs.MsgBoxNetReConnect);
                }
                break;
            case NetSocket.eEventIDs.SocketStateChanged:
                break;
            case NetSocket.eEventIDs.SocketError:
                SCENE.I.MsgBoxNetworkError();
                break;
        }
    }


    /// <summary>
    /// 서버 연결로 네트워크 정상인지 채크 ( 재접속 채크용 )
    /// </summary>
    public void CheckNetConnect()
    {
        _IsReConnect = true;  // Callback_EventSocket :: NetSocket.eEventIDs.SocketConnect 에서 구별플러그
        _NetSocket.Connect(CONFIG.GetCurrentConfigServerIP(), CONFIG.GetCurrentConfigServerPORT());
    }

    override public void virFixedUpdate()
    {
        _NetSocket.Update();
        _BCNetSocket.Update();
    }

    private void ParserCommand(Hashtable has)
    {
        _currentId = has["id"].ToString();

#if UNITY_EDITOR
        //Debug.Log(Time.frameCount + " <Color=#fff000> NET::ParserCommand - " + _currentId.ToString() + " </Color>");
#endif
        switch (_currentId)
        {
            // 로비씬 로딩 ( 팝업로딩화면이 유지된상태에서 로비씬 로딩 )
            case PKID.SCENE_LOAD_LOBBY:
                UI.loginLoadingPage.SceneLoadLobby();
                break;

            case PKID.LobbyConnect:
                _NetSocket.Connect(CONFIG.GetCurrentConfigServerIP(), CONFIG.GetCurrentConfigServerPORT());
                return;

            case PKID.LobbyDisconnect:
                _bSelfDisconnect = true;
                _BCNetSocket.Disconnect();
                _NetSocket.Disconnect();
                return;

            // 게스트 유저 아이디 정보가 레지스트리에 없다면 신규 가입 처리
            case PKID.GuestUserJoin:
                USER.I._TempGuestID = PlayerPrefHelper.GetUserID();
                if (string.IsNullOrEmpty(USER.I._TempGuestID))
                {
                    I.SendReqGuestUserJoin((id, msg) =>
                    {
                        if (!string.IsNullOrEmpty(msg))
                        {
                            PKGuestUserJoin pk = JsonConvert.DeserializeObject<PKGuestUserJoin>(msg);
                            if (pk.data.pid > 0) USER.I._TempGuestID = pk.data.pid.ToString();
                        }
                        remove(PKID.GuestUserJoin);
                    }, I.OnSendReqTimerout);
                }
                else
                {
                    remove(PKID.GuestUserJoin);
                }
                return;

            case PKID.LobbyLogin:
                I.SendRQLobbyLogin((id, msg) =>
                {
                    Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.ACCOUNT_LOGIN, "true");
                    USER.I._PKLogin = JsonConvert.DeserializeObject<PK.Login.RECEIVE>(msg);
                    remove(PKID.LobbyLogin);
                }, OnSendReqTimerout);
                return;

            case PKID.GuestUserUpdate:
                I.SendGuestUserUpdate();
                break;

            case PKID.UserInfo:
                I.SendReqLobbyUserInfo((id, msg) =>
                {
                    Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.USER_DATA_LOADING, "true");
                    USER.I.SetPKUserInfo(msg);
                    Main.I.AppsFlyerEvent(AFInAppEvents.USER,
                        AFInAppEvents.USER_LEVEL, USER.I._PKUserInfo.data.user_level.ToString(),
                        AFInAppEvents.LEVEL_UP, "false",
                        AFInAppEvents.COINS_BALANCE, USER.I._PKUserInfo.data.balance.ToString());
                    remove(PKID.UserInfo);
                }, OnSendReqTimerout);
                return;

            case PKID.ServerInfo:
                I.SendReqServerInfo((id, msg) =>
                {
                    PK.ServerInfo.RECEIVE info = JsonConvert.DeserializeObject<PK.ServerInfo.RECEIVE>(msg);
                    if(info != null)    {
                        for(int i=0; i<info.data.Length; i++){
                            if(info.data[i].server_name == "broadcast"){
                                _BCNetSocket.Connect(info.data[i].server_address);
                                I.AddMessage(PKID.BroadCastConnect);
                                break;
                            }
                        }
                    }
                    remove(PKID.ServerInfo);
                }, OnSendReqTimerout);
                return;

            case PKID.News:
                I.SendReqNews((id, msg) =>
                {
                    if (!string.IsNullOrEmpty(msg)) USER.I._PKNews = JsonConvert.DeserializeObject<PK.News.RECEIVE>(msg);
                    remove(PKID.News);
                }, I.OnSendReqTimerout);
                return;

            case PKID.CheckDeal:
                I.SendReqCheckDeal((id, msg) =>
                {
                    USER.I.SetPKCheckDeal(msg);
                    remove(PKID.CheckDeal);
                }, OnSendReqTimerout, "init", 0);
                return;

            case PKID.GetPurchaseItems:
                I.SendReqGetPurchaseItems((id, msg) =>
                {
                    USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
                    remove(PKID.GetPurchaseItems);
                }, OnSendReqTimerout, USER.I.GetDealKind().ToString(), "");
                return;

            //case PKID.GetVaultShop:
            //    I.SendReqGetVaultShop((id, msg) =>
            //    {
            //        USER.I._PKGetVaultShop = JsonConvert.DeserializeObject<PK.GetVaultShop.RECEIVE>(msg);
            //        remove(PKID.GetVaultShop);
            //    }, OnSendReqTimerout);
            //    return;

            case PKID.GamesInfo:
                I.SendReqGamesInfo((id, msg) =>
                {
                    USER.I._PKGamesInfo = JsonConvert.DeserializeObject<PK.GamesInfo.RECEIVE>(msg);
                    remove(PKID.GamesInfo);
                }, OnSendReqTimerout, USER.I._PKUserInfo.GetId());
                return;

            case PKID.GiftsCount:
                I.SendReqGiftsCount((id, msg) =>
                {
                    USER.I._PKGiftsCount = JsonConvert.DeserializeObject<PK.GiftsCount.RECEIVE>(msg);
                    remove(PKID.GiftsCount);
                }, OnSendReqTimerout);
                return;

            case PKID.ListGifts:
                I.SendReqListGifts((id, msg) =>
                {
                    USER.I.SetPKListGifts(msg);
                    remove(PKID.ListGifts);
                }, OnSendReqTimerout);
                return;

            case PKID.AppFriends:
                I.SendReqAppFriends((id, msg) =>
                {
                    if(!string.IsNullOrEmpty(msg)) USER.I.PKReciveSetAppFriends(msg);
                    remove(PKID.AppFriends);
                }, OnSendReqTimerout);
                return;

            case PKID.SendGiftChallengeItems:
                I.SendReqSendGiftChallengeItems((id, msg) =>
                {
                    USER.I._PKSendGiftChallengeItems = JsonConvert.DeserializeObject<PK.SendGiftChallengeItems.RECEIVE>(msg);
                    remove(PKID.SendGiftChallengeItems);
                }, OnSendReqTimerout);
                return;

            case PKID.InvitationChallengeCheck:
                I.SendReqInvitationChallengeCheck((id, msg) =>
                {
                    USER.I._PKInvitChallengeCheck = JsonConvert.DeserializeObject<PK.InviteChallengeCheck.RECEIVE>(msg);
                    remove(PKID.InvitationChallengeCheck);
                }, OnSendReqTimerout);
                return;

            case PKID.BroadCastConnect:
                SendBroadCastConnect();
                break;

            // 토너먼트 패킷
            case PKID.TournamentNowConfig:
                I.SendReqTmtNowConfig((id, msg) =>
                {
                    if (!string.IsNullOrEmpty(msg)) UI.Tournaments.PKNowConfig(JsonConvert.DeserializeObject<PK.TmtNowConfig.RECEIVE>(msg));
                    remove(PKID.TournamentNowConfig);
                },OnSendReqTimerout, HasToInt("tmtid"), HasToInt("gameid"));
                return;
            case PKID.TournamentNowRank:
                I.SendReqTmtNowRank((id, msg) =>
                {
                    if (!string.IsNullOrEmpty(msg)) UI.Tournaments.PKNowRank(JsonConvert.DeserializeObject<PK.TmtNowRank.RECEIVE>(msg));
                    remove(PKID.TournamentNowRank);
                }, OnSendReqTimerout, HasToInt("tmtid"), HasToInt("gameid"));
                return;
            case PKID.TournamentUserRank:
                I.SendReqTmtUserRank((id, msg) =>
                {
                    if (!string.IsNullOrEmpty(msg)) UI.Tournaments.PKUserRank(JsonConvert.DeserializeObject<PK.TmtUserRank.RECEIVE>(msg));
                    remove(PKID.TournamentUserRank);
                }, OnSendReqTimerout, HasToInt("tmtid"), HasToInt("gameid"));
                return;
            // collect bonus
            case PKID.BonusInfo:
                I.SendReqBonusInfo((id, msg) =>
                {
                    if (!string.IsNullOrEmpty(msg)) USER.I._PKBonusInfo = JsonConvert.DeserializeObject<PK.BonusInfo.RECEIVE>(msg);
                    remove(PKID.BonusInfo);
                }, OnSendReqTimerout);
                return;
        }
        remove(_currentId);
    }

    //=================================
    #region BroadCast Net Send
    public void SendBroadCastConnect()
    {
        PK.BroadCastConnect.SEND command = new PK.BroadCastConnect.SEND(PKID.BroadCastConnect, 
            USER.I.GetUserInfo().GetId(), USER.I.GetUserInfo().GetFirstName(), USER.I.GetUserInfo().GetUserPhotoURL());
        _BCNetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.BroadCastConnect, null, null);
    }
    public void SendWinCast(PK.WinCast.SendData data)
    {
        PK.WinCast.SEND command = new PK.WinCast.SEND(PKID.ReceiveWinCast, data);
        _BCNetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.ReceiveWinCast, null, null);
    }
    public void SendWinLike(long winId)
    {
        PK.WinLike.SEND command = new PK.WinLike.SEND(PKID.ReceiveWinLike, winId, USER.I.GetUserInfo().GetId());
        _BCNetSocket.SendData(xLitJson.JsonMapper.ToJson(command), PKID.ReceiveWinLike, null, null);
    }
    void BC_Callback_EventSocket(string id, string msg)
    {
        Debug.Log("<Color=#FF00FF> NET::BC_Callback_EventSocket > id : " + id + "</Color>");
        switch (id)
        {
            case NetSocket.eEventIDs.SocketConnect: break;
            case NetSocket.eEventIDs.SocketDisconnected: break;
            case NetSocket.eEventIDs.SocketStateChanged: break;
            case NetSocket.eEventIDs.SocketError: break;
        }
    }
    // 비동기로 서버에서 받는 패킷 
    void BC_Callback_ReceivePacket(string msg)
    {
        string _id = _BCNetSocket.ParserPacketID(msg);
        switch (_id)
        {
            case PKID.ReceiveWinCast:
                UI.BroadCast.AddMessage(BroadCastUI.IDs.SendWinCast, "msg", msg);
                break;
            case PKID.ReceiveWinLike:
                UI.BroadCast.AddMessage(BroadCastUI.IDs.SendWinLike, "msg", msg);
                break;
        }
    }
#endregion // BroadCast Net Send
    //=================================
}
