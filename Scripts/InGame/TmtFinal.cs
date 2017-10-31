using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

public class TmtFinal : MonoBehaviour {

    public Image _imgWindMill;
    public GameObject _objResult;
    public GameObject _objRank;
    public GameObject _objButton;
    public Text _myPrizePool;
    public Text _textRank;
    public Image[] _imgTrophy;

    PK.TmtUserRank.REData _UserRank;
    Tweener _TweenWindMill = null;

    public void Reset()
    {
        if (_TweenWindMill != null) _TweenWindMill.Kill();
    }

    public void ActiveShow()
    {
        _objResult.gameObject.SetActive(true);
        _objRank.gameObject.SetActive(false);
        _objButton.gameObject.SetActive(false);
        _TweenWindMill = _imgWindMill.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, -3600f), 120f, RotateMode.FastBeyond360);
        Invoke("RQUserRank", 15f);
    }

    public void SetRQUserRank(PK.TmtUserRank.RECEIVE info)
    {
        _UserRank = info.data;

        // 나의 랭크가 없다.. 일정 타임후에 NowConfig 요청
        if(_UserRank.rank == 0)
        {
            Invoke("RQNowConfig", 1f);
        }
        else // 랭크가 있다..랭크 표현후 NotConfi 요청
        {
            _objResult.gameObject.SetActive(false);
            _objRank.gameObject.SetActive(true);
            
            _myPrizePool.text = info.data.rank_coins.ToString("#,#0");

            // rank 표시 비활성
            _textRank.gameObject.SetActive(false);
            for (int i = 0; i < _imgTrophy.Length; i++)
                _imgTrophy[i].gameObject.SetActive(false);


            if (info.data.rank > 0 && info.data.rank <= 10)
            {
                _imgTrophy[info.data.rank - 1].gameObject.SetActive(true);
            }
            else if (info.data.rank >= 11 && info.data.rank < 100)
            {
                _imgTrophy[10].gameObject.SetActive(true);
            }
            else if (info.data.rank >= 100) // 11등부터는 텍스트로 표현
            {
                _imgTrophy[11].gameObject.SetActive(true);
            }

            UI.Tournaments._Menu.SetRank(info.data.rank);

            /*
            // 등수 맨트 
            if(info.data.rank > 4 && info.data.rank < 11) _imgRankMent[0].gameObject.SetActive(true);
            else if (info.data.rank >= 11 && info.data.rank < 100) _imgRankMent[1].gameObject.SetActive(true);
            else if (info.data.rank >= 100) _imgRankMent[2].gameObject.SetActive(true);
            */

            // 게스트 유저 구분
            if(USER.I.IsGuestLogin) // claim & share 버튼 감추기
            {
                _objButton.gameObject.SetActive(false);
                _objRank.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90f);
            }
            else
            {
                _objRank.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 136f);
                _objButton.gameObject.SetActive(true);
            }

            Invoke("RQNowConfig", 15f);
        }
    }

    void RQUserRank()
    {
        UI.Tournaments.AddMessage(TournamentsUI.IDs.RQUserRank);
        UI.Tournaments.AddMessage(TournamentsUI.IDs.UpdateUserRank);
    }
    void RQNowConfig()
    {
        UI.Tournaments.AddMessage(TournamentsUI.IDs.RQNowConfig);
    }
}
