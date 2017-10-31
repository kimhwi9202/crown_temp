using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using xLIB;

using DG.Tweening;

/// <summary>
/// global define popup ids
/// </summary>

/// <summary>
/// 팝업 윈도우 매니져 ( 객체는 프리팹으로 관리 한다 )
/// 독립된 팝업 윈도우는 프리팹의 차일드로 관리
/// </summary>
public class PopupUI : MonoBehaviour 
{
    [SerializeField]
    private GameObject _goPopup = null;
    private UIPopNoticeBox _noticeBox = null;   // 공지용 박스 (간단한 안내 메세지) 버튼존재 없음
    private List<UIPopupBase> _popList = new List<UIPopupBase>();

    public Image _ScreenBlock = null;

    public void Initialize()
    {
        this.gameObject.GetComponentsInChildren<UIPopupBase>(true, _popList);
        for (int i = 0; i < _popList.Count; i++)
        {
            if (_popList[i].name == "NoticeBox") _noticeBox = (UIPopNoticeBox)_popList[i];
            _popList[i].gameObject.SetActive(false);
        }
        this.gameObject.SetActive(true);

        ScreenTouchLock(false);
    }

    public void ScreenTouchLock(bool _lock)
    {
        _ScreenBlock.gameObject.SetActive(_lock);
    }


    /// <summary>
    /// 안내메세지창 
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void ShowNoticeBox(string msg, UIPopupBase.delegateClose eventClose)
    {
        ScreenTouchLock(true);
        _noticeBox.SetParamsData(99993, eventClose, msg);
        _noticeBox.gameObject.SetActive(true);
    }

    /// <summary>
    /// 특정 팝업 객체를 오브젝트 이름으로 찾을때 사용
    /// </summary>
    /// <param name="popName">Name of the pop.</param>
    /// <returns></returns>
    public UIPopupBase GetPopup(string popName)
    {
        return _popList.Find(x => x.name == popName);
    }


    public void AllHidePopup()
    {
        for (int i = 0; i < _popList.Count; i++)
        {
            _popList[i].gameObject.SetActive(false);
        }
        _noticeBox.gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업창 활성화에 사용된다. 모든 캔버스UI의 최 상위에 보여진다.
    /// - 사용 예제는 아래와 같다 -
    /// UI.Popup.ShowMsgBox("Successfull Change Character !");
    /// UI.Popup.ShowPopup<UIShopMsgBox>("ShopMsgBox", (int)DEF.ePopupID.ShopMsgBox, event_Close, "구매하시겠습니까 ?");
    /// </summary>
    /// <typeparam name="T"></typeparam> - clsss name
    /// <param name="popName">Name of the pop.</param> - object name
    /// <param name="popID">The pop identifier.</param> - 식별이 필요한 고유 ID
    /// <param name="_eventClose">The event close.</param> - 종료시 받을 이벤트 핸들러
    /// <param name="args">The arguments.</param> - 해쉬테이블 데이터
    public void ShowPopup<T>(string popName, int popID, UIPopupBase.delegateClose _eventClose, params object[] args) where T : UIPopupBase
    {
        T script = _popList.Find(x => x.name == popName) as T;
        if(script)
        {
            ScreenTouchLock(true);
            script.Initialize();
            script.gameObject.SetActive(true);
            script.SetParamsData(popID, _eventClose, args);
        }
    }
}
