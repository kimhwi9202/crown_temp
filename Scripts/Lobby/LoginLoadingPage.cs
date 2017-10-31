using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using DG.Tweening;

using xLIB;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif


/// <summary>
/// 인트로 화면의로그인 과 씬전환 및 게임로딩 페이지
/// </summary>
public class LoginLoadingPage : UIPopupBase
{
    public GameObject _objLoginButtonGroup;
    public RawImage _imgTitle;
    public Image _imgFadeInOut;
    public AniProgressBar _ProgressBar;
    public Text _textVersion;
    private bool _clickLock = false;
    private string _LobbySceneName;
    protected eGameList _GameKind;
    protected float _BeginTime;
    private bool _ProgressLock = false;
    

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);

        _textVersion.text = "Ver " + CONFIG.GetBuildVersion(); 

        m_id = id;
        eventClose = _eventClose;
        m_args = args;
        _ProgressBar.Reset();
        _objLoginButtonGroup.gameObject.SetActive(false);

        _clickLock = false;

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_args == null) return;

        _imgFadeInOut.color = Color.black;
        _imgFadeInOut.gameObject.SetActive(true);

        if ((SCENEIDs)m_args[0] == SCENEIDs.FirstLoginToLobby)
        {
            _imgFadeInOut.gameObject.SetActive(false);
            _ProgressBar.gameObject.SetActive(false);
            _LobbySceneName = m_args[1].ToString();
            StartCoroutine(LoginCheck());
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.ReLoginFacebook)
        {
            _imgFadeInOut.DOFade(0, 0.5f).OnComplete(OnFadeComplete);
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.FBSignOut)
        {
            _imgFadeInOut.DOFade(0, 0.5f).OnComplete(OnFadeComplete);
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.ReConnectServer)  // 재접속
        {
            _imgFadeInOut.DOFade(0, 0.5f).OnComplete(OnFadeComplete);
        }
        else
        {
            Close("cancel");
        }
    }
    /// <summary>
    /// 화면 페이드 인/아웃 완료 호출
    /// </summary>
    void OnFadeComplete()
    {
        _imgFadeInOut.gameObject.SetActive(false);
        if ((SCENEIDs)m_args[0] == SCENEIDs.ReLoginFacebook)
        {
            USER.I.IsGuestLogin = false;
            NET.I.AddMessage(PKID.LobbyDisconnect);
            NET.I.AddMessage(PKID.LobbyConnect);
            CommonLoginNeworkConnect();
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.FBSignOut)
        {
            _imgFadeInOut.gameObject.SetActive(false);
            _ProgressBar.gameObject.SetActive(false);
            USER.I.IsGuestLogin = false;
            _objLoginButtonGroup.gameObject.SetActive(true);
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.ReConnectServer)
        {
            _imgFadeInOut.gameObject.SetActive(false);
            _ProgressBar.gameObject.SetActive(false);
            _objLoginButtonGroup.gameObject.SetActive(false);
            if (USER.I.IsGuestLogin) // guest user
            {
                AssetBundleVersionCheck();
            }
            else  // fb user
            {
                StartCoroutine(LoginCheck());
            }
        }
    }

    #region 페이스북 & 게스트 로그인
    IEnumerator LoginCheck()
    {
        yield return new WaitForEndOfFrame();

        // 페이스북 로그인 인증 상태인지 채크
        Main.FB.Initialize((isInit, isLogged, msg) => 
        {
            if (isInit)  // 페이스북 초기화 완료
            {
                if (isLogged)  // 로그인 인증된 상태 - 0 
                {
                    Main.FB.LoadUserInfo((x) => 
                    {
                        if (x == true)
                        {
                            AssetBundleVersionCheck();
                        }
                        else
                        {
                            GuestLoinCheck();
                        }
                    });
                }
                else if (isInit)  // 페이스북 초기화
                {
                    GuestLoinCheck();
                }
            }
            else // 페이스북 초기화가 실패한경우다..뒤처리 필요
            {
                GuestLoinCheck();
            }
        });
    }

    // 게스트 자동 로그인 가능 여부 채크한다
    void GuestLoinCheck()
    {
        USER.I._TempGuestID = PlayerPrefHelper.GetUserID();
        if (!string.IsNullOrEmpty(USER.I._TempGuestID))
        {
            USER.I.IsGuestLogin = true;
            _objLoginButtonGroup.gameObject.SetActive(false);
            AssetBundleVersionCheck();
        }
        else
        {
            // 로그인 기록이 없다.. 로그인 버튼 활성
            _objLoginButtonGroup.gameObject.SetActive(true);
            _clickLock = false;
        }
    }

    /// <summary>
    /// 페이스북 로그인 버튼 클릭
    /// </summary>
    public void click_FBLogin()
    {
        SOUND.I.Play(DEF.SND.common_click);

        if (_clickLock) return;
        _clickLock = true;

        USER.I.IsGuestLogin = false;
        Main.FB.FBLogin((x)=>
        {
            _objLoginButtonGroup.gameObject.SetActive(false);
            if (x==true)
            {
                AssetBundleVersionCheck();
            }
            else  // 로그인 취소했다.. 로그인 상태 초기화 
            {
                _objLoginButtonGroup.gameObject.SetActive(true);
                _clickLock = false;
            }
        });
    }

    /// <summary>
    /// 게스트 로그인 버튼 클릭
    /// </summary>
    public void click_GuestLogin()
    {
        SOUND.I.Play(DEF.SND.common_click);

        if (_clickLock) return;
        _clickLock = true;

        USER.I.IsGuestLogin = true;
        _objLoginButtonGroup.gameObject.SetActive(false);
        AssetBundleVersionCheck();
    }
    #endregion

    /// <summary>
    /// 네트워크 패킷 순차적으로 받은 다음 호출된다. ( 페북 아이디 전환시에도 호출 )
    /// </summary>
    public void SceneLoadLobby()
    {
        if (_ProgressBar.gameObject.activeSelf == false)
        {
            _ProgressBar.gameObject.SetActive(true);
        }

        if ((SCENEIDs)m_args[0] == SCENEIDs.ReLoginFacebook ||
            (SCENEIDs)m_args[0] == SCENEIDs.FBSignOut ||
            (SCENEIDs)m_args[0] == SCENEIDs.ReConnectServer)
        {
            StartCoroutine(coResetLobby());
        }
        else if ((SCENEIDs)m_args[0] == SCENEIDs.FirstLoginToLobby)
        {
            _BeginTime = Time.time;
            // 로비씬에서 바로 작업하기 위해 필요
#if UNITY_EDITOR
            if (SCENE.I.IsCurrentActiveSceneName("1.Lobby"))
            {
                Close("x");
                return;
            }
#endif
            StartCoroutine(LoadScene(_LobbySceneName));
            Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.LOBBY_LOADING, "true");
        }
    }

    // 게스트에서 페이스북 전환후 패킷 받고 마지막 화면 처리
    IEnumerator coResetLobby()
    {
        yield return new WaitForEndOfFrame();
        _ProgressBar.AutoGague(0.5f, () => {
            SCENE.I.AddMessage(SCENEIDs.GuestToFBActiveLobby);
            Close("ok");
        });
    }

    #region Load Lobby Scene
    private IEnumerator LoadScene(string loadSceneName)
    {
#if UNITY_5
        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
#else
        AsyncOperation op = Application.LoadLevelAsync(loadSceneName);
#endif
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            // Report progress etc.
            //Debug.Log("scene progress = " + op.progress);
            //OnBundleUpdateProgress(op.progress);
            yield return null;
        }

        // 씬 로드가 너무 빠를경우 프로그래바의 값이 0.9 이하일경우 일정타임동안 프로그래스바 채워지는 액션이
        // 발동하도록 하기 위해 필요하다.
        if ((Time.time - _BeginTime) < 1.0f)
        {
            Debug.Log((Time.time - _BeginTime));
            _ProgressBar.AutoAddGague(2f, () =>
            {
                _ProgressBar.SetFillAmount(1.0f);
                op.allowSceneActivation = true;
                Close("ok");
            });
        }
        // 게임 프리팹 로드후 처리는 아직 미정
        else
        {
            _ProgressBar.SetFillAmount(1.0f);
            op.allowSceneActivation = true;
            Close("ok");
        }
    }
    #endregion


    #region Game Bundle Load
    /// <summary>
    /// 로그인후 로비씬 이전에 번틀 버젼채크를 하며, 완료후 로비씬 로딩이 이루어진다.
    /// </summary>
    private void AssetBundleVersionCheck()
    {
        _ProgressBar.gameObject.SetActive(true);
        _ProgressLock = false;
        _BeginTime = Time.time;

        if ((SCENEIDs)m_args[0] == SCENEIDs.FBSignOut ||
            (SCENEIDs)m_args[0] == SCENEIDs.ReConnectServer)
        {
            NET.I.AddMessage(PKID.LobbyDisconnect);
            NET.I.AddMessage(PKID.LobbyConnect);
            if (USER.I.IsGuestLogin) NET.I.AddMessage(PKID.GuestUserJoin);
            CommonLoginNeworkConnect();
        }
        else
        {
            _ProgressBar.SetFillAmount(0.03f);
            _ProgressBar.AutoAddGague(60f, null);

            StartCoroutine(BUNDLE.I.BeginVersionCheck(() => {
                _BeginTime = Time.time;
                _ProgressLock = false;
                StartCoroutine(BUNDLE.I.DownloadUpdateFromServer(DEF.GetLobbyBundleName(), OnBundleUpdateProgress_Lobby, ()=> {
                    StartCoroutine(BUNDLE.I.DownloadUpdateFromServer(DEF.GetGameCommonBundleName(), OnBundleUpdateProgress_Game, ()=> {
                        string prefabGameName = DEF.GetGameBundleName(DEF.FIRST_INSTALL_GAME);
                        StartCoroutine(BUNDLE.I.DownloadUpdateFromServer(prefabGameName, OnBundleUpdateProgress_Game, FirstLoginNetworkConnect));
                    }, OnLoadError));
                }, OnLoadError));
            }, OnLoadError));
        }
    }

    private void OnLoadError(string errorMsg)
    {
        UI.I.ShowMsgBox("Faild! Downloads : " + errorMsg, (id, args) => {
            Main.I.OnQuit();
        });
    }

    /// <summary>
    /// 프로그래스바 업데이트 처리 
    /// </summary>
    private void OnBundleUpdateProgress_Lobby(float pRatio)
    {
        if (_ProgressLock == false)
        {
            // 1초 이내 1 값이 왔다면 다운 받을게 없다는 거다. 
            if (pRatio == 1f && (Time.time - _BeginTime) < 1.0f)
            {
                _ProgressLock = true;
            }
            if (_ProgressBar.GetFillAmount() >= 1.0f) _ProgressBar.SetFillAmount(1f);
            else _ProgressBar.SetAddFillAmount(pRatio * 0.6f);
        }
    }
    private void OnBundleUpdateProgress_Game(float pRatio)
    {
        if (_ProgressLock == false)
        {
            // 1초 이내 1 값이 왔다면 다운 받을게 없다는 거다. 
            if (pRatio == 1f && (Time.time - _BeginTime) < 1.0f)
            {
                _ProgressLock = true;
            }
            if (_ProgressBar.GetFillAmount() >= 1.0f) _ProgressBar.SetFillAmount(1f);
            else _ProgressBar.SetAddFillAmount(pRatio * 0.2f);
        }
    }

    private void FirstLoginNetworkConnect()
    {
        // 0.5초 이내로 끝나면 일정 타임 액션처리
        if ((Time.time - _BeginTime) < 3.0f)
        {
            _ProgressBar.AutoAddGague(0.5f, () => {
                UI.I.LoadBundle();
                // 로비씬 로딩전에 서버연결을 먼저 처리
                NET.I.AddMessage(PKID.LobbyConnect);
                if (USER.I.IsGuestLogin) NET.I.AddMessage(PKID.GuestUserJoin);
                CommonLoginNeworkConnect();
            });
        }
        else
        {
            UI.I.LoadBundle();
            // 로비씬 로딩전에 서버연결을 먼저 처리
            NET.I.AddMessage(PKID.LobbyConnect);
            if (USER.I.IsGuestLogin) NET.I.AddMessage(PKID.GuestUserJoin);
            CommonLoginNeworkConnect();
        }
    }

    private void CommonLoginNeworkConnect()
    {
        NET.I.AddMessage(PKID.LobbyLogin);
        NET.I.AddMessage(PKID.UserInfo);
        NET.I.AddMessage(PKID.ServerInfo);
        NET.I.AddMessage(PKID.News);
        NET.I.AddMessage(PKID.CheckDeal);
        NET.I.AddMessage(PKID.GetPurchaseItems);
        //NET.I.AddMessage(PKID.GetVaultShop);
        NET.I.AddMessage(PKID.GuestUserUpdate);
        NET.I.AddMessage(PKID.BonusInfo);
        NET.I.AddMessage(PKID.GamesInfo);
        NET.I.AddMessage(PKID.GiftsCount);
        NET.I.AddMessage(PKID.ListGifts);
        NET.I.AddMessage(PKID.AppFriends);
        NET.I.AddMessage(PKID.SendGiftChallengeItems);
        NET.I.AddMessage(PKID.InvitationChallengeCheck);
        // 모든 패킷을 받은후에 다음 스케줄 처리해라.
        NET.I.AddMessage(PKID.SCENE_LOAD_LOBBY);
    }

    #endregion


}

