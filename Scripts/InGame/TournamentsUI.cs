using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using xLIB;
// Tournaments
public class TournamentsUI : Schedule
{
    public enum IDs
    {
        None = 100,
        RQNowConfig,
        RQNowRank,
        RQUserRank,
        StandyTimer,
        GameTimer,
        UpdateNowRank,
        Final,
        UpdateUserRank,
        XBtnClick,
        DefaultTabLayer,
    };

    public enum eViewIDs    {
        None, Standby, Play, Final,
    }

    public eViewIDs currentView = eViewIDs.None;

    public RectTransform _Panel;
    public TmtStandby   _Standby;
    public TmtPlay      _Play;
    public TmtFinal     _Final;
    public TmtMenu      _Menu;

    public GameObject _TabMyResults;
    public GameObject _TabMyInfo;
    public GameObject _TabMyRank;
    public GameObject _ScreenBlock;

    public Button _btnX;


    public PK.TmtNowConfig.RECEIVE _PKTmtNowConfig = null;
    public PK.TmtNowRank.RECEIVE _PKTmtNowRank = null;
    public PK.TmtUserRank.RECEIVE _PKTmtUserRank = null;

    public int _tmt_id = 0;
    public int _GameId = 0;
    bool _IsHide = true;
    bool _btnLock = false;

    int _LimitSecTime = 60;
    int _GameSecTime = 1;


    void Awake()
    {
        _Standby.gameObject.SetActive(false);
        _Play.gameObject.SetActive(false);
        _Final.gameObject.SetActive(false);
        _TabMyResults.gameObject.SetActive(false);
        _TabMyInfo.gameObject.SetActive(false);
        _TabMyRank.gameObject.SetActive(false);
        _ScreenBlock.gameObject.SetActive(false);

        SetCallback_HandleMessage(ParserCommand);
    }

    void ParserCommand(Hashtable has)
    {
        IDs _currentId = (IDs)has["id"].GetHashCode();

//#if UNITY_EDITOR
//        Debug.Log(Time.frameCount + " <Color=#fff000> TournamentsUI::ParserCommand - " + _currentId.ToString() + " </Color>");
//#endif
        switch (_currentId)
        {
            case IDs.RQNowConfig:
                NET.I.AddMessage(PKID.TournamentNowConfig, "tmtid", _tmt_id, "gameid", _GameId);
                break;
            case IDs.RQNowRank:
                NET.I.AddMessage(PKID.TournamentNowRank, "tmtid", _tmt_id, "gameid", _GameId);
                return;
            case IDs.RQUserRank:
                NET.I.AddMessage(PKID.TournamentUserRank, "tmtid", _tmt_id, "gameid", _GameId);
                return;
            case IDs.StandyTimer:
                ActiveViews(eViewIDs.Standby);
                _Standby.StartCountdownTimer(_LimitSecTime);
                break;
            case IDs.GameTimer:
                ActiveViews(eViewIDs.Play);
                _Play.SetRQNowRank(_PKTmtNowRank);
                _Play.StartCountdownTimer(_LimitSecTime);
                break;
            case IDs.UpdateNowRank:
                _Play.SetRQNowRank(_PKTmtNowRank);
                break;
            case IDs.Final:
                ActiveViews(eViewIDs.Final);
                _Final.ActiveShow();
                break;
            case IDs.UpdateUserRank:
                _Final.SetRQUserRank(_PKTmtUserRank);
                break;
            case IDs.XBtnClick:
                click_Active();
                break;
            case IDs.DefaultTabLayer:
                _btnX.transform.SetSiblingIndex(0);
                _TabMyResults.transform.SetSiblingIndex(1);
                _TabMyInfo.transform.SetSiblingIndex(2);
                _TabMyRank.transform.SetSiblingIndex(3);
                break;
        }
        remove(_currentId);
    }

    public void click_Active()
    {
        if (_btnLock) return;
        _btnLock = true;

        if (_IsHide)
        {   // 우측 이동
            _Menu.gameObject.SetActive(false);
            _Panel.DOAnchorPosX(0, 0.5f).OnComplete(() =>
            {  
                _IsHide = false;
                _btnLock = false;
            });
        }
        else
        {    // 감추기
            _Panel.DOAnchorPosX(-165f, 0.5f).OnComplete(() =>
            {   
                _IsHide = true;
                _btnLock = false;
                _Menu.gameObject.SetActive(true);
            });
        }
    }
    public void click_Claim()
    {
    }
    public void toggle_Share()
    {
    }



    public void LobbyToGame(int gameId)
    {
        ActiveViews(eViewIDs.None);
        _GameId = gameId;
        NET.I.AddMessage(PKID.TournamentNowConfig, "tmtid", 0, "gameid", gameId);
    }
    public void GameToLobby()
    {
        if (!_IsHide) click_Active();
        _Standby.Reset();
        _Play.Reset();
        _Final.Reset();
        ActiveViews(eViewIDs.None);

        _GameId = 0;
        this.gameObject.SetActive(false);
    }

    public void ActiveViews(eViewIDs id)
    {
        currentView = id;
        _Standby.gameObject.SetActive(false);
        _Play.gameObject.SetActive(false);
        _Final.gameObject.SetActive(false);
        if (id == eViewIDs.Standby) { _Standby.gameObject.SetActive(true); _Menu.SetState(0); } 
        else if (id == eViewIDs.Play) { _Play.gameObject.SetActive(true); _Menu.SetState(1); }
        else if (id == eViewIDs.Final) { _Final.gameObject.SetActive(true); _Menu.SetState(2); }
        _TabMyResults.gameObject.SetActive(true);
        _TabMyInfo.gameObject.SetActive(true);
        _TabMyRank.gameObject.SetActive(true);
        _ScreenBlock.gameObject.SetActive(true);
    }

    public void PKNowConfig(PK.TmtNowConfig.RECEIVE _info)
    {
        //if (_IsHide) click_Active();
        _PKTmtNowConfig = _info;
        _tmt_id = _info.data.subData[0].tmt_id;
        _GameId = (int)InGame.I.gameId;

        //Debug.Log(
        //    "_tmt_id:" + _info.data.subData[0].tmt_id +
        //    ",end_limit_time:" + _info.data.subData[0].end_limit_time +
        //    ",game_time:" + _info.data.subData[0].game_time +
        //    ",start_limit_time:" + _info.data.subData[0].start_limit_time +
        //    ",wait_time:" + _info.data.subData[0].wait_time
        //    );

        // 게임 플레이 타임 받은거다.. 플레이 환경으로 전환
        if(_info.data.subData[0].end_limit_time != 0)
        {
            _LimitSecTime = _info.data.subData[0].end_limit_time;
            _GameSecTime = _info.data.subData[0].game_time;
            base.AddMessage(IDs.RQNowRank);
            base.AddMessage(IDs.GameTimer);
        }
        else // 게임 플레이 전단계 대기 상태다
        {
            _LimitSecTime = _info.data.subData[0].start_limit_time;
            _GameSecTime = _info.data.subData[0].wait_time;
            base.AddMessage(IDs.StandyTimer);
        }
    }
    public void PKNowRank(PK.TmtNowRank.RECEIVE _info)
    {
        _PKTmtNowRank = _info;
        base.remove(IDs.RQNowRank);
    }
    public void PKUserRank(PK.TmtUserRank.RECEIVE _info)
    {
        _PKTmtUserRank = _info;
        base.remove(IDs.RQUserRank);
    }
    public void click_BtnX()
    {
        UI.Tournaments.AddMessage(IDs.XBtnClick);
    }
}
