using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LBBottomMenu : MonoBehaviour {
    public ToggleGroup tg_Group;
    private Toggle tg_select = null;
    private List<Toggle> tg_TabButtons = new List<Toggle>();

    public Image _imgComingSoon;

    void Awake()
    {
        // all slots / high roller button toogle
        tg_Group.GetComponentsInChildren<Toggle>(true, tg_TabButtons);
        for (int i = 0; i < tg_TabButtons.Count; i++)
        {
            tg_TabButtons[i].group = tg_Group;
            tg_TabButtons[i].onValueChanged.AddListener(event_ToggleOn);
            tg_TabButtons[i].enabled = true;
        }
    }

    public void Init()
    {
    }

    public void event_ToggleOn(bool toggle)
    {
        if (!toggle) return;

        if(tg_TabButtons[0].isOn == true) // toogle_HighRoller
        {
            UI.Popup.ShowNoticeBox("ComingSoon",(id,args)=> {
                if(args[0].ToString() == "x")
                {
                    tg_TabButtons[1].isOn = true;
                }
            });
        }
        else if (tg_TabButtons[1].isOn == true) // toogle_AllSlots
        {
            //Debug.Log("tg_TabButtons[1] = " + tg_TabButtons[1].gameObject.name);
        }
        tg_select = tg_TabButtons.Find(x => x.isOn);
        // 랜더링 레이어 순서 변경
        tg_select.gameObject.transform.SetSiblingIndex(3);
    }
}
