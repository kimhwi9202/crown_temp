using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using DG.Tweening;

namespace xLIB
{
    /// <summary>
    /// UGUI 용 리스트 뷰 상속형 ( 아이템 재활용 방식 )
    /// - UnityEngine.UI.ScrollRect 를 참조한다.
    /// - 가로, 세로중 둘중 한가지 방식만 사용가능 ( 차후 확장 예정 )
    /// - Anchor (기준축)는 가로는 (왼쪽에서 오른쪽) 세로는 (위에서 아래) 방향만 지원 ( 차후 확장 예정 )
    /// - 계층도
    ///   L ScrollView - Content / Viewport 자식으로 연결되어야 한다.
    ///      L Viewport (Mask, Background Image)
    ///        L Content ( List Item Parent Object )
    /// </summary>
    public class ScrollViewBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public class STNode
        {
            public int idx;
            public RectTransform tr;
        }
        protected  ScrollRect m_ScrollRect;
        protected  Vector3 m_FingerDir = Vector3.zero;
        protected  bool m_TouchLock = false;
        protected  int m_ItemMaxCount;
        protected  int m_CurCenterItemIndex;
        protected  LinkedList<STNode> m_ListNode = new LinkedList<STNode>();

        protected  int m_AddRowORColumn = 2;
        protected  Vector3 m_TotalItemSize;
        protected  Vector3 m_InitPosition;
        protected float m_InitBound;

        protected  int m_rowCount;
        protected  int m_columnCount;
        protected  int m_MaxItemCountInView;
        protected  int m_MaxItemCount;

        protected  float m_ExtentX2;
        protected  float m_ExtentY2;
        protected  float m_ExtentX;
        protected  float m_ExtentY;

        protected  System.Action<int, GameObject> UpdateItemCallback = null;
        protected  System.Action<int> UpdateItemIndexCallback = null;

        public GameObject _prefabe;
        public Vector2 m_ItemSize;
        public Vector2 m_Spacing = Vector3.zero;

        //private bool m_AutoScroll = false;

        public int CurrentCenterIndex
        {
            set { m_CurCenterItemIndex = value; }
            get { return m_CurCenterItemIndex; }
        }
        /// <summary>
        /// 외부에서 리스트 아이템총 갯수가 변경된다면 갱신해줘야 한다.
        /// </summary>
        public int CurrentItemMaxCount
        {
            set { m_ItemMaxCount = value; }
            get { return m_ItemMaxCount; }
        }
        public RectTransform GetContent() { return m_ScrollRect.content; }

        public RectTransform GetViewport() { return m_ScrollRect.viewport; }



        void Awake()
        {
            m_ScrollRect = this.GetComponent<ScrollRect>();
            m_ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            m_ScrollRect.inertia = false;
            if (m_ScrollRect.content == null || m_ScrollRect.viewport == null)
            {
                Debug.LogError("ScrollViewBase::Awake => Not Found Object - content & viewport");
            }
        }

        public void ScrollLock(bool _lock)
        {
            if(_lock) m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
            else m_ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        }

