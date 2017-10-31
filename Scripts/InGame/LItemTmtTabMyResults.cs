using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LItemTmtTabMyResults : MonoBehaviour {

    public Text _textDate;
    public Text _textTime;
    public Text _textPlace;

    public void SetInfo(PK.TmtBeforeMyHistory.REDataData data)
    {
        _textDate.text = data.start_date;
        _textTime.text = data.start_time;
        _textPlace.text = string.Format("#{0}",data.rank);
    }
}
