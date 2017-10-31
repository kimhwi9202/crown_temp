using UnityEngine;
using System.Collections;

using xLIB;
// http://blog.bsidesoft.com/?p=164
// 모바일 터치 스크린 좌표이벤트 처리
// 포인터는 스크린좌표즉 2D좌표값이다.
public class TouchEvent : Singleton<TouchEvent>
{
    public enum State { none, begin, move, end };
    public bool m_bLock = false;
    public State m_state = State.none;
    public delegate void listener(State type, int id, float x, float y, float dx, float dy);
    public static event listener begin0, begin1, begin2, begin3, begin4;
    public static event listener move0, move1, move2, move3, move4;
    public static event listener end0, end1, end2, end3, end4;

    Vector2[] delta = new Vector2[5];

    IEnumerator Start()
    {
        for( ;; )
        {
            //yield return new WaitForSeconds(0.033f);
            yield return new WaitForSeconds(0.002f);
            if (!m_bLock)
            {
#if UNITY_EDITOR
                MouseUpdate();
#else
                TouchUpdate();
#endif
            }
        }
    }

    public void Initialize(int touchId, listener begin, listener move, listener end)
    {
        if(touchId == 0) { begin0 += begin; move0 += move; end0 += end; }
        else if (touchId == 1) { begin1 += begin; move1 += move; end1 += end; }
        else if (touchId == 2) { begin2 += begin; move2 += move; end2 += end; }
        else if (touchId == 3) { begin3 += begin; move3 += move; end3 += end; }
        else if (touchId == 4) { begin4 += begin; move4 += move; end4 += end; }
    }

    void TouchUpdate()
    {
        int count = Input.touchCount;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Touch touch = Input.GetTouch(i);
            int id = touch.fingerId;
            Vector2 pos = touch.position;
            if (touch.phase == TouchPhase.Began) delta[id] = touch.position;
            float x, y, dx, dy;
            x = pos.x;
            y = pos.y;
            if (touch.phase == TouchPhase.Began)
            {
                dx = dy = 0;
            }
            else
            {
                dx = pos.x - delta[id].x;
                dy = pos.y - delta[id].y;
            }

            if (touch.phase == TouchPhase.Began)
            {
                switch (id)
                {
                    case 0: if (begin0 != null) begin0(State.begin, id, x, y, dx, dy); break;
                    case 1: if (begin1 != null) begin1(State.begin, id, x, y, dx, dy); break;
                    case 2: if (begin2 != null) begin2(State.begin, id, x, y, dx, dy); break;
                    case 3: if (begin3 != null) begin3(State.begin, id, x, y, dx, dy); break;
                    case 4: if (begin4 != null) begin4(State.begin, id, x, y, dx, dy); break;
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                switch (id)
                {
                    case 0: if (move0 != null) move0(State.move, id, x, y, dx, dy); break;
                    case 1: if (move1 != null) move1(State.move, id, x, y, dx, dy); break;
                    case 2: if (move2 != null) move2(State.move, id, x, y, dx, dy); break;
                    case 3: if (move3 != null) move3(State.move, id, x, y, dx, dy); break;
                    case 4: if (move4 != null) move4(State.move, id, x, y, dx, dy); break;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                switch (id)
                {
                    case 0: if (end0 != null) end0(State.end, id, x, y, dx, dy); break;
                    case 1: if (end1 != null) end1(State.end, id, x, y, dx, dy); break;
                    case 2: if (end2 != null) end2(State.end, id, x, y, dx, dy); break;
                    case 3: if (end3 != null) end3(State.end, id, x, y, dx, dy); break;
                    case 4: if (end4 != null) end4(State.end, id, x, y, dx, dy); break;
                }
            }
        }
    }
    void MouseUpdate()
    {
        int id = 0;
        Vector2 pos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0)) delta[0] = pos;
        float x, y, dx, dy;
        x = pos.x;
        y = pos.y;
        if (Input.GetMouseButtonDown(0))
        {
            dx = dy = 0;
        }
        else {
            dx = pos.x - delta[id].x;
            dy = pos.y - delta[id].y;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (begin0 != null) begin0(State.begin, id, x, y, dx, dy);
        }
        else if (Input.GetMouseButton(0))
        {
            if (move0 != null) move0(State.move, id, x, y, dx, dy);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (end0 != null) end0(State.end, id, x, y, dx, dy);
        }
    }
}

/* ex
public class Controller : MonoBehaviour
{
    bool selected = false;

    void Start()
    {
        TouchEvent.Instance();
        TouchEvent.begin0 += onTouch;
        TouchEvent.end0 += onTouch;
        TouchEvent.move0 += onTouch;
    }

    void onTouch(TouchEvent.State type, int id, float x, float y, float dx, float dy)
    {
        if (!selected && type == TouchEvent.State.begin)
        {
            selected = true;
            //Debug.Log("down:" + x + "," + y);
        }
        else if (selected && type == TouchEvent.State.end)
        {
            selected = false;
            //Debug.Log("end:" + x + "," + y + ", d:" + dx + "," + dy);
        }
        else if (selected && type == TouchEvent.State.move)
        {
            //Debug.Log("move:" + x + "," + y + ", d:" + dx + "," + dy);
        }
    }
}
*/