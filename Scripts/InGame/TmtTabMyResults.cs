using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;

// Tournaments TabMyResults Menu
public class TmtTabMyResults : MonoBehaviour
{
    private bool _IsHide = true;
    private bool _btnLock = false;
    [SerializeField]
    private float _InPos = -283f;
    public Image _imgSelect;

    public GameObject _ListGroup;
    public GameObject _prefabLItem;

    void Start()
    {
        _IsHide = true;
        _btnLock = false;
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(_InPos, 0);
        _imgSelect.gameObject.SetActive(false);
    }

    public void click_TabMyResults()
    {
        if (_btnLock) return;
        _btnLock = true;

        if (_IsHide)  // 창 오픈
        {
            UI.Tournaments._TabMyResults.transform.SetSiblingIndex(3);
            UpdateItems(() => {
                _imgSelect.gameObject.SetActive(true);  // 빨간색
                this.gameObject.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.5f).OnComplete(() => {
                    _IsHide = false;
                    _btnLock = false;
                    _imgSelect.gameObject.SetActive(false);
                });
            });
        }
        else
        {
            _imgSelect.gameObject.SetActive(true);  // 빨간색
            this.gameObject.GetComponent<RectTransform>().DOAnchorPosX(_InPos, 0.5f).OnComplete(() => {
                _IsHide = true;
                _btnLock = false;
                _imgSelect.gameObject.SetActive(false);

                UI.Tournaments.AddMessage(TournamentsUI.IDs.DefaultTabLayer);

                LItemTmtTabMyResults[] list = _ListGroup.GetComponentsInChildren<LItemTmtTabMyResults>();
                for(int i=0; i<list.Length; i++)
                {
                    DestroyObject(list[i].gameObject);
                }
            });
        }
    }

    void UpdateItems(System.Action complete)
    {
        NET.I.SendReqTmtBeforeMyHistory((id, msg) => {
            if (!string.IsNullOrEmpty(msg))
            {
                PK.TmtBeforeMyHistory.RECEIVE pk = JsonConvert.DeserializeObject<PK.TmtBeforeMyHistory.RECEIVE>(msg);
                if (pk.data.data != null)
                {
                    for (int i = 0; i < pk.data.data.Length; i++)
                    {
                        if (i < 10)
                        {
                            GameObject go = xLIB.xHelper.AddChild(_ListGroup.gameObject, _prefabLItem.gameObject);
                            go.GetComponent<LItemTmtTabMyResults>().SetInfo(pk.data.data[i]);
                        }
                    }
                }
            }
            if (complete != null) complete();
        }, NET.I.OnSendReqTimerout, UI.Tournaments._tmt_id, UI.Tournaments._GameId);
    }
}
