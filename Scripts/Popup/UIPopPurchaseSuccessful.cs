using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIPopPurchaseSuccessful : UIPopupBase
{
    public Text message;
    bool _first = false;
    public GameObject _objPurchaseBG;
    public GameObject _objWowBG;

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

        _objPurchaseBG.gameObject.SetActive(true);
        _objWowBG.gameObject.SetActive(false);

        long coins = System.Convert.ToInt64(args[0].ToString());
        
        if(args.Length > 1 && args[1] != null)
        {
            _first = true;
        }

        if (coins > 0) message.text = coins.ToString("#,#0");
        else message.text = "0";
    }
    public void FirstBuyNow()
    {
        _objPurchaseBG.gameObject.SetActive(false);
        _objWowBG.gameObject.SetActive(true);
    }

    public void click_ClaimNow()
    {
        click_event_x();
    }

    public override void click_event_ok()
    {
        if (_first)
        {
            _first = false;
            FirstBuyNow();
        }
        else base.click_event_ok();
    }

    public override void click_event_x()
    {
        if (_first)
        {
            _first = false;
            FirstBuyNow();
        }
        else base.click_event_x();
    }

}
