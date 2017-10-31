using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIInboxNoticeLView : UIListViewBase
{
    public int count = 0;
    public Image _imgBadage;
    public Text _textBadageCount;
    public override void Initialize()
    {
        for (int i = 0; i < USER.I._PKListGifts.data.Length; i++)
        {
            if (USER.I._PKListGifts.data[i].type == "promotion" ||
                USER.I._PKListGifts.data[i].sender_uid == 777)  // notice tab list
            {
                UIListItemBase item = CreatePrefabItem();
                ((UIInboxItem)item).SetItemInfo(0, USER.I._PKListGifts.data[i]);
                ++count;
                _textBadageCount.text = count.ToString();
            }
        }
        if (count <= 0)
        {
            _imgBadage.gameObject.SetActive(false);
        }
    }

    public override void callback_ItemClick(GameObject obj, params object[] args)
    {
        Debug.Log(obj.name + args[0]);
    }
}