        /// <summary>
        /// 리스트 아이템 설정
        /// </summary>
        /// <param name="callbackUpdate">The callback update.</param>
        public void Init(System.Action<int, GameObject> callbackUpdate, System.Action<int> callbackItemIndex)
        {
            Clear();
            UpdateItemCallback = callbackUpdate;
            UpdateItemIndexCallback = callbackItemIndex;

            Vector2 viewSize = new Vector2(m_ScrollRect.viewport.sizeDelta.x, m_ScrollRect.viewport.sizeDelta.y);
            m_TotalItemSize = m_ItemSize + m_Spacing;
            m_columnCount = Mathf.RoundToInt(viewSize.x / m_TotalItemSize.x);
            m_rowCount = Mathf.RoundToInt(viewSize.y / m_TotalItemSize.y);
            m_MaxItemCountInView = m_columnCount * m_rowCount;

            if (m_ScrollRect.horizontal)
            {
                m_columnCount = m_columnCount + m_AddRowORColumn;
                m_InitPosition.x = -(viewSize.x * 0.5f) + (m_TotalItemSize.x / 2.0f);
                m_InitPosition.y = m_TotalItemSize.y * (m_rowCount - 1) / 2.0f;
                m_InitBound = -(viewSize.x + m_Spacing.x) * 0.5f;
            }
            else if (m_ScrollRect.vertical)
            {
                m_rowCount = m_rowCount + m_AddRowORColumn;
                m_InitPosition.x = -m_TotalItemSize.x * (m_columnCount - 1) / 2.0f;
                m_InitPosition.y = (viewSize.y / 2.0f) - (m_TotalItemSize.y / 2.0f);
                m_InitBound = (viewSize.y - m_Spacing.y) * 0.5f;
            }
            m_MaxItemCount = m_columnCount * m_rowCount;

            m_ExtentX2 = m_TotalItemSize.x * m_columnCount;
            m_ExtentY2 = m_TotalItemSize.y * m_rowCount;
            m_ExtentX = m_ExtentX2 * 0.5f;
            m_ExtentY = m_ExtentY2 * 0.5f;

            for (int i = 0; i < m_MaxItemCount; i++)
            {
                STNode node = new STNode();
                GameObject item = Instantiate(_prefabe);
                node.tr = item.GetComponent<RectTransform>();
                node.tr.SetParent(m_ScrollRect.content);
                node.tr.anchorMin = new Vector2(0, 0.5f);
                node.tr.anchorMax = new Vector2(0, 0.5f);
                node.tr.pivot = new Vector2(0, 0.5f);
                node.tr.transform.localScale = new Vector3(1f, 1f, 1f);
                if (m_ScrollRect.horizontal) node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
                else node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
                node.tr.transform.localPosition = Vector3.zero;
                if (m_ScrollRect.horizontal) node.tr.anchoredPosition = new Vector3((i * m_TotalItemSize.x), 0, 0);
                else node.tr.anchoredPosition = new Vector3(0, -(i * m_TotalItemSize.y), 0);
                node.idx = i;
                m_ListNode.AddLast(node);
            }
            CurrentCenterIndex = 0;
            Reset();
        }

        private void Clear()
        {
            foreach (STNode item in m_ListNode)
            {
                GameObject.Destroy(item.tr.gameObject);
            }
            m_ListNode.Clear();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (m_TouchLock) return;
            if (m_ScrollRect.horizontal)
            {
                if (eventData.delta.x < 0) m_FingerDir = Vector3.left;
                else if (eventData.delta.x > 0) m_FingerDir = Vector3.right;

                // 모바일환경에선 없지만 개발과정에서 마우스로 오버페이스 예방
                if (m_ScrollRect.content.anchoredPosition.x > m_ScrollRect.viewport.sizeDelta.x)
                    m_ScrollRect.content.anchoredPosition = new Vector2(m_ScrollRect.viewport.sizeDelta.x, 0);
                else
                    Update_Horizontal();
            }
            else if (m_ScrollRect.vertical)
            {
                if (eventData.delta.y < 0) m_FingerDir = Vector3.down;
                else if (eventData.delta.y > 0) m_FingerDir = Vector3.up;

                // 모바일환경에선 없지만 개발과정에서 마우스로 오버페이스 예방
                //if (m_ScrollRect.content.anchoredPosition.y > m_ScrollRect.viewport.sizeDelta.y)
                //    m_ScrollRect.content.anchoredPosition = new Vector2(0, m_ScrollRect.viewport.sizeDelta.y);
                //else
                    Update_Vertical();
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_TouchLock) return;
            m_TouchLock = eventData.dragging;
            UpdatePosition();
        }

        Vector2 GetLastPosition()
        {
            Vector2 pos = Vector2.zero;
            pos.x = m_TotalItemSize.x * CurrentItemMaxCount - m_TotalItemSize.x;
            pos.y = m_TotalItemSize.y * CurrentItemMaxCount - m_TotalItemSize.y;
            return pos;
        }

