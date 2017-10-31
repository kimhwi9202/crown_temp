using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class UIPopupBase : MonoBehaviour
{
    public delegate void delegateClose(int id, params object[] args);
    public delegateClose eventClose;

    public RectTransform popupTarget;
    public bool ignoreTimeScale;
    public float popupTime = 0.2f;
    public System.Action onClose;
    public object[] m_args = null;
    public int m_id = 0;
    public Vector3 orginalScale = new Vector3(1f,1f,1f);

    private bool m_ActiveTween = true;
    protected int m_TweenStyle = 1;  // 스케일 
    protected bool m_Init = false;

    public virtual void Initialize() {
    }
    public bool IsInit() {
        if (m_Init) return true;
        m_Init = true;
        return false;
    }
    
    public void ActiveTween(bool _active)
    {
        m_ActiveTween = _active;
    }

    public void Disactive()
    {
        if(UI.Popup) UI.Popup.ScreenTouchLock(false);
        this.gameObject.SetActive(false);
        if (onClose != null)
            onClose();
        if (eventClose != null)
            eventClose(m_id, m_args);
    }

    protected virtual void OnEnable()
    {
        if (!m_ActiveTween) return;

        if (m_TweenStyle == 1)
        {
            if (popupTarget == null)
                popupTarget = transform as RectTransform;

            popupTarget.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            popupTarget.DOScale(orginalScale, popupTime).SetEase(Ease.OutBack);//.OnComplete(()=>UI.Popup.ScreenTouchLock(true));
        }
    }
    /// <summary>
    /// 팝업 윈도우의 데이터 세팅 함수
    /// 윈도우 Active를 먼저 선행후 데이터 처리를 한다면  MonoBehaviour 함수를 이용할수 있다.
    /// 세팅이후에 Active 처리를 한다면 MonoBehaviour 함수를 이용하면 안된다.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="_eventClose">The event close.</param>
    /// <param name="args">The arguments.</param>
    public virtual void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        m_id = id;
        eventClose = _eventClose;
        m_args = args;
    }

    public virtual void Close(params object[] args)
    {
        m_args = args;

        if (m_ActiveTween && m_TweenStyle == 1)
        {
            popupTarget.DOScale(Vector3.zero, 0.1f).SetEase(Ease.Linear).OnComplete(Disactive);
            //Disactive();
        }
        else Disactive();
    }
    
    // 버튼 이벤트 뒤처리가 필요없을때 간략한 이벤트 처리
    public virtual void click_event_ok()
    {
        SOUND.I.Play(DEF.SND.common_click);
        Close("ok");
    }
    public virtual void click_event_cancel()
    {
        SOUND.I.Play(DEF.SND.common_click);
        Close("cancel");
    }
    public virtual void click_event_x()
    {
        SOUND.I.Play(DEF.SND.common_click);
        Close("x");
    }

}
