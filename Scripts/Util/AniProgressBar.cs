using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

public class AniProgressBar : MonoBehaviour
{
    public Image _gagueMask;
    public RectTransform  _effect;
    public Text _label;
    public float _AniSpeed = 80f;

    protected Vector2 _EffectSize;
    public bool _auto = false;
    public bool _autoAdd = false;
    protected float _playTime = 2f;
    protected float _time;
    protected System.Action OnAutoComplete;
    float _tempfill = 0;

    Tweener tweenBar = null;

    // Update is called once per frame
    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        _auto = false;
        _autoAdd = false;
        OnAutoComplete = null;
        _label.text = "0%";
        _gagueMask.fillAmount = 0;
        _effect.sizeDelta = _gagueMask.GetComponent<RectTransform>().sizeDelta;
        _EffectSize = _effect.sizeDelta;
        tweenBar.Kill();
    }

    public float GetFillAmount() { return _gagueMask.fillAmount; }
    public void SetAddFillAmount(float _amount)
    {
        _gagueMask.fillAmount += _amount;
        _label.text = string.Format("{0:D}%", (int)(_gagueMask.fillAmount * 100f));
        if (_gagueMask.fillAmount >= 1.0f)  // end
        {
            _gagueMask.fillAmount = 1.0f;
            _label.text = "100%";
            _effect.sizeDelta = _gagueMask.GetComponent<RectTransform>().sizeDelta;
        }
    }
    public void SetFillAmount(float _amount)
    {
        _label.text = string.Format("{0:D}%", (int)(_amount*100f));
        _gagueMask.fillAmount = _amount;
        if(_amount >= 1.0f)  // end
        {
            _gagueMask.fillAmount = 1.0f;
            _label.text = "100%";
            _effect.sizeDelta = _gagueMask.GetComponent<RectTransform>().sizeDelta;
        }
    }

    public void AutoGague(float playTime, System.Action OnComplete)
    {
        _autoAdd = false;
        _auto = true;
        _playTime = playTime;
        _time = Time.time;
        OnAutoComplete = OnComplete;
    }

    public void AutoAddGague(float playTime, System.Action OnComplete)
    {
        _auto = false;
        _autoAdd = true;
        _playTime = playTime;
        _time = Time.time;
        _tempfill = _gagueMask.fillAmount;
        OnAutoComplete = OnComplete;
    }

    void Update()
    {
        if(_auto)
        {
            if( (Time.time - _time) <= _playTime )
            {
                SetFillAmount((Time.time - _time) / _playTime);
            }
            else
            {
                _auto = false;
                SetFillAmount(1.0f);
                if (OnAutoComplete != null) OnAutoComplete();
            }
        }
        else if( _autoAdd)
        {
            if (_gagueMask.fillAmount >= 0.99f)
            {
                _autoAdd = false;
                SetFillAmount(1.0f);
                if (OnAutoComplete != null) OnAutoComplete();
            }
            else if ((Time.time - _time) <= _playTime)
            {
                SetFillAmount(_tempfill + ((Time.time - _time) / _playTime));
            }
            else
            {
                _autoAdd = false;
                SetFillAmount(1.0f);
                if (OnAutoComplete != null) OnAutoComplete();
            }
        }

        if (_gagueMask.fillAmount > 0)
        {
            if (_effect.sizeDelta.x >= (_EffectSize.x * 10f))
            {
                _effect.sizeDelta = new Vector2(_EffectSize.x + _gagueMask.fillAmount + (_AniSpeed * Time.deltaTime), _effect.sizeDelta.y);
            }
            _effect.sizeDelta = new Vector2(_effect.sizeDelta.x + _gagueMask.fillAmount + (_AniSpeed * Time.deltaTime), _effect.sizeDelta.y);
        }
    }
}
