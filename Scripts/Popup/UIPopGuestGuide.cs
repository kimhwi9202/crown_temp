using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIPopGuestGuide : UIPopupBase
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        m_id = id;
        eventClose = _eventClose;
        m_args = args;
    }
}
