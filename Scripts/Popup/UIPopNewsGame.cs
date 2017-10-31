using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPopNewsGame : UIPopupBase
{
    public RawImage _rawBG;
    public Button _btnX;
    public Button _btnClickToSpin;

    int _value = 0;
    string _url = "";

    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (IsInit()) return;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SOUND.I.Play(DEF.SND.popup_open);
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);

        m_id = id;
        eventClose = _eventClose;
        m_args = args;

        _rawBG.gameObject.SetActive(false);
        _btnX.gameObject.SetActive(false);
        _btnClickToSpin.gameObject.SetActive(false);

        _value = System.Convert.ToInt32(args[1]);

        _url = args[0].ToString();

        StartCoroutine(coLoadPicture(_url));
    }

    IEnumerator coLoadPicture(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            _rawBG.texture = www.texture;
            int width = www.texture.width;
            int height = www.texture.height;

            _rawBG.gameObject.SetActive(true);
            _btnX.gameObject.SetActive(true);
            _btnClickToSpin.gameObject.SetActive(true);

            _rawBG.color = Color.white;
            _rawBG.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            _btnX.GetComponent<RectTransform>().anchoredPosition = new Vector2(width / 2, height / 2);
            _btnClickToSpin.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(height / 2) + 45f);
        }
        else
        {
            _btnX.gameObject.SetActive(true);
        }
    }

    public void click_ClickToSpin()
    {
        Close("ok", _value);
    }
}
