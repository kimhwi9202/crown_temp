using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LItemTmtTabRank : MonoBehaviour {

    public RawImage _imgPhoto;
    public Image[] _imgRank;
    public Text _textName;
    public Text _textPirze;

    public void SetInfo(PK.TmtNowAccount.REDataRank data)
    { 
        for(int i=0; i<_imgRank.Length; i++)   _imgRank[i].gameObject.SetActive(false);

        _textName.text = data.first_name;
        _textPirze.text = data.earned_total.ToString("#,#0");

        if (data.rank < 4) _imgRank[data.rank - 1].gameObject.SetActive(true);
        else if( data.rank >= 4 && data.rank < 40) _imgRank[3].gameObject.SetActive(true);
        else _imgRank[4].gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(data.picture))
        {
            StartCoroutine(coLoadPicture(data.picture));
        }
    }

    IEnumerator coLoadPicture(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            _imgPhoto.texture = www.texture;
        }
    }

}
