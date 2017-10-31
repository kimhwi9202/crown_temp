using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using xLIB;

using Games.UI;


public class UI : SingletonSchedule<UI>
{
    public enum IDs
    {
        TouchLock,
        TouchUnLock,

        RQCheckDeal,
        RQOutOfCoin,
        RQGetBuyCoins,
        RQCoinsStore,

        UpdateBuyButton,

        PopDailySpinShop,
        PopFirstPurchaseOffer,
        PopSweetOffer,
        PopCoinStore,
        PopDailySpin,
        PopGift,
        PopSetting,
        PopCoinsStore,
        PopPurchaseSuccessful,
    }

    static Canvas WaitLoading = null;
    static public LoginLoadingPage loginLoadingPage = null;// 로그인 씬, 번들 로딩 페이지
    static public GameLoadingPage UIGameLoadingPage = null;// 게임로딩 페이지
    static public UIPopMsgBox MsgBox = null;         // 메세지 박스 (ok, cancel) 버튼 존재
    //static public UIPopPigBank PigBank = null;
    static public UIPopGift Gift = null;
    static public PopupUI Popup = null;
    static public CoinsUI Coins = null;
    static public GameUI Game = null;
    static public TournamentsUI Tournaments = null;
    static public BroadCastUI BroadCast = null;
    static public PayTableUI PayTable = null;
    static public InGame GameMain = null;
    static public UIPopGuestGuide GuestGuide = null;
    static public UITutorial Tutorial = null;
    protected eGameList loadGame = eGameList.none;
    private Transform thisTrans = null;

    public override void virAwake()
    {
        thisTrans = this.gameObject.GetComponent<Transform>();
        SetCallback_HandleMessage(ParserCommand);
    }

    private T CreateUIPrefab<T>(Transform parent, string value, bool bundle=true) where T : Object
    {
        T result = null;
        GameObject go = null;

        if (bundle) go = BUNDLE.I.LoadAsset<GameObject>(value);
        else go = (GameObject)Resources.Load(value, typeof(GameObject));
        
        go = GameObject.Instantiate(go);
        go.GetComponent<Transform>().SetParent(parent);
        go.GetComponent<Canvas>().worldCamera = Main.I.MainCamera;
        go.SetActive(false);

        result = go.GetComponent<T>();

        if (result == null)
            Debug.LogWarning("################# Not Found Pefab :" + value);

        return result;
    }

    public void Initialize()
    {
        // 고정적인 빌드에 포함되는 리소스 
        WaitLoading = CreateUIPrefab<Canvas>(thisTrans, "Prefabs/WaitLoading", false);
        loginLoadingPage = CreateUIPrefab<LoginLoadingPage>(thisTrans, "Prefabs/LoginLoadingPage", false);
        MsgBox = CreateUIPrefab<UIPopMsgBox>(thisTrans, "Prefabs/MsgBox", false);
/*
        // 가변적으로 필요시 번들및 빌드에서 처리할 리소스들
        UIGameLoadingPage = CreateUIPrefab<GameLoadingPage>(thisTrans, "Prefabs/GameLoadingPage", false);
        Gift = CreateUIPrefab<UIPopGift>(thisTrans, "Prefabs/Gift", false);
        Gift.gameObject.SetActive(false);
        Tournaments = CreateUIPrefab<TournamentsUI>(thisTrans, "Prefabs/Tournaments_UI", false);
        Tournaments.gameObject.SetActive(false);
        BroadCast = CreateUIPrefab<BroadCastUI>(thisTrans, "Prefabs/BroadCast_UI", false);
        BroadCast.gameObject.SetActive(true);
        Coins = CreateUIPrefab<CoinsUI>(thisTrans, "Prefabs/Coins_UI", false);
        Coins.gameObject.SetActive(true);
        Coins.Initialize();
        GuestGuide = CreateUIPrefab<UIPopGuestGuide>(thisTrans, "Prefabs/GuestGuide", false);
        Game = CreateUIPrefab<GameUI>(thisTrans, "Prefabs/Game_UI", false);
        if (eTutorial.on == CONFIG.CurrentTutorial)
        {
            Tutorial = CreateUIPrefab<UITutorial>(thisTrans, "Prefabs/Tutorial_UI", false);
        }
*/
    }

