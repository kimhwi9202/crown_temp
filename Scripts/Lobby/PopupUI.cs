using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using xLIB;

using DG.Tweening;

/// <summary>
/// global define popup ids
/// </summary>

/// <summary>
/// �˾� ������ �Ŵ��� ( ��ü�� ���������� ���� �Ѵ� )
/// ������ �˾� ������� �������� ���ϵ�� ����
/// </summary>
public class PopupUI : MonoBehaviour 
{
    [SerializeField]
    private GameObject _goPopup = null;
    private UIPopNoticeBox _noticeBox = null;   // ������ �ڽ� (������ �ȳ� �޼���) ��ư���� ����
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
    /// �ȳ��޼���â 
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void ShowNoticeBox(string msg, UIPopupBase.delegateClose eventClose)
    {
        ScreenTouchLock(true);
        _noticeBox.SetParamsData(99993, eventClose, msg);
        _noticeBox.gameObject.SetActive(true);
    }

    /// <summary>
    /// Ư�� �˾� ��ü�� ������Ʈ �̸����� ã���� ���
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
    /// �˾�â Ȱ��ȭ�� ���ȴ�. ��� ĵ����UI�� �� ������ ��������.
    /// - ��� ������ �Ʒ��� ���� -
    /// UI.Popup.ShowMsgBox("Successfull Change Character !");
    /// UI.Popup.ShowPopup<UIShopMsgBox>("ShopMsgBox", (int)DEF.ePopupID.ShopMsgBox, event_Close, "�����Ͻðڽ��ϱ� ?");
    /// </summary>
    /// <typeparam name="T"></typeparam> - clsss name
    /// <param name="popName">Name of the pop.</param> - object name
    /// <param name="popID">The pop identifier.</param> - �ĺ��� �ʿ��� ���� ID
    /// <param name="_eventClose">The event close.</param> - ����� ���� �̺�Ʈ �ڵ鷯
    /// <param name="args">The arguments.</param> - �ؽ����̺� ������
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
