using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using xLIB;


public class GameUI : Schedule
{
    public enum IDs {
        None, InGameHandle, ActiveShowInfo, UpdateShowInfo, EventSpin,
    }
    public RectTransform _CabinetTop = null;
    public RectTransform _CabinetBottom = null;
    #region Define HANDLE MESSAGE
    /// <summary>
    /// 사용자레벨, Balance, 총배팅금액 정보 업데이트
    /// </summary>
    public const string OnClick_ToLobby = "OnClick_ToLobby";
    public const string OnClick_Menu = "OnClick_Menu";
    public const string OnClick_Spin = "OnClick_Spin";
    public const string OnClick_SpinLong = "OnClick_SpinLong";
    public const string OnClick_BetMinus = "OnClick_BetMinus";
    public const string OnClick_BetPlus = "OnClick_BetPlus";
    public const string OnClick_MaxBet = "OnClick_MaxBet";
    public const string OnClick_BuyCoin = "OnClick_BuyCoin";
    public const string OnClick_VipBuyCoin = "OnClick_VipBuyCoin";
    public const string OnClick_VipDealCoin = "OnClick_VipDealCoin";
    public const string OnClick_PayTable = "OnClick_PayTable";

    public const string OnClick_DEBUG = "OnClick_DEBUG";
    #endregion

    const string TAG = "GameUI>>";
    public delegate void delHandleMessage(params object[] args);
    public event delHandleMessage eventHandleMessage = null;

    public Button _btnDEBUG;

    // coins button group
    public Button _btnBuyCoins;  /// 일반 결제버튼
    public Button _btnLongBuyCoins;  /// 일반 결제버튼
    public GameObject _objBuyAndDealGroup;
    public Button _btnDeal;  /// deal 결제버튼
    public Text _txtDealTime;   /// 딜 타임
    public Button _btnDealNoTime;

    public GameObject _fxPointer;
    public Image _imgGuestGuide;

    public Button _btnToLobby;  /// 로비로 이동버튼
    public Button _btnMenu;     /// 메뉴 버튼
    public RawImage _spritePhoto;/// 사용자 이미지
    public UserLevelCtrl _ctrlUserLevel;/// 사용자레벨 컨트롤
    public Text _txtBalance;    /// 사용자 balance 값
    public Button _btnPayTable; /// 페이테이블 버튼
    public Text _txtTotalWin;   /// Total win 값
    public Button _btnBetMinus; /// 라인베팅 감소
    public Button _btnBetPlus;  /// 라인베팅 증가
    public Text _txtTotalBet;   /// Total bet 금액
    public Toggle _toggleMaxBet;/// MaxBet 버튼
    public Text _txtMainInfo;   /// 메인인포 텍스트

    public SubInfoWindow _SubInfo;
    public WinBalance _WinBalance;
    public DisplayWinType _DisplayWinType;
    public WinPopup _WinPopup;
    public LevelUpPopup _LevelUpPopup;

    protected bool _LockButton;
    public bool IsLockButton() { return _LockButton; }

    private SpinData _spinData = null;
    public SpinData SpinData {
        get { return _spinData; }
        set { _spinData = value;  }
    }

    /// <summary>
    /// Total Win 값
    /// </summary>
    public long _totalWin = 0;
    /// <summary>
    /// Tween용 total win 값
    /// </summary>
    public long _tweenTotalWin = 0;
    float _speedTweenLong = 1.0f;
    long _tweenBalance = 0;
    Tweener _tweenerTotalWin;
    int _spin_count = 0;

    void Awake()
    {
        _CabinetTop.gameObject.SetActive(false);
        _CabinetBottom.gameObject.SetActive(false);
        USER.I.onUpdatePhoto += onUpdatePhoto;
        USER.I.onUpdateUserInfo += onUpdateUserInfo;
        SetCallback_HandleMessage(ParserCommand);
        if(_imgGuestGuide != null)_imgGuestGuide.gameObject.SetActive(false);
#if LOCAL_DEBUG || UNITY_EDITOR
        _btnDEBUG.gameObject.SetActive(true);
#endif
    }

    void onUpdatePhoto()
    {
        if(USER.I.CurProfileTexture != null)
            _spritePhoto.texture = USER.I.CurProfileTexture;
    }
    void onUpdateUserInfo()
    {
        _txtBalance.text = USER.I.GetUserInfo().GetBalance().ToString("#,#0");
        int level = 0;
        float levelPercent = 0f;
        USER.I.GetUserLevel(out level, out levelPercent);
        _ctrlUserLevel.UpdateUserLevel(level, levelPercent);

        // guest guide
        /*
        if (USER.I.IsGuestLogin)
        {
            if (level > 20) _imgGuestGuide.gameObject.SetActive(true);
        }
        else _imgGuestGuide.gameObject.SetActive(false);
        */
        if (eventHandleMessage != null) eventHandleMessage("OnUpdateUserInfo");
    }

