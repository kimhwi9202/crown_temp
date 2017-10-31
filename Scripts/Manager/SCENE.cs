using UnityEngine;
using System.Collections;
using xLIB;
using Games.UI;
using Newtonsoft.Json;
using DG.Tweening;

#if UNITY_5
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// global define scene ids
/// </summary>
public enum SCENEIDs
{
    None = 100,
    Company,            // 회사명 타이틀 화면

    MsgBoxNetReConnect,
    NetReConnect,
    FirstLoginToLobby,       // 페북,게스트 로그인 및 로비씬 로딩 처리
    FirstActiveLobby,

    InitLobby,

    FBSignOut,
    FacebookShare,
    InviteFriends,
    ReConnectServer,
    GuestToFacebook,
    LoginGuestToFacebook,
    GuestToFBActiveLobby,
    ReLoginFacebook,
    LobbyToGame,        // 로비화면에서 게임 씬 전환
    GameToLobby,        // 게임화면세서 로비 씬 전환
    Quit,
    DEBUG_LOADING_COMPLETED,

    NewsDailySpin,
    NewsGoldenSpin,
    NewsFirstPurchase,
    NewsSweetOffer,
    NewsNormal,
    NewsGame,
    NewsGameRun,
    NewsInbox,
};

/// <summary>
/// 
/// </summary>
/// <seealso cref="xLIB.SingletonSchedule{SCENE}" />
public class SCENE : SingletonSchedule<SCENE>
{
    protected Scene FirstSceneInfo;

    public override void virAwake()
    {
        SetCallback_HandleMessage(ParserCommand);
    }

    public void Initialize()
    {
        FirstSceneInfo = SceneManager.GetActiveScene();
        Debug.Log("FirstSceneName = " + FirstSceneInfo.name);
    }


    public bool IsFirstSceneName(string name)
    {
        if (FirstSceneInfo.name == name) return true;
        return false;
    }
    public bool IsCurrentActiveSceneName(string name)
    {
        Scene info = SceneManager.GetActiveScene();
        if (info.name == name) return true;
        return false;
    }
    public string GetCurrentActiveSceneName()
    {
        Scene info = SceneManager.GetActiveScene();
        return info.name;
    }

