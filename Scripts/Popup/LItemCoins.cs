using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LItemCoins : UIListItemBase
{
    public enum eType { green, red, coin1, coin2, coin3, coin4, coin5, coin6, best, most, }

    public Image[] _imgState;
    public Text _textCoins;
    public Text _textRegularCoins;
    public Text _textPrice;
    public Text _textRegularPrice;
    public Button _btnRed;
    public Button _btnGreen;

    public Image[] _imgBackground;

    eType _labelType = eType.green;
    eType _iconType = eType.green;
    int _index = 0;

    PK.GetPurchaseItems.REDataPacks _original_Info = null;
    PK.GetPurchaseItems.REDataPacks _info = null;

    // 일반적인 아이템 리스트 정보 세팅
    public void SetInfo(int index, PK.GetPurchaseItems.REDataPacks info)
    {
        _info = info;
        _original_Info = null;
        _index = index;

        if (index == 2 || index == 5)
        {
            _imgBackground[0].gameObject.SetActive(false);
            _imgBackground[1].gameObject.SetActive(true);
        }
        else
        {
            _imgBackground[0].gameObject.SetActive(true);
            _imgBackground[1].gameObject.SetActive(false);
        }

        if (index == 2 || index == 5) _labelType = eType.red;
        else _labelType = eType.green;

        if (index == 2) _iconType = eType.most;
        else if(index == 5) _iconType = eType.best;
        else _iconType = eType.coin1 + index;

        UpdateItem();
    }
    public override void UpdateItem()
    {
        for (int i = 0; i < _imgState.Length; i++)
            _imgState[i].gameObject.SetActive(false);

        _textCoins.text = _info.coins.ToString("#,#0");
        _textPrice.text = string.Format("${0:f}", _info.price);

        _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 0);
        _textPrice.GetComponent<RectTransform>().anchoredPosition = new Vector2(133f, 0);
        _textRegularCoins.gameObject.SetActive(false);
        _textRegularPrice.gameObject.SetActive(false);

        if (USER.I.GetDealKind() == eDealKind.first)
        {
            _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 0);
            _textRegularCoins.gameObject.SetActive(false);
        }
        else
        {
            if (USER.I.SaleType == eSaleType.normal)
            {
                _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 0);
                _textRegularCoins.gameObject.SetActive(false);
            }
            else
            {
                _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 12f);
                _textRegularCoins.gameObject.SetActive(true);
                _textRegularCoins.text = _info.regular_coins.ToString("#,#0");
            }
        }

        _imgState[(int)_labelType].gameObject.SetActive(true);
        _imgState[(int)_iconType].gameObject.SetActive(true);
    }

    // 프로모션 코드 적용으로 받은 아이템 정보 세팅
    public void SetPromotionInfo(int index, PK.GetPurchaseItems.REDataPacks original_Info,  PK.GetPurchaseItems.REDataPacks info)
    {
        _info = info;
        _original_Info = original_Info;
        _index = index;

        if (index == 2 || index == 5)
        {
            _imgBackground[0].gameObject.SetActive(false);
            _imgBackground[1].gameObject.SetActive(true);
        }
        else
        {
            _imgBackground[0].gameObject.SetActive(true);
            _imgBackground[1].gameObject.SetActive(false);
        }

        if (index == 2 || index == 5) _labelType = eType.red;
        else _labelType = eType.green;

        if (index == 2) _iconType = eType.most;
        else if (index == 5) _iconType = eType.best;
        else _iconType = eType.coin1 + index;

        UpdatePromotionItem();
    }
    /// <summary>
    /// 리스트뷰 화면데이터 업데이트
    /// </summary>
    public void UpdatePromotionItem()
    {
        for (int i = 0; i < _imgState.Length; i++)
            _imgState[i].gameObject.SetActive(false);

        _textCoins.text = _info.coins.ToString("#,#0");
        _textPrice.text = string.Format("${0:f}", _info.price);

        _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 0);
        _textPrice.GetComponent<RectTransform>().anchoredPosition = new Vector2(133f, 0);
        _textRegularCoins.gameObject.SetActive(false);
        _textRegularPrice.gameObject.SetActive(false);

        _imgState[(int)_labelType].gameObject.SetActive(true);
        _imgState[(int)_iconType].gameObject.SetActive(true);

        if (_original_Info.coins != _info.coins)
        {
            _textRegularCoins.gameObject.SetActive(true);
            _textCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(-162f, 12f);
            _textRegularCoins.text = _original_Info.coins.ToString("#,#0");
        }

        if (_original_Info.price != _info.price)
        {
            _textRegularPrice.gameObject.SetActive(true);
            _textPrice.GetComponent<RectTransform>().anchoredPosition = new Vector2(133f, 12f);
            _textRegularPrice.text = string.Format("WAS {0:f}", _original_Info.price);
        }
    }

    public void click_BuyRed()
    {
        UI.SetWaitLoading(true);

        //OnItemClickDelegate(this.gameObject, _info.product_url, _info.id);
        OnItemClickDelegate(this.gameObject, _info);
    }
    public void click_BuyGreen()
    {
        UI.SetWaitLoading(true);

        //OnItemClickDelegate(this.gameObject, _info.product_url, _info.id);
        OnItemClickDelegate(this.gameObject, _info);
    }
}
