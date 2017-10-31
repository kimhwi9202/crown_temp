using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LItemSendGift : UIListItemBase {

    protected bool isAppFriend = false;
    public RawImage _imgPhoto;
    public Text _textName;
    public Toggle _toggleCheck;
    protected PK.AppFriends.REData _FriendsInfo = null;
    //bool toggleSwitch = false;

    public long _id;
    protected string _url;

    void Start()
    {
        _toggleCheck.isOn = true;
        //Debug.Log("id:" + _id + "url:" + _url);
        USER.I.AddFacebookPicture(_url, (x) =>
        {
            _imgPhoto.texture = x;
        });
    }


    public void SetData(PK.AppFriends.REData info)
    {
        isAppFriend = true;
        _FriendsInfo = info;
        _id = System.Convert.ToInt64(info.uid);
        _url = info.picture;
        //_textBalance.text = info.balance.ToString("#,#0");
        _textName.text = info.GetName();
        /*
        if (info.id == USER.I.GetUserInfo().GetIdString())
        {
            _toggleCheck.GetComponent<Toggle>().interactable = false;
        }*/
    }

    public void SetToggleCheck(bool check)
    {
        _toggleCheck.isOn = check; 
    }


    // Update is called once per frame
    public void toggle_Check ()
    {
        //toggleSwitch = !toggleSwitch;
        Debug.Log("toggle_Check=" + _toggleCheck.isOn);
    }
    public bool IsOn()
    {
        return _toggleCheck.isOn;
    }

    /// <summary>
    /// 선물 보내고 나서 뒤 프로세서를 모르겠다..??? 나중에 처리할것.
    /// </summary>
    public void click_SendGift()
    {
        List<string> friendIDs = new List<string>();
        friendIDs.Add(_FriendsInfo.uid);

        NET.I.SendReqSendGifts((id, msg) =>
        {
            Debug.Log("LItemFriends::click_SendGift - msg: " + msg);
            //LobbySendGiftsVO gifts = JsonConvert.DeserializeObject<LobbySendGiftsVO>(msg);
        }, NET.I.OnSendReqTimerout, friendIDs);
    }

}
