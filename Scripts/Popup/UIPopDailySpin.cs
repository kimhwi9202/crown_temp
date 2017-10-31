using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopDailySpin : UIPopupBase
{
    public enum eAction{None,Ready,Play,Stop,Result,}
    eAction _action = eAction.None;

    public EffectBalance _ebSpinBonus;
    public EffectBalance _ebLevelBonus;
    public EffectBalance _ebFriendsBonus;
    public EffectBalance _ebTotalBonus;

    public Image _imgSpinBoard;
    public Button _btnSpin;
    public Button _btnExit;
    public Button _btnCollect;

    public GameObject _objResult;
    public Animator _ani;

    public GameObject _fxStartPoint;
    public Image _imgGuestGuide;

    public float _ActionTime = 2.0f;
    public Ease _EaseType = Ease.InOutCubic;

    PK.DailySpin.RECEIVE _reqInfo = null;
    bool _news = false;




    // 휠 포지션별 당첨금 인덱스 0부터
    // 휠 포지션 각도 계산은 ( 0번 인덱스(1000K)가 180도 위치에 있다. ) -> float angle =  (360f - (index * 24f)) + 180f;
    List<int> _WheelList = new List<int>{
        1000000, 30000, 500000, 40000, 250000,
        50000, 200000, 60000, 150000, 70000,
        100000, 80000, 30000, 90000, 100000};


    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (IsInit()) return;
        SetAction(eAction.None);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SOUND.I.Play(DEF.SND.popup_open);
        _ani.SetInteger("event", 0);
        _btnSpin.enabled = true;
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);
        m_id = id;
        eventClose = _eventClose;
        m_args = args;

        if (args != null && args.Length > 0) _news = System.Convert.ToBoolean(args[0]);

        _btnExit.gameObject.SetActive(true);

        _ebSpinBonus.SetBalance(0);
        _ebLevelBonus.SetBalance(0);
        _ebFriendsBonus.SetBalance(0);
        _ebTotalBonus.SetBalance(0);

        // 게스트 정책
        if(_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(USER.I.IsGuestLogin);
    }

    public void ReceiveResult(int index)
    {
        int bonus = _WheelList[index];
        // 회전량이 부족해 보여서 10배로 상향 360->3600 으로 변경
        float angle = (3600f - (index * 24f)) + 180f;
        //Debug.Log("********** RecevieResult - index:" + index + " angle:" + angle + " bonus:" + bonus);
        SOUND.I.Play(DEF.SND.dailyspin_wheel);
        _imgSpinBoard.transform.DORotate(new Vector3(0f, 0f, -angle), 6f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine).OnComplete(EndSpinRotate);
    }

    void EndSpinRotate()
    {
        SOUND.I.PlayStop(DEF.SND.dailyspin_wheel);
        SOUND.I.Play(DEF.SND.dailyspin_win);

        SetAction(eAction.Stop);

        if (_reqInfo.data.spinBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
        _ebSpinBonus.PlayTweenBalance(()=> {
            SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
            if (_reqInfo.data.spinBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
            if (_reqInfo.data.levelBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
            _ebLevelBonus.PlayTweenBalance(() => {
                SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                if (_reqInfo.data.levelBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
                if (_reqInfo.data.friendsBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
                _ebFriendsBonus.PlayTweenBalance(() => {
                    SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                    if (_reqInfo.data.friendsBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
                    long total = _reqInfo.data.spinBonus + _reqInfo.data.levelBonus + _reqInfo.data.friendsBonus;
                    if (total > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
                    _ebTotalBonus.PlayTweenBalance(() =>
                    {
                        SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                        SOUND.I.Play(DEF.SND.dailyspin_count_end);
                        SetAction(eAction.Result);
                    }, 0, total, 1f, 0.2f);
                }, 0, _reqInfo.data.friendsBonus, 1f, 0.2f);
            }, 0, _reqInfo.data.levelBonus, 1f, 0.2f);
        }, 0, _reqInfo.data.spinBonus, 1f, 0.2f);
    }


    void SetAction(eAction action)
    {
        _action = action;
        switch(action)
        {
            case eAction.None:
                _objResult.gameObject.SetActive(false);
                _btnCollect.enabled = false;
                break;
            case eAction.Ready:
                _btnExit.gameObject.SetActive(false);
                break;
            case eAction.Play:
                _btnExit.gameObject.SetActive(false);
                _ani.SetInteger("event", 1);
                break;
            case eAction.Stop:
                _ani.SetInteger("event", 2);
                _objResult.gameObject.SetActive(true);
                break;
            case eAction.Result:
                _btnCollect.enabled = true;
                break;
        }
    }

    void FixedUpdate()
    {
        switch (_action)
        {
            case eAction.Play:
                ReceiveResult(_reqInfo.data.wheelIndex);
                _action = eAction.None;
                break;
        }
    }

    #region Button Event
    public void click_Spin()
    {
        _btnSpin.enabled = false;
        SOUND.I.Play(DEF.SND.common_click);

        SetAction(eAction.Ready);
        NET.I.SendReqDailySpin((_id, msg) =>
        {
            _reqInfo = JsonConvert.DeserializeObject<PK.DailySpin.RECEIVE>(msg);
            USER.I.GetUserInfo().data.daily_spin_enable = 0;
            SetAction(eAction.Play);

            long total = _reqInfo.data.spinBonus + _reqInfo.data.levelBonus + _reqInfo.data.friendsBonus;
            Main.I.AppsFlyerEvent(AFInAppEvents.FREECOINS, 
                AFInAppEvents.FREE_SOURCE, "DailySpin", 
                AFInAppEvents.FREE_COINS, total.ToString(),
                AFInAppEvents.FREE_COUNT, "1" );

        }, NET.I.OnSendReqTimerout, 1);
    }

    public void click_ResultCollect()
    {
        _btnCollect.enabled = false;
        CollectAction();
    }

    void CollectAction()
    {
        SOUND.I.Play(DEF.SND.common_click);
        USER.I.GetUserInfo().data.user_level = _reqInfo.data.userLevel;
        USER.I.GetUserInfo().Balance = _reqInfo.data.balance;

        if (_action == eAction.Result)
        {
            FX.I.PlayCoins(_fxStartPoint, Lobby.I._TopMenu._imgCoinIcon.gameObject, () => {
                if (_news == false) UI.I.AddMessage(UI.IDs.PopDailySpinShop);
                Close("ok");
            });
        }
    }
    #endregion //Button Event

}
