using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;

public class UIPopSweetOffer : UIPopupBase
{
    public Text _textExpiresTime;

    public Text[] _textCoins;
    public Text[] _textPercentage;
    public Text[] _textPrice;

    PK.GetPurchaseItems.RECEIVE _PurchaseItems = null;
    bool _news = false;
    bool _buyClick = false;

    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (IsInit()) return;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SOUND.I.Play(DEF.SND.popup_open);
        _news = false;
    }
    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);

        m_id = id;
        eventClose = _eventClose;
        m_args = args;
        _buyClick = false;

        if (args != null && args.Length > 0)
        {
            _news = System.Convert.ToBoolean(args[0]);
        }

        _PurchaseItems = USER.I._PKGetPurchaseItems;

        // 상품 리스트 세팅
        for (int i = 0; i < _PurchaseItems.data.packs.Length; i++)
        {
            _textCoins[i].text = _PurchaseItems.data.packs[i].coins.ToString("#,#0");
            _textPercentage[i].text = string.Format("{0}%", _PurchaseItems.data.packs[i].free_percentage);
            _textPrice[i].text = string.Format("${0:f}", _PurchaseItems.data.packs[i].price);
        }

        StartCoroutine(coUpdateTime());
    }

    IEnumerator coUpdateTime()
    {
        // 이벤트 종료 1초전에 팝업창 닫기 처리
        if (SYSTIMER.I.GetDeal().RemainTime > 1)
        {
            if (eView.Game == Main.I.CurrentView)
            {
                _textExpiresTime.text = "EXPIRES IN  " + UI.Game._txtDealTime.text;
            }
            else // 로비에서 호출
            {
                _textExpiresTime.text = "EXPIRES IN  " + Lobby.I._TopMenu._textDealTime.text;
            }
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(coUpdateTime());
        }
        // 구매 버튼 클릭 상태가 아닐경우 닫기 처리
        else if(_buyClick == false)
        {
            Close("x");
        }
    }


    public void click_BuyCoins_1()
    {
        _buyClick = true;
        SOUND.I.Play(DEF.SND.common_click);
        UI.SetWaitLoading(true);

        PLATFORM.I.BuyPurchase(_PurchaseItems.data.packs[0], (result, google, apple) => {
            if (result != "failed")
            {
                NET.I.SendReqPurchase((id, msg) =>
                {
                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());

                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[0].product_url, google, apple);
            }
            else
            {
                UI.SetWaitLoading(false);
#if UNITY_EDITOR
                NET.I.TestSendReqPurchase((id, msg) =>
                {
                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[0].product_url);
#endif
            }
        });
    }

    public void click_BuyCoins_2()
    {
        _buyClick = true;
        SOUND.I.Play(DEF.SND.common_click);
        UI.SetWaitLoading(true);

        PLATFORM.I.BuyPurchase(_PurchaseItems.data.packs[1], (result, google, apple) => {
            if (result != "failed")
            {
                NET.I.SendReqPurchase((id, msg) =>
                {
                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());

                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[1].product_url, google, apple);
            }
            else
            {
                UI.SetWaitLoading(false);
#if UNITY_EDITOR
                NET.I.TestSendReqPurchase((id, msg) =>
                {
                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[1].product_url);
#endif
            }
        });
    }
    public void click_BuyCoins_3()
    {
        _buyClick = true;
        SOUND.I.Play(DEF.SND.common_click);
        UI.SetWaitLoading(true);

        PLATFORM.I.BuyPurchase(_PurchaseItems.data.packs[2], (result, google, apple) => {
            if (result != "failed")
            {
                NET.I.SendReqPurchase((id, msg) =>
                {
                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());
                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[2].product_url, google, apple);
            }
            else
            {
                UI.SetWaitLoading(false);
#if UNITY_EDITOR
                NET.I.TestSendReqPurchase((id, msg) =>
                {
                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    if (_news) Close("buy", info.data.coins);
                    else { UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins); Close("x"); }
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[2].product_url);
#endif
            }
        });
    }
    public override void click_event_x()
    {
        if (_buyClick) return;
        _buyClick = true;
        if (!_news) UI.I.AddMessage(UI.IDs.RQCoinsStore, "tag", "non_sale", "promotion", "");
        base.click_event_x();
    }
}
