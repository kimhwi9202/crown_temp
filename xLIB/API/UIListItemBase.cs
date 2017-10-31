using UnityEngine;
using System.Collections;

/// <summary>
/// 리스트 아이템 공통 베이스 클래스
/// 버튼 이벤트 클릭 콜백을 다룬다.
/// </summary>
public class UIListItemBase : MonoBehaviour
{
    #region 추상함수 선언
    /// <summary>
    /// 리스트뷰 화면데이터 업데이트
    /// </summary>
    public virtual void UpdateItem() { }
    #endregion  // 추상함수 선언

    protected UIListViewBase.OnItemClick OnItemClickDelegate { get; set; }

    public void Initialize(Transform parent, UIListViewBase.OnItemClick callback)
    {
        OnItemClickDelegate = callback;
        transform.SetParent(parent);
        transform.localScale = Vector3.one;
        transform.localPosition = new Vector2(0, 0);
        transform.gameObject.SetActive(true);
    }

}
