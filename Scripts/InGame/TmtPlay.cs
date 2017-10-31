using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// Tournaments game play
public class TmtPlay : MonoBehaviour
{
    public GameObject _prefabLItem;
    public GameObject _ListGroup;
    public Image _imgBlueLine;
    public Image _imgRedLine;
    public Text _remainTime;
    public Text _PrizePoolTotal;

    List<PK.TmtNowRank.REDataData> _ListRank = new List<PK.TmtNowRank.REDataData>();  // 랭크 데이터
    List<LItemTmtRank> _RankItemList = new List<LItemTmtRank>();  // 랭크 리스트 아이템

    int _MaxItem = 5;

    int _SendTime = 0;
    long _LastTick = 0;
    int _LimitSecTime = 60;
    int _myRank = 0;
    int _myBeforeRank = 0;
    Tweener _TweenBlueLine = null;
    Tweener _TweenRedLine = null;


    void Awake()
    {
        for (int i = 0; i < _MaxItem; i++)
        {
            GameObject go = xLIB.xHelper.AddChild(_ListGroup.gameObject, _prefabLItem.gameObject);
            go.GetComponent<LItemTmtRank>().Clean();
            _RankItemList.Add(go.GetComponent<LItemTmtRank>());
        }
    }

    public void Reset()
    {
        if (_TweenBlueLine != null) _TweenBlueLine.Kill();
        if (_TweenRedLine != null) _TweenRedLine.Kill();
        _SendTime = 0;
        _LimitSecTime = 0;
        _myBeforeRank = 0;
        _myRank = 0;
    }

    public void SetRQNowRank(PK.TmtNowRank.RECEIVE info)
    {
        if (info == null ) return;

        _ListRank.Clear();
        for(int i=0; i<info.data.data.Length; i++)
        {
            if(info.data.data[i].user_id == USER.I.GetUserInfo().GetId())
            {
                _myBeforeRank = _myRank;  // 이전 랭크 기록
                _myRank = info.data.data[i].rank;  // 나의 랭크 기록
            }
            _ListRank.Add(info.data.data[i]);
        }

        if (info.data.subData.Length > 0)
            _PrizePoolTotal.text = info.data.subData[0].coins.ToString("#,#0");

        UpdateListItem();
    }

    void UpdateListItem()
    {
        // 기존 데이터 삭제
        for(int i=0; i< _MaxItem; i++)
        {
            _RankItemList[i].Clean();
        }
        // 새롭게 데이터 세팅
        for(int i=0; i<_ListRank.Count; i++)
        {
            if(i < _MaxItem)
                _RankItemList[i].SetInfo(_ListRank[i]);
        }

        // 내가 랭킹정보에 포함되어 있다.
        if (_myRank != 0)
        {
            //_objSpinToEnter.gameObject.SetActive(false);
        }
        else  // 내가 랭킹에 없다 spinToEnter 활성
        {
            _RankItemList[_MaxItem-1].SetSpinToEnter(true, USER.I.CurProfileTexture);
        }

        UI.Tournaments._Menu.SetRank(_myRank);

        // 랭크에 변화가 있다.
        if (_myRank != _myBeforeRank)
        {
            if(_myRank > _myBeforeRank)  // 랭크 up
            {
                UI.Tournaments._Menu.RankUp();
                //PlayUpRank()
            }
            else if (_myRank < _myBeforeRank)  // 랭크 down
            {
                UI.Tournaments._Menu.RankDown();
            }
        }

    }


    public void StartCountdownTimer(int GameSec)
    {
        if (_TweenBlueLine != null) _TweenBlueLine.Kill();
        if (_TweenRedLine != null) _TweenRedLine.Kill();

        _SendTime = 0;
        _LimitSecTime = GameSec;
        _LastTick = (System.DateTime.UtcNow.Ticks / 10000000L);

        _imgBlueLine.fillAmount = 0f;
        _TweenBlueLine = _imgBlueLine.DOFillAmount(1f, _LimitSecTime).SetEase(Ease.Linear).OnUpdate(TweenUpdateTime).OnComplete(TweenEndTime);

        _imgRedLine.fillAmount = 0f;
        _TweenRedLine = _imgRedLine.DOFillAmount(1f, _LimitSecTime).SetEase(Ease.Linear).OnUpdate(TweenUpdateTime).OnComplete(TweenEndTime);
    }

    void TweenUpdateTime()
    {
        long curTimeTick = (System.DateTime.UtcNow.Ticks / 10000000L);
        long iTick = curTimeTick - _LastTick;
        int diffSecond = (int)(_LimitSecTime - iTick);

        int min = diffSecond / 60;
        int sec = diffSecond - (min * 60);

        if (min <= 0 && sec <= 0) _remainTime.text = "00:00";
        else _remainTime.text = string.Format("{0:00}:{1:00}", min, sec);

        if(diffSecond < 0)
        {
            _TweenBlueLine.Kill();
            _TweenRedLine.Kill();
        }
        // 10초 단위로 랭킹정보 요청
        else if ((diffSecond % 10) == 0)
        {
            if (_SendTime != diffSecond)  // 1초 미만의 업데이트 한번만 보내기 위한 스위치
            {
                _SendTime = diffSecond;
                if (curTimeTick > _LastTick)
                {
                    UI.Tournaments.AddMessage(TournamentsUI.IDs.RQNowRank);
                    UI.Tournaments.AddMessage(TournamentsUI.IDs.UpdateNowRank);
                }
            }
        }
        
        // 10초전은 빨간 레드 효과 활성
        if (diffSecond <= 10 && _imgRedLine.gameObject.activeSelf == false)
        {
            _imgBlueLine.gameObject.SetActive(false);
            _imgRedLine.gameObject.SetActive(true);
        }
    }

    void TweenEndTime()
    {
        _TweenBlueLine.Kill();
        _TweenRedLine.Kill();
        //UI.Tournaments.AddMessage(TournamentsUI.IDs.RQUserRank);
        // 결과화면 먼저 호출뒤 15초 후에 UserRank 요청해라 (서버 정산시간 필요)
        UI.Tournaments.AddMessage(TournamentsUI.IDs.Final);
    }

    public void PlayUpRank(int i)
    {
        _RankItemList[i].PlayUpRank(() => {
            float curY = _RankItemList[i].GetComponent<RectTransform>().anchoredPosition.y;
            _RankItemList[i].GetComponent<RectTransform>().DOAnchorPosY(curY + 58f, 3f);
        });
    }

    public void PlayDnRank(int i)
    {
        _RankItemList[i].PlayDnRank(() => {
            float curY = _RankItemList[i].GetComponent<RectTransform>().anchoredPosition.y;
            _RankItemList[i].GetComponent<RectTransform>().DOAnchorPosY(curY - 58f, 3f);
        });
    }


}
