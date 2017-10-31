using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LItemFriends : UIListItemBase
{
    public GameObject _btnSendGift;
    public RawImage _imgPhoto;
    public Image _imgRank;
    public Text _textName;
    public Text _textBalance;
    PK.AppFriends.REData _Info = null;

    public void SetInfo(PK.AppFriends.REData info)
    {
        _Info = info;
        _textName.text = info.GetName();
        _textBalance.text = info.balance.ToString("#,#0");
        if (info.ranking > 3) _imgRank.gameObject.SetActive(false);
        else _imgRank.sprite = DB.Icon.GetFrinedsRankSprite(info.ranking);

        if(info.id == USER.I.GetUserInfo().GetIdString())
        {
            _btnSendGift.GetComponent<Button>().interactable = false;
        }

        USER.I.AddFacebookPicture(info.picture, (x) =>
        {
            _imgPhoto.texture = x;
        });
    }

    /// <summary>
    /// 친구에게 선물 보내주기
    /// </summary>
    public void click_SendGift()
    {
        List<string> friendIDs = new List<string>();
        friendIDs.Add(_Info.uid);

        NET.I.SendReqSendGifts((id, msg) =>
        {
            Debug.Log("LItemFriends::click_SendGift - msg: " + msg);
            //LobbySendGiftsVO gifts = JsonConvert.DeserializeObject<LobbySendGiftsVO>(msg);
        }, NET.I.OnSendReqTimerout, friendIDs);
    }
}
