using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LItemInbox : UIListItemBase
{
    private enum eType { promotion, system, normal, giftable, }
    private PK.ListGifts.REData _ItemInfo;
    private eType _type = eType.normal;

    public int _toggleIndex;

    public RectTransform _promotion;
    public Text _promotionContentText;
    public Text _promotionRemainDayText;
    public Text _promotionRemainHourText;

    public RectTransform _system;
    public Text _systemSubjectText;
    public Text _systemContentText;
    public Text _systemRemainTimeText;
    public Image _systemIconImage;

    public RectTransform _normal;
    public Text _normalSubjectText;
    public Text _normalContentText;
    public Text _normalRemainTimeText;
    public Image _normalIconImage;

    public Image _imgPhotoGroup;
    public RawImage _imgPhoto;

    public Button _btnNormalCollect;
    public Button _btnNormalGiftBack;

    public string _PhotoURL = "";

    /// <summary>
    /// 리스트뷰 화면데이터 업데이트
    /// </summary>
    public override void UpdateItem()
    {
        if (_type == eType.promotion)
        {
            _promotionContentText.text = _ItemInfo.message;// InBoxDataHelper.GetAmountString(_ItemInfo.type, _ItemInfo.amount);
            _promotionRemainDayText.text = InBoxDataHelper.GetPromotionDay(_ItemInfo.limit_hour);
            _promotionRemainHourText.text = InBoxDataHelper.GetPromotionHour(_ItemInfo.limit_hour);
        }
        else if (_type == eType.system)
        {
            _systemSubjectText.text = InBoxDataHelper.GetMessageString(_ItemInfo.type, _ItemInfo.name, _ItemInfo.message);
            _systemContentText.text = InBoxDataHelper.GetAmountString(_ItemInfo.type, _ItemInfo.amount);
            _systemRemainTimeText.text = InBoxDataHelper.GetLeftHour(_ItemInfo.limit_hour);
            _systemIconImage.sprite = InBoxDataHelper.GetInboxIconSprite(_ItemInfo.type);
        }
        else if (_type == eType.normal)
        {
            _normalSubjectText.text = InBoxDataHelper.GetMessageString(_ItemInfo.type, _ItemInfo.name, _ItemInfo.message);
            _normalContentText.text = InBoxDataHelper.GetAmountString(_ItemInfo.type, _ItemInfo.amount);
            _normalRemainTimeText.text = InBoxDataHelper.GetLeftHour(_ItemInfo.limit_hour);
            _normalIconImage.sprite = InBoxDataHelper.GetInboxIconSprite(_ItemInfo.type);
        }
        else if (_type == eType.giftable)
        {
            _normalSubjectText.text = _ItemInfo.name;
            _normalContentText.text = InBoxDataHelper.GetAmountString(_ItemInfo.type, _ItemInfo.amount);
            _normalRemainTimeText.text = InBoxDataHelper.GetLeftHour(_ItemInfo.limit_hour);
            _normalIconImage.sprite = InBoxDataHelper.GetInboxIconSprite(_ItemInfo.type);
        }
    }

    public void SetItemInfo(int toggleIndex, PK.ListGifts.REData info)
    {
        _toggleIndex = toggleIndex;
        _ItemInfo = info;
        _promotion.gameObject.SetActive(false);
        _system.gameObject.SetActive(false);
        _normal.gameObject.SetActive(false);

        if (info.type == "promotion")  // apply
        {
            _type = eType.promotion;
            _promotion.gameObject.SetActive(true);
        }
        else if (info.sender_uid == 777)  // collect
        {
            _type = eType.system;
            _system.gameObject.SetActive(true);
        }
        else // normal collect   collect & giftback
        {
            _type = eType.normal;
            _normal.gameObject.SetActive(true);
            _imgPhotoGroup.gameObject.SetActive(false);

            if (USER.I._PKAppFriends != null)  // 내 친구중에 giftable 참인경우의 친구가 보냇을경우는 gift 버튼만 활성
            {
                for (int i = 0; i < USER.I._PKAppFriends.data.Length; i++)
                {
                    if (USER.I._PKAppFriends.data[i].uid == info.sender_uid.ToString() && USER.I._PKAppFriends.data[i].giftable == true)
                    {
                        _type = eType.giftable;
                        Debug.Log("InBox normal = giftable : " + info.sender_uid);
                        break;
                    }
                }
            }

            if (_type == eType.giftable)
            {
                _btnNormalCollect.gameObject.SetActive(false);
                _btnNormalGiftBack.gameObject.SetActive(true);
                _normalIconImage.gameObject.SetActive(false);
                _imgPhotoGroup.gameObject.SetActive(true);
                _PhotoURL = info.picture;
            }
            else
            {
                _PhotoURL = "";
                _normalIconImage.gameObject.SetActive(true);
                _imgPhotoGroup.gameObject.SetActive(false);
                _btnNormalCollect.gameObject.SetActive(true);
                _btnNormalGiftBack.gameObject.SetActive(false);
            }
        }
        UpdateItem();
    }

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(_PhotoURL))
        {
            StartCoroutine(coLoadPicture(_PhotoURL));
        }
    }

    IEnumerator coLoadPicture(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            _imgPhoto.texture = www.texture;
        }
    }


    public void click_Promotion()
    {
        OnItemClickDelegate(this.gameObject, "click_Promotion", _ItemInfo.gift_id);
    }
    public void click_System()
    {
        Main.I.AppsFlyerEvent(AFInAppEvents.FREECOINS,
        AFInAppEvents.FREE_SOURCE, _ItemInfo.message,
        AFInAppEvents.FREE_COINS, _ItemInfo.amount.ToString(),
        AFInAppEvents.FREE_COUNT, "1");

        OnItemClickDelegate(this.gameObject, "click_System", _ItemInfo.gift_id);
    }
    public void click_NormalCollect()
    {
        OnItemClickDelegate(this.gameObject, "click_NormalCollect", _ItemInfo.gift_id);
    }
    public void click_NormalGiftBack()
    {
        OnItemClickDelegate(this.gameObject, "click_NormalGiftback", _ItemInfo.gift_id);
    }
}
