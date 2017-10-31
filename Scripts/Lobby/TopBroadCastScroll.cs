using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;


public class TopBroadCastScroll : MonoBehaviour {

    private int itemMax = 5;
    private float itemHeight = 70f;
    private float itemReStartPos = 0;//1760f;
    private float speed = 3f;
    private float beginPos = 70f;
    public GameObject prefabItem;
    public RectTransform ViewposrtTrans;

    private List<RectTransform> list = new List<RectTransform>();
    private List<PK.WinCast.REData> TempCastList = new List<PK.WinCast.REData>();
    private Tweener[] tweenScroll = null;

    private bool LockGetList = false;  // 버퍼에서 데이터 가져오기 락
    private bool IsPlay = false;
    private int count = 0;

    // Use this for initialization
    void Start ()
    {
        itemReStartPos = beginPos + ((itemMax - 1) * itemHeight);
        tweenScroll = new Tweener[itemMax];

        for (int i=0; i< itemMax; i++)
        {
            GameObject obj = Instantiate(prefabItem) as GameObject;
            obj.transform.SetParent(ViewposrtTrans);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, beginPos + (i * itemHeight));
            obj.transform.gameObject.SetActive(true);

            LItemTopBroadCast info = obj.gameObject.GetComponent<LItemTopBroadCast>();
            info.ClearInfo();
            list.Add(info.GetComponent<RectTransform>());
        }
    }

    public void Play(bool play)
    {
        if (tweenScroll != null)
        {
            for (int i = 0; i < tweenScroll.Length; i++)
                tweenScroll[i].Kill();
        }

        IsPlay = play;
        if (play && this.gameObject.activeSelf)
        {
            SYSTIMER.I.TopCastAlram.SetAlramEvent(onScrollComplete, 6f);
            SYSTIMER.I.TopCastAlram.Play();
            //UI.BroadCast.Test();
        }
        else
        {
            SYSTIMER.I.TopCastAlram.Pause();
        }
    }

    // 일정 시간에 호출된다.
    void onScrollComplete()
    {
        if (this.gameObject.activeSelf)
        {
            if(DB.I.GetCurrentWinTopCastCount() > 0 && LockGetList==false)
            {
                LockGetList = true;
                TempCastList = DB.I.GetWinTopCastList(); // Queue 가져오면서 버퍼 제거
                //Debug.Log("GetDB Buffer = " + TempCastList.Count);
            }

            for (int i = 0; i < itemMax; i++)
            {
                RePosition(i);
                tweenScroll[i] = list[i].DOAnchorPosY(list[i].anchoredPosition.y - itemHeight, speed).SetEase(Ease.Linear);

                if (list[i].anchoredPosition.y >= beginPos)
                {
                    // 데이터 세팅
                    if (TempCastList.Count > 0 && LockGetList)
                    {   // 빈 아이템에 세팅
                        if (list[i].GetComponent<LItemTopBroadCast>().IsInfo() == false)
                        {
                            //Debug.Log("Set Cast Data = " + TempCastList[0].userName);
                            list[i].GetComponent<LItemTopBroadCast>().SetInfo(TempCastList[0]);
                            TempCastList.RemoveAt(0);
                            // 버퍼에 있는 데이터 다 사용했다.. 다시 가져오기 세팅
                            if (TempCastList.Count <= 0) LockGetList = false;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 위에서 아래
    /// </summary>
    /// <param name="i">The i.</param>
    void RePosition(int i)
    {
        if (list[i].anchoredPosition.y <= -itemHeight)
        {
            // 마지막 아이템 찾아라..
            float y = list[i].anchoredPosition.y;
            for (int n = 0; n < itemMax; n++)
            {
                if (list[n].anchoredPosition.y > y)
                    y = list[n].anchoredPosition.y;
            }

            list[i].anchoredPosition = new Vector2(0, y + itemHeight);
            // 추가할 데이터가 있을경우만 지운다
            if (LockGetList == true)
            {
                list[i].GetComponent<LItemTopBroadCast>().ClearInfo();
            }
        }
    }
}
