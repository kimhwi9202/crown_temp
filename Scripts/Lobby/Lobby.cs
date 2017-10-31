using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using xLIB;

public class Lobby : MonoBehaviour
{
    public static Lobby I;

    public Canvas _CanvasBackground;
    public Canvas _CanvasTopMenu;
    public Canvas _CanvasBottomMenu;
    public Canvas _CanvasList;

    public LBTopMenu _TopMenu;
    public LBBottomMenu _BottomMenu;
    public GameListView _GameListView;
    public BroadCastScroll _BroadCastScroll;
    public TopBroadCastScroll _TopBroadCastScroll;

    private int runGameId = 0;
    private bool init = false;

    void Awake()
    {
        I = this;
        _CanvasBackground.worldCamera = Main.I.MainCamera;
        _CanvasTopMenu.worldCamera = Main.I.MainCamera;
        _CanvasBottomMenu.worldCamera = Main.I.MainCamera;
        _CanvasList.worldCamera = Main.I.MainCamera;

        _CanvasBackground.GetComponent<CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        _CanvasTopMenu.GetComponent<CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        _CanvasBottomMenu.GetComponent<CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
        _CanvasList.GetComponent<CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();
    }

    void Start()
    {
        if(init == false)
        {
            init = true;
#if UNITY_EDITOR
            if (!Main.I.IsFirstMainScene && SCENE.I.IsCurrentActiveSceneName("1.Lobby"))
                return;
#endif
            SCENE.I.AddMessage(SCENEIDs.InitLobby);
        }
    }

    public void Initialize()
    {
        _TopMenu.Init();
        _BottomMenu.Init();

        if (USER.I.IsGuestLogin)
        {
        }
        else
        {
            USER.I.LoadUserPhoto();  // 유저의 프로필 사진 다운로드
        }
        USER.I.UpdateAllUserInfo();
        _GameListView.UpdateListItem();
        _BroadCastScroll.Play(true);
        _TopBroadCastScroll.Play(true);

        if (!Main.I.IsScreen43Ratio())
        {
            _TopBroadCastScroll.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250f);
        }
        else
        {
            _TopBroadCastScroll.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -9f);
        }
    }

    public void LobbyToGame()
    {
        _GameListView.LobbyToGame();
        _BroadCastScroll.Play(false);
        _TopBroadCastScroll.Play(false);
        this.gameObject.SetActive(false);
    }
    public void GameToLobby()
    {
        this.gameObject.SetActive(true);
        _BroadCastScroll.Play(true);
        _TopBroadCastScroll.Play(true);
        _GameListView.GameToLobby();
    }


    // 게임을 직접 실행한다.
    public void RunNewsGame(eGameList gameId)
    {
        UI.SetWaitLoading(true);
        if (DEF.IsUseGameID(gameId))
        {
            runGameId = (int)gameId;
            string bundleName = DEF.GetGameBundleName(gameId);
            AssetBundle bundle = xLIB.BUNDLE.I.GetBundle(bundleName);
            if (bundle != null)
            {
                SCENE.I.AddMessage(SCENEIDs.LobbyToGame, "game", runGameId);
            }
            else if (xLIB.BUNDLE.I.IsLocalFileCached(bundleName))
            {
                SCENE.I.AddMessage(SCENEIDs.LobbyToGame, "game", runGameId);
            }
            else
            {
                StartCoroutine(xLIB.BUNDLE.I.DownloadUpdateFromServer(bundleName, null, ()=> {
                    SCENE.I.AddMessage(SCENEIDs.LobbyToGame, "game", runGameId);
                }));
            }
        }
    }

}