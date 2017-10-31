using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using xLIB;
public class TestListView : MonoBehaviour {

    public GameObject _prefabs;
    public ScrollRectEx sr;
    private List<int> m_ItemList = new List<int>();

    // Use this for initialization
    void Start () {
        for (int i = 0; i < 5; ++i)
        {
            m_ItemList.Add(i);
        }
        sr.Init(OnUpdateItem, _prefabs, m_ItemList.Count, new Vector2(1200, 530), Vector2.zero);
    }

    public void OnUpdateItem(int index, GameObject go)
    {
        //LItem item = go.GetComponent<LItem>();
        //item.UpdateData();// = index.ToString();
    }
}
