using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using DG.Tweening;

public class xScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    class STNode
    {
        public int idx;
        public RectTransform tr;
    }
    private ScrollRect _ScrollRect;
    public RectTransform gridTrans;
    public Vector2 _cellsize;
    public Vector2 _spacing;
    private Vector3 _fingerDir = Vector3.zero;
    private PointerEventData _dragData;
    private int _maxItemCount = 5;
    private bool _dragging;
    public bool flag = false;
    private bool _touchLock = false;
    private LinkedList<STNode> _node = new LinkedList<STNode>();
    [SerializeField]
    public GameObject _prefabe;

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

    public bool m_bHorizontal = false;

    public System.Action<int> OnUpdateItemIndex = null;
    public System.Action<int, GameObject> OnUpdateItem = null;
    public System.Func<int> OnGetItemCount = null;

    void Awake()
    {
        _ScrollRect = this.GetComponent<ScrollRect>();
        gridTrans = _ScrollRect.content;
    }

    public void Init(int item_cx, int item_cy)
    {
        m_ItemSize = new Vector2(item_cx, item_cy);

        Vector2 viewSize = new Vector2(_ScrollRect.viewport.sizeDelta.x, _ScrollRect.viewport.sizeDelta.y);
        m_TotalItemSize = m_ItemSize + _spacing;
        m_columnCount = Mathf.RoundToInt(viewSize.x / m_TotalItemSize.x);
        m_rowCount = Mathf.RoundToInt(viewSize.y / m_TotalItemSize.y);
        m_MaxItemCountInView = m_columnCount * m_rowCount;

        if (_ScrollRect.horizontal)
        {
            m_bHorizontal = true;
            m_columnCount = m_columnCount + m_AddRowORColumn;

            m_InitPosition.x = -(viewSize.x * 0.5f) + (m_TotalItemSize.x / 2.0f);
            m_InitPosition.y = m_TotalItemSize.y * (m_rowCount - 1) / 2.0f;
            m_InitBound = -(viewSize.x + _spacing.x) * 0.5f;
        }
        else if (_ScrollRect.vertical)
        {
            m_bHorizontal = false;
            m_rowCount = m_rowCount + m_AddRowORColumn;

            m_InitPosition.x = -m_TotalItemSize.x * (m_columnCount - 1) / 2.0f;
            m_InitPosition.y = (viewSize.y / 2.0f) - (m_TotalItemSize.y / 2.0f);
            m_InitBound = (viewSize.y - _spacing.y) * 0.5f;
        }
        m_MaxItemCount = m_columnCount * m_rowCount;

        m_ExtentX2 = m_TotalItemSize.x * m_columnCount;
        m_ExtentY2 = m_TotalItemSize.y * m_rowCount;
        m_ExtentX = m_ExtentX2 * 0.5f;
        m_ExtentY = m_ExtentY2 * 0.5f;


        _maxItemCount = m_MaxItemCount;
        for (int i = 0; i < _maxItemCount; i++)
        {
            STNode node = new STNode();
            GameObject item = Instantiate(_prefabe);
            node.tr = item.GetComponent<RectTransform>();
            node.tr.parent = gridTrans;
            node.tr.anchorMin = new Vector2(0, 0.5f);
            node.tr.anchorMax = new Vector2(0, 0.5f);
            node.tr.pivot = new Vector2(0, 0.5f);
            node.tr.transform.localScale = new Vector3(1f, 1f, 1f);
            if(m_bHorizontal) node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
            else node.tr.sizeDelta = new Vector2(m_ItemSize.x, m_ItemSize.y);
            node.tr.transform.localPosition = Vector3.zero;
            if (m_bHorizontal) node.tr.anchoredPosition = new Vector3((i * m_TotalItemSize.x), 0, 0);
            else node.tr.anchoredPosition = new Vector3(0, -(i * m_TotalItemSize.y), 0);
            node.idx = i;
            _node.AddLast(node);
        }
    }

    public Vector3 GetDirection()
    {
        return _fingerDir;
    }

    private void Clear()
    {
        foreach (STNode item in _node)
        {
            GameObject.Destroy(item.tr.gameObject);
        }
        _node.Clear();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_touchLock) return;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_touchLock) return;
        if (_ScrollRect.horizontal)
        {
            if (eventData.delta.x < 0) _fingerDir = Vector3.left;
            else if (eventData.delta.x > 0) _fingerDir = Vector3.right;
        }
        else if (_ScrollRect.vertical)
        {
            if (eventData.delta.y < 0) _fingerDir = Vector3.down;
            else if (eventData.delta.y > 0) _fingerDir = Vector3.up;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_touchLock) return;
        _touchLock = eventData.dragging;
        UpdatePosition();
    }

    public void PrevPos()
    {
        _fingerDir = Vector3.right;
        UpdatePosition();
    }

    public void NextPos()
    {
        _fingerDir = Vector3.left;
        UpdatePosition();
    }

    Vector2 GetLastPosition()
    {
        Vector2 pos = Vector2.zero;
        pos.x = m_TotalItemSize.x * OnGetItemCount() - m_TotalItemSize.x;
        pos.y = m_TotalItemSize.y * OnGetItemCount() - m_TotalItemSize.y;
        return pos; 
    }

    private Vector2 GetNextTargetPos()
    {
        int idx = 0;
        Vector2 pos = Vector2.zero;

        if (Vector3.right == _fingerDir)
        {
            idx = Mathf.FloorToInt(Mathf.Abs(gridTrans.anchoredPosition.x) / m_TotalItemSize.x);
            if (gridTrans.anchoredPosition.x > 0) // 오버
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
        else if (Vector3.left == _fingerDir)
        {
            float last = GetLastPosition().x;
            idx = Mathf.FloorToInt(gridTrans.anchoredPosition.x / m_TotalItemSize.x);
            if (gridTrans.anchoredPosition.x > -last)
            {
                idx = Mathf.Abs(idx);
                pos.x = -Mathf.Abs(idx * m_TotalItemSize.x);
            }
            else // 마지막 아이템
            {
                idx = OnGetItemCount()-1;
                pos.x = -last;
            }
            //Debug.Log(idx + " finger left = " + pos);
        }
        else if(Vector3.up == _fingerDir)
        {
            float last = GetLastPosition().y;
            idx = Mathf.FloorToInt(-gridTrans.anchoredPosition.y / m_TotalItemSize.y);
            if (gridTrans.anchoredPosition.y < last)
            {
                idx = Mathf.Abs(idx);
                pos.y = Mathf.Abs(idx * m_TotalItemSize.y);
            }
            else // 마지막 아이템
            {
                idx = OnGetItemCount() - 1;
                pos.y = last;
            }
            //Debug.Log(idx + " finger up = " + pos);
        }
        else if (Vector3.down == _fingerDir)
        {
            idx = Mathf.FloorToInt(Mathf.Abs(gridTrans.anchoredPosition.y) / m_TotalItemSize.y);
            if (gridTrans.anchoredPosition.y < 0)  // 오버
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

    void UpdatePosition()
    {
        if (_ScrollRect.horizontal)
        {
            Update_Horizontal();
            //Debug.Log(GetNextTargetPos());
            gridTrans.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(()=> { _touchLock = false; });
        }
        else if (_ScrollRect.vertical)
        {
            Update_Vertical();
            gridTrans.DOAnchorPos(GetNextTargetPos(), 0.5f).SetEase(Ease.OutCubic).OnComplete(() => { _touchLock = false; });
        }
    }

    void Update_Horizontal()
    {
        int realIndex = 0;
        //bool flag = true;
        float distance = 0.0f;
        Vector3 pos;
        Vector2 clip = gridTrans.anchoredPosition;
        RectTransform trans = null;
        STNode item = null;

        //while (flag)
        {
            item = _node.First.Value;
            pos = item.tr.anchoredPosition;
            distance = pos.x - Mathf.Abs(clip.x);

            if (distance < -m_ExtentX)
            {
                for (int i = 0; i < m_rowCount; ++i)
                {
                    item = _node.First.Value;
                    trans = item.tr;
                    pos = trans.anchoredPosition;

                    realIndex = item.idx + m_MaxItemCount;
                    if (i == 0 && realIndex >= OnGetItemCount())
                    {
                        //flag = false;
                        break;
                    }

                    item.idx = realIndex;
                    pos.x += m_ExtentX2;
                    trans.anchoredPosition = pos;
                    trans.name = realIndex.ToString();

                    if (realIndex < OnGetItemCount())
                    {
                        OnUpdateItem(realIndex, item.tr.gameObject);
                        item.tr.gameObject.SetActive(true);
                    }
                    else
                    {
                        item.tr.gameObject.SetActive(false);
                    }

                    _node.RemoveFirst();
                    _node.AddLast(item);
                }
            }
            else
            {
                //break;
            }
        }

        //flag = true;
        //while (flag)
        {
            item = _node.Last.Value;
            pos = item.tr.anchoredPosition;
            distance = pos.x - Mathf.Abs(clip.x);

            if (distance > m_ExtentX)
            {
                for (int i = 0; i < m_rowCount; ++i)
                {
                    item = _node.Last.Value;
                    trans = item.tr;
                    pos = trans.anchoredPosition;

                    realIndex = item.idx - m_MaxItemCount;
                    if (realIndex >= 0 && realIndex < OnGetItemCount())
                    {
                        item.idx = realIndex;
                        pos.x -= m_ExtentX2;
                        trans.anchoredPosition = pos;
                        trans.name = realIndex.ToString();

                        OnUpdateItem(realIndex, item.tr.gameObject);

                        item.tr.gameObject.SetActive(true);
                        _node.RemoveLast();
                        _node.AddFirst(item);
                    }
                    else
                    {
                        //flag = false;
                        break;
                    }
                }
            }
            else
            {
               // break;
            }
        }

    }

    private void Update_Vertical()
    {
        int realIndex = 0;
        //bool flag = true;
        float distance = 0.0f;
        Vector3 pos;
        Vector2 clip = -gridTrans.anchoredPosition;
        RectTransform trans = null;
        STNode item = null;

        //while (flag)
        {
            item = _node.First.Value;
            pos = item.tr.anchoredPosition;
            distance = pos.y - clip.y;

            if (distance > m_ExtentY)
            {
                for (int i = 0; i < m_columnCount; ++i)
                {
                    item = _node.First.Value;
                    trans = item.tr;
                    pos = trans.anchoredPosition;

                    realIndex = item.idx + m_MaxItemCount;
                    if (i == 0 && realIndex >= OnGetItemCount())
                    {
                      //  flag = false;
                        break;
                    }

                    item.idx = realIndex;
                    pos.y -= m_ExtentY2;
                    trans.anchoredPosition = pos;
                    trans.name = realIndex.ToString();
                    if (realIndex < OnGetItemCount())
                    {
                        OnUpdateItem(realIndex, item.tr.gameObject);
                        item.tr.gameObject.SetActive(true);
                    }
                    else
                    {
                        item.tr.gameObject.SetActive(false);
                    }

                    _node.RemoveFirst();
                    _node.AddLast(item);
                }
            }
            else
            {
                //break;
            }
        }

        //flag = true;
        //while (flag)
        {
            item = _node.Last.Value;
            pos = item.tr.anchoredPosition;
            distance = pos.y - clip.y;

            if (distance < -m_ExtentY)
            {
                for (int i = 0; i < m_columnCount; ++i)
                {
                    item = _node.Last.Value;
                    trans = item.tr;
                    pos = trans.anchoredPosition;

                    realIndex = item.idx - m_MaxItemCount;
                    if (realIndex >= 0 && realIndex < OnGetItemCount())
                    {
                        item.idx = realIndex;
                        pos.y += m_ExtentY2;
                        trans.anchoredPosition = pos;
                        trans.name = realIndex.ToString();

                        OnUpdateItem(realIndex, item.tr.gameObject);

                        item.tr.gameObject.SetActive(true);
                        _node.RemoveLast();
                        _node.AddFirst(item);
                    }
                    else
                    {
                        //flag = false;
                        break;
                    }
                }
            }
            else
            {
               // break;
            }
        }
    }
}