    public void UpdateBuyCoinsUI()
    {
        // Buy & Deal Button
        _objBuyAndDealGroup.gameObject.SetActive(true);
        _btnLongBuyCoins.gameObject.SetActive(false);

        // Non PU ( Buy 버튼만 활성 )
        if (USER.I.IsDealKind(eDealKind.first))
        {
            _btnDeal.gameObject.SetActive(false);
            _btnDealNoTime.gameObject.SetActive(true);
        }
        else // PU & No Sale 일때 딜 버튼 활성
        {
            if (USER.I._PKCheckDeal.data.remaining > 0 && USER.I.GetSaleType() == eSaleType.normal)
            {
                _btnDeal.gameObject.SetActive(true);
                _btnDealNoTime.gameObject.SetActive(false);
            }
            else // 세일기간이다.
            {
                _btnLongBuyCoins.gameObject.SetActive(true);
                _objBuyAndDealGroup.gameObject.SetActive(false);
                _btnDeal.gameObject.SetActive(false);
                _btnDealNoTime.gameObject.SetActive(true);
            }
        }
    }

#region 맴버함수
    /// <summary>
    /// 사용자레벨 업데이트
    /// </summary>
    public bool UpdateUserLevel(long _bounus)
    {
        int level = 0;
        float levelPercent = 0f;
        USER.I.GetUserLevel(out level, out levelPercent);
        _ctrlUserLevel.UpdateUserLevel(level, levelPercent);

        // guest guide
        /*
        if (USER.I.IsGuestLogin)
        {
            if (level > 20) _imgGuestGuide.gameObject.SetActive(true);
        }
        else _imgGuestGuide.gameObject.SetActive(false);
        */

        // 레벨업 상태인가?
        if (level > USER.I.oldUserLevel)
        {
            USER.I.oldUserLevel = level;
            _LevelUpPopup.SetData(level, _bounus);

            Main.I.AppsFlyerEvent(AFInAppEvents.USER,
                    AFInAppEvents.USER_LEVEL, USER.I._PKUserInfo.data.user_level.ToString(),
                    AFInAppEvents.LEVEL_UP, "true",
                    AFInAppEvents.COINS_BALANCE, USER.I._PKUserInfo.data.balance.ToString());

            return true;
        }
        return false;
    }

    /// <summary>
    /// 사용자 balance값 업데이트
    /// </summary>
    public void UpdateBalance()
    {
        // 오링시 구매 팝업 
        if (USER.I.GetUserInfo().Balance <= 0)
        {
            ShowPopupBuyCoins();
        }

        //Debug.Log("GameUI::UpdateBalance -> _tweenBalance: " + _tweenBalance + ", user: " + USER.I.GetUserInfo().Balance);

        DOTween.To(() => _tweenBalance, x => _tweenBalance = x, USER.I.GetUserInfo().Balance, _speedTweenLong).OnUpdate(() =>
        {
            _txtBalance.text = _tweenBalance.ToString("#,#0");
        });
    }

    /// <summary>
    /// Total win 값 업데이트
    /// </summary>
    public void UpdateTotalWin(long totalWin, float tweenSpeed)
    {
        //Debug.Log(TAG + "UpdateTotalWin >> totalWin = " + totalWin);
        _totalWin = totalWin;

        _tweenerTotalWin = DOTween.To(() => _tweenTotalWin, x => _tweenTotalWin = x, totalWin, tweenSpeed).OnUpdate(() =>
        {
            _txtTotalWin.text = _tweenTotalWin.ToString("#,#0");
        });
    }

    public void ResetTotalWin()
    {
        _totalWin = 0;
        _tweenTotalWin = 0;
        _txtTotalWin.text = "0";
    }

    public void StopTotalPayTween()
    {
        //_winBalanceController.StopTween();
        _tweenerTotalWin.Complete();
    }

    /// <summary>
    /// Gets the total win.
    /// </summary>
    /// <returns></returns>
    public long GetTotalWin()
    {
        //Debug.Log(TAG + "GetTotalWin >> _totalWin = " + _totalWin);
        return _totalWin;
    }
#endregion


#region 상단 UI 컴포넌트
    public void OnClickToLobby()
    {
        //Debug.Log(TAG + "OnClickToLobby");
        if (eventHandleMessage != null) eventHandleMessage(OnClick_ToLobby);
        SCENE.I.AddMessage(SCENEIDs.GameToLobby);
    }