    void LoadScene(string scene)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(scene);
#else
        Application.LoadLevel(scene);
#endif
    }


    /// <summary>
    /// 네트워크 연결끊김으로 재접속 여부 팝업창 처리
    /// </summary>
    public void MsgBoxNetworkError()
    {
        MsgBoxNetworkReConnect();
    }

    /// <summary>
    /// 네트워크 연결끊김으로 재접속 여부 팝업창 처리
    /// </summary>
    public void MsgBoxNetworkReConnect()
    {
        UI.SetWaitLoading(true);

        UI.I.ShowMsgBox("Would you like to reconnect the network ?", (_id, args) => {
            UI.SetWaitLoading(false);
            if (args[0].ToString() == "ok")
            {
                // 3초후에 재접속 창 다시 띄운다
                SYSTIMER.I.ReConnectAlram.SetAlramEvent(onReConnectUpdate, 3f, true);
                // 서버 연결 시도 .. 성공한다면 NetSocket.eEventIDs.SocketConnect 메세지 온다.
                NET.I.CheckNetConnect();
            }
            else // cancel
            {
                // 종료 처리
                Main.I.OnQuit();
            }
        });
    }

    // 일정시간에 컨넥트 접속테스트가 성공 못하면 다시 재접속 창을 띄운다.
    /// <summary>
    /// Ons the re connect update.
    /// </summary>
    void onReConnectUpdate()
    {
        SYSTIMER.I.ReConnectAlram.Stop();
        I.AddMessage(SCENEIDs.MsgBoxNetReConnect);
    }

    /// <summary>
    /// 서버 연결끊김으로 재접속 처리 루틴 처리한다
    /// 로비네트워크에서만 채크한다.
    /// </summary>
    public void NetworkReConnect()
    {
        // 재접속 팝업창 업데이트 종료
        SYSTIMER.I.ReConnectAlram.Stop();

        if (Main.I.CurrentView == eView.Lobby)
        {
            UI.SetWaitLoading(false);

            NET.I.removeAll();  // 스케줄 아이디 삭제
            USER.I.ResetReConnect();
            UI.I.ResetReConnect();
            UI.Coins.GameToLobby();

            if (USER.I.IsGuestLogin)    USER.I._TempGuestID = PlayerPrefHelper.GetUserID();
            NET.I.AddMessage(PKID.LobbyLogin);
            NET.I.AddMessage(PKID.UserInfo);
            NET.I.AddMessage(PKID.ServerInfo);
            NET.I.AddMessage(PKID.CheckDeal);
            NET.I.AddMessage(PKID.GetPurchaseItems);
            //NET.I.AddMessage(PKID.GetVaultShop);
            NET.I.AddMessage(PKID.BonusInfo);
            NET.I.AddMessage(PKID.GamesInfo);
            NET.I.AddMessage(PKID.GiftsCount);
            NET.I.AddMessage(PKID.ListGifts);
            NET.I.AddMessage(PKID.AppFriends);
            NET.I.AddMessage(PKID.SendGiftChallengeItems);
            NET.I.AddMessage(PKID.InvitationChallengeCheck);
        }
        else if (Main.I.CurrentView == eView.Game)
        {
            UI.SetWaitLoading(false);  // 인게임 로그인에서 해제

            NET.I.removeAll();  // 스케줄 아이디 삭제
            USER.I.ResetReConnect();
            UI.I.ResetReConnect();

            if (USER.I.IsGuestLogin) USER.I._TempGuestID = PlayerPrefHelper.GetUserID();
            NET.I.AddMessage(PKID.LobbyLogin);
            NET.I.AddMessage(PKID.UserInfo);
            NET.I.AddMessage(PKID.ServerInfo);
            NET.I.AddMessage(PKID.CheckDeal);
            NET.I.AddMessage(PKID.GetPurchaseItems);
            //NET.I.AddMessage(PKID.GetVaultShop);
            NET.I.AddMessage(PKID.BonusInfo);
            NET.I.AddMessage(PKID.GamesInfo);
            NET.I.AddMessage(PKID.GiftsCount);
            NET.I.AddMessage(PKID.ListGifts);
            NET.I.AddMessage(PKID.AppFriends);
            NET.I.AddMessage(PKID.SendGiftChallengeItems);
            NET.I.AddMessage(PKID.InvitationChallengeCheck);

            StartCoroutine(coGameReConnect());
        }
    }

    IEnumerator coGameReConnect()
    {
        yield return new WaitForSeconds(1f);
        // 현재 게임 분석
        eGameList gameKind = UI.I.GetCurrentGameKind();
        UI.Game.AddMessage(GameUI.IDs.InGameHandle, "msg", "reconnect");  // 해당게임에 재접속 해라 명령.
    }

    void ParserCommand(Hashtable has)
    {
        SCENEIDs _IDs = (SCENEIDs)has["id"].GetHashCode();

#if UNITY_EDITOR
        Debug.Log(Time.frameCount + " <Color=#fff000> SCENE::ParserCommand - " + _IDs.ToString() + " </Color>");
#endif
        switch (_IDs)
        {
            case SCENEIDs.Company:
                Main.I.ShowCompany(true);
                Main.I._logo.color = Color.white;
                Main.I._logo.DOFade(0, 1f).SetDelay(2f).OnComplete(() => {
                    remove(SCENEIDs.Company);
                });
                return;

            case SCENEIDs.MsgBoxNetReConnect:
                MsgBoxNetworkReConnect();
                break;

            case SCENEIDs.NetReConnect:
                NetworkReConnect();
                break;

            case SCENEIDs.FirstLoginToLobby:
                Main.I.ShowCompany(false);
                UI.ShowLoginLoadingPage((id, args) =>
                {
                    remove(SCENEIDs.FirstLoginToLobby);
                }, SCENEIDs.FirstLoginToLobby, has["name"].ToString());
                return;

            case SCENEIDs.FirstActiveLobby:
                //UI.I.FirstActiveLobby();
                break;

            case SCENEIDs.FBSignOut:
                {
                    SOUND.I.PlayStop(DEF.SND.lobby_bgm);
                    USER.I.ResetReConnect();
                    UI.SetWaitLoading(false);
                    UI.ShowLoginLoadingPage((id, args) =>
                    {
                        remove(SCENEIDs.FBSignOut);
                    }, SCENEIDs.FBSignOut);
                }
                return;
            
            case SCENEIDs.FacebookShare:
                break;

            case SCENEIDs.InitLobby:
                UI.I.FirstActiveLobby();
                break;

            case SCENEIDs.InviteFriends:
                Main.I.GetFBController().InviteFriends();
                break;

            case SCENEIDs.ReConnectServer:
                UI.SetWaitLoading(false);
                USER.I.ResetReConnect();
                UI.ShowLoginLoadingPage((id, args) =>
                {
                    remove(SCENEIDs.ReConnectServer);
                }, SCENEIDs.ReConnectServer);
                return;

            case SCENEIDs.GuestToFacebook:
                {
                    if (USER.I.IsGuestLogin)
                    {
                        UI.I.ShowGuestGuide((_id, args) =>
                        {
                            if (args[0].ToString() == "ok")
                            {
                                AddMessage(SCENEIDs.LoginGuestToFacebook);
                                remove(SCENEIDs.GuestToFacebook);
                            }
                            else // cancel
                            {
                                remove(SCENEIDs.GuestToFacebook);
                            }
                        });
                    }
                    else remove(SCENEIDs.GuestToFacebook);
                }
                return;

            case SCENEIDs.LoginGuestToFacebook:
                {
                    UI.SetWaitLoading(true);
                    Main.FB.FBLogin((x) =>
                    {
                        if (x == true)
                        {
                            SOUND.I.PlayStop(DEF.SND.lobby_bgm);
                            long guestId = System.Convert.ToInt64(PlayerPrefHelper.GetUserID());
                            long fbid = System.Convert.ToInt64(Main.FB.GetFbLoginInfo().id);
                            NET.I.SendReqGuestToFacebook((id, msg) =>
                            {
                                // reset data
                                USER.I.ResetReConnect();
                                I.AddMessage(SCENEIDs.ReLoginFacebook);
                            }, NET.I.OnSendReqTimerout, guestId, fbid);
                        }
                        else
                        {
                            UI.SetWaitLoading(false);
                        }
                    });
                }
                break; 

            case SCENEIDs.ReLoginFacebook:
                UI.SetWaitLoading(false);
                UI.ShowLoginLoadingPage((id, args) =>
                {
                    remove(SCENEIDs.ReLoginFacebook);
                }, SCENEIDs.ReLoginFacebook);
                return;

            case SCENEIDs.GuestToFBActiveLobby:
                UI.I.GuestToFBActiveLobby();
                break;

            case SCENEIDs.LobbyToGame:
                int gameID = System.Convert.ToInt32(has["game"].ToString());
                if (DEF.IsUseGameID((eGameList)gameID))
                {
                    if (Lobby.I != null) Lobby.I.gameObject.SetActive(false);
                    UI.ShowGameLoadingPage((id, args) =>
                    {
                        if (args[0].ToString() == "ok")
                        {
                            //UI.I.LobbyToGame((eGameList)args[1]);
                            remove(SCENEIDs.LobbyToGame);
                        }
                    }, SCENEIDs.LobbyToGame, gameID);
                    return;
                }
                break;

            case SCENEIDs.GameToLobby:
                UI.I.GameToLobby();
                break;

            case SCENEIDs.Quit:
                UI.I.ShowMsgBox("Are you sure you want to quit the game?", (id,args)=> {
                    if(args[0].ToString() == "ok")
                    {
                        Main.I.OnQuit();
                    }
                });
                break;

            case SCENEIDs.DEBUG_LOADING_COMPLETED:
                {
                    UI.GameMain = GameObject.FindObjectOfType<InGame>();
                    if (UI.GameMain.gameId == DEF.FIRST_INSTALL_GAME)
                    {
                        BUNDLE.I.UnLoadBundle(DEF.GetGameBundleName(DEF.FIRST_INSTALL_GAME));  // 로비에서 다운받기 때문에 삭제
                    }
                    UI.I.LobbyToGame(UI.GameMain.gameId);
                    UI.GameMain.Init();
                }
                break;


            // first news popups
            case SCENEIDs.NewsDailySpin:
                UI.Popup.ShowPopup<UIPopDailySpin>("DailySpin", (int)_IDs, (id, args) => {
                    if (args[0].ToString() == "x") base.removeAt(SCENEIDs.NewsGoldenSpin);  // 취소면 NewsGoldenSpin 메세지 강제 제거
                    remove(SCENEIDs.NewsDailySpin);
                }, true);
                return;
            case SCENEIDs.NewsGoldenSpin:
                UI.Popup.ShowPopup<UIPopDailySpinShop>("DailySpinShop", (int)_IDs, (id, args) => {
                    remove(SCENEIDs.NewsGoldenSpin);
                }, null);
                return;
            case SCENEIDs.NewsFirstPurchase:
                UI.Popup.ShowPopup<UIPopFirstPurchaseOffer>("FirstPurchaseOffer", (int)_IDs, (id, args) => {
                    remove(SCENEIDs.NewsFirstPurchase);
                }, true);
                return;
            case SCENEIDs.NewsSweetOffer:
                UI.Popup.ShowPopup<UIPopSweetOffer>("SweetOffer", (int)_IDs, (id, args) => {
                    if (args[0].ToString() == "buy"){
                        UI.Popup.ShowPopup<UIPopPurchaseSuccessful>("PurchaseSuccessful", 0, (id2, args2) => {
                            remove(SCENEIDs.NewsSweetOffer);
                        }, args[1]);
                    }
                    else remove(SCENEIDs.NewsSweetOffer);
                }, true);
                return;
            case SCENEIDs.NewsNormal:
                UI.Popup.ShowPopup<UIPopNewsNormal>("NewsNormal", (int)_IDs, (id, args) => {
                    remove(SCENEIDs.NewsNormal);
                }, has["url"], has["value"]);
                return;
            case SCENEIDs.NewsGame:
                UI.Popup.ShowPopup<UIPopNewsGame>("NewsGame", (int)_IDs, (id, args) => {
                    remove(SCENEIDs.NewsGame);
                    if (args[0].ToString() == "ok") AddMessage(SCENEIDs.NewsGameRun, "gameid", args[1]);
                    else AddMessage(SCENEIDs.NewsInbox);
                }, has["url"], has["value"]);
                return;
            case SCENEIDs.NewsGameRun:
                Lobby.I.RunNewsGame((eGameList)HasToInt("gameid"));
                break;
            case SCENEIDs.NewsInbox:
                //UI.Popup.ShowPopup<UIPopGift>("Gift", (int)_IDs, null, "InBox");
                UI.I.AddMessage(UI.IDs.PopGift, "tab", "InBox");
                break;

        }
        I.remove(_IDs);
    }
}
