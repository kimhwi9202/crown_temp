using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// Tournaments TabRank Menu
public class TmtTabInfo : MonoBehaviour
{
    private bool _IsHide = true;
    private bool _btnLock = false;
    [SerializeField]
    private float _InPos = -504f;
    public Image _imgSelect;

    public GameObject _prefabLItem;

    void Start()
    {
        _IsHide = true;
        _btnLock = false;
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(_InPos, 0);
        _imgSelect.gameObject.SetActive(false);
    }

    public void click_TabInfo()
    {
        if (_btnLock) return;
        _btnLock = true;

        if (_IsHide)
        {
            UI.Tournaments._TabMyInfo.transform.SetSiblingIndex(3);
            _imgSelect.gameObject.SetActive(true);
            this.gameObject.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.5f).OnComplete(() => {
                _IsHide = false;
                _btnLock = false;
                _imgSelect.gameObject.SetActive(false);
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
            });
        }
    }
}
