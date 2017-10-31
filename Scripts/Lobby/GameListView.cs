using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using DG.Tweening;
using xLIB;
/// <summary>
/// 로비의 게임리스트 관리 뷰
/// 게임 아이템의 이벤트를 처리하며, 스크롤 특징은 여기서 정의
/// </summary>
public class GameListView : ScrollViewBase
{
    /// <summary>
    /// 리스트 뷰의 아이템 하나에 게임 3개를 표현하기 위한 정보 클래스
    /// </summary>
    public class STGameItem{
        public List<PK.GamesInfo.REData> list = new List<PK.GamesInfo.REData>();
    }
    /// <summary>
    /// 리스트 뷰의 게임정보 리스트다.
    /// 리스트 뷰의 아이템 갯수와 일치한다.
    /// </summary>
    private List<STGameItem> m_ItemList = new List<STGameItem>();
    /// <summary>
    /// 리스트 뷰 페이지 ( 페이지 단위는 리스트뷰의 아이템 하나 )
    /// </summary>
    public GameListPage _GamePage;

    /// <summary>
    /// 서버 리스트에서 선별된 게임 리스트 
    /// </summary>
    public List<PK.GamesInfo.REData> _GameList = new List<PK.GamesInfo.REData>();

    private int _oldPage = -1;

    private long begin_time = 0;

    private GameLItem _curGameLItem = null;

    // Use this for initialization
    void Start()
    {
        base.Init(OnUpdateItem, OnUpdateItemIndex);
        // 리스트 뷰 생성뒤 마스크 범위를 임의로 늘려준다.. 
        // 다음 리스트 아이템 보여주기 위함
        base.GetViewport().sizeDelta = new Vector2(1278f, 530f);

        StartCoroutine(JackpotUpdate());
    }

    public void OnUpdateItemIndex(int index)
    {
        if (_oldPage != index)
        {
            _oldPage = index;
        }
        _GamePage.SetPage(index);
        //Debug.Log("GameListView Paage Index = " + _oldPage);
    }
    public void OnUpdateItem(int index, GameObject go)
    {
        //Debug.Log("GameListView OnUpdateItem Index = " + index);
        GameLItem item = go.GetComponent<GameLItem>();
        if (item)
        {
            item.UpdateItem(index, m_ItemList[index], callback_GameSelected);
        }
    }

    void ClearAllListItem()
    {
        m_ItemList.Clear();
        _GameList.Clear();
        CurrentItemMaxCount = 0;
    }

    public void UpdateListItem()
    {
        ClearAllListItem();

        // 게임 가능한 리스트 선별
        for (int i = 0; i < USER.I._PKGamesInfo.data.Length; i++)
        {
            if(DEF.IsUseGameID((eGameList)USER.I._PKGamesInfo.data[i].game_id))
            {
                //Debug.Log("GameList Info = " + USER.I._PKGamesInfo.data[i].game_id.ToString() + " , status : " + USER.I._PKGamesInfo.data[i].status);
                _GameList.Add(USER.I._PKGamesInfo.data[i]);
            }
        }

        int count = _GameList.Count;
        for (int i = 0; i < count; i+=3)
        {
            if (i < count)
            {
                STGameItem item = new STGameItem();
                if (i < count) item.list.Add(_GameList[i]);
                if (i + 1 < count) item.list.Add(_GameList[i + 1]);
                if (i + 2 < count) item.list.Add(_GameList[i + 2]);
                m_ItemList.Add(item);
            }
        }
/*
#if UNITY_EDITOR
        for(int i=0; i<m_ItemList.Count; i++)
            for (int k = 0; k < m_ItemList[i].list.Count; k++)
                Debug.Log(i + " > m_ItemList Info = " + m_ItemList[i].list[k].game_id);

#endif
*/
        CurrentItemMaxCount = m_ItemList.Count;
        Reset();
        _GamePage.Init(m_ItemList.Count);
    }

    /// <summary>
    /// 게임리스트에서 선택한 게임을 번들까지 받은상태다.. 로비에 화면전환 처리 넘긴다.
    /// </summary>
    /// <param name="id">The identifier.</param>
    void callback_GameSelected(int id)
    {
        //SOUND.I.Play(DEF.SND.game_click);
        //Debug.Log("event_clickItem = " + id);
        //SCENE.I.AddMessage(SCENEIDs.LobbyToGame, "game", id);
    }


    public void LobbyToGame()
    {

    }

    public void GameToLobby()
    {
        StartCoroutine(JackpotUpdate());
    }

    IEnumerator JackpotUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(600f);  // 5분
            // 서버에 게임 리스트 재 요청 
            // 잭팟 정보 갱신 처리
            NET.I.SendReqGamesInfo((id, msg) =>
            {
                USER.I._PKGamesInfo = JsonConvert.DeserializeObject<PK.GamesInfo.RECEIVE>(msg);
            }, NET.I.OnSendReqTimerout, USER.I._PKUserInfo.GetId());
        }
    }

}
