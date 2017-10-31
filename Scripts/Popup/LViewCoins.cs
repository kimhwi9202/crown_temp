using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using xLIB;

public class LViewCoins : UIListViewBase
{
    public int count = 0;
    public Image _imgBadage;
    public Text _textBadageCount;
    // 오리지널은 세일및 쿠폰 적용 전 데이터다, 쿠폰적용취소시에 원래 데이터로 변환 필요해서 따로 기록해서 사용
    private List<PK.GetPurchaseItems.REDataPacks> original_ItemList = new List<PK.GetPurchaseItems.REDataPacks>();
    string product_url;

    public override void Initialize()
    {
        base.RemoveAllItems();
        if (USER.I._PKGetPurchaseItems != null)
        {
            for (int i = 0; i < USER.I._PKGetPurchaseItems.data.packs.Length; i++)
            {
                UIListItemBase item = base.CreatePrefabItem();
                ((LItemCoins)item).SetInfo(i, USER.I._PKGetPurchaseItems.data.packs[i]);
                original_ItemList.Add(USER.I._PKGetPurchaseItems.data.packs[i]);
            }
            // scroll lock
            if (original_ItemList.Count <= 6)
            {
                GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
            }
            else
            {
                GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Unrestricted;
            }
        }
    }

    public void UpdatePromotion()
    {
        base.RemoveAllItems();
        if (USER.I._PKGetPurchaseItems != null)
        {
            for (int i = 0; i < USER.I._PKGetPurchaseItems.data.packs.Length; i++)
            {
                UIListItemBase item = base.CreatePrefabItem();
                ((LItemCoins)item).SetPromotionInfo(i, original_ItemList[i], USER.I._PKGetPurchaseItems.data.packs[i]);
            }
        }
    }

    public override void callback_ItemClick(GameObject obj, params object[] args)
    {
        PK.GetPurchaseItems.REDataPacks _ItemInfo = (PK.GetPurchaseItems.REDataPacks)args[0];
        product_url = _ItemInfo.product_url;

        PLATFORM.I.BuyPurchase(_ItemInfo, (result, google, apple) => {
            if (result != "failed")
            {
                //Debug.Log("Purchase : ProductID:" + productID + ", receipt:" + receipt);
                NET.I.SendReqPurchase((id, msg) =>
                {
                    Main.I.AppsFlyerEvent_Purchase(PLATFORM.I.GetIAPData());

                    PK.Purchase.RECEIVE info = JsonConvert.DeserializeObject<PK.Purchase.RECEIVE>(msg);

                    USER.I.GetUserInfo().Balance = info.data.balance;
                    USER.I.onUpdateUserInfo();

                    UI.I.AddMessage(UI.IDs.TouchLock);
                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    UI.I.AddMessage(UI.IDs.TouchUnLock);
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins);
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, product_url, google, apple);
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

                    UI.I.AddMessage(UI.IDs.TouchLock);
                    UI.I.AddMessage(UI.IDs.RQCheckDeal);
                    UI.I.AddMessage(UI.IDs.UpdateBuyButton);
                    UI.I.AddMessage(UI.IDs.TouchUnLock);
                    UI.I.AddMessage(UI.IDs.PopPurchaseSuccessful, "coins", info.data.coins);
                    UI.SetWaitLoading(false);
                }, NET.I.OnSendReqTimerout, product_url);
#endif
            }
        });
    }
}
