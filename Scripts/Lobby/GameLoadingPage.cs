using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using DG.Tweening;

using xLIB;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif


/// <summary>
/// 인트로 화면의로그인 과 씬전환 및 게임로딩 페이지
/// </summary>
public class GameLoadingPage : UIPopupBase
{
    public RawImage _imgTitle;
    public Image _imgFadeInOut;
    public AniProgressBar _ProgressBar;
    protected eGameList _GameKind;
    protected float _BeginTime;
    protected string _bundleName;

    public override void SetParamsData(int id, delegateClose _eventClose, params object[] args)
    {
        base.ActiveTween(false);

        m_id = id;
        eventClose = _eventClose;
        m_args = args;
        _ProgressBar.Reset();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_args == null) return;

        _imgFadeInOut.color = Color.black;
        _imgFadeInOut.gameObject.SetActive(true);
        _ProgressBar.gameObject.SetActive(true);
        _ProgressBar.Reset();

        if ((SCENEIDs)m_args[0] == SCENEIDs.LobbyToGame)
        {
            _ProgressBar.SetFillAmount(0.03f);
            _ProgressBar.AutoAddGague(10f, null);
            PK.GamesInfo.REData info = USER.I.GetGameListInfo((int)m_args[1]);

            _GameKind = (eGameList)m_args[1];
            _bundleName = DEF.GetGameBundleName(_GameKind);

            // gamecommon 번들에 넣어둔 이미지 로딩
            Sprite spr = DEF.GetGameLoadingImage(_GameKind);
            if (spr != null)
            {
                _imgTitle.texture = spr.texture;
                _imgFadeInOut.DOFade(0, 0.3f).OnComplete(OnFadeComplete);
            }
            else  // 혹시나 로딩 실패하면 기존 웹다운방식
            {
                DB.I.WebDownloadImage(info.loading_image, (x) =>
                {
                    _imgTitle.texture = x;
                    _imgFadeInOut.DOFade(0, 0.3f).OnComplete(OnFadeComplete);
                });
            }
        }
        else
        {
            Close("cancel");
        }
    }
    /// <summary>
    /// 화면 페이드 인/아웃 완료 호출
    /// </summary>
    void OnFadeComplete()
    {
        _imgFadeInOut.gameObject.SetActive(false);
        _BeginTime = Time.time;
       
        StartCoroutine(BUNDLE.I.DownloadUpdateFromServer(_bundleName, OnUpdateProgress, OnLoadComplete));
    }

    /// <summary>
    /// 프로그래스바 업데이트 처리 
    /// </summary>

    private void OnUpdateProgress(float pRatio)
    {
        if(_ProgressBar.GetFillAmount() >= 1.0f) _ProgressBar.SetFillAmount(1f);
        else _ProgressBar.SetAddFillAmount(pRatio);
    }

    /// <summary>
    /// 게임 로딩 완료 처리 
    /// </summary>
    private void OnLoadComplete()
    {
        // 0.5초 이내로 끝나면 일정 타임 액션처리
        if ((Time.time - _BeginTime) < 2.0f)
        {
            _ProgressBar.AutoAddGague(0.5f, () =>
            {
                LobbyToGame();
                //Close("ok", _GameKind);
            });
        }
        // 게임 프리팹 로드후 처리는 아직 미정
        else LobbyToGame();// Close("ok", _GameKind);
    }

    void LobbyToGame()
    {
        UI.I.LobbyToGame(_GameKind);
        SYSTIMER.I.CheckTiemrCallback(1f, () => {
            Close("ok");
        });
    }
}

