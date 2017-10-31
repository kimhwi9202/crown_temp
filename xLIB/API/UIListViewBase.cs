using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI용 리스트뷰 공통 클래스
/// 리스트 아이템의 생성과 버튼클릭 콜백을 관리
/// </summary>
/// <seealso cref="UnityEngine.MonoBehaviour" />
//[RequireComponent(typeof(ScrollRect))]
//[RequireComponent(typeof(Mask))]
public class UIListViewBase : MonoBehaviour
{
    public delegate void OnItemClick(GameObject obj, params object[] args);
    private List<UIListItemBase> _listItem = new List<UIListItemBase>();
    public UIListItemBase _prefabItem;
    public GameObject _grid;

    public virtual void Initialize() { }
    public virtual void callback_ItemClick(GameObject obj, params object[] args) { }

    /// <summary>
    /// 프리팹 리스트 아이템 생성 (프리팹은 UIListItemBase 를 상속받아야한다)
    /// </summary>
    /// <param name="count">The count.</param>
    public UIListItemBase CreatePrefabItem()
    {
        UIListItemBase item = Instantiate(_prefabItem);
        item.Initialize(_grid.transform, callback_ItemClick);
        _listItem.Add(item);
        return item;
    }

    /// <summary>
    /// 리스트 아이템 모두 제거
    /// </summary>
    public void RemoveAllItems()
    {
        foreach (var i in _listItem)
            Destroy(i.gameObject);
        _listItem.Clear();
    }

    public List<UIListItemBase> GetItemList() { return _listItem; }
}
