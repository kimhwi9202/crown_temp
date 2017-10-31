using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InGame : MonoBehaviour {
    static public InGame I;
    public eGameList gameId;
    public GameObject objCanvasGame;
    public Sprite[] _spritePayTable;
    [HideInInspector]
    public string ServerURL = "";

#if UNITY_EDITOR
    public bool test_soundOn = true;
    public int  test_FrameRate = 60;
#endif

    void Awake()
    {
        I = this;
    }

    public void Init()
    {
        UI.SetWaitLoading(false);

        objCanvasGame.GetComponent<Canvas>().worldCamera = Main.I.MainCamera;
        objCanvasGame.GetComponent<CanvasScaler>().matchWidthOrHeight = Main.I.GetMatchWidthOrHeight();

        PK.GamesInfo.REData info = USER.I.GetGameListInfo((int)gameId);
        if(info != null)
        {
            ServerURL = info.connection_url;
            // 페이 테이블 정보 페이지 스프라이트 세팅
            UI.PayTable.SetPageSprite(_spritePayTable);
            objCanvasGame.SendMessage("StartGameInitialize");
        }
        else
        {
            Debug.LogError("InGame::Init - Not GameID:" + gameId);
        }

#if UNITY_EDITOR
        this.gameObject.AddComponent<TestNetwork>();
        //Edit - Project Setting - Quality
        // 로 가서 vSync Count 란을 Don't Sync로 셋팅하면 코드에서 셋팅한 프레임으로 동작
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = test_FrameRate;
        SOUND.I.SetSoundOn(test_soundOn);
#endif
    }
}
