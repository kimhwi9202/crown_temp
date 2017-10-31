using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelUpPopup : MonoBehaviour {

    public Text _textlevel;
    public Text _textBonus;
    public GameObject _objBody;
    public Image _imgGuestGuide;
    void Start()
    {
        _objBody.SetActive(false);
        if(_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(false);
    }

    public void SetData(int level, long bonus)
    {
        _objBody.SetActive(true);
        SOUND.I.Play(DEF.SND.popup_open);
        _textlevel.text = level.ToString();
        _textBonus.text = bonus.ToString("#,#0");
        // guest guide
        /*
        if (USER.I.IsGuestLogin)
        {
            if (level > 20) _imgGuestGuide.gameObject.SetActive(true);
        }
        else _imgGuestGuide.gameObject.SetActive(false);
        */
    }

    public void toggle_Share()
    {
    }

    public void click_Continue()
    {
        UI.Game.AddMessage(GameUI.IDs.InGameHandle, "msg", "close_popuplevelup");
        _objBody.SetActive(false);
    }
}
