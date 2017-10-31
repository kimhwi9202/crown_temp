using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReWinLike : MonoBehaviour {
    public Image[] _bgList;
    public Image[] _titleList;
    public RawImage _imgPhoto;
    public Text _userName;
    bool _lock = false;
    float closeTime = 5f;
    PK.WinLike.REData _info = null;

    public void SetInfo(PK.WinLike.REData info)
    {
        _info = info;

        for (int i = 0; i < _bgList.Length; i++) _bgList[i].gameObject.SetActive(false);
        for (int i = 0; i < _titleList.Length; i++) _titleList[i].gameObject.SetActive(false);

        int idxBG = Random.Range(0, _bgList.Length);
        int idxTitle = Random.Range(0, _titleList.Length);

        _bgList[idxBG].gameObject.SetActive(true);
        _titleList[idxTitle].gameObject.SetActive(true);

        _userName.text = info.first_name;
        if (_info != null)
        {
            StartCoroutine(coLoadPicture(_info.url));
            StartCoroutine(coCloseTime(closeTime));
        }
    }
/*
    void OnEnable()
    {
        if (_info != null)
        {
            StartCoroutine(coLoadPicture(_info.url));
            StartCoroutine(coCloseTime(closeTime));
        }
    }
    */

    IEnumerator coLoadPicture(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            _imgPhoto.texture = www.texture;
        }
    }

    IEnumerator coCloseTime(float time)
    {
        yield return new WaitForSeconds(time);
        _lock = true;
        gameObject.SetActive(false);
        DestroyImmediate(gameObject);
    }
}