    public void LoadBundle()
    {
#if UNITY_EDITOR
        SOUND.I.LoadLocalDirectoryAudioClipsPackage("Lobby/", "Sounds", (x) => {
            if (!SOUND.I.IsPlay(DEF.SND.lobby_bgm))
                SOUND.I.Play(DEF.SND.lobby_bgm, true);
        });
#else
        SOUND.I.LoadAssetBundleAudioClipsPackage(DEF.GetLobbyBundleName());
        if(!SOUND.I.IsPlay(DEF.SND.lobby_bgm))
            SOUND.I.Play(DEF.SND.lobby_bgm, true);

#endif  
        // 고정적인 빌드에 포함되는 리소스 
        Popup = CreateUIPrefab<PopupUI>(thisTrans, "Popup_UI");
        Popup.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        Popup.Initialize();

        // 가변적으로 필요시 번들및 빌드에서 처리할 리소스들
        UIGameLoadingPage = CreateUIPrefab<GameLoadingPage>(thisTrans, "GameLoadingPage");
        UIGameLoadingPage.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();

        Gift = CreateUIPrefab<UIPopGift>(thisTrans, "Gift");
        Gift.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        Gift.gameObject.SetActive(false);

        Tournaments = CreateUIPrefab<TournamentsUI>(thisTrans, "Tournaments_UI");
        Tournaments.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        Tournaments.gameObject.SetActive(false);

        BroadCast = CreateUIPrefab<BroadCastUI>(thisTrans, "BroadCast_UI");
        BroadCast.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        BroadCast.gameObject.SetActive(true);

        Coins = CreateUIPrefab<CoinsUI>(thisTrans, "Coins_UI");
        Coins.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        Coins.gameObject.SetActive(true);
        Coins.Initialize();

        Game = CreateUIPrefab<GameUI>(thisTrans, "Game_UI");
        Game.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();

        GuestGuide = CreateUIPrefab<UIPopGuestGuide>(thisTrans, "GuestGuide");
        GuestGuide.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();

        PayTable = CreateUIPrefab<PayTableUI>(thisTrans, "PayTable_UI");
        PayTable.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();

        if (eTutorial.on == CONFIG.CurrentTutorial)
        {
            Tutorial = CreateUIPrefab<UITutorial>(thisTrans, "Tutorial_UI");
        }
    }

    static public void SetWaitLoading(bool wait)
    {
        WaitLoading.gameObject.SetActive(wait);
    }

    public void SetTouchLock(bool _lock)
    {
        SetWaitLoading(_lock);
    }

    /// <summary>
    /// Shows the loading.
    /// </summary>
    /// <param name="eventClose">The event close.</param>
    /// <param name="args">The arguments.</param>
    static public void ShowLoginLoadingPage(UIPopupBase.delegateClose eventClose, params object[] args)
    {
        loginLoadingPage.SetParamsData(88888, eventClose, args);
        loginLoadingPage.gameObject.SetActive(true);
    }
    /// <summary>
    /// Shows the loading.
    /// </summary>
    /// <param name="eventClose">The event close.</param>
    /// <param name="args">The arguments.</param>
    static public void ShowGameLoadingPage(UIPopupBase.delegateClose eventClose, params object[] args)
    {
        UIGameLoadingPage.SetParamsData(88889, eventClose, args);
        UIGameLoadingPage.gameObject.SetActive(true);
    }
    /// <summary>
    /// 메세지 박스 전용
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void ShowMsgBox(string msg)
    {
        MsgBox.gameObject.SetActive(true);
        MsgBox.SetParamsData(99991, null, msg);
    }

    /// <summary>
    /// 메세지 박스 전용
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void ShowMsgBox(string msg, UIPopupBase.delegateClose eventClose)
    {
        MsgBox.SetParamsData(99992, eventClose, msg);
        MsgBox.gameObject.SetActive(true);
    }

    /// <summary>
    /// 게스트 유도정책 팝업
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void ShowGuestGuide(UIPopupBase.delegateClose eventClose)
    {
        GuestGuide.SetParamsData(99993, eventClose);
        GuestGuide.gameObject.SetActive(true);
    }

