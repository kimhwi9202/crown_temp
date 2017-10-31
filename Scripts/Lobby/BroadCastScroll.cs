using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;


public class BroadCastScroll : MonoBehaviour {

    private int itemMax = 5;
    private float itemWidth = 440f;
    private float itemReStartPos = 0;//1760f;
    private float speed = 5f;
    private float beginPos = 1334f;
    public GameObject prefabItem;
    public RectTransform ViewposrtTrans;

    private List<RectTransform> list = new List<RectTransform>();
    private List<PK.WinCast.REData> TempCastList = new List<PK.WinCast.REData>();
    private Tweener[] tweenScroll = null;

    private bool LockGetList = false;  // 버퍼에서 데이터 가져오기 락
    private bool IsPlay = false;

    // Use this for initialization
    void Start ()
    {
        itemReStartPos = beginPos + ((itemMax - 1) * itemWidth);
        tweenScroll = new Tweener[itemMax];

        for (int i=0; i< itemMax; i++)
        {
            GameObject obj = Instantiate(prefabItem) as GameObject;
            obj.transform.SetParent(ViewposrtTrans);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(beginPos + (i * itemWidth), 0);
            obj.transform.gameObject.SetActive(true);

            LItemBroadCast info = obj.gameObject.GetComponent<LItemBroadCast>();
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
        if(play && this.gameObject.activeSelf) onScrollComplete();
    }

    void onScrollComplete()
    {
        if (this.gameObject.activeSelf)
        {
            if(DB.I.GetCurrentWinCastCount() > 0 && LockGetList==false)
            {
                LockGetList = true;
                TempCastList = DB.I.GetWinCastList(); // Queue 가져오면서 버퍼 제거
                Debug.Log("GetDB Buffer = " + TempCastList.Count);
            }

            for (int i = 0; i < itemMax; i++)
            {
                RePosition(i);
                if (i == itemMax - 1) tweenScroll[i] = list[i].DOAnchorPosX(list[i].anchoredPosition.x - itemWidth, speed).SetEase(Ease.Linear).OnComplete(onScrollComplete);
                else tweenScroll[i] = list[i].DOAnchorPosX(list[i].anchoredPosition.x - itemWidth, speed).SetEase(Ease.Linear);

                if(list[i].anchoredPosition.x >= beginPos)
                {
                    // 데이터 세팅
                    if (TempCastList.Count > 0 && LockGetList)
                    {   // 빈 아이템에 세팅
                        if(list[i].GetComponent<LItemBroadCast>().IsInfo() == false)
                        {
                            Debug.Log("Set Cast Data = " + TempCastList[0].userName);
                            list[i].GetComponent<LItemBroadCast>().SetInfo(TempCastList[0]);
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
    /// 왼쪽에서 안보이는 아이템 맨 뒤로 재배치
    /// </summary>
    /// <param name="i">The i.</param>
     void RePosition(int i)
    {
        if (list[i].anchoredPosition.x < 0)
        {
            // 마지막 아이템 찾아라..
            float x = list[i].anchoredPosition.x;
            for (int n = 0; n < itemMax; n++)
            {
                if (list[n].anchoredPosition.x > x)
                    x = list[n].anchoredPosition.x;
            }

            list[i].anchoredPosition = new Vector2(x + itemWidth, 0);
            // 추가할 데이터가 있을경우만 지운다
            if (LockGetList == true)
            {
                list[i].GetComponent<LItemBroadCast>().ClearInfo();
            }
        }
    }
}
