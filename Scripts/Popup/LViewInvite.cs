using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LViewInvite : UIListViewBase
{
    public int count = 0;
    public Image _imgSearch;
    public Toggle _tgSelectAll;

    public InputField _inputSearch;
    protected string defSearch = "SEARCH FRIENDS...";

    public GameObject _objNoItems;
    public GameObject _objNoFacebookUser;

    public GameObject fxStartPoint;

    public override void Initialize()
    {
        count = 0;
        base.RemoveAllItems();
        _objNoItems.SetActive(false);
        _objNoFacebookUser.SetActive(false);

        if (USER.I.IsGuestLogin)
        {
            _objNoFacebookUser.SetActive(true);
            _imgSearch.gameObject.SetActive(false);
            GetComponent<ScrollRect>().viewport.gameObject.SetActive(false);
        }
        else
        {
            if (Main.I.GetFBController()._FBInvitableFriends != null)
            {
                for (int i = 0; i < Main.I.GetFBController()._FBInvitableFriends.data.data.Length; i++)
                {
                    FBInvitableFriendItem friendInfo = Main.I.GetFBController()._FBInvitableFriends.data.data[i];
                    UIListItemBase item = CreatePrefabItem();
                    ((LItemInvite)item).SetData(friendInfo);
                    ++count;
                }
            }
            if (count <= 0) ShowNoItems(true);
            else ShowNoItems(false);
        }
    }

    void ShowNoItems(bool show)
    {
        if (show)
        {
            _objNoItems.gameObject.SetActive(true);
            _imgSearch.gameObject.SetActive(false);
            GetComponent<ScrollRect>().viewport.gameObject.SetActive(false);
        }
        else
        {
            _objNoItems.gameObject.SetActive(false);
            _imgSearch.gameObject.SetActive(true);
            GetComponent<ScrollRect>().viewport.gameObject.SetActive(true);
            _tgSelectAll.isOn = true;
        }
    }

    public void toggle_SelectAll()
    {
        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            ((LItemInvite)item).SetToggleCheck(_tgSelectAll.isOn);
        }
    }

    public void click_Invite()
    {
        if (USER.I._PKInvitChallengeStatus != null)
        {
            SOUND.I.Play(DEF.SND.common_click);
            UI.SetWaitLoading(true);
            List<string> tokenList = new List<string>();

            for (int i = 0; i < base.GetItemList().Count; i++)
            {
                UIListItemBase item = base.GetItemList()[i];
                LItemInvite InviteItem = ((LItemInvite)item);
                if (InviteItem.IsOn() && InviteItem.gameObject.activeSelf)
                {
                    tokenList.Add(InviteItem.GetInviteID());
                }
            }

            if (tokenList.Count > 0)
            {
                Main.I.GetFBController().FBInvite(tokenList, (ok, x) =>
                {
                    if (ok == true)
                    {
                        NET.I.SendReqInvitationChallengeParticipate((id, msg) =>
                        {
                            PK.InviteChallengeParticipate.RECEIVE data = JsonConvert.DeserializeObject<PK.InviteChallengeParticipate.RECEIVE>(msg);

                            if(data.data.user_amount > 0) USER.I.GetUserInfo().Balance = data.data.user_amount;
                            FX.I.PlayCoins(fxStartPoint, Lobby.I._TopMenu._imgCoinIcon.gameObject, () => {
                                // 동전 애니메니션 끝나면 shop 호출
                            });

                            // 리스트 제거
                            List<FBInvitableFriendItem> newData = new List<FBInvitableFriendItem>();
                            for (int i = 0; i < base.GetItemList().Count; i++)
                            {
                                UIListItemBase item = base.GetItemList()[i];
                                LItemInvite InviteItem = ((LItemInvite)item);
                                if (InviteItem.IsOn() == false)
                                {
                                    newData.Add(((LItemInvite)GetItemList()[i])._InviteInfo);
                                }
                            }
                            Main.I.GetFBController()._FBInvitableFriends.data.data = newData.ToArray();
                            Initialize();
                            UI.SetWaitLoading(false);

                        }, NET.I.OnSendReqTimerout, x);
                    }
                    else UI.SetWaitLoading(false);
                });
            }
            else UI.SetWaitLoading(false);
            
        }
    }

    public void click_FBConnect()
    {
        SOUND.I.Play(DEF.SND.common_click);
        SCENE.I.AddMessage(SCENEIDs.LoginGuestToFacebook);
        UI.Gift.click_event_x();
    }


    // Search 
    public void click_InputCancel()
    {
        _inputSearch.text = defSearch;
        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            ((LItemInvite)item).gameObject.SetActive(true);
        }
    }
    public void click_InputSearch()
    {
        string name = _inputSearch.text.ToLower();
        if (name.Length <= 0 || name == defSearch.ToLower()) return;

        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            string itemName = ((LItemInvite)item)._textName.text.ToLower();
            if (itemName.Contains(name))
            {
                ((LItemInvite)item).gameObject.SetActive(true);
            }
            else
            {
                ((LItemInvite)item).gameObject.SetActive(false);
            }
        }
    }
    public void click_InputEndEdit()
    {
        click_InputSearch();
    }
}
