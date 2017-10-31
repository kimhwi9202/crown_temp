using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;

public class UIPopFirstPurchaseOffer : UIPopupBase
{
    public GameObject _objFirstBG;
    public GameObject _objWowBG; // 첫 구매시 띄어줌.
    public Image _imgSaleMsg;


    public Text[] _textCoins;
    public Text[] _textPercentage;
    public Text[] _textPrice;

    PK.GetPurchaseItems.RECEIVE _PurchaseItems = null;
    bool _news = false;


    public override void Initialize()
    {
        //if (Main.I.IsScreen43Ratio()) base.orginalScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (IsInit()) return;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        _news = false;
        SOUND.I.Play(DEF.SND.popup_open);
        _objFirstBG.gameObject.SetActive(true);
        _objWowBG.gameObject.SetActive(false);
    }

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);

        m_id = id;
        eventClose = _eventClose;
        m_args = args;

        if (args != null && args.Length > 0) _news = System.Convert.ToBoolean(args[0]);

        _PurchaseItems = USER.I._PKGetPurchaseItems;

        // 상품 리스트 세팅
        for (int i = 0; i < _PurchaseItems.data.packs.Length; i++)
        {
            _textCoins[i].text = _PurchaseItems.data.packs[i].coins.ToString("#,#0");
            _textPercentage[i].text = string.Format("{0}%", _PurchaseItems.data.packs[i].free_percentage);
            _textPrice[i].text = string.Format("${0:f}", _PurchaseItems.data.packs[i].price);
        }

        if (USER.I.SaleType == eSaleType.normal)
        {
            _imgSaleMsg.gameObject.SetActive(false);
        }
        else
        {
            _imgSaleMsg.gameObject.SetActive(true);
        }
    }

    public void FirstBuyNow()
    {
        _objFirstBG.gameObject.SetActive(false);
        _objWowBG.gameObject.SetActive(true);
    }


    public void click_BuyNow_1()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.SetWaitLoading(true);

        PLATFORM.I.BuyPurchase(_PurchaseItems.data.packs[0], (result, google, apple) => {
            if (result != "failed")
            {
                //Debug.Log("Purchase : ProductID:" + productID + ", receipt:" + receipt);
                NET.I.SendReqPurchase((id, msg) =>
                {
                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());

                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    //FirstBuyNow();
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins, "first", true);
                    UI.SetWaitLoading(false);
                    click_event_x();
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
                    //FirstBuyNow();
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins, "first", true);
                    UI.SetWaitLoading(false);
                    click_event_x();
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[0].product_url);
#endif
            }
        });
    }
    public void click_BuyNow_2()
    {
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
                    //FirstBuyNow();
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins, "first", true);
                    UI.SetWaitLoading(false);
                    click_event_x();
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
                    //FirstBuyNow();
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins, "first", true);
                    UI.SetWaitLoading(false);
                    click_event_x();
                }, NET.I.OnSendReqTimerout, _PurchaseItems.data.packs[1].product_url);
#endif
            }
        });
    }

    public void click_ClaimNow()
    {
        click_event_x();
    }

    public override void click_event_x()
    {
        base.click_event_x();
        // 첫 로그인 뉴스팝업 호출상태가 아니면 취소시에 바로 코인샵 데이터요청후 팝업띄운다.
        if(!_news) UI.I.AddMessage(UI.IDs.RQCoinsStore, "tag","non_sale", "promotion", "");
    }
}
