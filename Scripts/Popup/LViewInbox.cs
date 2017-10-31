using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LViewInbox : UIListViewBase
{
    public int count = 0;
    public Image _imgNoItems;
    public Image _imgCount;
    public Text _textCount;

    public Image _imgGuestGuide;

    public override void Initialize()
    {
        count = 0;
        _imgCount.gameObject.SetActive(false);
        if (_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(false);
        RemoveAllItems();
        
        for (int i = 0; i < USER.I._PKListGifts.data.Length; i++)
        {
            UIListItemBase item = CreatePrefabItem();
            ((LItemInbox)item).SetItemInfo(1, USER.I._PKListGifts.data[i]);
            ++count;
        }

        if (count <= 0)
        {
            ShowNoItems(true);
        }
        else
        {
            ShowNoItems(false);
            _imgCount.gameObject.SetActive(true);
            _textCount.text = count.ToString();
        }
    }

    void ShowNoItems(bool show)
    {
        if (show)
        {
            if(USER.I.IsGuestLogin)
            {
                if (_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(true);
                _imgNoItems.gameObject.SetActive(false);
                GetComponent<ScrollRect>().viewport.gameObject.SetActive(false);
            }
            else
            {
                if (_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(false);
                _imgNoItems.gameObject.SetActive(true);
                GetComponent<ScrollRect>().viewport.gameObject.SetActive(false);
            }
        }
        else
        {
            if (_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(false);
            _imgNoItems.gameObject.SetActive(false);
            GetComponent<ScrollRect>().viewport.gameObject.SetActive(true);
        }
    }


    public void click_CollectGiftAll()
    {
        UI.SetWaitLoading(true);

        List<long> list = new List<long>();
        for (int i = 0; i < USER.I._PKListGifts.data.Length; i++)
        {
            list.Add(USER.I._PKListGifts.data[i].gift_id);
        }

        if(list.Count <= 0)
        {
            UI.SetWaitLoading(false);
            return;
        }

        long[] gift_array = list.ToArray();

        NET.I.SendReqAcceptGifts((id, msg) =>
        {
            PK.AcceptGifts.RECEIVE receive = JsonConvert.DeserializeObject<PK.AcceptGifts.RECEIVE>(msg);
            if (receive.data.balance > 0)
            {
                USER.I.GetUserInfo().Balance = receive.data.balance;
                USER.I.onUpdateUserInfo();
            }
            NET.I.SendReqListGifts((id2, msg2) => {
                USER.I.SetPKListGifts(msg2);
                Initialize();
                UI.SetWaitLoading(false);
            }, NET.I.OnSendReqTimerout);
        }, NET.I.OnSendReqTimerout, gift_array);
    }
    public void click_InviteFriends()
    {
        SOUND.I.Play(DEF.SND.common_click);

        SCENE.I.AddMessage(SCENEIDs.InviteFriends);
        UI.Gift.click_event_x();
    }
    public override void callback_ItemClick(GameObject obj, params object[] args)
    {
        UI.SetWaitLoading(true);
        // 선물받기 이후의 처리 방식 프로세서를 모른다..
        NET.I.SendReqAcceptGifts((id, msg) =>
        {
            PK.AcceptGifts.RECEIVE receive = JsonConvert.DeserializeObject<PK.AcceptGifts.RECEIVE>(msg);
            // 밸런스 갱신.
            if (receive.data.balance > 0)
            {
                USER.I.GetUserInfo().Balance = receive.data.balance;
                USER.I.onUpdateUserInfo();
            }
            // 일단 전체 리스트 다시 요청
            NET.I.SendReqListGifts((id2, msg2) =>
            {
                USER.I.SetPKListGifts(msg2);
                // 여기서 업데이트 호출은 주 쓰레드 호출 오류 생긴다. (소켓 쓰레드 이기 때문)
                Initialize();
                UI.SetWaitLoading(false);
            }, NET.I.OnSendReqTimerout);

        }, NET.I.OnSendReqTimerout, new long[] { System.Convert.ToInt32(args[1]) });
    }

    // 게스트 유저일경우
    public void click_FBConnect()
    {
        UI.Gift.click_event_x();
        SCENE.I.AddMessage(SCENEIDs.LoginGuestToFacebook);
    }
}