    public void OnClickMenu()
    {
        //Debug.Log(TAG + "OnClickMenu");
        if (eventHandleMessage != null) eventHandleMessage(OnClick_Menu);
        UI.Popup.ShowPopup<UIPopSettings>("Settings", 0, null);
    }
#endregion  // 상단 UI 컴포넌트

#region 하단 UI 컴포넌트
    public void OnClickVipBuyCoin()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_VipBuyCoin);
        SOUND.I.Play(DEF.SND.common_click);
        ShowPopupBuyCoins();
    }
    public void OnClickVipDealCoin()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_VipDealCoin);
        SOUND.I.Play(DEF.SND.common_click);
        ShowPopupBuyCoins();
    }

    public void OnClickBuyCoin()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_BuyCoin);
        SOUND.I.Play(DEF.SND.common_click);
        ShowPopupBuyCoins();
    }

    public void OnClickPayTable()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_PayTable);
        UI.PayTable.SwitchActive();
    }

    public void OnClickBetMinus()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_BetMinus);
    }

    public void OnClickBetPlus()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_BetPlus);
    }

    public void OnValueChangeMaxBet()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_MaxBet);
    }

    public void OnClickSpin()
    {
        //Debug.Log("GameUI::OnClickSpin");
        if (eventHandleMessage != null) eventHandleMessage(OnClick_Spin);
    }

    public void OnClickSpinLong()
    {
        //Debug.Log("GameUI::OnClickSpinLong");
        if (eventHandleMessage != null) eventHandleMessage(OnClick_SpinLong);
    }
    public void OnClickDEBUG()
    {
        if (eventHandleMessage != null) eventHandleMessage(OnClick_DEBUG);
    }
    #endregion  // 상단 UI 컴포넌트


    /// <summary>
    /// Shows the popup buy coins.
    /// 밸런스 오링및 구매버튼 클릭시 코인샵 오픈처리
    /// </summary>
    public void ShowPopupBuyCoins()
    {
        UI.I.AddMessage(UI.IDs.TouchLock);
        UI.I.AddMessage(UI.IDs.RQCheckDeal);
        UI.I.AddMessage(UI.IDs.UpdateBuyButton);
        UI.I.AddMessage(UI.IDs.RQGetBuyCoins);
        UI.I.AddMessage(UI.IDs.TouchUnLock);
    }

    /// <summary>
    /// 상황에 따른 특정버튼 Lock
    /// </summary>
    /// <param name="bLock">if set to <c>true</c> [b lock].</param>
    public void LockButton(bool bLock)
    {
        _LockButton = bLock;
        _btnToLobby.interactable = !bLock;
        _btnMenu.interactable = !bLock;

        _btnBuyCoins.interactable = !bLock;
        _btnLongBuyCoins.interactable = !bLock;
        _btnDeal.interactable = !bLock;
        _btnDealNoTime.interactable = !bLock;

        _btnPayTable.interactable = !bLock;
        _btnBetMinus.interactable = !bLock;
        _btnBetPlus.interactable = !bLock;
        _toggleMaxBet.interactable = !bLock;
    }


    public void GameToLobby()
    {
        if (_spin_count > 0)
        {
            Main.I.AppsFlyerEvent(AFInAppEvents.GAME, AFInAppEvents.SPIN_COUNT, _spin_count.ToString());
            _spin_count = 0;
        }
        this.gameObject.SetActive(false);
        _CabinetTop.gameObject.SetActive(false);
        _CabinetBottom.gameObject.SetActive(false);
    }
    public void LobbyToGame()
    {
        _spin_count = 0;
        this.gameObject.SetActive(true);
        _CabinetTop.gameObject.SetActive(true);
        _CabinetBottom.gameObject.SetActive(true);
        UpdateBuyCoinsUI();
        ResetTotalWin();
    }

    private void ParserCommand(Hashtable has)
    {
        IDs _id = (IDs)has["id"].GetHashCode();
#if UNITY_EDITOR
        Debug.Log(Time.frameCount + " <Color=#fff000> GameUI::Parser - " + _id.ToString() + " </Color>");
#endif
        switch (_id)
        {
            case IDs.InGameHandle: { if (eventHandleMessage != null) eventHandleMessage(has["msg"].ToString()); } break;
            case IDs.ActiveShowInfo: _SubInfo.ShowInfo((bool)has["show"]); break;
            case IDs.UpdateShowInfo: _SubInfo.UpdateInfoText(has["info"].ToString()); break;
            case IDs.EventSpin:
                {
                    ++_spin_count;
                    if(_spin_count >= 50)
                    {
                        Main.I.AppsFlyerEvent(AFInAppEvents.GAME, AFInAppEvents.SPIN_COUNT, _spin_count.ToString());
                        _spin_count = 0;
                    }
                }
                break;
        }

        base.remove(_id);
    }
}
