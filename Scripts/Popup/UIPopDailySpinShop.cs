using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopDailySpinShop : UIPopupBase
{
    public enum eAction { None, Ready, Play, Stop, Result, }
    eAction _action = eAction.None;

    public EffectBalance _ebSpinBonus;
    public EffectBalance _ebLevelBonus;
    public EffectBalance _ebFriendsBonus;
    public EffectBalance _ebTotalBonus;

    public Image _imgSpinBoard;
    public Button _btnSpin;
    public Button _btnExit;
    public Button _btnCollect;

    public GameObject _objBuyNowGroup;
    public Text _textSpinCount;
    public GameObject _objResult;
    public Animator _anim;

    public GameObject _fxStartPoint;
    public Image _imgGuestGuide;

    public float _ActionTime = 6f;
    public Ease _EaseType = Ease.InSine;

    PK.GetDailyWheelShop.RECEIVE _reqShopInfo = null;
    PK.WheelPurchase.REDataSpinData[] _reqInfoArray = null;
    bool receive = false;

    protected int _SpinCount = 0;


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
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        base.ActiveTween(false);

        SetAction(eAction.None);
        SOUND.I.Play(DEF.SND.popup_open);
        _btnSpin.enabled = true;
        if (!_anim.isInitialized)
        {
        }
        _anim.SetInteger("event", 0);
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        receive = false;
        m_id = id;
        eventClose = _eventClose;
        m_args = args;

        _SpinCount = 0;
        _textSpinCount.text = "0";
        _btnExit.gameObject.SetActive(true);
        _objBuyNowGroup.gameObject.SetActive(true);

        _ebSpinBonus.SetBalance(0);
        _ebLevelBonus.SetBalance(0);
        _ebFriendsBonus.SetBalance(0);
        _ebTotalBonus.SetBalance(0);

        // 게스트 정책
        _imgGuestGuide.gameObject.SetActive(USER.I.IsGuestLogin);

        NET.I.SendReqGetDailyWheelShop((id2, msg2) =>
        {
            _reqShopInfo = JsonConvert.DeserializeObject<PK.GetDailyWheelShop.RECEIVE>(msg2);
            receive = true; 
        }, NET.I.OnSendReqTimerout);
    }


    public void ReceiveResult(int index)
    {
        int bonus = _WheelList[index];
        // 회전량이 부족해 보여서 10배로 상향 360->3600 으로 변경
        float angle = (3600f - (index * 24f)) + 180f;
        //SOUND.I.Play(DEF.SND.dailyspin_counting, false, 2f);
        SOUND.I.Play(DEF.SND.dailyspin_wheel);
        //Debug.Log("********** RecevieResult - index:" + index + " angle:" + angle + " bonus:" + bonus);
        _imgSpinBoard.transform.DORotate(new Vector3(0f, 0f, -angle), 6f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine).OnComplete(EndSpinRotate);
    }

    void EndSpinRotate()
    {
        SOUND.I.PlayStop(DEF.SND.dailyspin_wheel);
        SOUND.I.Play(DEF.SND.dailyspin_win);

        SetAction(eAction.Stop);

        int i = _SpinCount - 1;

        if (_reqInfoArray[i].spinBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
        _ebSpinBonus.PlayTweenBalance(() => {
            SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
            if (_reqInfoArray[i].spinBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
            if (_reqInfoArray[i].levelBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
            _ebLevelBonus.PlayTweenBalance(() => {
                SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                if (_reqInfoArray[i].levelBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
                if (_reqInfoArray[i].friendsBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
                _ebFriendsBonus.PlayTweenBalance(() => {
                    SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                    if (_reqInfoArray[i].friendsBonus > 0) SOUND.I.Play(DEF.SND.dailyspin_count_end);
                    long total = _reqInfoArray[i].spinBonus + _reqInfoArray[i].levelBonus + _reqInfoArray[i].friendsBonus;
                    if (total > 0) SOUND.I.Play(DEF.SND.dailyspin_counting);
                    _ebTotalBonus.PlayTweenBalance(() =>
                    {
                        SOUND.I.PlayStop(DEF.SND.dailyspin_counting);
                        SOUND.I.Play(DEF.SND.dailyspin_count_end);

                        SetAction(eAction.Result);

                    }, 0, total, 1f, 0.2f);
                }, 0, _reqInfoArray[i].friendsBonus, 1f, 0.1f);
            }, 0, _reqInfoArray[i].levelBonus, 1f, 0.1f);
        }, 0, _reqInfoArray[i].spinBonus, 1f, 0.1f);
    }


    void SetAction(eAction action)
    {
        _action = action;
        switch (action)
        {
            case eAction.None:
                _objResult.gameObject.SetActive(false);
                _btnCollect.enabled = false;
                break;
            case eAction.Ready:
                _objResult.gameObject.SetActive(false);
                break;
            case eAction.Play:
                _btnExit.gameObject.SetActive(false);
                _anim.SetInteger("event", 1);
                break;
            case eAction.Stop:
                _anim.SetInteger("event", 2);
                _objResult.gameObject.SetActive(true);
                break;
            case eAction.Result:
                _btnCollect.enabled = true;
                break;
        }
    }

    void FixedUpdate()
    {
        if (receive)
        {
            receive = false;
            SetAction(eAction.Ready);
        }

        switch (_action)
        {
            case eAction.Play:
                ReceiveResult(_reqInfoArray[_SpinCount-1].wheelIndex);
                _action = eAction.None;
                break;
        }
    }

    #region Button Event
    public void click_Spin()
    {
        if (_SpinCount <= 0 || _btnSpin.enabled == false) return;

        //UI.SetWaitLoading(true);

        _btnSpin.enabled = false;
        SetAction(eAction.Ready);

        _textSpinCount.text = string.Format("{0}", _SpinCount - 1);

        SetAction(eAction.Play);
    }

    public void click_BuyNow_2()
    {
        BuyNow(0);
    }
    public void click_BuyNow_5()
    {
        BuyNow(1);
    }
    public void click_BuyNow_10()
    {
        BuyNow(2);
    }

    void BuyNow(int idx)
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.SetWaitLoading(true);

        PLATFORM.I.BuyPurchase(_reqShopInfo.data.packs[idx], (result, google, apple) => {
            if (result != "failed")
            {
                NET.I.SendReqWheelPurchase((_id, msg) =>
                {
                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());

                    PK.WheelPurchase.RECEIVE info = JsonConvert.DeserializeObject<PK.WheelPurchase.RECEIVE>(msg);
                    _reqInfoArray = info.data.spinData;
                    //SetAction(eAction.Play);
                    if(idx==0) _SpinCount = 2;
                    else if (idx == 1) _SpinCount = 5;
                    else if (idx == 2) _SpinCount = 10;
                    _textSpinCount.text = _SpinCount.ToString();
                    _objBuyNowGroup.gameObject.SetActive(false);
                    _btnExit.gameObject.SetActive(false);
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _reqShopInfo.data.packs[idx].product_url, google, apple);
            }
            else
            {
                UI.SetWaitLoading(false);
#if UNITY_EDITOR
                NET.I.TestSendReqWheelPurchase((_id, msg) =>
                {
                    PK.WheelPurchase.RECEIVE info = JsonConvert.DeserializeObject<PK.WheelPurchase.RECEIVE>(msg);
                    _reqInfoArray = info.data.spinData;
                    //SetAction(eAction.Play);
                    if (idx == 0) _SpinCount = 2;
                    else if (idx == 1) _SpinCount = 5;
                    else if (idx == 2) _SpinCount = 10;
                    _textSpinCount.text = _SpinCount.ToString();
                    _objBuyNowGroup.gameObject.SetActive(false);
                    _btnExit.gameObject.SetActive(false);
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _reqShopInfo.data.packs[idx].product_url);
#endif
            }
        });
    }

    // 보상금 회수
    public void click_ResultCollect()
    {
        _btnCollect.enabled = false;
        // update user info 
        USER.I.GetUserInfo().data.user_level = _reqInfoArray[_SpinCount-1].userLevel;
        USER.I.GetUserInfo().Balance += _reqInfoArray[_SpinCount-1].balance;
        if (_action == eAction.Result)
        {
            FX.I.PlayCoins(_fxStartPoint, Lobby.I._TopMenu._imgCoinIcon.gameObject, () => {

                --_SpinCount;  // 스핀 카운트 차감
                if (_SpinCount <= 0)
                {
                    Close("ok");
                }
                else
                {
                    _ebSpinBonus.SetBalance(0);
                    _ebLevelBonus.SetBalance(0);
                    _ebFriendsBonus.SetBalance(0);
                    _ebTotalBonus.SetBalance(0);
                    _btnSpin.enabled = true;
                    _anim.SetInteger("event", 0);
                }
            });
        }
    }

    
    
    #endregion //Button Event

}
