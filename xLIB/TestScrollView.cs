using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using xLIB;

public class TestScrollView : ScrollViewBase {

    private List<int> m_ItemList = new List<int>();

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < 5; ++i)
        {
            m_ItemList.Add(i);
        }
        Init(OnUpdateItem, null);
        CurrentItemMaxCount = m_ItemList.Count;
    }

    public void OnUpdateItem(int index, GameObject go)
    {
        //LItem item = go.GetComponent<LItem>();
        //item.UpdateData();// = index.ToString();
    }
}
