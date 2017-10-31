using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using DG.Tweening;

public class CoinsUI : MonoBehaviour
{
    // Coins Shop 
    public ShopCoins  System_ShopCoins;   

    // fb connect , gift
    public GameObject System_GifFacebook;
    public GameObject _objGiftGroup;
    public GameObject _objFacebookGroup;
    public Image _imgInboxCount;
    public Text _InboxCount;

    // Collect Group ( collect event / spin event )
    public GameObject System_CollectSpin;

    public GameObject _objSpinGroup;
    public Button _btnSpin;

    public GameObject _objCoinGroup;
    public Image _BounusTime;
    public Image _BounusCoins;
    public Button _btnCollect;
    public Button _btnReady;
    public Text _BonusCoins;
    public Text _BonusTimer;

    // etc
    public GameObject fxStartPoint;

    void Awake()
    {
        ShowShopCoins(false);
        System_GifFacebook.SetActive(false);
        USER.I.onUpdateInbox = onUpdateInbox;
    }

    public void Initialize()
    {
        System_ShopCoins.gameObject.SetActive(false);
        System_CollectSpin.gameObject.SetActive(false);
    }

    /// <summary>
    /// 첫 로그인후 로비에 진입시 처리
    /// </summary>
    public void FirstAcitveLobby()
    {
        // collect bounus
        EnableCollectBonus();

        SYSTIMER.I.BounusAlram.SetAlramEvent(onSwitchTimeAndConis, 5f);
        SYSTIMER.I.BounusAlram.Play();

        GameToLobby();

        if (USER.I.IsGuestLogin)
        {
            _objGiftGroup.SetActive(false);
            _objFacebookGroup.SetActive(true);
        }
        else
        {
            _objGiftGroup.SetActive(true);
            _objFacebookGroup.SetActive(false);
        }

        onUpdateInbox();
    }

    public void LobbyToGame()
    {
        System_ShopCoins.gameObject.SetActive(false);
        System_CollectSpin.SetActive(false);
        System_GifFacebook.SetActive(false);
    }
    public void GameToLobby()
    {
        System_ShopCoins.gameObject.SetActive(false);
        System_CollectSpin.SetActive(true);
        System_GifFacebook.SetActive(true);
    }


    #region ShopCoins System

    public void ShowShopCoins(bool show)
    {
        System_ShopCoins.gameObject.SetActive(show);
        if (show)
        {
            System_ShopCoins._textLevel.text = USER.I.GetUserInfo().GetUserLevel().ToString();
            int bonus = (int)System.Convert.ToDouble(USER.I._PKGetPurchaseItems.data.packs[0].level_bonus_percentage);
            if (bonus > 0) System_ShopCoins._textBonus.text = bonus.ToString() + "%";
            else System_ShopCoins._textBonus.text = "0%";

            // 스핀은 비활성처리 해야 한다.
            _objSpinGroup.gameObject.SetActive(false);
            // 골드 코인 활성
            _objCoinGroup.gameObject.SetActive(true);
        }

        EnableCollectBonus();

        // 인게임에선 코인샵과 함께 활성/비활성 처리한다.
        if (eView.Game == Main.I.CurrentView)
        {
            System_CollectSpin.SetActive(show);
        }
    }

    #endregion // #region ShopCoins System


    #region Gift & Facebook System
    void onUpdateInbox()
    {
        if (USER.I._PKListGifts.data.Length > 0)
        {
            _imgInboxCount.gameObject.SetActive(true);
            _InboxCount.text = USER.I._PKListGifts.data.Length.ToString();
        }
        else
        {
            _imgInboxCount.gameObject.SetActive(false);
        }
    }

