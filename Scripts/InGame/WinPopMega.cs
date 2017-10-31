using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
public class WinPopMega : WinPopBase
{
    public override void PlayWinPopup(int winRate)
    {
        gameObject.SetActive(true);
        _toggleShare.isOn = true;
        _btnContinue.enabled = true;
        _toggleShare.enabled = true;

        _goBG.SetActive(true);
        _goWinBody.SetActive(true);

        //UpdateWinInfo(Document.Instance.GetMultipleWinValue());
        UpdateWinInfo(winRate);
    }

    public override void StopWinPopup()
    {
        _btnContinue.enabled = false;
        _toggleShare.enabled = false;

        _goBG.SetActive(false);
        _goWinBody.SetActive(false);
        CompletedPopup();

        if (_toggleShare.isOn)
        {
            if (USER.I.IsGuestLogin)
            {
                UI.I.ShowGuestGuide((_id, args) =>
                {
                    if (args[0].ToString() == "ok")
                    {
                        SCENE.I.AddMessage(SCENEIDs.LoginGuestToFacebook);
                    }
                });
                //SOUND.I.Play(DEF.SND.win_cheers);
                gameObject.SetActive(false);
            }
            else
            {
                NET.I.SendReqRegisterBonus((id, msg) =>
                {
                    PK.RegisterBonus.RECEIVE info = JsonConvert.DeserializeObject<PK.RegisterBonus.RECEIVE>(msg);
                    Main.I.GetFBController().Share(
                        DEF.SHARE.GetWinBonusURL(info.data.bonus_code, DEF.eSlotWinType.megaWin),
                        DEF.SHARE.GetWinMegaURL(), () =>
                        {
                            //SOUND.I.Play(DEF.SND.win_cheers);
                            gameObject.SetActive(false);
                        });
                }, NET.I.OnSendReqTimerout, (int)UI.GameMain.gameId, UI.Game.SpinData.winID);
            }
        }
        else
        {
            //SOUND.I.Play(DEF.SND.win_cheers);
            gameObject.SetActive(false);
        }
    }

    public override void UpdateWinInfo(int winRate)
    {
        _txtWinRate.text = winRate.ToString("#,#0");
    }

    public override void OnClickContinue()
    {
        StopWinPopup();
    }
}
