using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PayTableUI : MonoBehaviour {

    public Image _imgPage;
    protected int _curPage = 0;
    protected Sprite[] _sprite = null;


    void Start()
    {
        _curPage = 0;
    }

    public void SwitchActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void SetPageSprite(Sprite[] spr)
    {
        _sprite = spr;
        _curPage = 0;
        if(spr.Length > 0) _imgPage.sprite = spr[0];
    }

    public void click_Prev()
    {
        SOUND.I.Play(DEF.SND.common_click);
        if (_sprite != null && _sprite.Length > 0)
        {
            --_curPage;
            if (_curPage < 0) _curPage = 0;
            _imgPage.sprite = _sprite[_curPage];
        }
    }
    public void click_BackToGame()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.Game.OnClickPayTable();
    }
    public void click_Next()
    {
        SOUND.I.Play(DEF.SND.common_click);
        if (_sprite != null && _sprite.Length > 0)
        { 
            ++_curPage;
            if (_curPage > _sprite.Length - 1) _curPage = _sprite.Length - 1;
            _imgPage.sprite = _sprite[_curPage];
        }
    }
}
