using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class TmtStandby : MonoBehaviour {

    public Text _TimeClock;
    public Image _imgWindMill;

    public Image _imgBlueGroup;
    public Image _imgRedGroup;

    public Image _BlueTimeLine;
    public Image _BlueCircleLight;

    public Image _RedTimeLine;
    public Image _RedCircleLight;

    public Image _RedBGLight;

    long _LastTick = 0;
    int _LimitSecTime = 60;
    Tweener _TweenBlueLine = null;
    Tweener _TweenRedLine = null;
    Tweener _TweenWindMill = null;
    Tweener _TweenRedBGLight = null;

    public void Reset()
    {
        if (_TweenBlueLine != null) _TweenBlueLine.Kill();
        if (_TweenRedLine != null) _TweenRedLine.Kill();
        if (_TweenWindMill != null) _TweenWindMill.Kill();
        if (_TweenRedBGLight != null) _TweenRedBGLight.Kill();
        _BlueTimeLine.fillAmount = 0f;
        _RedTimeLine.fillAmount = 0f;
        _imgBlueGroup.gameObject.SetActive(true);
        _imgRedGroup.gameObject.SetActive(false);
        _LimitSecTime = 0;
    }

    public void StartCountdownTimer(int LimitSec)
    {
        if (_TweenBlueLine != null) _TweenBlueLine.Kill();
        if (_TweenRedLine != null) _TweenRedLine.Kill();
        if (_TweenWindMill != null) _TweenWindMill.Kill();
        if (_TweenRedBGLight != null) _TweenRedBGLight.Kill();

        _imgRedGroup.gameObject.SetActive(false);
        _LimitSecTime = LimitSec;
        _LastTick = (System.DateTime.UtcNow.Ticks / 10000000L);
        //Debug.Log("StartCountdownTimer:" + _LimitSecTime);

        _BlueTimeLine.fillAmount = 0f;
        _RedTimeLine.fillAmount = 0f;
        _TweenBlueLine = _BlueTimeLine.DOFillAmount(1f, _LimitSecTime).SetEase(Ease.Linear).OnUpdate(TweenUpdateTime).OnComplete(TweenEndTime);
        _TweenRedLine = _RedTimeLine.DOFillAmount(1f, _LimitSecTime).SetEase(Ease.Linear).OnUpdate(TweenUpdateTime).OnComplete(TweenEndTime);
        _TweenWindMill = _imgWindMill.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, -3600f), 120f, RotateMode.FastBeyond360);
    }

    void TweenUpdateTime()
    {
        long curTimeTick = (System.DateTime.UtcNow.Ticks / 10000000L);
        long iTick = curTimeTick - _LastTick;
        int diffSecond = (int)(_LimitSecTime - iTick);

        int min = diffSecond / 60;
        int sec = diffSecond - (min * 60);

        if (min <= 0 && sec <= 0) _TimeClock.text = "00:00";
        else _TimeClock.text = string.Format("{0:00}:{1:00}", min, sec);

        UI.Tournaments._Menu.SetTime(_TimeClock.text);

        // 36조각 스타일
        /*
        float temp = 1f / 36f;
        _RedCircle.fillAmount = Mathf.Floor(_BlueTimeLine.fillAmount / temp) * temp;
        _RedCircleLight.GetComponent<RectTransform>().localRotation = Quaternion.Euler(180f, 0, (_RedCircle.fillAmount * 360f));
        */
        _BlueCircleLight.GetComponent<RectTransform>().localRotation = Quaternion.Euler(180f, 0, (_BlueTimeLine.fillAmount * 360f));
        _RedCircleLight.GetComponent<RectTransform>().localRotation = Quaternion.Euler(180f, 0, (_RedTimeLine.fillAmount * 360f));

        if (diffSecond < 0)
        {
            _TweenBlueLine.Kill();
        }
        // 10초전은 빨간 레드 효과 활성
        else if (diffSecond <= 10 && _imgRedGroup.gameObject.activeSelf == false)
        {
            _imgBlueGroup.gameObject.SetActive(false);
            _imgRedGroup.gameObject.SetActive(true);
            _TweenRedBGLight = _RedBGLight.DOBlendableColor(Color.red, 0.5f).SetEase(Ease.InOutElastic).SetLoops(-1);
        }
    }

    void TweenEndTime()
    {
        _TweenBlueLine.Kill();
        _TweenRedLine.Kill();
        _TweenWindMill.Kill();
        _TweenRedBGLight.Kill();
        UI.Tournaments.AddMessage(TournamentsUI.IDs.RQNowConfig);
    }
}
