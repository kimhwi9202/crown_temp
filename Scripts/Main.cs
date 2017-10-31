#if UNITY_EDITOR
#define LOCAL_DEBUG
#endif

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using xLIB;

/// <summary>
/// 어플 실행시 최상위 컨트롤 타워 역할을 한다.
/// 메인 오브젝트는 필히 Tag 가 Player 로 설정해야만 모든 씬에 복사해서 사용하더라도 오직 하나만 존재하게 된다.
/// 씬 로딩시에 메인 오브젝트가 있더라도 중복 실행 되지 않는다.
/// </summary>
/// 

public class Main : MainSingleton<Main> 
{
    public GameObject _objCompany = null;
    public Image _logo;
    [HideInInspector]   private FBController _FBController = null;
    [HideInInspector]   public GameObject _Game = null;

    static public FBController FB { get { return I._FBController; } set { I._FBController = value; } }
    protected eView _nowView = eView.none;
    public eView CurrentView { get{ return _nowView; } set { _nowView = value; } }
    public bool IsFirstMainScene = false;

    private int srw, srh;

    public Camera MainCamera;
    //static public Camera MainCamera { get { return I._MainCamera; } set { I._MainCamera = value; } }
    public bool _drawGUI = false;
    // http://arthyun.asuscomm.com/SloticaMobile/
    // https://d3kjdk8bsa0don.cloudfront.net/mobile/
    // slotica1!@#


    public bool IsScreen43Ratio()
    {
        if (Camera.main.aspect >= 1.6f) return false;
        return true;
    }

    public bool IsScreenLow()
    {
        if (Screen.width < 1334f) return true;
        return false;
    }
    public float GetMatchWidthOrHeight()
    {
        if (Screen.width < 1334f) return 0;
        return 1;
    }