    /// <summary>
    /// 재접속 이전에 초기화 과정 필요한일처리
    /// </summary>
    public void ResetReConnect()
    {
        loginLoadingPage.gameObject.SetActive(false);
        UIGameLoadingPage.gameObject.SetActive(false);
        MsgBox.gameObject.SetActive(false); 
        //PigBank.gameObject.SetActive(false);
        Gift.gameObject.SetActive(false);
        Popup.AllHidePopup();
        Coins.LobbyToGame();
        //Game = null;
        //Tournaments = null;
        //BroadCast = null;
        //PayTable = null;
    }


    /// <summary>
    /// 첫 로그인후 띄워야 할 팝업들 정의
    /// </summary>
    public void GuestToFBActiveLobby()
    {
        FirstActiveLobby();
    }
    public void FirstActiveLobby()
    {
#if UNITY_EDITOR
        // test flag
        //        USER.I.GetUserInfo().data.liked = 0;
        //USER.I.GetUserInfo().data.daily_spin_enable = 1;
#endif
        if (!SOUND.I.IsPlay(DEF.SND.lobby_bgm))
            SOUND.I.Play(DEF.SND.lobby_bgm, true);

        Main.I.CurrentView = eView.Lobby;
        Lobby.I.Initialize();
        Coins.FirstAcitveLobby();

#if UNITY_EDITOR
//        return;
#endif

        if (USER.I.IsDealKind(eDealKind.first) == false)
        {
            if (USER.I._PKCheckDeal.data.remaining > 0)
                SYSTIMER.I.GetDeal().BeginRemainTime(UI.I.onUpdateDealTimer, USER.I._PKCheckDeal.data.remaining);
        }

        // News 팝업 띄우기
        //if (USER.I.GetUserInfo().data.liked == 0)
        //    SCENE.I.AddMessage(SCENEIDs.NewsLikeUS);

        if (USER.I.GetUserInfo().data.daily_spin_enable > 0)
        {
            SCENE.I.AddMessage(SCENEIDs.NewsDailySpin);
            SCENE.I.AddMessage(SCENEIDs.NewsGoldenSpin);
        }

        if (USER.I.GetDealKind() == eDealKind.first)
            SCENE.I.AddMessage(SCENEIDs.NewsFirstPurchase);
        else if (USER.I._PKCheckDeal.data.remaining > 0 && USER.I.GetSaleType() == eSaleType.normal)
            SCENE.I.AddMessage(SCENEIDs.NewsSweetOffer);

        if (USER.I._PKNews != null)
        {
            for (int i = 0; i < USER.I._PKNews.data.Length; i++)
            {
                PK.News.REData data = USER.I._PKNews.data[i];
                if (data.type == "popup")
                {
                    SCENE.I.AddMessage(SCENEIDs.NewsNormal, "url", data.image_url, "value", data.popup_value);
                }
                else if (data.type == "game_popup")
                {
                    SCENE.I.AddMessage(SCENEIDs.NewsGame, "url", data.image_url, "value", data.popup_value);
                }
            }
        }

        SCENE.I.AddMessage(SCENEIDs.NewsInbox);
    }


    public eGameList GetCurrentGameKind()
    {
        return loadGame;
    }

    /// <summary>
    /// 로비에서 게임화면으로 전환
    /// </summary>
    /// <param name="eGameName">Name of the e game.</param>
    public void LobbyToGame(eGameList eGameName)
    {
        SOUND.I.PlayStop(DEF.SND.lobby_bgm);

        loadGame = eGameName;
        Main.I.CurrentView = eView.Game;

        string prefabGameName = DEF.GetGamePrefabName(loadGame);
        GameObject obj = BUNDLE.I.LoadAsset<GameObject>(prefabGameName);
        if (obj)
        {
            GameObject go = GameObject.Instantiate(obj);
            go.name = prefabGameName;
            go.transform.SetParent(this.gameObject.transform);
            SOUND.I.LoadAssetBundleAudioClipsPackage(DEF.GetGameBundleName(loadGame));

            GameMain = go.gameObject.GetComponent<InGame>();
            GameMain.gameId = eGameName;
            GameMain.Init();

            Main.I.AppsFlyerEvent(AFInAppEvents.GAME, AFInAppEvents.GAME_SELECT, eGameName.ToString());
        }

        if (Lobby.I != null) Lobby.I.LobbyToGame();
        Coins.LobbyToGame();
        Game.LobbyToGame();
        BroadCast.LobbyToGame();
    }

