using UnityEngine;
using System.Collections;

public class WinPopup : MonoBehaviour
{
    public WinPopBig _dlgBigWin;
    public WinPopMega _dlgMegaWin;
    public WinPopJackpot _dlgJackpotWin;

    public bool m_bIsFreeSpinCompleted = false;

    void Start()
    {
        _dlgBigWin.gameObject.SetActive(false);
        _dlgMegaWin.gameObject.SetActive(false);
        _dlgJackpotWin.gameObject.SetActive(false);
    }


    public void PlayPopup(string winType, int winRate)
    {
        SOUND.I.Play(DEF.SND.popup_open);

        if (winType == DEF.eSlotWinType.bigWin.ToString())
        {
            _dlgBigWin.PlayWinPopup(winRate);
        }
        else if(winType == DEF.eSlotWinType.megaWin.ToString())
        {
            _dlgMegaWin.PlayWinPopup(winRate);
        }
        else if (winType == DEF.eSlotWinType.jackpot.ToString())
        {
            _dlgJackpotWin.PlayWinPopup(winRate);
        }

        
    }

    public void Set_m_bIsFreeSpinCompleted_true()
    {
        m_bIsFreeSpinCompleted = true;
    }
}
