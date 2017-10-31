using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class SubInfoWindow : MonoBehaviour
{
    public Image _imgBody;
    public Text  _textInfo;
    const float OFFSET = 8f;
    bool _isShowing = false;

    void Start()
    {
        ShowInfo(false);
    }

    #region 맴버함수
    /// <summary>
    /// 인포창 Show/Hide 처리
    /// </summary>
    /// <param name="bShow">if set to <c>true</c> [b show].</param>
    public void ShowInfo(bool bShow)
    {
        if (_isShowing == bShow) return;
        _isShowing = bShow;

        _textInfo.text = "";

        float moveY = 0f;
        Ease showEase = Ease.OutExpo;

        if (bShow)
        {
            moveY = OFFSET;
            showEase = Ease.OutExpo;
        }
        else
        {
            moveY = -OFFSET;
            showEase = Ease.InExpo;
        }
        _imgBody.transform.DOMoveY(moveY, 0.5f).SetRelative(true).SetEase(showEase);
    }

    /// <summary>
    /// 정보 업데이트
    /// </summary>
    /// <param name="info">The information.</param>
    public void UpdateInfoText(string info)
    {
        _textInfo.text = info;
    }
    #endregion  // 맴버함수
    
    /*#region 테스트코드
    void Test()
    {
        StartCoroutine(LateTest());
    }

    IEnumerator LateTest()
    {
        yield return new WaitForSeconds(3.0f);
        ShowInfo(true);

        yield return new WaitForSeconds(3.0f);
        ShowInfo(false);

        StartCoroutine(LateTest());
    }
    #endregion  // 테스트코드*/

}