    protected override void virAwake()
    {
        if (IsScreenLow() == false)
        {
            float size = 0.6f;
            int x = (int)(Screen.width * size);
            int y = 0;

            if (Camera.main.aspect >= 2.1)// 19.5:9
                y = (int)((int)(Screen.width * size) / 19.5) * 9;
            else if (Camera.main.aspect > 2)// 18.5:9
                y = (int)((int)(Screen.width * size) / 18.5) * 9;
            else if (Camera.main.aspect == 2)// 18:9
                y = ((int)(Screen.width * size) / 18) * 9;
            else if (Camera.main.aspect >= 1.7)// 16:9
                y = ((int)(Screen.width * size) / 16) * 9;
            else if (Camera.main.aspect > 1.6)// 5:3
                y = ((int)(Screen.width * size) / 5) * 3;
            else if (Camera.main.aspect == 1.6)// 16:10
                y = ((int)(Screen.width * size) / 16) * 10;
            else if (Camera.main.aspect >= 1.5)// 3:2
                y = ((int)(Screen.width * size) / 3) * 2;
            else// 4:3
                y = ((int)(Screen.width * size) / 4) * 3;

            Debug.Log("Screen " + Screen.width + " / " + Screen.height + " / " + x + " / " + y + " / " + Camera.main.aspect);
            Screen.SetResolution(x, y, true);
        }

        // 빌드버젼 정의 및 Config File Download 절대주소 정의
        if (CONFIG.IsRunningAndroid())
        {
#if LOCAL_DEBUG
            CONFIG.BuildSetting(new CONFIG.STVersion(1, 3, 34), "http://arthyun.asuscomm.com/SloticaMobile/");
#else
            CONFIG.BuildSetting(new CONFIG.STVersion(1, 3, 34), "https://d3kjdk8bsa0don.cloudfront.net/mobile/");
#endif
        }
        else if (CONFIG.IsRunningiOS())
        {
            CONFIG.BuildSetting(new CONFIG.STVersion(1, 0, 0), "http://arthyun.asuscomm.com/SloticaMobileHD/");
        }

#if LOCAL_DEBUG
        this.gameObject.AddComponent<FPS>();
#endif

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    protected override void virStart()
    {
        if (_objCompany) _objCompany.gameObject.SetActive(false);
        FB = gameObject.AddComponent<FBController>();  // Main이 유일하게 Object라서 여기에 컨트롤 적용

#if LOCAL_DEBUG
        string str = CONFIG.GetXMLConfigSystemInfo();
        Debug.Log(str);
#endif

        NET.I.Initialize();
        SCENE.I.Initialize();
        UI.I.Initialize();
        USER.I.Initialize(); // 페이스북 컨트롤 접근권한은 USER에 주자
        FX.I.Initialize();
        DB.I.Initialize();
        SYSTIMER.I.Initialize();
        PLATFORM.I.Initialize();
        SOUND.I.Initialize();
        // check version
        CONFIG.CheckMatchClientVersion((ok,msg)=> {
            Debug.Log(msg);
            if (ok)
            {
                StartScene();
            }
            else
            {
                UI.I.ShowMsgBox(msg, (id,args)=> {
                    Debug.Log(args[0].ToString());
                    if (args[0].ToString() == "ok")
                    {
                        PLATFORM.I.OpenURL_AppsStorePackageDownload();
                    }
                    else
                    {
                        Main.I.OnQuit();
                    }
                });
            }
        });
    }


    void StartScene()
    {
        if (SCENE.I.IsCurrentActiveSceneName("0.Start"))
        {
            I.IsFirstMainScene = true;
            SCENE.I.AddMessage(SCENEIDs.FirstLoginToLobby, "name", "1.Lobby");
            SCENE.I.AddMessage(SCENEIDs.FirstActiveLobby);
        }
        // 로비씬에서 바로 작업하기 위해 필요
#if UNITY_EDITOR
        else if (SCENE.I.IsCurrentActiveSceneName("1.Lobby"))
        {
            SCENE.I.AddMessage(SCENEIDs.FirstLoginToLobby, "name", "1.Lobby");
            SCENE.I.AddMessage(SCENEIDs.InitLobby);
        }
        // 게임씬에서 바로 시작
        else
        {
            SCENE.I.AddMessage(SCENEIDs.FirstLoginToLobby, "name", SCENE.I.GetCurrentActiveSceneName());
            SCENE.I.AddMessage(SCENEIDs.DEBUG_LOADING_COMPLETED);
        }
#endif
    }

    public FBController GetFBController() { return _FBController; }

    public void ShowCompany(bool show)
    {
        Main.I.CurrentView = eView.Login;
        if (_objCompany) _objCompany.gameObject.SetActive(show);
    }

    void Update()
    {
        if (CONFIG.IsRunningAndroid() || CONFIG.IsRunningiOS())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UI.I.ShowMsgBox("Are you sure you want to quit the game?", (id, args) => {
                    if (args[0].ToString() == "ok")
                    {
                        Main.I.OnQuit();
                    }
                });
            }
        }
    }

    public void OnQuit()
    {
        Main.I.AppsFlyerEvent(AFInAppEvents.PLAY, AFInAppEvents.APP_NORMAL_EXIT, "true");
        Application.Quit();
    }


    /// <summary>
    /// Appses the flyer event.
    /// </summary>
    public void AppsFlyerEvent_Purchase(DEF.IAPData iapInfo)
    {
        Dictionary<string, string> eventValue = new Dictionary<string, string>();

        bool bFirstPurchase = USER.I._PKUserInfo.data.total_purchase == 0 ? true : false;

        eventValue.Add(AFInAppEvents.CONTENT_ID, iapInfo.id.ToString());
        eventValue.Add(AFInAppEvents.FIRST_PAYDAY_IN_3DAYS, USER.I.bFirstPayDayin3Day.ToString());
        eventValue.Add(AFInAppEvents.FIRST_PURCHASE_ITEM, bFirstPurchase.ToString());
        eventValue.Add(AFInAppEvents.PURCHASE_ACCUMULATE_ACCOUNT, USER.I._PKUserInfo.data.total_purchase.ToString());
        eventValue.Add(AFInAppEvents.PURCHASE_COINS, iapInfo.coins.ToString());
        eventValue.Add(AFInAppEvents.PURCHASE_COUNT, "1");
        eventValue.Add(AFInAppEvents.CURRENCY, "USD");
        eventValue.Add(AFInAppEvents.REVENUE, iapInfo.price.ToString());

        AppsFlyer.trackRichEvent(AFInAppEvents.PURCHASE, eventValue);

        // 유저 업데이트 패킷을 보낼필요없이 토탈구매만 갱신처리
        // 첫구매 팝업 예외처리 플러그로도 이용하고 있다.
        USER.I._PKUserInfo.data.total_purchase += (long)iapInfo.price;
    }

    public void AppsFlyerEvent(string type, params object[] args)
    {
        Dictionary<string, string> eventValue = new Dictionary<string, string>();
        if (args != null && args.Length > 0)
        {
            for(int i=0; i<args.Length; i+=2)
            {
                eventValue.Add(args[i].ToString(), args[i+1].ToString());
            }
        }
        AppsFlyer.trackRichEvent(type, eventValue);
    }

#if LOCAL_DEBUG
    public bool CheatKey(string key)
    {
        if (key == "clear")
        {
            xLIB.BUNDLE.DeleteBundleHashPrefs();
            return true;
        }
        else if(key == "cast")
        {
#if UNITY_EDITOR
            UI.BroadCast.Test();
#endif
            return true;
        }

        return false;
    }
#endif

}