    /// <summary>
    /// 게임화면에서 로비화면으로 전환
    /// </summary>
    public void GameToLobby()
    {
        if(!SOUND.I.IsPlay(DEF.SND.lobby_bgm))
            SOUND.I.Play(DEF.SND.lobby_bgm, true);

        Main.I.CurrentView = eView.Lobby;
        if (GameMain != null)
        {
            SOUND.I.RemoveClipPackage(DEF.GetGameBundleName(loadGame));
            BUNDLE.I.UnLoadBundle(DEF.GetGameBundleName(loadGame), true);
            Destroy(GameMain.gameObject);
            GameMain = null;
            loadGame = eGameList.none;
        }

        if (Lobby.I != null) Lobby.I.GameToLobby(); 

        Coins.GameToLobby();
        Game.GameToLobby();
        Tournaments.GameToLobby();
        BroadCast.GameToLobby();

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        NET.I.SendReqLobbyUserInfo((id, msg) =>
        {
            USER.I.SetPKUserInfo(msg);
            USER.I.UpdateAllUserInfo();

            Main.I.AppsFlyerEvent(AFInAppEvents.GAME, AFInAppEvents.GAME_EXIT, "true");

        }, null);
    }


    /// <summary>
    /// Coinses the store.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="promotion_code">The promotion code.</param>
    void RQCoinsStore(string tag, string promotion_code)
    {
        NET.I.SendReqGetPurchaseItems((id2, msg2) =>
        {
            USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
            base.AddMessage(IDs.PopCoinsStore);
            base.remove(IDs.RQCoinsStore);

        }, NET.I.OnSendReqTimerout, tag, promotion_code);
    }

    void UpdateCoinsButton()
    {
        // 인게임에서 호출
        if (eView.Game == Main.I.CurrentView)
        {
            Game.UpdateBuyCoinsUI();
        }
        else // 로비에서 호출
        {
            Lobby.I._TopMenu.UpdateBuyCoinsUI();
        }
    }


    /// <summary>
    /// Checks the deal.
    /// </summary>
    void RQCheckDeal()
    {
        NET.I.SendReqCheckDeal((id, msg) => 
        {
            USER.I.SetPKCheckDeal(msg);
            if (USER.I.IsDealKind(eDealKind.first) == false)
            {
                if (USER.I._PKCheckDeal.data.remaining > 0)
                    SYSTIMER.I.GetDeal().BeginRemainTime(onUpdateDealTimer, USER.I._PKCheckDeal.data.remaining);
            }
            base.remove(IDs.RQCheckDeal);
        }, NET.I.OnSendReqTimerout, "deal", 0);
    }

    void RQOutOfCoin()
    {
        Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.OUT_OF_COINS, "true");

