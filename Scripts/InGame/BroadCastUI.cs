using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using xLIB;

public class BroadCastUI : Schedule
{
    public enum IDs
    {
        None = 100,
        SendWinCast,
        SendWinLike,
    };

    public enum eViewIDs    {
        None, MagaWin, Jackpot, 
    }

    public eViewIDs currentView = eViewIDs.None;
    public RectTransform _Panel;

    public GameObject _prefabWinLike;
    public RectTransform _WinLikeGroup;
    public MagaWin _MagaWin;
    public Jackpot _Jackpot;


    void Awake()
    {
        SetCallback_HandleMessage(ParserCommand);
    }

    void ParserCommand(Hashtable has)
    {
        IDs _currentId = (IDs)has["id"].GetHashCode();
#if UNITY_EDITOR
        Debug.Log(Time.frameCount + " <Color=#fff000> BroadCastUI::ParserCommand - " + _currentId.ToString() + " </Color>");
#endif
        switch (_currentId)
        {
            case IDs.SendWinCast: SendWinCast(has["msg"].ToString()); break;
            case IDs.SendWinLike: if (eView.Game == Main.I.CurrentView) SendWinLike(has["msg"].ToString()); break;
        }
        remove(_currentId);
    }

    public void LobbyToGame()
    {
        _MagaWin.gameObject.SetActive(false);
        _Jackpot.gameObject.SetActive(false);
        _WinLikeGroup.gameObject.SetActive(false);
    }

    public void GameToLobby()
    {
        _MagaWin.gameObject.SetActive(false);
        _Jackpot.gameObject.SetActive(false);

        ReWinLike[] childs = _WinLikeGroup.GetComponentsInChildren<ReWinLike>();
        for(int i=0; i<childs.Length; i++)
        {
            DestroyImmediate(childs[i].gameObject);
        }
        _WinLikeGroup.gameObject.SetActive(false);
    }

    public void SendWinCast(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;

        PK.WinCast.RECEIVE pk = JsonConvert.DeserializeObject<PK.WinCast.RECEIVE>(msg);

        DB.I.AddMessage(DB.IDs.WinCast, "data", pk);

        // 로비 화면에도 표현 필요해서 기록
        if (eView.Game == Main.I.CurrentView)
        {
            if (pk.data.winType.Equals("magawin"))
            {
                if (_MagaWin.gameObject.activeSelf == false)
                {
                    _MagaWin.gameObject.SetActive(true);
                    _MagaWin.SetInfo(pk.data);
                }
            }
            else if (pk.data.winType.Equals("jackpot"))
            {
                if (_Jackpot.gameObject.activeSelf == false)
                {
                    _Jackpot.gameObject.SetActive(true);
                    _Jackpot.SetInfo(pk.data);
                }
            }
        }
    }

    public void SendWinLike(string msg)
    {
        ReWinLike[] childs = _WinLikeGroup.GetComponentsInChildren<ReWinLike>();
        if (childs.Length >= 6) return;

        if (string.IsNullOrEmpty(msg)) return;

        if (_WinLikeGroup.gameObject.activeSelf == false)
        {
            _WinLikeGroup.gameObject.SetActive(true);
        }

        PK.WinLike.RECEIVE pk = JsonConvert.DeserializeObject<PK.WinLike.RECEIVE>(msg);

        GameObject go = GameObject.Instantiate(_prefabWinLike) as GameObject;
        if (!go) return;

        ReWinLike item = go.GetComponent<ReWinLike>();
        if (!item) return;

        item.GetComponent<RectTransform>().SetParent(_WinLikeGroup);
        item.GetComponent<RectTransform>().localScale = Vector3.one;
        item.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        item.GetComponent<RectTransform>().gameObject.SetActive(true);
        item.SetInfo(pk.data);
    }
#if UNITY_EDITOR
    public void Test()
    {
        StartCoroutine(coTest());
    }

