﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LItemTopBroadCast : UIListItemBase
{
    public GameObject objPhoto;
    public RawImage _imgPhoto;
    public Text _txtMsg;
    public GameObject effect = null;
    PK.WinCast.REData _info = null;
    public Image _imgMega;
    public Image _imgJackpot;

    public void SetInfo(PK.WinCast.REData info)
    {
        if (info != null)
        {
            _info = info;
            _txtMsg.gameObject.SetActive(true);
            _txtMsg.text = string.Format("{0} <color=#FEE900>X{1}</color> {2}", info.userName, info.winMultiply, info.gameName);

            if(info.winType == "magawin")
            {
                _imgMega.gameObject.SetActive(true);
                _imgJackpot.gameObject.SetActive(false);
            }
            else if (info.winType == "jackpot")
            {
                _imgMega.gameObject.SetActive(false);
                _imgJackpot.gameObject.SetActive(true);
            }

            if (this.gameObject.activeSelf)
                StartCoroutine(coLoadPicture(info.pictureURL));
        }
    }
    IEnumerator coLoadPicture(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (this.gameObject.activeSelf && string.IsNullOrEmpty(www.error))
        {
            objPhoto.SetActive(true);
            _imgPhoto.texture = www.texture;
        }
    }

    public bool IsInfo() { return _info != null ? true : false; }

    // 아이템 재배치로 지워야 할 경우
    public void ClearInfo()
    {
        _info = null;
        _txtMsg.gameObject.SetActive(false);
        objPhoto.SetActive(false);
        _imgMega.gameObject.SetActive(false);
        _imgJackpot.gameObject.SetActive(false);
    }
}
