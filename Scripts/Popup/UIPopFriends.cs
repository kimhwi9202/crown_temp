using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

public class UIPopFriends: UIPopupBase
{
    public UIListViewBase _FriendsListView;
    public UIListViewBase _PlayNowListView;

    public Text _textRanked;

    private bool _effectOnOff = false;
    public Image _effec1;
    public Image _effec2;

    public ToggleGroup tg_Group;
    private Toggle tg_select = null;
    private List<Toggle> tg_TabButtons = new List<Toggle>();

    public override void Initialize()
    {
        if (IsInit()) return;

        tg_Group.GetComponentsInChildren<Toggle>(true, tg_TabButtons);
        for (int i = 0; i < tg_TabButtons.Count; i++)
        {
            tg_TabButtons[i].group = tg_Group;
            tg_TabButtons[i].onValueChanged.AddListener(event_ToggleOn);
            tg_TabButtons[i].enabled = true;
        }

        _FriendsListView.Initialize();
        _PlayNowListView.Initialize();
        _PlayNowListView.gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SOUND.I.Play(DEF.SND.popup_open);

        tg_TabButtons[1].isOn = true;
        tg_TabButtons[0].isOn = false;
        event_ToggleOn(true);

        StartCoroutine(coOnOffEffect());
    }


    public void event_ToggleOn(bool toggle)
    {
        if (!toggle) return;

        //Debug.Log("UIPopFriends::event_ToggleOn - " + toggle);
        _FriendsListView.gameObject.SetActive(tg_TabButtons[1].isOn);
        _PlayNowListView.gameObject.SetActive(tg_TabButtons[0].isOn);

        /*
        if(_FriendsListView.gameObject.activeSelf)
        {
            if(((LViewFriends)_FriendsListView).count <= 0)
            {
                _objNoItem.gameObject.SetActive(true);
            }
        }
        if (_PlayNowListView.gameObject.activeSelf)
        {
            if (((LViewPlayNow)_PlayNowListView).count <= 0)
            {
                _objNoItem.gameObject.SetActive(true);
            }
        }*/

        tg_select = tg_TabButtons.Find(x => x.isOn);
        // 랜더링 레이어 순서 변경
        tg_select.gameObject.transform.SetSiblingIndex(5);
    }

    /// <summary>
    /// 탭버튼 내용없을때 친구초대 버튼클릭
    /// </summary>
    public void click_NoItems()
    {
        SOUND.I.Play(DEF.SND.common_click);
    }

    IEnumerator coOnOffEffect()
    {
        yield return new WaitForSeconds(0.5f);

        _effec1.gameObject.SetActive(_effectOnOff);
        _effec2.gameObject.SetActive(!_effectOnOff);

        _effectOnOff = !_effectOnOff;

        yield return StartCoroutine(coOnOffEffect());
    }
}

