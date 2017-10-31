using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

/// <summary>
///  https://d3kjdk8bsa0don.cloudfront.net/images/game_icons/150x150/game_box_38_02.png
///  https://d3kjdk8bsa0don.cloudfront.net/images/event/event_newyear.png
///  https://d3kjdk8bsa0don.cloudfront.net/images/loading/ingame/game_lodding_40.png
/// </summary>
public class GameLItemCabinet : MonoBehaviour
{
    public enum eState
    {
        None,       // 클라이언트에 없는 게임
        Download,   // 번들 다운로드 필요한 게임
        Play,       // 바로 게임 플레이 가능
        ComingSoon,
        BeginDownloads,
    }
    protected System.Action<int> callbackSelected;

    public eState _state = eState.None;

    public EffectBalance textJackpot;
    public Image imgFeatured;
    public Image imgNew;
    public Image imgPopular;

    public GameObject objState;
    public Image imgComingSoon;
    public Image imgShadow;
    public Image imgDownloads;
    public Image imgLock;

    public Image imgLoading;
    public Image imgLoadBar;
    public Image imgLoadLight;
    public Image imgLoadLabel;

    public Text textPercent;

    private PK.GamesInfo.REData _Info = null;
    public int _id = 0;
    public string _url;
    public string _bundleName;

    private long saveJackpot = 0;
    private long curUpdateTime = 0;
    private CabinetEffect effect = null;
    protected Image imgBody = null;

    protected float _playTime = 2f;
    protected float _time;
    float _tempfill = 0;
    bool IsGameDownloading = false;

    void Awake()    {    }

    void OnEnable()
    {
        if (_Info != null)
        {
            if (DEF.IsUseGameID((eGameList)_id))
            {
                if (_Info.status == "active")
                {
                    // 게임 번들 존재 여부 확인
                    _bundleName = DEF.GetGameBundleName((eGameList)_id);
                    AssetBundle bundle = xLIB.BUNDLE.I.GetBundle(_bundleName);
                    if (bundle != null)
                    {
                        SetState(eState.Play);
                    }
                    else if (xLIB.BUNDLE.I.IsLocalFileCached(_bundleName))
                    {
                        SetState(eState.Play);
                    }
                    else
                    {
                        SetState(eState.Download);
                    }
                    // 잭팟 액션 시작
                    UpdateJackpotPool();
                }
                else if (_Info.status == "coming_soon")
                {
                    SetState(eState.ComingSoon);
                }
                else if (_Info.status == "maintenance")
                {

                }
            }
            else
            {
               _url = "";
               _bundleName = "";
                InitState();
            }
        }
    }
    private T CreateUIPrefab<T>(RectTransform parent, string value) where T : Object
    {
        T result = null;
        GameObject go = null;

        go = xLIB.BUNDLE.I.LoadAsset<GameObject>(value);
        if (go != null)
        {
            go = GameObject.Instantiate(go);
            go.GetComponent<RectTransform>().SetParent(parent);
            go.GetComponent<Transform>().localPosition = Vector3.zero;
            go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            go.GetComponent<RectTransform>().transform.localRotation = Quaternion.identity;
            go.GetComponent<RectTransform>().transform.localScale = Vector3.one;
            go.SetActive(true);

            result = go.GetComponent<T>();
        }
        return result;
    }
    // 리스트 공백용 or 게임 리소스 준비안될때 
    public void InitDefault()
    {
        _Info = null;
        _id = 0;
        _url = "";
        _bundleName = "";
        this.gameObject.name = "none";
        callbackSelected = null;

        if (imgBody != null)
        {
            Destroy(imgBody.gameObject);
            imgBody = null;
        }
        if (effect != null)
        {
            Destroy(effect.gameObject);
            effect = null;
        }

        this.gameObject.SetActive(true);
        saveJackpot = 0;
        textJackpot.gameObject.SetActive(false);
        imgComingSoon.gameObject.SetActive(false);
        imgBody = CreateUIPrefab<Image>(this.GetComponent<RectTransform>(), "CabinetBody");
        imgBody.transform.SetSiblingIndex(0);

        SetState(eState.None);
    }
    // 이 케비넷 아이템은 재활용되므로 초기화시 기존 프리팹 생성들은 제거해야 한다.
    public void Init(PK.GamesInfo.REData info, System.Action<int> callback)
    {
        if (imgBody != null)
        {
            Destroy(imgBody.gameObject);
            imgBody = null;
        }
        if (effect != null)
        {
            Destroy(effect.gameObject);
            effect = null;
        }
        this.gameObject.name = info.game_id.ToString();

        this.gameObject.SetActive(true);

        imgBody = CreateUIPrefab<Image>(this.GetComponent<RectTransform>(), "CabinetBody_" + info.game_id.ToString());
        if (imgBody != null)
        {
            _Info = info;
            callbackSelected = callback;
            _id = info.game_id;
            _url = info.icon;

            curUpdateTime = SYSTIMER.GetCurTime();

            ClacJackpotPool();

            imgBody.transform.SetSiblingIndex(0);
            effect = CreateUIPrefab<CabinetEffect>(this.GetComponent<RectTransform>(), "CabinetEffect_" + _id.ToString());
            if (effect != null)
            {
                effect.transform.SetSiblingIndex(1);
                effect.GetComponent<RectTransform>().anchoredPosition = new Vector2(-117.8f, -40.9f);
                effect.enabled = false;
            }

            OnEnable();
            SetTag(_Info.tag);
        }
        else
        {
            InitDefault();
        }
    }