    IEnumerator coTest()
    {
        float time = (float)Random.Range(0, 2);
        yield return new WaitForSeconds(time);
        if( Random.Range(0,10) > 5 )
        {
            PK.WinCast.RECEIVE pk = new PK.WinCast.RECEIVE();
            pk.cmd = "wincast";
            pk.data = new PK.WinCast.REData();
            pk.data.pictureURL = "https://scontent.xx.fbcdn.net/v/t1.0-1/p50x50/13882213_299368023787703_7713884658653284540_n.jpg?oh=96ebfd2bb08c1ad11f0570250fcdbb1f&oe=59478CD8";
            pk.data.winType = "magawin";
            pk.data.winMultiply = "2000";
            pk.data.winID = 39195;
            pk.data.userID = 1099;
            pk.data.gameID = 42;
            pk.data.userName = "Kale Brown" + Random.Range(1, 100).ToString();
            pk.data.win = "30,000K";
            switch (Random.Range(0, 4))
            {
                case 0: pk.data.gameName = eGameList.emeraldSevens.ToString(); break;
                case 1: pk.data.gameName = eGameList.flyingPiggy.ToString(); break;
                case 2: pk.data.gameName = eGameList.HighDiamonds.ToString(); break;
                case 3: pk.data.gameName = eGameList.HotLotto.ToString(); break;
            }
            UI.BroadCast.AddMessage(BroadCastUI.IDs.SendWinCast, "msg", xLitJson.JsonMapper.ToJson(pk));
        }
        time = (float)Random.Range(0, 2);
        yield return new WaitForSeconds(time);
        if (Random.Range(0, 10) > 5)
        {
            PK.WinCast.RECEIVE pk = new PK.WinCast.RECEIVE();
            pk.cmd = "wincast";
            pk.data = new PK.WinCast.REData();
            pk.data.pictureURL = "https://scontent.xx.fbcdn.net/v/t1.0-1/p50x50/13882213_299368023787703_7713884658653284540_n.jpg?oh=96ebfd2bb08c1ad11f0570250fcdbb1f&oe=59478CD8";
            pk.data.winType = "jackpot";
            pk.data.winMultiply = "2000";
            pk.data.winID = 39195;
            pk.data.userID = 1099;
            pk.data.gameID = 42;
            pk.data.userName = "Kale Brown" + Random.Range(1, 100).ToString();
            pk.data.win = "30,000K";
            pk.data.gameName = "";
            switch (Random.Range(0, 4))
            {
                case 0: pk.data.gameName = eGameList.emeraldSevens.ToString(); break;
                case 1: pk.data.gameName = eGameList.flyingPiggy.ToString(); break;
                case 2: pk.data.gameName = eGameList.HighDiamonds.ToString(); break;
                case 3: pk.data.gameName = eGameList.HotLotto.ToString(); break;
            }
            UI.BroadCast.AddMessage(BroadCastUI.IDs.SendWinCast, "msg", xLitJson.JsonMapper.ToJson(pk));
        }
        
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.2f);
            if (Random.Range(0, 10) > 5)
            {
                PK.WinLike.RECEIVE pk = new PK.WinLike.RECEIVE();
                pk.cmd = "winlike";
                pk.data = new PK.WinLike.REData();
                pk.data.win_id = 39121;
                if (Random.Range(0, 10) > 4) pk.data.url = "https://scontent.xx.fbcdn.net/v/t1.0-1/p50x50/13882213_299368023787703_7713884658653284540_n.jpg?oh=96ebfd2bb08c1ad11f0570250fcdbb1f&oe=59478CD8";
                else  pk.data.url = "https://scontent.xx.fbcdn.net/v/t1.0-1/p50x50/12552844_1037887392948855_5343347008246523113_n.jpg?oh=77c5c0bdcabc9d215b004e0cf3b64de1&oe=590696B9";
                pk.data.first_name = "Си";
                UI.BroadCast.AddMessage(BroadCastUI.IDs.SendWinLike, "msg", xLitJson.JsonMapper.ToJson(pk));
            }
        }
        
        yield return StartCoroutine(coTest());
    }
#endif

}
