using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using DG.Tweening;

namespace xLIB
{
    /// <summary>
    /// UGUI 용 리스트 뷰 확장용 ( 아이템 재활용 방식 )
    /// - UnityEngine.UI.ScrollRect 를 상속받아서 구현 
    /// - 가로, 세로중 둘중 한가지 방식만 사용가능 ( 차후 확장 예정 )
    /// - Anchor (기준축)는 가로는 (왼쪽에서 오른쪽) 세로는 (위에서 아래) 방향만 지원 ( 차후 확장 예정 )
    /// - 계층도
    ///   L ScrollView ( ScrollRectEx ) - Content / Viewport 자식으로 연결되어야 한다.
    ///      L Viewport (Mask, Background Image)
    ///        L Content ( List Item Parent Object )
    /// </summary>
    public class ScrollRectEx : ScrollRect, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        class STNode
        {
            public int idx;
            public RectTransform tr;
        }

        private Vector2 m_Spacing = Vector2.zero;
        private Vector3 m_FingerDir = Vector3.zero;
        private bool m_TouchLock = false;
        private LinkedList<STNode> m_ListNode = new LinkedList<STNode>();
        public int m_AddRowORColumn = 2;
        public Vector2 m_ItemSize;
        public Vector3 m_TotalItemSize;
        public Vector3 m_InitPosition;
        public float m_InitBound;

        public int m_rowCount;
        public int m_columnCount;
        public int m_MaxItemCountInView;
        public int m_MaxItemCount;

        public float m_ExtentX2;
        public float m_ExtentY2;
        public float m_ExtentX;
        public float m_ExtentY;

        public System.Action<int, GameObject> UpdateItemCallback = null;


        private int m_ItemMaxCount;
        /// <summary>
        /// 외부에서 리스트 아이템총 갯수가 변경된다면 갱신해줘야 한다.
        /// </summary>
        public int CurrentItemMaxCount
        {
            set { m_ItemMaxCount = value; }
            get { return m_ItemMaxCount; }
        }


        protected override void Awake()
        {
            base.Awake();

            this.movementType = MovementType.Unrestricted;
            this.inertia = false;
            if (this.content == null || this.viewport == null)
            {
                Debug.LogError("ScrollRectEx::Awake => Not Found Object - content & viewport");
            }
        }