        NET.I.SendReqGetPurchaseItems((id2, msg2) =>
        {
            USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
            if (USER.I.GetDealKind() == eDealKind.first) base.AddMessage(IDs.PopFirstPurchaseOffer);
            else base.AddMessage(IDs.PopSweetOffer);
            base.remove(IDs.RQOutOfCoin);
        }, NET.I.OnSendReqTimerout, USER.I.GetDealKind().ToString(), "");
    }

    //- pro_code: promotion code가 있으면 promotion code에 따라 세일이 적용된 금액으로 서버가 아이템을 돌려줍니다.
    //- tag: 유저에게 보여줄 아이템 타입 (non_sale, 2x_sale, 3x_sale, promotion, first, exclusive, exclusive_vip)
    void RQGetBuyCoins()
    {
        // 첫 구매자 처리 Non PU
        if (USER.I.IsDealKind(eDealKind.first) == true)
        {
            NET.I.SendReqGetPurchaseItems((id2, msg2) =>
            {
                USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
                base.AddMessage(IDs.PopFirstPurchaseOffer);
                base.remove(IDs.RQGetBuyCoins);
            }, NET.I.OnSendReqTimerout, "first", "");
        }
        else // 구매자 PU
        {
            // 딜구매타임 찬스이고 세일기간이 아닐때 띄운다.
            if (USER.I._PKCheckDeal.data.remaining > 0 && USER.I.GetSaleType() == eSaleType.normal)
            {
                NET.I.SendReqGetPurchaseItems((id2, msg2) => {
                    USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
                    base.AddMessage(IDs.PopSweetOffer);
                    base.remove(IDs.RQGetBuyCoins);
                }, NET.I.OnSendReqTimerout, USER.I.GetDealKind().ToString(), "");
            }
            else // 구매타임 찬스가 아닐때는 세일타입으로 띄운다.
            {
                NET.I.SendReqGetPurchaseItems((id2, msg2) => {
                    USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
                    base.AddMessage(IDs.PopCoinsStore);
                    base.remove(IDs.RQGetBuyCoins);
                }, NET.I.OnSendReqTimerout, USER.I.GetBuyCoinsTag().ToString(), "");
            }
        }
    }
    public void onUpdateDealTimer(int val, string time)
    {
        // 게임화면
        if (eView.Game == Main.I.CurrentView)
        {
            Game._txtDealTime.text = time;
        }
        else // 로비에서 호출
        {
            Lobby.I._TopMenu._textDealTime.text = time;
        }
        //Debug.Log("onDealUpdatetime = val:" + val + ",time:" + time);
    }

    private void ParserCommand(Hashtable has)
    {
        IDs _id = (IDs)has["id"].GetHashCode();

#if UNITY_EDITOR
        Debug.Log(Time.frameCount + " <Color=#fff000> UI::Parser - " + _id.ToString() + " </Color>");
#endif
        switch (_id)
        {
            case IDs.TouchLock: SetTouchLock(true); break;
            case IDs.TouchUnLock: SetTouchLock(false); break;

            case IDs.RQCheckDeal: RQCheckDeal(); return;
            case IDs.RQOutOfCoin: RQOutOfCoin(); return;
            case IDs.RQGetBuyCoins: RQGetBuyCoins(); return;
            case IDs.RQCoinsStore:
                RQCoinsStore(has["tag"].ToString(), has["promotion"].ToString());
                return;
            case IDs.UpdateBuyButton: UpdateCoinsButton(); break;

            case IDs.PopCoinsStore:
                SetTouchLock(false);
                Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.OPEN_SHOP, "CoinsStore");
                Coins.ShowShopCoins(true);
                break;
            case IDs.PopDailySpinShop:
                SetTouchLock(false);
                Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.OPEN_SHOP, "DailySpinShop");
                Popup.ShowPopup<UIPopDailySpinShop>("DailySpinShop", (int)_id, null, null);
                break;
            case IDs.PopFirstPurchaseOffer:
                SetTouchLock(false);
                Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.OPEN_SHOP, "FirstPurchaseOffer");
                Popup.ShowPopup<UIPopFirstPurchaseOffer>("FirstPurchaseOffer", (int)_id, null, null);
                break;
            case IDs.PopSweetOffer:
                SetTouchLock(false);
                Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.OPEN_SHOP, "SweetOffer");
                Popup.ShowPopup<UIPopSweetOffer>("SweetOffer", (int)_id, null, null);
                break;
            case IDs.PopDailySpin:
                SetTouchLock(false);
                Popup.ShowPopup<UIPopDailySpin>("DailySpin", (int)_id, null);
                break;
            case IDs.PopGift:
                if (Facebook.Unity.FB.IsLoggedIn)//로그인상태일때는 친구갱신 후 팝업//
                {
                    SetTouchLock(true);
                    Main.FB.StartCoroutine(Main.FB.LoadAppFriends(()=> {
                        SetTouchLock(false);
                        Gift.Initialize();
                        Gift.gameObject.SetActive(true);
                        Gift.SetParamsData((int)_id, null, null);
                    })); 
                }
                else
                {
                    Gift.Initialize();
                    Gift.gameObject.SetActive(true);
                    Gift.SetParamsData((int)_id, null, null);
                }
                break;
            case IDs.PopSetting:
                SetTouchLock(false);
                Popup.ShowPopup<UIPopSettings>("Settings", (int)_id, null);
                break;
            case IDs.PopPurchaseSuccessful:
                SetTouchLock(false);
                if(has["first"] != null)  Popup.ShowPopup<UIPopPurchaseSuccessful>("PurchaseSuccessful", (int)_id, null, has["coins"], has["first"]);
                else Popup.ShowPopup<UIPopPurchaseSuccessful>("PurchaseSuccessful", (int)_id, null, has["coins"]);
                break;
        }
        remove(_id);
    }

}