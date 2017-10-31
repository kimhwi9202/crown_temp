using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class UserLevelCtrl : MonoBehaviour
{
    const string TAG = "UserLevelCtrl";

    #region 유니티 컴포넌트
    /// <summary>
    /// 레벨 게이지용 이미지
    /// </summary>
    public Image _imgLevel;
    /// <summary>
    /// 사용자레벨 텍스트
    /// </summary>
    public Text _txtLevel;
    #endregion  // 유니티 컴포넌트

    #region 맴버변수
    float _speedTween = 0.8f;
    float _tweenUserLevelPercent = 0;
    #endregion  // 맴버변수


    #region 맴버함수
    public void UpdateUserLevel(int userLevel, float userLevelPercent)
    {
        _txtLevel.text = userLevel.ToString("#,#0");
        DOTween.To(() => _tweenUserLevelPercent, x => _tweenUserLevelPercent = x, userLevelPercent, _speedTween).OnUpdate(TweenUpdateUserLevel);
    }

    void TweenUpdateUserLevel()
    {
        _imgLevel.fillAmount = _tweenUserLevelPercent;
    }
    #endregion  // 맴버함수

}
