using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameListPage : MonoBehaviour {

    public GameObject _prefabIcon;
    private List<ItemPageIcon> _list = new List<ItemPageIcon>();

    public void Clear()
    {
        for (int i=0; i<_list.Count; i++)
        {
            GameObject.DestroyImmediate(_list[i].gameObject, true);
        }
        _list.Clear();
    }
    public void Init(int count)
    {
        Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject obj = xLIB.xHelper.AddChild(this.gameObject, _prefabIcon.gameObject);
            ItemPageIcon icon = obj.GetComponent<ItemPageIcon>();
            icon.SetSelect(false);
            _list.Add(icon);
        }

        _list[0].SetSelect(true);
    }

    public void SetPage(int index)
    {
        if (index < 0 || index >= _list.Count) return;

        for (int i = 0; i < _list.Count; i++)
            _list[i].SetSelect(false);

        _list[index].SetSelect(true);
    }

}