        /// <summary>
        /// 리스트 아이템 설정
        /// </summary>
        /// <param name="callbackUpdate">The callback update.</param>
        /// <param name="prefabsItem">프리팹 오브젝트.</param>
        /// <param name="maxItemCount">리스트 아이템 총 갯수.</param>
        /// <param name="itemSize">아이템 크기.</param>
        /// <param name="spacing">아이템별 간격.</param>
        public void Init(System.Action<int, GameObject> callbackUpdate, GameObject prefabsItem, int maxItemCount, Vector2 itemSize, Vector2 spacing)
        {
            UpdateItemCallback = callbackUpdate;
            CurrentItemMaxCount = maxItemCount;
            m_ItemSize = itemSize;
            m_Spacing = spacing;

            Vector2 viewSize = new Vector2(this.viewport.sizeDelta.x, this.viewport.sizeDelta.y);
            m_TotalItemSize = m_ItemSize + m_Spacing;
            m_columnCount = Mathf.RoundToInt(viewSize.x / m_TotalItemSize.x);
            m_rowCount = Mathf.RoundToInt(viewSize.y / m_TotalItemSize.y);
            m_MaxItemCountInView = m_columnCount * m_rowCount;

            if (this.horizontal)
            {
                m_columnCount = m_columnCount + m_AddRowORColumn;
                m_InitPosition.x = -(viewSize.x * 0.5f) + (m_TotalItemSize.x / 2.0f);
                m_InitPosition.y = m_TotalItemSize.y * (m_rowCount - 1) / 2.0f;
                m_InitBound = -(viewSize.x + m_Spacing.x) * 0.5f;
            }
            else if (this.vertical)
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
                GameObject item = Instantiate(prefabsItem);
                node.tr = item.GetComponent<RectTransform>();
                node.tr.parent = this.content;
                node.tr.anchorMin = new Vector2(0, 0.5f);
                node.tr.anchorMax = new Vector2(0, 0.5f);
                node.tr.pivot = new Vector2(0, 0.5f);
                node.tr.transform.localScale = new Vector3(1f, 1f, 1f);
                if (this.horizontal) node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
                else node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
                node.tr.transform.localPosition = Vector3.zero;
                if (this.horizontal) node.tr.anchoredPosition = new Vector3((i * m_TotalItemSize.x), 0, 0);
                else node.tr.anchoredPosition = new Vector3(0, -(i * m_TotalItemSize.y), 0);
                node.idx = i;
                m_ListNode.AddLast(node);
            }
            ResetPos();
        }

        public Vector3 GetDirection()
        {
            return m_FingerDir;
        }

        private void Clear()
        {
            foreach (STNode item in m_ListNode)
            {
                GameObject.Destroy(item.tr.gameObject);
            }
            m_ListNode.Clear();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (m_TouchLock) return;
            if (this.horizontal)
            {
                if (eventData.delta.x < 0) m_FingerDir = Vector3.left;
                else if (eventData.delta.x > 0) m_FingerDir = Vector3.right;

                Vector2 dragPos = new Vector2(eventData.delta.x, 0);
                this.content.anchoredPosition += dragPos;
                // 모바일환경에선 없지만 개발과정에서 마우스로 오버페이스 예방
                if (this.content.anchoredPosition.x > viewport.sizeDelta.x)
                    this.content.anchoredPosition = new Vector2(viewport.sizeDelta.x, 0);
                else
                    Update_Horizontal();
            }
            else if (this.vertical)
            {
                if (eventData.delta.y < 0) m_FingerDir = Vector3.down;
                else if (eventData.delta.y > 0) m_FingerDir = Vector3.up;

                Vector2 dragPos = new Vector2(0, eventData.delta.y);
                this.content.anchoredPosition += dragPos;
                Update_Vertical();
            }
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (m_TouchLock) return;
            m_TouchLock = eventData.dragging;
            UpdatePosition();
        }

        public void PrevPos()
        {
            m_FingerDir = Vector3.right;
            UpdatePosition();
        }

        public void NextPos()
        {
            m_FingerDir = Vector3.left;
            UpdatePosition();
        }

        Vector2 GetLastPosition()
        {
            Vector2 pos = Vector2.zero;
            pos.x = m_TotalItemSize.x * m_ItemMaxCount - m_TotalItemSize.x;
            pos.y = m_TotalItemSize.y * m_ItemMaxCount - m_TotalItemSize.y;
            return pos;
        }

        private Vector2 GetNextTargetPos()
        {
            int idx = 0;
            Vector2 pos = Vector2.zero;

            if (Vector3.right == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(this.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (this.content.anchoredPosition.x > 0) // 오버
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
                idx = Mathf.FloorToInt(this.content.anchoredPosition.x / m_TotalItemSize.x);
                if (this.content.anchoredPosition.x > -last)
                {
                    idx = Mathf.Abs(idx);
                    pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                }
                else // 마지막 아이템
                {
                    idx = m_ItemMaxCount - 1;
                    pos.x = -last;
                }
                //Debug.Log(idx + " finger left = " + pos);
            }
            else if (Vector3.up == m_FingerDir)
            {
                float last = GetLastPosition().y;
                idx = Mathf.FloorToInt(-this.content.anchoredPosition.y / m_TotalItemSize.y);
                if (this.content.anchoredPosition.y < last)
                {
                    idx = Mathf.Abs(idx);
                    pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                }
                else // 마지막 아이템
                {
                    idx = m_ItemMaxCount - 1;
                    pos.y = last;
                }
                //Debug.Log(idx + " finger up = " + pos);
            }
            else if (Vector3.down == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(this.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (this.content.anchoredPosition.y < 0)  // 오버
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

        public int GetCurrentIndex()
        {
            int idx = 0;
            //Vector2 pos = Vector2.zero;
            if (Vector3.right == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(this.content.anchoredPosition.x) / m_TotalItemSize.x);
                if (this.content.anchoredPosition.x > 0) idx = 0;
            }
            else if (Vector3.left == m_FingerDir)
            {
                float last = GetLastPosition().x;
                idx = Mathf.FloorToInt(this.content.anchoredPosition.x / m_TotalItemSize.x);
                if (this.content.anchoredPosition.x > -last)
                    idx = Mathf.Abs(idx);
                else // 마지막 아이템
                    idx = CurrentItemMaxCount - 1;
            }
            else if (Vector3.up == m_FingerDir)
            {
                float last = GetLastPosition().y;
                idx = Mathf.FloorToInt(-this.content.anchoredPosition.y / m_TotalItemSize.y);
                if (this.content.anchoredPosition.y < last)
                    idx = Mathf.Abs(idx);
                else // 마지막 아이템
                    idx = CurrentItemMaxCount - 1;
            }
            else if (Vector3.down == m_FingerDir)
            {
                idx = Mathf.FloorToInt(Mathf.Abs(this.content.anchoredPosition.y) / m_TotalItemSize.y);
                if (this.content.anchoredPosition.y < 0)
                    idx = 0;
            }

            return idx;
        }

        private Vector2 GetNextTargetPos(int idx)
        {
            int cur = GetCurrentIndex();
            Vector2 pos = Vector2.zero;
            if (this.horizontal)
            {
                if (idx > cur)
                {
                    if (this.content.anchoredPosition.x > 0) pos.x = 0;
                    else pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                }
                else if (idx < cur)
                {
                    float last = GetLastPosition().x;
                    if (this.content.anchoredPosition.x > -last) pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
                    else pos.x = -last;
                }
            }
            else
            {
                if (idx > cur)
                {
                    float last = GetLastPosition().y;
                    if (this.content.anchoredPosition.y < last) pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                    else pos.y = last;
                }
                else if (idx < cur)
                {
                    if (this.content.anchoredPosition.y < 0) pos.y = 0;
                    else pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
                }
            }
            return pos;
        }

        void UpdatePosition()
        {
            if (this.horizontal)
            {
                this.content.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    Update_Horizontal();
                    m_TouchLock = false;
                });
            }
            else if (this.vertical)
            {
                this.content.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    Update_Vertical();
                    m_TouchLock = false;
                });
            }
        }

        void Update_Horizontal()
        {
            int realIndex = 0;
            STNode item = null;
            Vector3 pos = Vector3.zero;
            Vector2 clip = this.content.anchoredPosition;
            float distance = 0f;

            bool flag = true;
            while (flag)
            {
                item = m_ListNode.First.Value;
                pos = item.tr.anchoredPosition;
                distance = pos.x - Mathf.Abs(clip.x);

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
                            UpdateItemCallback(realIndex, item.tr.gameObject);
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
                distance = pos.x - Mathf.Abs(clip.x);

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

                            UpdateItemCallback(realIndex, item.tr.gameObject);

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
            Vector2 clip = -this.content.anchoredPosition;
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
                            UpdateItemCallback(realIndex, item.tr.gameObject);
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

                            UpdateItemCallback(realIndex, item.tr.gameObject);

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
            StartCoroutine(coUpdateReset(movePos, index));
        }

        IEnumerator coUpdateReset(Vector2 pos, int index)
        {
            yield return new WaitForEndOfFrame();
            this.content.anchoredPosition = pos;
            if (this.horizontal) Update_Horizontal();
            else Update_Vertical();
        }

        // 스크롤뷰 리셋.
        public void ResetPos()
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
                    UpdateItemCallback(index, item.tr.gameObject);
                    item.tr.gameObject.SetActive(true);
                }
                else
                {
                    item.tr.gameObject.SetActive(false);
                }

                if (this.horizontal)
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



    }
}
/*
public class TestListView : MonoBehaviour
{

    public GameObject _prefabs;
    public ScrollRectEx sr;
    private List<int> m_ItemList = new List<int>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 5; ++i)
        {
            m_ItemList.Add(i);
        }
        sr.Init(OnUpdateItem, _prefabs, m_ItemList.Count, new Vector2(1200, 530), Vector2.zero);
    }

    public void OnUpdateItem(int index, GameObject go)
    {
        //LItem item = go.GetComponent<LItem>();
        //item.UpdateData();// = index.ToString();
    }
}
*/