    // 아이템 활성화시 첫 구성에 필요한 작업을 한다.
    public void ClacJackpotPool()
    {
        saveJackpot = 0;

        if (_Info != null &&  this.gameObject.activeSelf)
        {
            // 리스트뷰에서 일정시간에 게임 리스트를 요청했을거다..
            // 갱신 타임에 한번씩 잭팟정보를 확인한다.
            _Info = USER.I.GetGameListInfo(_Info.game_id);
            if (_Info != null)
            {
                if (_Info.jackpot_pool > 0)
                {
                    textJackpot.gameObject.SetActive(true);
                    // 1/10 뺀금액을 시작금액으로 일정횟수만큼 갱신효과를 위해
                    // 차액이 1/10억보다 부족할경우 처리필요
                    long temp = _Info.jackpot_pool / 10;// 1000000000;
                    saveJackpot = _Info.jackpot_pool - textJackpot.GetCurBalance();  // 현재 잭팟의 차액
                    if (saveJackpot > temp) saveJackpot = temp;  // 차액이 크다면 기본 1/10억으로 맞춘다.
                    textJackpot.SetBalance(_Info.jackpot_pool - saveJackpot);
                }
                else
                {
                    textJackpot.gameObject.SetActive(false);
                }
            }
        }
        else textJackpot.gameObject.SetActive(false);
    }


    // 일정시간에 업데이트 횟수만큼 액션을 보여준다. 
    // 일정시간 이후 새로운 패킷을 요청 새롭게 시작처리
    // 이 함수는 현재 선택된 게임 리스트의 아이템만 활성화 운영된다.
    void UpdateJackpotPool()
    {
        if (!textJackpot.gameObject.activeSelf) return;

        // 일정시간에 서버 갱신 데이터 업데이트 ( 5분30초에 한번씩 갱신된 잭팟 정보 갱신 )
        // 상위 클래스 리스트뷰에서 패킷을 5분에 한번씩 갱신한다.
        if(SYSTIMER.GetCurTime() - curUpdateTime > 630f) 
        {
            curUpdateTime = SYSTIMER.GetCurTime();
            ClacJackpotPool();
            if (saveJackpot <= 0)
            {
                Invoke("UpdateJackpotPool", 30f);
                return;
            }
        }

        // 기본적으로 남은금액의 1/10 
        long temp = saveJackpot / 10;

        // 1/10 금액에서도 랜덤값으로 일정금액 정의
        long random_pool = temp / Random.Range(2, 10);

        // 숫자 밸런스 액션에 필요한 값만큼 차감
        if (random_pool < saveJackpot) saveJackpot -= random_pool;
        else random_pool = saveJackpot;

        // 랜덤 액션 타임 정의 (활성화된 게임 리스트가 동일한 행동을 피하기 위함)
        float delay = Random.Range(5, 10);

        long target = textJackpot.GetCurBalance() + random_pool;

        textJackpot.PlayTweenBalance(() => 
        {
            UpdateJackpotPool();
        }, textJackpot.GetCurBalance(), target, 1f, delay);
    }