    public void click_Gift()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.PopGift, "tab", "SendGift");
    }

    public void click_FBConnect()
    {
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.PopGift, "tab", "InBox");
    }
    #endregion //Gift & Facebook System


    #region Collect & Spin System

    /// <summary>
    /// Collect event 상태인지 채크
    /// </summary>
    public void EnableCollectBonus()
    {
        if (IsCollectEventON()) // 이벤트 활성됐다.
        {
            // 시간 오버시에는 -값이 올수 있다.. ( 받을 코인값을 여기서 설정해라 )
            _BonusCoins.text = USER.I._PKBonusInfo.data.coins.ToString("#,#0");

            if (IsCollectEventTime()) // 보너스 받을 시간이 남았냐?
            {
                SetReadyButton(false);
                _btnCollect.gameObject.SetActive(false);
                _objCoinGroup.gameObject.SetActive(false);
                _objSpinGroup.gameObject.SetActive(true);

                SYSTIMER.I.GetBonus().BeginRemainTime(onUpdateBonusTimer, USER.I._PKBonusInfo.data.time);
            }
            else
            {
                _objSpinGroup.gameObject.SetActive(false);
                _objCoinGroup.gameObject.SetActive(true);

                // 코인샵이 활성상태에 따라 버튼 활성 분류
                if (System_ShopCoins.gameObject.activeSelf)
                {
                    _btnCollect.interactable = true;
                    _btnCollect.gameObject.SetActive(true);
                    SetReadyButton(false);
                }
                else
                {
                    _btnCollect.gameObject.SetActive(false);
                    SetReadyButton(true);
                }
            }
        }
        else System_CollectSpin.gameObject.SetActive(false);
    }

    void SetReadyButton(bool show)
    {
        if (show)
        {
            _btnReady.gameObject.SetActive(true);
            _btnReady.interactable = true;
            SYSTIMER.I.ReadyAlram.SetAlramEvent(onReadyButtonUpdate, 6f);
            SYSTIMER.I.ReadyAlram.Play();
        }
        else
        {
            _btnReady.gameObject.SetActive(false);
            SYSTIMER.I.ReadyAlram.Pause();
        }
    }

    void onReadyButtonUpdate()
    {
        if(_btnReady.GetComponent<Image>().color.a != 0)
            _btnReady.GetComponent<Image>().DOFade(0, 3f);
        else
            _btnReady.GetComponent<Image>().DOFade(1f, 3f);
    }

    /// <summary>
    /// Collect Event 활성상태인가?
    /// </summary>
    bool IsCollectEventON()
    {
        if (USER.I._PKBonusInfo != null)
            if (USER.I._PKBonusInfo.data.status == "true")
                    return true;
        return false;
    }
    /// <summary>
    /// Collect Event 대기 타임 시간인가?
    /// </summary>
    bool IsCollectEventTime()
    {
        if (IsCollectEventON())
            if (USER.I._PKBonusInfo.data.time > 0)
                return true;
        return false;
    }

    void onSwitchTimeAndConis()
    {
        if (IsCollectEventTime())
        {
            if (_BounusTime.gameObject.activeSelf)
            {
                _BounusTime.gameObject.SetActive(false);
                _BounusCoins.gameObject.SetActive(true);
            }
            else
            {
                _BounusTime.gameObject.SetActive(true);
                _BounusCoins.gameObject.SetActive(false);
            }
        }
    }
    void onUpdateBonusTimer(int val, string time)
    {
        //Debug.Log("onUpdateBonusTimer = val:" + val + ",time:" + time);
        _BonusTimer.text = time;
        if(val <= 0)
        {
            _btnCollect.gameObject.SetActive(true);
        }
    }

    public void click_Spin()
    {
        SOUND.I.Play(DEF.SND.common_click);

        if (USER.I.GetUserInfo().data.daily_spin_enable > 0)
        {
            UI.Popup.ShowPopup<UIPopDailySpin>("DailySpin", 1, null, null);
        }
        else
        {
            UI.I.AddMessage(UI.IDs.PopDailySpinShop);
        }
    }

    public void click_Collect() {
        SOUND.I.Play(DEF.SND.common_click);
        _btnCollect.interactable = false;

        // 보너스 획득 패킷
        NET.I.SendReqCollectBonus((id, msg) =>
        {
            if (!string.IsNullOrEmpty(msg))
            {
                USER.I._PKCollectBonus = JsonConvert.DeserializeObject<PK.CollectBonus.RECEIVE>(msg);
                USER.I.GetUserInfo().Balance = USER.I._PKCollectBonus.data.balance;
                // 코인 이펙트
                FX.I.PlayCoins(fxStartPoint, Lobby.I._TopMenu._imgCoinIcon.gameObject, () => {

                    /// 보너스 정보 갱신 패킷
                    NET.I.SendReqBonusInfo((id2, msg2) =>
                    {
                        if (!string.IsNullOrEmpty(msg2))
                        {
                            USER.I._PKBonusInfo = JsonConvert.DeserializeObject<PK.BonusInfo.RECEIVE>(msg2);
                            EnableCollectBonus();
                        }
                    }, NET.I.OnSendReqTimerout);

                });
            }
        }, NET.I.OnSendReqTimerout);
    }

    public void click_Ready()
    {
        _btnReady.interactable = false;
        SOUND.I.Play(DEF.SND.common_click);
        UI.I.AddMessage(UI.IDs.TouchLock);
        UI.I.AddMessage(UI.IDs.RQCheckDeal);
        UI.I.AddMessage(UI.IDs.UpdateBuyButton);
        UI.I.AddMessage(UI.IDs.RQGetBuyCoins);
        UI.I.AddMessage(UI.IDs.TouchUnLock);
    }

    #endregion //Collect & Spin System

}
