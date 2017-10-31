using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Big, Megawin 팝업컨트롤 추상클래스
/// </summary>
/// <seealso cref="UnityEngine.MonoBehaviour" />
public abstract class WinPopBase : MonoBehaviour
{
    public GameObject _goWinBody;
    public GameObject _goBG;

    public Text _txtWinRate;
    public Toggle _toggleShare;
    public Button _btnContinue;
        

    #region 추상함수 구현
    /// <summary>
    /// 팝업 정보 업데이트
    /// </summary>
    /// <param name="winRate">The win rate.</param>
    public abstract void UpdateWinInfo(int winRate);
    /// <summary>
    /// 팝업윈도우 시작
    /// </summary>
    public abstract void PlayWinPopup(int winRate);
    /// <summary>
    /// 팝업윈도우 종료
    /// </summary>
    public abstract void StopWinPopup();
    /// <summary>
    /// Continue 버튼 클릭
    /// </summary>
    public abstract void OnClickContinue();
    #endregion  // 추상함수 구현

    #region 맴버함수
    protected void CompletedPopup()
    {
        //PlayWinManager.Instance.AddMessage(PlayWinManager.CHECK_IS_AUTOSPIN);
        UI.Game.AddMessage(GameUI.IDs.InGameHandle, "msg", "winpopup_continue");
    }
    #endregion  // 맴버함수
}