        private Vector2 GetNextTargetPos()
        {
            int idx = 0;
            Vector2 pos = Vector2.zero;

            if (Vector3.right == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(m_ScrollRect.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (m_ScrollRect.content.anchoredPosition.x > 0) // 오버
                {
                    idx = 0;
                    pos.x = 0;
                }
                else
                {
                    pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                }
                //Debug.Log(idx + " finger right = " + pos);
            }
            else if (Vector3.left == m_FingerDir)
            {
                float last = GetLastPosition().x;
                idx = Mathf.FloorToInt(m_ScrollRect.content.anchoredPosition.x / m_TotalItemSize.x);
                if (m_ScrollRect.content.anchoredPosition.x > -last)
                {
                    idx = Mathf.Abs(idx);
                    pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                }
                else // 마지막 아이템
                {
                    idx = CurrentItemMaxCount - 1;
                    pos.x = -last;
                }
                //Debug.Log(idx + " finger left = " + pos);
            }
            else if (Vector3.up == m_FingerDir)
            {
                float last = GetLastPosition().y;
                idx = Mathf.FloorToInt(-m_ScrollRect.content.anchoredPosition.y / m_TotalItemSize.y);
                if (m_ScrollRect.content.anchoredPosition.y < last)
                {
                    idx = Mathf.Abs(idx);
                    pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                }
                else // 마지막 아이템
                {
                    idx = CurrentItemMaxCount - 1;
                    pos.y = last;
                }
                //Debug.Log(idx + " finger up = " + pos);
            }
            else if (Vector3.down == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(m_ScrollRect.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (m_ScrollRect.content.anchoredPosition.y < 0)  // 오버
                {
                    idx = 0;
                    pos.y = 0;
                }
                else
                {
                    pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                }
                //Debug.Log(idx + " finger down = " + pos);
            }

            return pos;
        }

        private int GetTargetPosIndex()
        {
            int idx = 0;
            if (Vector3.right == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(m_ScrollRect.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (m_ScrollRect.content.anchoredPosition.x > 0)    idx = 0;
            }
            else if (Vector3.left == m_FingerDir)
            {
                float last = GetLastPosition().x;
                idx = Mathf.FloorToInt(m_ScrollRect.content.anchoredPosition.x / m_TotalItemSize.x);
                if (m_ScrollRect.content.anchoredPosition.x > -last)
                    idx = Mathf.Abs(idx);
                else // 마지막 아이템
                    idx = CurrentItemMaxCount - 1;
            }
            else if (Vector3.up == m_FingerDir)
            {
                float last = GetLastPosition().y;
                idx = Mathf.FloorToInt(-m_ScrollRect.content.anchoredPosition.y / m_TotalItemSize.y);
                if (m_ScrollRect.content.anchoredPosition.y < last)
                    idx = Mathf.Abs(idx);
                else // 마지막 아이템
                    idx = CurrentItemMaxCount - 1;
            }
            else if (Vector3.down == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(m_ScrollRect.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (m_ScrollRect.content.anchoredPosition.y < 0)
                    idx = 0;
            }

            return idx;
        }

        private Vector2 GetNextTargetPos(int idx)
        {
            int cur = GetTargetPosIndex();
            Vector2 pos = Vector2.zero;
            if (m_ScrollRect.horizontal)
            {
                if (idx > cur)
                {
                    if (m_ScrollRect.content.anchoredPosition.x > 0) pos.x = 0;
                    else pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                }
                else if (idx < cur)
                {
                    float last = GetLastPosition().x;
                    if (m_ScrollRect.content.anchoredPosition.x > -last) pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                    else pos.x = -last;
                }
            }
            else
            { 
                if (idx > cur)
                {
                    float last = GetLastPosition().y;
                    if (m_ScrollRect.content.anchoredPosition.y < last) pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                    else pos.y = last;
                }
                else if (idx < cur)
                {
                    if (m_ScrollRect.content.anchoredPosition.y < 0) pos.y = 0;
                    else pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                }
            }
            return pos;
        }


        void UpdatePosition()
        {
            if (m_ScrollRect.horizontal)
            {
                m_ScrollRect.content.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(() => {
                    Update_Horizontal();
                    m_TouchLock = false;
                    CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.x) / m_TotalItemSize.x);
                    if (UpdateItemIndexCallback != null) UpdateItemIndexCallback(CurrentCenterIndex);
                    //Debug.Log(m_ScrollRect.content.anchoredPosition.x + " / " + CurrentCenterIndex + " / " + cur);
                });
            }
            else if (m_ScrollRect.vertical)
            {
                m_ScrollRect.content.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(() => {
                    Update_Vertical();
                    m_TouchLock = false;
                    CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.y) / m_TotalItemSize.y);
                    if(UpdateItemIndexCallback!=null) UpdateItemIndexCallback(CurrentCenterIndex);
                    //Debug.Log(m_ScrollRect.content.anchoredPosition.y + " / " + CurrentCenterIndex);
                });
            }
        }


