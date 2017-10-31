using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIPopMsgBox : UIPopupBase
{
    public Text message;
    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        base.Initialize();
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        m_id = id;
        eventClose = _eventClose;
        m_args = args;
        message.text = args[0].ToString();
    }
}
