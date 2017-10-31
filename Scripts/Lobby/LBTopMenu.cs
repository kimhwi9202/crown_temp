using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

public class LBTopMenu : MonoBehaviour {
    public Image _imgCoinIcon;
    public RawImage _imgPhoto;
    public UserLevelCtrl _ctrlUserLevel;/// 사용자레벨 컨트롤
    public Text _textBalance;
    public Button _btnFacebook;

    // coins button group
    public Button _btnLongBuyCoins;
    public GameObject _objBuyAndDealGroup;
    public Button _btnDeal;
    public Button _btnDealNoTime;
    public Text _textDealTime;

    public GameObject[] objEffect;
    public GameObject objCoinEffect;


    void Awake()
    {
        USER.I.onUpdatePhoto += onUpdatePhoto;
        USER.I.onUpdateUserInfo += onUpdateUserInfo;
    }

    public void Init()
    {
        UpdateBuyCoinsUI();
    }

    // 페이스북 연동및 로그인 여부
    public void UpdateBuyCoinsUI()
    {
        //_btnFacebook.gameObject.SetActive( USER.I.IsGuestLogin );
        //_imgPhoto.gameObject.SetActive(!USER.I.IsGuestLogin);

        // Buy & Deal Button
        _objBuyAndDealGroup.gameObject.SetActive(true);
        _btnLongBuyCoins.gameObject.SetActive(false);

        // Non PU ( Buy 버튼만 활성 )
        if (USER.I.IsDealKind(eDealKind.first))
        {
            _btnDeal.gameObject.SetActive(false);
            _btnDealNoTime.gameObject.SetActive(true);
        }
        else  // PU & No Sale 일때 딜 버튼 활성
        {
            if (USER.I._PKCheckDeal.data.remaining > 0 && USER.I.GetSaleType() == eSaleType.normal)
            {
                _btnDeal.gameObject.SetActive(true);
                _btnDealNoTime.gameObject.SetActive(false);
            }
            else // -1 버튼 히든 ( 세일기간 )
            {
                _btnLongBuyCoins.gameObject.SetActive(true);
                _objBuyAndDealGroup.gameObject.SetActive(false);
                _btnDeal.gameObject.SetActive(false);
                _btnDealNoTime.gameObject.SetActive(true);
            }
        }
    }

    void onUpdateUserInfo()
    {
        _textBalance.text = USER.I.GetUserInfo().GetBalance().ToString("#,#0");

        int level = 0;
        float levelPercent = 0f;
        USER.I.GetUserLevel(out level, out levelPercent);
        _ctrlUserLevel.UpdateUserLevel(level, levelPercent);
        // guest guide
        /*
        if(USER.I.IsGuestLogin)
        {
            if(level > 20) _imgGuestGuide.gameObject.SetActive(true);
        }
        else _imgGuestGuide.gameObject.SetActive(false);
        */
    }
    void onUpdatePhoto()
    {
        if(USER.I.CurProfileTexture) _imgPhoto.texture = USER.I.CurProfileTexture;
    }

    // 코인획득 마지막 액션 이펙트
    public void PlayCoinEffect()
    {
        objCoinEffect.gameObject.SetActive(false);
        objCoinEffect.gameObject.SetActive(true);
        StartCoroutine(coTimeOutCoinEffect());
    }

    IEnumerator coTimeOutCoinEffect()
    {
        yield return new WaitForSeconds(4f);
        objCoinEffect.gameObject.SetActive(false);
    }

    public void ShowEffect(bool show)
    {
        for(int i=0; i<objEffect.Length; i++)
        {
            objEffect[i].gameObject.SetActive(show);
        }
    }


    public void click_Facebook()
    {
        SOUND.I.Play(DEF.SND.common_click);
        SCENE.I.AddMessage(SCENEIDs.GuestToFacebook);
    }
    public void click_LongBuyCoin()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.TouchLock);
        UI.I.AddMessage(UI.IDs.RQCheckDeal);
        UI.I.AddMessage(UI.IDs.UpdateBuyButton);
        UI.I.AddMessage(UI.IDs.RQGetBuyCoins);
        UI.I.AddMessage(UI.IDs.TouchUnLock);
    }

    public void click_BuyCoin()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.TouchLock);
        UI.I.AddMessage(UI.IDs.RQCheckDeal);
        UI.I.AddMessage(UI.IDs.UpdateBuyButton);
        UI.I.AddMessage(UI.IDs.RQGetBuyCoins);
        UI.I.AddMessage(UI.IDs.TouchUnLock);
    }
    public void click_Deal()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.TouchLock);
        UI.I.AddMessage(UI.IDs.RQCheckDeal);
        UI.I.AddMessage(UI.IDs.UpdateBuyButton);
        UI.I.AddMessage(UI.IDs.RQGetBuyCoins);
        UI.I.AddMessage(UI.IDs.TouchUnLock);
    }
    public void click_Settings()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.PopSetting);
    }
}
