using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using xLIB;

public class LViewPromotion : ScrollViewBase
{
    private System.Action<string> callback_SelectCode;
    private int _maxLine = 5;
    private Vector2 _pos;
    private List<PK.GetUserPromotionList.REDataList> m_ItemList = new List<PK.GetUserPromotionList.REDataList>();

    public void Init(System.Action<string> callback, Vector2 pos)
    {
        callback_SelectCode = callback;
        _pos = pos;
    }

    public void UpdateListItem()
    {
        ClearAllListItem();
        if (USER.I._PKGetUserPromotionList.data.data == null) return;

        int count = USER.I._PKGetUserPromotionList.data.data.Length;
        for (int i = 0; i < count; i++)
        {
            m_ItemList.Add(USER.I._PKGetUserPromotionList.data.data[i]);
        }
        //Debug.Log(" pro count = " + count);

        Rect rt;
        if (count > _maxLine) rt = GetRectSize(_maxLine);
        else rt = GetRectSize(count);

        GetComponent<RectTransform>().anchoredPosition = new Vector2(rt.x, rt.y);
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
        GetComponent<RectTransform>().sizeDelta = new Vector2(rt.width, rt.height);

        GetComponent<ScrollRect>().viewport.pivot = new Vector2(0.5f, 0.5f);
        GetComponent<ScrollRect>().viewport.sizeDelta = new Vector2(rt.width, rt.height);

        GetComponent<ScrollRect>().content.anchoredPosition = new Vector2(0, 0);


        base.Init(OnUpdateItem, null);
        if(count <= _maxLine) base.ScrollLock(true);
        else base.ScrollLock(false);
        base.CurrentItemMaxCount = m_ItemList.Count;
        base.Reset();
    }

    Rect GetRectSize(int count)
    {
        Rect rt = new Rect();

        rt.x = _pos.x;
        rt.y = _pos.y + (count * 60f) + 5f;
        rt.width = 647f;
        rt.height = count * 60f;

        return rt;
    }

    public void OnUpdateItem(int index, GameObject go)
    {
        LItemPromotionCode item = go.GetComponent<LItemPromotionCode>();
        if (item)
        {
            item.Init(callback_SelectCode, m_ItemList[index].pro_code, m_ItemList[index].pro_name, m_ItemList[index].end_time);
        }
    }
    void ClearAllListItem()
    {
        m_ItemList.Clear();
        CurrentItemMaxCount = 0;
    }

}
