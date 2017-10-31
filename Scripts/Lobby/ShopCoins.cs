using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

using xLIB;

/// <summary>
/// 
/// </summary>
/// <seealso cref="UnityEngine.MonoBehaviour" />
public class ShopCoins : MonoBehaviour
{
    public UIListViewBase _LViewCoins;
    public LViewPromotion _LViewPromotion;
    public GameObject _objPromotionCode;
    public Text _textLevel;
    public Text _textBonus;
    public InputField _inputPromotion;

    public Button _btnCodeApply;
    public Button _btnCodeCancel;
    public Image _imgCodeLock;

    public Image _imgGuestGuide;

    // Use this for initialization
    void Awake()
    {
        _LViewPromotion.Init(callback_SelectPromotionCode, _objPromotionCode.GetComponent<RectTransform>().anchoredPosition);
    }
    void Start()
    {
        _LViewPromotion.gameObject.SetActive(false);
        _LViewCoins.Initialize();
    }

    void OnEnable()
    {
        if(_imgGuestGuide != null) _imgGuestGuide.gameObject.SetActive(USER.I.IsGuestLogin);
    }

    public void click_x()
    {
        UI.Coins.ShowShopCoins(false);
    }

    /// <summary>
    /// 프로모션 쿠폰 리스트 버튼클릭 ( 쿠폰 리스트 활성/비활성 처리 )
    /// </summary>
    public void click_PromotionCode()
    {
        _LViewPromotion.gameObject.SetActive(!_LViewPromotion.gameObject.activeSelf);
        if (_LViewPromotion.gameObject.activeSelf)
        {
            NET.I.SendReqGetUserPromotionList((id, msg) =>
            {
                USER.I._PKGetUserPromotionList = JsonConvert.DeserializeObject<PK.GetUserPromotionList.RECEIVE>(msg);
                _LViewPromotion.UpdateListItem();
            }, NET.I.OnSendReqTimerout);
        }
    }

    /// <summary>
    /// 프로모션 쿠폰 리스트에서 쿠폰 선택 이벤트
    /// </summary>
    /// <param name="msg">The MSG.</param>
    public void callback_SelectPromotionCode(string code)
    {
        _LViewPromotion.gameObject.SetActive(false);

        _inputPromotion.text = code;

        NET.I.SendReqGetPurchaseItems((id, msg) =>
        {
            PK.GetPurchaseItems.RECEIVE pk = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
            if (pk.data.packs != null)
            {
                USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
                ((LViewCoins)_LViewCoins).UpdatePromotion();

                _btnCodeApply.gameObject.SetActive(false);
                _btnCodeCancel.gameObject.SetActive(true);
                _imgCodeLock.gameObject.SetActive(true);
            }
        }, NET.I.OnSendReqTimerout, "promotion", _inputPromotion.text);
    }

    public void click_CodeApply()
    {
        if(_inputPromotion.text == "SELECT A COUPON") return;
        //"get_purchase_items"
        //"get_user_promotion_list"
        _LViewPromotion.gameObject.SetActive(false);

        NET.I.SendReqGetPurchaseItems((id, msg) =>
        {
            PK.GetPurchaseItems.RECEIVE pk = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
            if (pk.data.packs != null)
            {
                USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
                ((LViewCoins)_LViewCoins).UpdatePromotion();
            }
            else
            {
                _LViewCoins.Initialize();
                _inputPromotion.text = "SELECT A COUPON";
                UI.I.ShowMsgBox("Invalid code!");
            }
        }, NET.I.OnSendReqTimerout, "promotion", _inputPromotion.text);
    }

    public void click_CodeCancel()
    {
        _LViewPromotion.gameObject.SetActive(false);

        _inputPromotion.text = "SELECT A COUPON";
        //"get_purchase_items"
        //"get_user_promotion_list"
        NET.I.SendReqGetPurchaseItems((id2, msg2) =>
        {
            USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg2);
            _LViewCoins.Initialize();
            _btnCodeApply.gameObject.SetActive(true);
            _btnCodeCancel.gameObject.SetActive(false);
            _imgCodeLock.gameObject.SetActive(false);
        }, NET.I.OnSendReqTimerout, "non_sale", "");
    }

    /// 쿠폰코드 입력 이벤트
    /// </summary>
    public void Input_Promotion()
    {
#if LOCAL_DEBUG
        if(Main.I.CheatKey(_inputPromotion.text.ToLower()))
        {
            _inputPromotion.text = "";
            return;
        }
#endif
        if(_btnCodeCancel.gameObject.activeSelf)
        {
            return;
        }

        _LViewPromotion.gameObject.SetActive(false);

        NET.I.SendReqGetPurchaseItems((id, msg) =>
        {
            PK.GetPurchaseItems.RECEIVE pk = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
            if(pk.data.packs != null)
            {
                USER.I._PKGetPurchaseItems = JsonConvert.DeserializeObject<PK.GetPurchaseItems.RECEIVE>(msg);
                _LViewCoins.Initialize();
            }
        }, NET.I.OnSendReqTimerout, "promotion", _inputPromotion.text);
    }
}