    void InitState()
    {
        if (effect != null)
        {
            effect._anim.enabled = false;
            effect._titleAni.gameObject.SetActive(false);
        }
        imgFeatured.gameObject.SetActive(false);
        imgNew.gameObject.SetActive(false);
        imgPopular.gameObject.SetActive(false);

        imgComingSoon.gameObject.SetActive(false);
        imgShadow.gameObject.SetActive(false);
        imgDownloads.gameObject.SetActive(false);
        imgLock.gameObject.SetActive(false);

        imgLoading.gameObject.SetActive(false);
        imgLoadBar.gameObject.SetActive(false);
        imgLoadLight.gameObject.SetActive(false);
        imgLoadLabel.gameObject.SetActive(false);
    }

    void SetState(eState state)
    {
        _state = state;

        InitState();

        switch (state)
        {
            case eState.None:
                break;
            case eState.ComingSoon:
                {
                    textJackpot.gameObject.SetActive(false);
                    imgComingSoon.gameObject.SetActive(true);
                    imgShadow.gameObject.SetActive(true);
                }
                break;
            case eState.Play:
                {
                    SetTag(_Info.tag);
                    if (effect != null)
                    {
                        effect._anim.enabled = true;
                        effect._titleAni.gameObject.SetActive(true);
                    }
                }
                break;
            case eState.Download:
                {
                    SetTag(_Info.tag);
                    imgShadow.gameObject.SetActive(true);
                    imgDownloads.gameObject.SetActive(true);
                }
                break;
            case eState.BeginDownloads:
                {
                    InitState();
                    imgShadow.gameObject.SetActive(true);
                    imgLoading.gameObject.SetActive(true);
                    imgLoadBar.gameObject.SetActive(true);
                    imgLoadLight.gameObject.SetActive(true);
                    imgLoadLabel.gameObject.SetActive(true);
                }
                break;
        }
    }


    public void SetLock(bool _lock)
    {
        imgLock.gameObject.SetActive(_lock);
        if (imgLock.gameObject.activeSelf)
        {
            imgShadow.gameObject.SetActive(true);
        }
        else
        {
            imgShadow.gameObject.SetActive(false);
        }
    }

    public void SetTag(string _tag)
    {
        imgFeatured.gameObject.SetActive(false);
        imgPopular.gameObject.SetActive(false);
        imgNew.gameObject.SetActive(false);
        if (_tag == "featured") imgFeatured.gameObject.SetActive(true);
        else if (_tag == "new") imgNew.gameObject.SetActive(true);
        else if (_tag == "popular") imgPopular.gameObject.SetActive(true);
    }

    public void BeginDownloads(bool _begin)
    {
        imgLoading.gameObject.SetActive(false);
        imgDownloads.gameObject.SetActive(_begin);
        if (_begin)
        {
            objState.gameObject.SetActive(true);
            imgBody.color = new Color32(104, 104, 104, 255);
        }
        else
        {
            objState.gameObject.SetActive(true);
            imgBody.color = Color.white;
        }
    }

