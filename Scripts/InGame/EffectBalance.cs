using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class EffectBalance : MonoBehaviour {

    public Text _txtBalance;
    public enum eType    { None, Charge, }
    public eType _Type = eType.None;
    public enum eFormatType    { Normal, Dollar, }
    public eFormatType _FormatType = eFormatType.Normal;


    long _curBalance = 0;
    Tweener _tweenerBalance;
    Sequence _seqBalance;
    long _tweenBalance = 0;
    long _targetBalance = 0;

    bool _stopTween = false;
    Coroutine _coroutineTween;
    Coroutine _coroutineMultiEffect = null;

    string _fmt = "#,#0";


    public bool _test;

    // Use this for initialization
    void Awake () {
        _txtBalance = this.GetComponent<Text>();
        SetBalance(0);
    }

    public void SetBalanceFormat(eFormatType fmt)
    {
        _FormatType = fmt;
    }

    public long GetCurBalance() { return _curBalance; }

    /// <summary>
    /// 이펙트 액션 없이 밸런스 값 설정
    /// </summary>
    /// <param name="balance">The balance.</param>
    public void SetBalance(long balance)
    {
        _curBalance = balance;
        if (balance == 0)
        {
            if (_FormatType == eFormatType.Normal) _txtBalance.text = "0";
            else if (_FormatType == eFormatType.Dollar) _txtBalance.text = "$ 0";
        }
        else
        {
            if (_FormatType == eFormatType.Normal) _fmt = "#,#0";
            else if (_FormatType == eFormatType.Dollar) _fmt = "$ #,#0";
            _txtBalance.text = balance.ToString(_fmt);
        }
    }
    /// <summary>
    /// 변경될 밸런스까지 숫자 업글 이펙트
    /// </summary>
    /// <param name="targetBalance">The target balance.</param>
    /// <param name="speed">The speed.</param>
    public void SetTweenBalance(long targetBalance, float speed=1.0f)
    {
        _tweenBalance = _curBalance;
        _targetBalance = targetBalance;
        _tweenerBalance = DOTween.To(() => _tweenBalance, x => _tweenBalance = x, _targetBalance, speed).OnUpdate(TweenUpdateBalance).OnComplete(TweenCompletedBalance);
    }
    void TweenUpdateBalance()
    {
        _curBalance = _tweenBalance;
        _txtBalance.text = _curBalance.ToString("#,#0");
    }
    void TweenCompletedBalance()
    {
        SetBalance(_targetBalance);
        _stopTween = false;
        //_seqBalance = DOTween.Sequence();
        //_seqBalance.Append(_goBalance.transform.DOScale(1.6f, 0.3f));
        //_seqBalance.Append(_goBalance.transform.DOScale(0.0f, 0.1f));
        //_seqBalance.Play().SetEase(Ease.OutQuad).OnComplete(TweenCompleteScale);
    }

    
    
    // 추가 이펙트 작업 미비 계속 추가예정

    void TweenCompleteScale()
    {
        //_goBalance.SetActive(false);
        //_goBalance.transform.DOScale(1.0f, 0.1f);
        _seqBalance.Rewind();
    }

    public void StopTween()
    {
        //GameStateManager.Instance.SetSlotState(SlotState.PlayWinsTweenStopped);
        _stopTween = true;
        _tweenerBalance.Complete();
        StopCoroutine(_coroutineTween);
        if (_coroutineMultiEffect != null)
            StopCoroutine(_coroutineMultiEffect);
        EndTweenBalance();
    }
    void EndTweenBalance()
    {
    }





    public void PlayTweenBalance(System.Action onComplete, long beginBalance, long endBalance, float speed, float dealyTime)
    {
        TweenBalance(onUpdateBalance, onComplete, beginBalance, endBalance, speed, dealyTime);
    }
    void TweenBalance(System.Action update, System.Action complete, long beginBalance, long endBalance, float speed, float delay)
    {
        if (_FormatType == eFormatType.Normal) _fmt = "#,#0";
        else if (_FormatType == eFormatType.Dollar) _fmt = "$ #,#0";
        _tweenBalance = beginBalance;
        _targetBalance = endBalance;
        _tweenerBalance = DOTween.To(() => _tweenBalance, x => _tweenBalance = x, _targetBalance, speed).SetDelay(delay).OnUpdate( () => 
        {
            if (update != null) update();
            else onUpdateBalance();
        } ).OnComplete( () =>
        {
            if (complete != null) complete();
            else onCompleteBalace();
        } );
    }
    void onUpdateBalance()
    {
        _curBalance = _tweenBalance;
        _txtBalance.text = _curBalance.ToString(_fmt);
    }
    void onCompleteBalace()
    {
        _curBalance = _targetBalance;
        _txtBalance.text = _curBalance.ToString(_fmt);
    }
    
}
