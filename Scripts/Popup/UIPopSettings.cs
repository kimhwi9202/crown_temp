using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using DG.Tweening;

public class UIPopSettings : UIPopupBase
{
    public RawImage _imgPhoto;
    public Text _textUserName;
    public Text _textUserUID;
    public Toggle _tgSounds;
    public Toggle _tgNotification;
    public Button _btnSignOut;
    public Button _btnInBox;
    public Button _btnFBLogin;

    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (IsInit()) return;
        _imgPhoto.gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SOUND.I.Play(DEF.SND.popup_open);
        _btnInBox.interactable = true;
        _btnFBLogin.interactable = true;

        if (USER.I.CurProfileTexture != null)
        {
            _imgPhoto.gameObject.SetActive(true);
            _imgPhoto.texture = USER.I.CurProfileTexture;
        }
        _textUserName.text = USER.I.GetUserInfo().GetName();
        _textUserUID.text = "UID : "+USER.I.GetUserInfo().GetId().ToString();
        _tgSounds.isOn = PlayerPrefHelper.GetSoundOn();
        _tgNotification.isOn = PlayerPrefHelper.GetNofiOn();

        if(USER.I.IsGuestLogin)
        {
            _btnSignOut.gameObject.SetActive(false);
            _btnInBox.gameObject.SetActive(true);
            _btnFBLogin.gameObject.SetActive(true);
            _btnFBLogin.interactable = true;
        }
        else
        {
            _btnSignOut.gameObject.SetActive(true);
            _btnInBox.gameObject.SetActive(false);
            _btnFBLogin.gameObject.SetActive(false);
        }
    }

    public void click_Sounds(bool on)
    {
        SOUND.I.Play(DEF.SND.common_click);
        PlayerPrefHelper.SetSoundOn(_tgSounds.isOn);
        SOUND.I.SetSoundOn(_tgSounds.isOn);

        if (_tgSounds.isOn == false)
        {
            SOUND.I.PlayAllStop();
            if (eView.Game == Main.I.CurrentView)
            {
                UI.Game.AddMessage(GameUI.IDs.InGameHandle, "msg", "sound_off");
            }
        }
        else
        {
            if (eView.Game == Main.I.CurrentView)
            {
                UI.Game.AddMessage(GameUI.IDs.InGameHandle, "msg", "sound_on");
            }
            else
            {
                if (!SOUND.I.IsPlay(DEF.SND.lobby_bgm))
                    SOUND.I.Play(DEF.SND.lobby_bgm, true);
            }
        } 
    }
    public void click_Notification(bool on)
    {
        SOUND.I.Play(DEF.SND.common_click);
        PlayerPrefHelper.SetNofiOn(_tgNotification.isOn);
    }
    public void click_SignOut()
    {
        SOUND.I.Play(DEF.SND.common_click);
        try
        {
            FB.LogOut();
            PlayerPrefs.DeleteAll();
        }
        catch (System.Exception)
        {
        }

        PlayerPrefHelper.SetLoginType(eLoginType.none);
        SCENE.I.AddMessage(SCENEIDs.GameToLobby);
        SCENE.I.AddMessage(SCENEIDs.FBSignOut);
        Close("x");
    }
    public void click_InBox()
    {
        _btnInBox.interactable = false;
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.PopGift, "tab", "InBox");
        Close("x");
    }
    public void click_FBLogin()
    {
        _btnFBLogin.interactable = false;
        SOUND.I.Play(DEF.SND.common_click);
        SCENE.I.AddMessage(SCENEIDs.GameToLobby);
        SCENE.I.AddMessage(SCENEIDs.GuestToFacebook);
        Close("ok");
    }
    public void click_Terms()
    {
        SOUND.I.Play(DEF.SND.common_click);
        PLATFORM.I.OpenURL_Terms();
        Close("x");
    }
    public void click_Rate()
    {
        SOUND.I.Play(DEF.SND.common_click);
        PLATFORM.I.OpenURL_Rate();
        Close("x");
    }
    public void click_TalkToNancy()
    {
        SOUND.I.Play(DEF.SND.common_click);
        PLATFORM.I.OpenURL_TalkToNancy(USER.I.GetUserInfo().GetId().ToString(), USER.I.GetUserInfo().GetName(), "");
        Close("x");
    }
}

