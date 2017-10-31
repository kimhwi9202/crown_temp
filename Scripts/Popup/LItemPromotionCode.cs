using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LItemPromotionCode : MonoBehaviour {

    public System.Action<string> callbackSelectCode;

    public Text _textMsg;
    public Text _textTime;
    string _PromotionCode = "";
    // Use this for initialization

    public void Init(System.Action<string> callback, string pro_code, string msg, long end_time)
    {
        callbackSelectCode = callback;
        _PromotionCode = pro_code;
        _textMsg.text = msg;// string.Format("MEGASTART UPDATE : {0}% BONUS COUPON", msg);
        System.TimeSpan span = new System.TimeSpan(end_time * 10000000L);
        _textTime.text = string.Format("ONE TIIME USE ONLY {0:d} DAY", span.Days);
    }
	
	// Update is called once per frame
	public void click_Select() {
        if (callbackSelectCode != null) callbackSelectCode(_PromotionCode);
	}
}
