using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LItemTmtTabRankAccount : MonoBehaviour {

    public Text _textRank;
    public Text _textPer;

    public void SetInfo(PK.TmtNowAccount.REDataAccount data)
    {
        _textRank.text = string.Format("# {0}", data.rank);
        _textPer.text = string.Format("{0} %", data.accumulate_per);
    }
}
