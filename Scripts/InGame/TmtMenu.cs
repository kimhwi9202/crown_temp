using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class TmtMenu : MonoBehaviour {

    public Image _imgDown;
    public Image _imgUp;
    public Text _textRank;
    public Image _title;
    public Image _LightBG;
    public Image[] _imgTrophy;
    // Use this for initialization
    void Start () {
        _imgDown.gameObject.SetActive(false);
        _imgUp.gameObject.SetActive(false);
    }


    public void click_Button()
    {
        //this.gameObject.GetComponent<Image>().DOFade(0, 1.5f);
        UI.Tournaments.AddMessage(TournamentsUI.IDs.XBtnClick);
        //RankUp();
    }	

    // 0,1,2
    public void SetState(int n)
    {

    }


    public void SetTime(string time)
    {
        if(!_textRank.gameObject.activeSelf)
            _textRank.gameObject.SetActive(true);
        _textRank.text = time; 
    }

    public void SetRank(int rank)
    {
        if(rank <= 0) _textRank.gameObject.SetActive(false);
        else _textRank.gameObject.SetActive(true);
        _textRank.text = "# " + rank.ToString("#,#0");
    }

    public void RankUp()
    {
        _textRank.gameObject.SetActive(false);

        _LightBG.gameObject.SetActive(true);
        _imgUp.gameObject.SetActive(true);
        StartCoroutine(coPlayEndUpDown(0));
    }
    public void RankDown()
    {
        _textRank.gameObject.SetActive(false);
        _LightBG.gameObject.SetActive(true);
        _imgDown.gameObject.SetActive(true);
        StartCoroutine(coPlayEndUpDown(1));
    }

    IEnumerator coPlayEndUpDown(int type)
    {
        yield return new WaitForSeconds(3f);

        _textRank.gameObject.SetActive(true);
        _LightBG.gameObject.SetActive(false);
        _imgDown.gameObject.SetActive(false);
        _imgUp.gameObject.SetActive(false);
    }
}

