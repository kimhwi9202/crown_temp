using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using DG.Tweening;

public class UIPopGift : UIPopupBase
{
    public Image _imgSendGiftBG;
    public Image _imgInviteBG;
    public Image _imgInboxBG;

    public UIListViewBase _LVSendGift;
    public UIListViewBase _LVInvite;
    public UIListViewBase _LVInbox;

    public ToggleGroup tg_Group;
    public Toggle[] tg_TabButtons;


    public override void Initialize()
    {
        if (IsInit()) return;

        for (int i = 0; i < tg_TabButtons.Length; i++)
        {
            tg_TabButtons[i].group = tg_Group;
            tg_TabButtons[i].onValueChanged.AddListener(event_ToggleOn);
            tg_TabButtons[i].enabled = true;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SOUND.I.Play(DEF.SND.popup_open);

        // 팝업 오픈시 페북 유저면 친구초대 도전 상태 얻는다.
        // invitation challenge status 
        if(USER.I.IsGuestLogin == false)
        {
            UI.SetWaitLoading(true);
            NET.I.SendReqInvitationChallengeStatus((id, msg) =>
            {
                UI.SetWaitLoading(false);
                USER.I._PKInvitChallengeStatus = JsonConvert.DeserializeObject<PK.InviteChallengeStatus.RECEIVE>(msg);
            }, NET.I.OnSendReqTimerout);
        }
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        m_id = id;
        eventClose = _eventClose;
        m_args = args;

        tg_TabButtons[0].isOn = false;
        tg_TabButtons[1].isOn = false;
        tg_TabButtons[2].isOn = false;

        if (args != null && args.Length > 0)
        {
            if (args[0].ToString() == "SendGift") tg_TabButtons[0].isOn = true;
            else if (args[0].ToString() == "Invite") tg_TabButtons[1].isOn = true;
            else if (args[0].ToString() == "InBox") tg_TabButtons[2].isOn = true;
        }
        event_ToggleOn(true);

        _LVInvite.Initialize();
        _LVInbox.Initialize();
        _LVSendGift.Initialize();
    }

    public void event_ToggleOn(bool toggle)
    {
        if (!toggle) return;

        _imgSendGiftBG.gameObject.SetActive(tg_TabButtons[0].isOn);
        _imgInviteBG.gameObject.SetActive(tg_TabButtons[1].isOn);
        _imgInboxBG.gameObject.SetActive(tg_TabButtons[2].isOn);

        _LVSendGift.gameObject.SetActive(tg_TabButtons[0].isOn);
        _LVInvite.gameObject.SetActive(tg_TabButtons[1].isOn);
        _LVInbox.gameObject.SetActive(tg_TabButtons[2].isOn);
    }

    public void UpdateListView(int tabIdx = 2)
    {
        if(tabIdx==0) _LVInvite.Initialize();
        else if (tabIdx == 1) _LVSendGift.Initialize();
        else
        {
            _LVInvite.Initialize();
            _LVSendGift.Initialize();
        }
    }
}