        void Update_Horizontal()
        {
            int realIndex = 0;
            STNode item = null;
            Vector3 pos = Vector3.zero;
            float x = Mathf.Floor(m_ScrollRect.content.anchoredPosition.x);
            float distance = 0f;

            bool flag = true;
            while (flag)
            { 
                item = m_ListNode.First.Value;
                pos = item.tr.anchoredPosition;
                distance = pos.x - Mathf.Abs(x);

                if (distance < -m_ExtentX)
                {
                    for (int i = 0; i < m_rowCount; ++i)
                    {
                        item = m_ListNode.First.Value;
                        pos = item.tr.anchoredPosition;

                        realIndex = item.idx + m_MaxItemCount;
                        if (i == 0 && realIndex >= CurrentItemMaxCount)
                        {
                            flag = false;
                            break;
                        }

                        item.idx = realIndex;
                        pos.x += m_ExtentX2;
                        item.tr.anchoredPosition = pos;
                        item.tr.name = realIndex.ToString();

                        if (realIndex < CurrentItemMaxCount)
                        {
                            if (UpdateItemCallback != null) UpdateItemCallback(realIndex, item.tr.gameObject);
                            item.tr.gameObject.SetActive(true);
                        }
                        else
                        {
                            item.tr.gameObject.SetActive(false);
                        }

                        m_ListNode.RemoveFirst();
                        m_ListNode.AddLast(item);
                    }
                }
                else break;
            }

            flag = true;
            while (flag)
            {
                item = m_ListNode.Last.Value;
                pos = item.tr.anchoredPosition;
                distance = pos.x - Mathf.Abs(x);

                if (distance > m_ExtentX)
                {
                    for (int i = 0; i < m_rowCount; ++i)
                    {
                        item = m_ListNode.Last.Value;
                        pos = item.tr.anchoredPosition;

                        realIndex = item.idx - m_MaxItemCount;
                        if (realIndex >= 0 && realIndex < CurrentItemMaxCount)
                        {
                            item.idx = realIndex;
                            pos.x -= m_ExtentX2;
                            item.tr.anchoredPosition = pos;
                            item.tr.name = realIndex.ToString();

                            if (UpdateItemCallback != null) UpdateItemCallback(realIndex, item.tr.gameObject);

                            item.tr.gameObject.SetActive(true);
                            m_ListNode.RemoveLast();
                            m_ListNode.AddFirst(item);
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                else break;
            }
            
        }
        private void Update_Vertical()
        {
            int realIndex = 0;
            Vector2 clip = -m_ScrollRect.content.anchoredPosition;
            STNode item = null;
            Vector3 pos = Vector3.zero;
            float distance = 0f;

            bool flag = true;
            while (flag)
            {
                item = m_ListNode.First.Value;
                pos = item.tr.anchoredPosition;
                distance = pos.y - clip.y;

                if (distance > m_ExtentY)
                {
                    for (int i = 0; i < m_columnCount; ++i)
                    {
                        item = m_ListNode.First.Value;
                        pos = item.tr.anchoredPosition;

                        realIndex = item.idx + m_MaxItemCount;
                        if (i == 0 && realIndex >= CurrentItemMaxCount)
                        {
                            flag = false;
                            break;
                        }

                        item.idx = realIndex;
                        pos.y -= m_ExtentY2;
                        item.tr.anchoredPosition = pos;
                        item.tr.name = realIndex.ToString();
                        if (realIndex < CurrentItemMaxCount)
                        {
                            if (UpdateItemCallback != null) UpdateItemCallback(realIndex, item.tr.gameObject);
                            item.tr.gameObject.SetActive(true);
                        }
                        else
                        {
                            item.tr.gameObject.SetActive(false);
                        }

                        m_ListNode.RemoveFirst();
                        m_ListNode.AddLast(item);
                    }
                }
                else break;
            }

            flag = true;
            while (flag)
            {
                item = m_ListNode.Last.Value;
                pos = item.tr.anchoredPosition;
                distance = pos.y - clip.y;

                if (distance < -m_ExtentY)
                {
                    for (int i = 0; i < m_columnCount; ++i)
                    {
                        item = m_ListNode.Last.Value;
                        pos = item.tr.anchoredPosition;

                        realIndex = item.idx - m_MaxItemCount;
                        if (realIndex >= 0 && realIndex < CurrentItemMaxCount)
                        {
                            item.idx = realIndex;
                            pos.y += m_ExtentY2;
                            item.tr.anchoredPosition = pos;
                            item.tr.name = realIndex.ToString();

                            if (UpdateItemCallback != null) UpdateItemCallback(realIndex, item.tr.gameObject);

                            item.tr.gameObject.SetActive(true);
                            m_ListNode.RemoveLast();
                            m_ListNode.AddFirst(item);
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                else break;
            }
        }

        // 현재 뷰를 해당 인덱스가 있는 곳으로 옮겨준다.
        public void SetFocus(int index)
        {
            if (index >= CurrentItemMaxCount)
                return;

            Vector2 movePos = GetNextTargetPos(index);
            m_ScrollRect.content.anchoredPosition = movePos;
            if (m_ScrollRect.horizontal)
            {
                Update_Horizontal();
                CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (UpdateItemIndexCallback != null) UpdateItemIndexCallback(CurrentCenterIndex);
            }
            else
            {
                Update_Vertical();
                CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (UpdateItemIndexCallback != null) UpdateItemIndexCallback(CurrentCenterIndex);
            }
            //StartCoroutine(coUpdateSetFocus(movePos, index));
        }

        IEnumerator coUpdateSetFocus(Vector2 pos, int index)
        {
            yield return new WaitForEndOfFrame();
            m_ScrollRect.content.anchoredPosition = pos;
            if (m_ScrollRect.horizontal)
            {
                Update_Horizontal();
                CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (UpdateItemIndexCallback != null) UpdateItemIndexCallback(CurrentCenterIndex);
            }
            else
            {
                Update_Vertical();
                CurrentCenterIndex = (int)Mathf.Abs(Mathf.Floor(m_ScrollRect.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (UpdateItemIndexCallback != null) UpdateItemIndexCallback(CurrentCenterIndex);
            }
        }

        // 스크롤뷰 리셋.
        public void Reset()
        {
            int index = 0;
            int rowIndex = 0, columnIndex = 0;
            Vector3 position = new Vector3();

            foreach (STNode item in m_ListNode)
            {
                item.idx = index;
                item.tr.name = index.ToString();
                if (index < CurrentItemMaxCount)
                {
                    if(UpdateItemCallback != null) UpdateItemCallback(index, item.tr.gameObject);
                    item.tr.gameObject.SetActive(true);
                }
                else
                {
                    item.tr.gameObject.SetActive(false);
                }

                if (m_ScrollRect.horizontal)
                {
                    rowIndex = index % m_rowCount;
                    columnIndex = index / m_rowCount;
                }
                else
                {
                    rowIndex = index / m_columnCount;
                    columnIndex = index % m_columnCount;
                }

                position.x = m_InitPosition.x + (m_TotalItemSize.x * columnIndex);
                position.y = m_InitPosition.y - (m_TotalItemSize.y * rowIndex);
                item.tr.anchoredPosition = position;
                ++index;
            }
        }

        public void SetAutoScroll(bool _auto)
        {
            //m_AutoScroll = true;
            return;
            float last = GetLastPosition().x - m_ItemSize.x;
            m_ScrollRect.content.DOAnchorPosX(-last, Mathf.Abs(last/40)).OnUpdate(PlayScroll).OnComplete(EndScroll);
        }

        public void PlayScroll()
        {
            Update_Horizontal();
        }

        public void EndScroll()
        {
            SetFocus(0);
            float last = GetLastPosition().x - m_ItemSize.x;
            m_ScrollRect.content.DOAnchorPosX(-last, Mathf.Abs(last/40)).OnUpdate(PlayScroll).OnComplete(EndScroll);
        }
    }
}
/*

public class TestScrollView : ScrollViewBase
{

    private List<int> m_ItemList = new List<int>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 5; ++i)
        {
            m_ItemList.Add(i);
        }
        Init(OnUpdateItem, m_ItemList.Count);
    }

    public void OnUpdateItem(int index, GameObject go)
    {
        //LItem item = go.GetComponent<LItem>();
        //item.UpdateData();// = index.ToString();
    }
}
*/