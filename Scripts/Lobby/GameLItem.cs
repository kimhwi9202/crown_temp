using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 로비의 게임리스트 아이템 클래스
/// 아이템 특징은 여기서 설정하며, 이벤트는 부모인 리스트뷰에서 처리한다.
/// </summary>
public class GameLItem : MonoBehaviour
{
    public System.Action<int> callbackSelectGame;

    Vector2[] _Pos = { new Vector2(33f,35f), new Vector2(389,35f), new Vector2(749,35f) };
    public int _index = 0;
    public GameObject prefabsCabinet;
    public GameLItemCabinet[] _CabinetList;
    public List<PK.GamesInfo.REData> _list = new List<PK.GamesInfo.REData>();

    void Awake()
    {
        if (prefabsCabinet != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject go = xLIB.xHelper.AddChild(this.gameObject, prefabsCabinet);
                if (go)
                {
                    _CabinetList[i] = go.GetComponent<GameLItemCabinet>();
                    if (_CabinetList[i] != null)
                    {
                        _CabinetList[i].GetComponent<RectTransform>().anchoredPosition = _Pos[i];
                    }
                }
            }
        }
    }


    public void UpdateItem(int index, GameListView.STGameItem info, System.Action<int> callback)
    {
        _index = index;
        _list = info.list;
        callbackSelectGame = callback;

        if (prefabsCabinet != null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_CabinetList[i] != null)
                {
                    if (i < _list.Count)
                    {
                        _CabinetList[i].Init(_list[i], callbackSelectGame);
                        //Debug.Log(index + " cabinet info = " + _CabinetList[i]._id);
                    }
                    else
                    {
                        _CabinetList[i].InitDefault();
                        //Debug.Log(index + " cabinet default info = " + _CabinetList[i]._id);
                    }
                }
            }
        }
    }
}
