using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using DG.Tweening;

public class MagaWin : MonoBehaviour
{
    bool _lock = false;
    bool _lockSend = false;
    float closeTime = 10f;
    public Text _textWinMultiply;
    public Text _textUserName;
    public RawImage _imgPhoto;

    public GameObject _fxPointer;

    PK.WinCast.REData _info = null;

    public void SetInfo(PK.WinCast.REData info)
    {
        _info = info;
        _lock = false;
        _lockSend = false;
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(170f, 0);
        _textWinMultiply.text = string.Format("x{0:D}", info.winMultiply);
        _textUserName.text = info.userName;
        StartCoroutine(coLoadPicture(info.pictureURL));
        gameObject.GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f);
        StartCoroutine(coCloseTime(closeTime));
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

    IEnumerator coCloseTime(float time)
    {
        yield return new WaitForSeconds(time);
        _lock = true;
        if (!_lockSend)
        {
            gameObject.GetComponent<RectTransform>().DOAnchorPosX(170f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
        }
    }

    public void click_Like()
    {
        if (_lock || _lockSend) return;
        _lockSend = true;

        NET.I.SendWinLike(_info.winID);

        NET.I.SendReqGetBroadCastReward((id, msg) =>
        {
            PK.GetBroadcastReward.RECEIVE pk = JsonConvert.DeserializeObject<PK.GetBroadcastReward.RECEIVE>(msg);
            // 보상금 받는 연출 필요
            if (pk.data.balance > 0) USER.I.GetUserInfo().Balance = pk.data.balance;

            FX.I.PlayLikeCoins(_fxPointer, UI.Game._fxPointer, () =>
            {
                USER.I.onUpdateUserInfo();
                if (!_lock)
                {
                    gameObject.SetActive(false);
                }
            });
        }, NET.I.OnSendReqTimerout, _info.winType);
    }
}
