using UnityEngine;
using System.Collections;

public class DisplayWinType : MonoBehaviour
{
    public GameObject _goBigWin;
    public GameObject _goMegaWin;
    public GameObject _goJackpotWin;

    DEF.eSlotWinType _curWinType = DEF.eSlotWinType.none;

    public void PlayWinTypeEffect(string winType)
    {
        if (_curWinType == DEF.eSlotWinType.bigWin)
            _goBigWin.SetActive(false);

        if (winType == DEF.eSlotWinType.bigWin.ToString())
        {
            _goBigWin.SetActive(true);
            _curWinType = DEF.eSlotWinType.bigWin;
        }
        else if (winType == DEF.eSlotWinType.megaWin.ToString())
        {
            _goMegaWin.SetActive(true);
            _curWinType = DEF.eSlotWinType.megaWin;
        }
        else if (winType == DEF.eSlotWinType.jackpot.ToString())
        {
            _goMegaWin.SetActive(false);
            _goJackpotWin.SetActive(true);
            _curWinType = DEF.eSlotWinType.jackpot;
        }
    }

    public void StopWinTypeEffect()
    {
        //return;
        _curWinType = DEF.eSlotWinType.none;
        //SOUND.I.Play(IGDEF)
        _goBigWin.SetActive(false);
        _goMegaWin.SetActive(false);
        _goJackpotWin.SetActive(false);
    }
}
