using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

public class LItemTmtRank : MonoBehaviour
{
    public Image _imgBgBase;    
    public Image _imgBgActive;
    public Image _imgSpinToEnter;

    // 1,2등 교체 이펙트
    public Image _imgUpDowney;  
    public Image _imgDnDowney;
    // 2~5등 교체 이펙트
    public Image _imgUpRank;
    public Image _imgDnRank;

    public Image[] _imgTrophy;
    public Image[] _imgRank;

    public Image _PhotoGroup;
    public RawImage _imgPhoto;
    public Text _textPrizePool;
    protected PK.TmtNowRank.REDataData _info = null;

    public long UserId;
    public int rank;
    public bool IsMyData = false;

    private float Height = 58;  // 아이템 높이

    public void Clean()
    {
        _info = null;
        rank = 0;
        IsMyData = false;
        UserId = 0;
        for (int i = 0; i < _imgTrophy.Length; i++) _imgTrophy[i].gameObject.SetActive(false);
        for (int i = 0; i < _imgRank.Length; i++) _imgRank[i].gameObject.SetActive(false);
        _imgBgBase.gameObject.SetActive(false);
        _imgBgActive.gameObject.SetActive(false);
        _imgSpinToEnter.gameObject.SetActive(false);
        _imgUpDowney.gameObject.SetActive(false);
        _imgDnDowney.gameObject.SetActive(false);
        _imgUpRank.gameObject.SetActive(false);
        _imgDnRank.gameObject.SetActive(false);
        _PhotoGroup.gameObject.SetActive(false);
    }

    public void PlayUpRank(System.Action complete)
    {
        _imgUpRank.gameObject.SetActive(true);
        _imgUpRank.GetComponent<Transform>().DOScaleY(1.5f, 3f).OnComplete(() => {
            if (complete != null) complete();
            _imgUpRank.GetComponent<Transform>().DOScaleY(1f, 3f);
        });
    }
    public void PlayDnRank(System.Action complete)
    {
        _imgDnRank.gameObject.SetActive(true);
        _imgDnRank.GetComponent<Transform>().DOScaleY(1.5f, 3f).OnComplete(() => { if (complete != null) complete(); });
    }


    public void SetSpinToEnter(bool active, Texture2D myPic = null)
    {
        _PhotoGroup.gameObject.SetActive(active);
        _imgPhoto.texture = myPic;
        _imgSpinToEnter.gameObject.SetActive(active);
    }

    public void SetInfo(PK.TmtNowRank.REDataData data)
    {
        Clean();
        _info = data;
        UserId = _info.user_id;

        if (USER.I.GetUserInfo().GetId() == _info.user_id)
        {
            _imgPhoto.texture = USER.I.CurProfileTexture;
        }

        if (_info.rank <= 5)
        {
            _imgRank[_info.rank - 1].gameObject.SetActive(true);
            if (_info.rank <= 3) _imgTrophy[_info.rank-1].gameObject.SetActive(true);
        }

        //_textRank.text = _info.rank.ToString();
        if (_info.earned_total > 0)
        {
            _textPrizePool.gameObject.SetActive(true);
            _textPrizePool.text = _info.earned_total.ToString("#,#0");
        }
        else _textPrizePool.gameObject.SetActive(false);

        if(!string.IsNullOrEmpty(_info.picture))
        {
            _PhotoGroup.gameObject.SetActive(true);
            StartCoroutine(coLoadPicture(_info.picture));
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
