using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LViewSendGift : UIListViewBase
{
    public int count = 0;
    public Image _imgSearch;
    public Toggle _tgSelectAll;

    public InputField _inputSearch;
    protected string defSearch = "SEARCH FRIENDS...";

    public GameObject _objNoItems;
    public GameObject _objNoFacebookUser;

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
            if (USER.I._PKAppFriends != null)
            {
                for (int i = 0; i < USER.I._PKAppFriends.data.Length; i++)
                {
                    if (USER.I._PKAppFriends.data[i].giftable == true)
                    {
                        UIListItemBase item = base.CreatePrefabItem();
                        ((LItemSendGift)item).SetData(USER.I._PKAppFriends.data[i]);
                        ++count;
                    }
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

    public void click_Send()
    {
        if (USER.I._PKInvitChallengeStatus != null)
        {
            UI.SetWaitLoading(true);

            List<string> IDs = new List<string>();
            for (int i = 0; i < base.GetItemList().Count; i++)
            {
                UIListItemBase item = base.GetItemList()[i];
                LItemSendGift giftItem = ((LItemSendGift)item);
                if (giftItem.IsOn() && giftItem.gameObject.activeSelf)
                {
                    IDs.Add(giftItem._id.ToString());
                }
            }

            if (IDs.Count > 0)
            {
                Main.I.GetFBController().FBSendGift(IDs, (ok, x) =>
                {
                    if (ok == true)
                    {
                        // 전송만 한다.
                        NET.I.SendReqSendGiftChallenge((id, msg) =>
                        {
                            // 친구 리스트 다시 요청
                            NET.I.SendReqAppFriends((id2, msg2) =>
                            {
                                if (!string.IsNullOrEmpty(msg2))
                                {
                                    USER.I.PKReciveSetAppFriends(msg2);
                                    Initialize();
                                }
                                UI.SetWaitLoading(false);

                            }, NET.I.OnSendReqTimerout);
                        }, null, IDs);
                    }
                    else UI.SetWaitLoading(false);
                });
            }
            else UI.SetWaitLoading(false);
        }
    }
    public void click_InviteFriends()
    {
        SOUND.I.Play(DEF.SND.common_click);
        SCENE.I.AddMessage(SCENEIDs.InviteFriends);
        UI.Gift.click_event_x();
    }
    public void click_FBConnect()
    {
        SOUND.I.Play(DEF.SND.common_click);
        SCENE.I.AddMessage(SCENEIDs.LoginGuestToFacebook);
        UI.Gift.click_event_x();
    }

    /// <summary>
    /// Select All 버튼 클릭 (리스트 아이템 동기화 처리)
    /// </summary>
    public void toggle_SelectAll()
    {
        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            ((LItemSendGift)item).SetToggleCheck(_tgSelectAll.isOn);
        }
    }

    public void click_InputCancel()
    {
        _inputSearch.text = defSearch;
        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            ((LItemSendGift)item).gameObject.SetActive(true);
        }
    }
    public void click_InputSearch()
    {
        string name = _inputSearch.text.ToLower();
        if (name.Length <= 0 || name == defSearch.ToLower()) return;

        for (int i = 0; i < base.GetItemList().Count; i++)
        {
            UIListItemBase item = base.GetItemList()[i];
            string itemName = ((LItemSendGift)item)._textName.text.ToLower();
            if (itemName.Contains(name))
            {
                ((LItemSendGift)item).gameObject.SetActive(true);
            }
            else
            {
                ((LItemSendGift)item).gameObject.SetActive(false);
            }
        }
    }
    public void click_InputEndEdit()
    {
        click_InputSearch();
    }

}