    /// <summary>
    /// 게임 리스트 클릭했다.
    /// </summary>
    public void click_OnClick()
    {
        if (IsGameDownloading) return;

        if (_state == eState.Download)
        {
            // 메뉴 활동 초기화 .. 로딩 완료시 참조가 0 이면 바로 게임 으로 넘어간다.
            if (DEF.GameDownloadingCount < 4)  //3개만
            {
                DEF.GameDownloadingCount++;
                LoadGameBundle();
            }
        }
        else if (_state == eState.Play && DEF.GameDownloadingCount == 0)
        {
            UI.SetWaitLoading(true);
            DG.Tweening.Sequence mySeq = DOTween.Sequence();
            mySeq.Append(this.gameObject.GetComponent<Transform>().DOScale(0.8f, 0.05f).SetEase(Ease.OutSine));
            mySeq.Append(this.gameObject.GetComponent<Transform>().DOScale(1f, 0.05f).SetEase(Ease.OutSine));
            mySeq.Play().OnComplete(() => {
                SCENE.I.AddMessage(SCENEIDs.LobbyToGame, "game", _Info.game_id);
            });
        }
    }

    void EndSpinRotate()
    {
        imgLoading.gameObject.SetActive(false);
    }


    #region LoadBundle
    private void LoadGameBundle()
    {
        SetState(eState.BeginDownloads);
        imgLoadBar.fillAmount = 0;

        _playTime = 30f;
        _time = Time.time;
        _tempfill = imgLoadBar.fillAmount;

        StartCoroutine(coUpdateLoadBar());
        
        string bundleName = DEF.GetGameBundleName((eGameList)_id);
        StartCoroutine(xLIB.BUNDLE.I.DownloadUpdateFromServer(bundleName, null, OnLoadComplete, OnLoadError));
    }

    IEnumerator coUpdateLoadBar()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (imgLoadBar.fillAmount >= 1.0f) yield break;
            OnUpdateProgress(_tempfill + ((Time.time - _time) / _playTime));
        }
    }

    private void OnUpdateProgress(float pRatio)
    {
        if (pRatio >= imgLoadBar.fillAmount)
        {
            textPercent.text = string.Format("{0:D}%", (int)(pRatio * 100f));
            if (imgLoadBar.fillAmount >= 1.0f) textPercent.text = "100%";
            imgLoadBar.fillAmount = pRatio;// + 0.1f;
            imgLoadLight.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, -(imgLoadBar.fillAmount * 360f));
        }
    }
    private void OnLoadError(string errorMsg)
    {
        
        imgLoadBar.fillAmount = 1f; // coUpdateLoadBar stop
        IsGameDownloading = false;
        DEF.GameDownloadingCount -= 1;
        UI.I.ShowMsgBox("Faild! Downloads Game : " + errorMsg, (id,args)=> {
            imgLoadBar.fillAmount = 0;
            SetState(eState.Download);
        });
    }
    private void OnLoadComplete()
    {
        if(imgLoadBar.fillAmount < 1.0f)
        {
            // 프로그래스바 다 안찾다.. 
            StopCoroutine(coUpdateLoadBar());

            _playTime = 3f;
            _time = Time.time;
            _tempfill = imgLoadBar.fillAmount;

            StartCoroutine(coAutoComplete());
        }
        else
        {
            imgLoadBar.fillAmount = 1.0f;
            IsGameDownloading = false;
            DEF.GameDownloadingCount -= 1;
            SetState(eState.Play);
        }
    }

    // 게이지 자동으로 풀 처리후 완료처리
    IEnumerator coAutoComplete()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (imgLoadBar.fillAmount >= 1.0f)
            {
                imgLoadBar.fillAmount = 1.0f;
                IsGameDownloading = false;
                DEF.GameDownloadingCount -= 1;
                SetState(eState.Play);
                yield break;
            }
            else OnUpdateProgress(_tempfill + ((Time.time - _time) / _playTime));
        }
    }

    #endregion
}

