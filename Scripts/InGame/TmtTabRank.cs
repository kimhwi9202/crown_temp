using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;

// Tournaments TabRank Menu
public class TmtTabRank : MonoBehaviour
{
    private bool _IsHide = true;
    private bool _btnLock = false;
    [SerializeField]
    private float _InPos = -287f;
    public Image _imgSelect;

    public GameObject _RankListGroup;
    public GameObject _AccountListGroup;
    public GameObject _AccountBackground;

    public GameObject _prefabLItemRank;
    public GameObject _prefabLItemAccount;

    void Start()
    {
        _IsHide = true;
        _btnLock = false;
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(_InPos, 0);
        _imgSelect.gameObject.SetActive(false);
    }

    public void click_TabRank()
    {
        if (_btnLock) return;
        _btnLock = true;

        if (_IsHide)
        {
            UpdateItems(() => {
                UI.Tournaments._TabMyRank.transform.SetSiblingIndex(3);
                _imgSelect.gameObject.SetActive(true);
                this.gameObject.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.5f).OnComplete(() => {
                    _IsHide = false;
                    _btnLock = false;
                    _imgSelect.gameObject.SetActive(false);
                });
            });
        }
        else
        {
            _imgSelect.gameObject.SetActive(true);
            this.gameObject.GetComponent<RectTransform>().DOAnchorPosX(_InPos, 0.5f).OnComplete(() => {
                _IsHide = true;
                _btnLock = false;
                _imgSelect.gameObject.SetActive(false);
                UI.Tournaments.AddMessage(TournamentsUI.IDs.DefaultTabLayer);

                LItemTmtTabRank[] list = _RankListGroup.GetComponentsInChildren<LItemTmtTabRank>();
                for (int i = 0; i < list.Length; i++)
                {
                    DestroyObject(list[i].gameObject);
                }
                LItemTmtTabRankAccount[] list2 = _AccountListGroup.GetComponentsInChildren<LItemTmtTabRankAccount>();
                for (int i = 0; i < list2.Length; i++)
                {
                    DestroyObject(list2[i].gameObject);
                }
            });
        }
    }
    void UpdateItems(System.Action complete)
    {
        NET.I.SendReqTmtNowAccount((id, msg) => {
            if (!string.IsNullOrEmpty(msg))
            {
                PK.TmtNowAccount.RECEIVE pk = JsonConvert.DeserializeObject<PK.TmtNowAccount.RECEIVE>(msg);
                if (pk.data.rank != null)
                {
                    for (int i = 0; i < pk.data.rank.Length; i++)
                    {
                        if (i < 5)
                        {
                            GameObject go = xLIB.xHelper.AddChild(_RankListGroup.gameObject, _prefabLItemRank.gameObject);
                            go.GetComponent<LItemTmtTabRank>().SetInfo(pk.data.rank[i]);
                        }
                    }
                }

                if (pk.data.account != null)
                {
                    if(pk.data.account.Length > 0) _AccountBackground.gameObject.SetActive(true);
                    else _AccountBackground.gameObject.SetActive(false);
                    for (int i = 0; i < pk.data.account.Length; i++)
                    {
                        if (i < 7)
                        {
                            GameObject go = xLIB.xHelper.AddChild(_AccountListGroup.gameObject, _prefabLItemAccount.gameObject);
                            go.GetComponent<LItemTmtTabRankAccount>().SetInfo(pk.data.account[i]);
                        }
                    }
                }
            }
            if (complete != null) complete();
        }, NET.I.OnSendReqTimerout, UI.Tournaments._tmt_id, UI.Tournaments._GameId);
    }
}
