using UnityEngine;
using System.Collections.Generic;
using xLIB;

using DG.Tweening;

#region Casche
/// <summary>
/// FX 이펙트 객체 캐쉬
/// </summary>
public class STFXCache
{
    public int count;
    public GameObject prefab;
    public List<GameObject> buffer = new List<GameObject>();    // 버퍼..
    private Transform _parent;

    public void remove_all()
    {
        count = 0;
        prefab = null;
        buffer.Clear();
    }

    private void _create()
    {
        //GameObject obj = GameObject.Instantiate(prefab) as GameObject;
        GameObject obj = xHelper.AddChild(_parent.gameObject, prefab);
        //obj.transform.parent.SetParent(_parent);// FX.I.gameObject.transform;
        //obj.layer = layer;
        obj.SetActive(false);
        buffer.Add(obj);
    }

    public void CreateBuffer(Transform parent, GameObject _prefab, int _count, int _layer=0)
    {
        //GameObject go = xHelper.AddChild(parent.gameObject, _prefab);
        //_parent.SetParent(go.GetComponent<Transform>().transform);
        _parent = parent;
        count = 0;
        prefab = _prefab;
        xHelper.SetLayerRecursively(prefab, _layer);
        for (int i = 0; i < _count; i++) _create();
    }

    public GameObject Attach()
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            GameObject obj = (GameObject)buffer[i];
            if (obj != null && obj.activeSelf == false)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        _create();
        int idx = buffer.Count - 1;

        GameObject new_obj = (GameObject)buffer[idx];
        new_obj.SetActive(true);

        return new_obj;
    }

    public GameObject Attach(Vector3 _pos)    { return Attach(_pos, Quaternion.identity);}
    public GameObject Attach(Vector3 _pos, Quaternion _quat)
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            GameObject obj = (GameObject)buffer[i];
            if (obj != null && obj.activeSelf == false)
            {
                obj.transform.position = _pos;
                obj.transform.localRotation = _quat;
                obj.SetActive(true);
                return obj;
            }
        }

        _create();
        int idx = buffer.Count - 1;

        GameObject new_obj = (GameObject)buffer[idx];
        new_obj.transform.position = _pos;
        new_obj.transform.localRotation = _quat;
        new_obj.SetActive(true);

        return new_obj;
    }
    public GameObject GetBuffer()
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            GameObject obj = (GameObject)buffer[i];
            if (obj != null && obj.activeSelf == false)
            {
                return obj;
            }
        }

        _create();
        int idx = buffer.Count - 1;

        GameObject new_obj = (GameObject)buffer[idx];
        return new_obj;
    }

    public GameObject LocalPosAttach(Vector3 _pos)
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            GameObject obj = (GameObject)buffer[i];
            if (obj != null && obj.activeSelf == false)
            {
                obj.transform.localPosition = _pos;
                obj.SetActive(true);
                return obj;
            }
        }

        _create();
        int idx = buffer.Count - 1;

        GameObject new_obj = (GameObject)buffer[idx];
        new_obj.transform.localPosition = _pos;
        new_obj.SetActive(true);

        return new_obj;
    }
}
#endregion // cache

/// <summary>
/// 이펙트 매니져 ( 객체의 캐쉬화해서 재활용한다 ) 
/// 수요가 많을시에 생성로직은 있지만 활용 빈도가 낮은것을 제거 하는 로직은 아직 없다
/// </summary>
/// <seealso cref="xLIB.Singleton{FX}" />
public class FX : Singleton<FX>
{
    protected Dictionary<string, GameObject> m_pfRefs;
    protected string m_url;
    protected int m_ver;

    protected STFXCache _Coin = new STFXCache();
    protected GameObject _objCoin;
    protected STFXCache _CoinBang = new STFXCache();
    protected GameObject _objCoinBang;
    protected STFXCache _PigCoin = new STFXCache();
    protected GameObject _objPigCoin;
    protected STFXCache _LikeCoin = new STFXCache();
    protected GameObject _objLikeCoin;

    protected GameObject Get_Prefab(string keyName)
    {
        GameObject Ref = null;
        if (m_pfRefs.ContainsKey(keyName)) m_pfRefs.TryGetValue(keyName, out Ref);
        else
        {
            Ref = Resources.Load(keyName, typeof(GameObject)) as GameObject;
            if (Ref) m_pfRefs.Add(keyName, Ref);
            else Debug.Log("@@@@@@@@@@@@  Not Found FX Prefab = " + keyName + " @@@@@@@@@@@@");
        }
        return Ref;
    }
    protected GameObject Get_BundlePrefab(string keyName)
    {
        GameObject Ref = null;
        if (m_pfRefs.ContainsKey(keyName)) m_pfRefs.TryGetValue(keyName, out Ref);
        else
        {
            Ref = BUNDLE.I.LoadAsset<GameObject>(keyName);
            if (Ref) m_pfRefs.Add(keyName, Ref);
            else Debug.Log("@@@@@@@@@@@@  Not Found FX Prefab = " + keyName + " @@@@@@@@@@@@");
        }
        return Ref;
    }

    public void Initialize()
    {
        m_pfRefs = new Dictionary<string, GameObject>();

        _Coin.CreateBuffer(this.gameObject.transform, Get_Prefab("Particle/ef_CoinCollect"), 1);
        _objCoin = _Coin.GetBuffer();

        //_CoinBang.CreateBuffer(this.gameObject.transform, Get_Prefab("Particle/balancce_coin_Ani"), 1);
        //_objCoinBang = _CoinBang.GetBuffer();

        _PigCoin.CreateBuffer(this.gameObject.transform, Get_Prefab("Particle/ef_PigCoin"), 10);
        _objPigCoin = _PigCoin.GetBuffer();

        _LikeCoin.CreateBuffer(this.gameObject.transform, Get_Prefab("Particle/ef_LikeCoin"), 2);
        _objLikeCoin = _PigCoin.GetBuffer();
    }

    public void PlayLikeCoins(GameObject start, GameObject end, System.Action complete)
    {
        GameObject objLikeCoin = _PigCoin.GetBuffer();

        //타겟의 포지션을 월드좌표에서 ViewPort좌표로 변환하고 다시 ViewPort좌표를 NGUI월드좌표로 변환합니다.
        Vector3 startPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(start.GetComponent<Transform>().position));
        startPos.z = 90f;
        //Debug.Log("FX start pos : " + startPos);

        Vector3 endPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(end.GetComponent<Transform>().position));
        endPos.z = 90f;
        //endPos.y -= 4.5f;  // 코인 중앙으로 조절..
        //Debug.Log("FX end pos : " + endPos);

        objLikeCoin.GetComponent<Transform>().position = startPos;
        objLikeCoin.GetComponent<Transform>().localScale = new Vector3(15f, 15f, 15f);
        objLikeCoin.SetActive(true);

        objLikeCoin.GetComponent<Transform>().DOScale(5f, 1.0f);

        objLikeCoin.GetComponent<Transform>().DOMove(endPos, 1.0f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            objLikeCoin.SetActive(false);
            if (complete != null) complete();
        });
    }

    public void PlayPigCoins(GameObject start, GameObject end, System.Action complete)
    {
        //SOUND.I.Play(DEF.SND.balance_flow);
        GameObject objPigCoin = _PigCoin.GetBuffer();

        //타겟의 포지션을 월드좌표에서 ViewPort좌표로 변환하고 다시 ViewPort좌표를 NGUI월드좌표로 변환합니다.
        Vector3 startPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(start.GetComponent<Transform>().position));
        startPos.z = 90f;
        //Debug.Log("FX start pos : " + startPos);

        Vector3 endPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(end.GetComponent<Transform>().position));
        endPos.z = 90f;
        endPos.y -= 4.5f;  // 코인 중앙으로 조절..
        //Debug.Log("FX end pos : " + endPos);

        objPigCoin.GetComponent<Transform>().position = startPos;
        objPigCoin.GetComponent<Transform>().localScale = new Vector3(50f, 50f, 0f);
        objPigCoin.SetActive(true);

        objPigCoin.GetComponent<Transform>().DOScale(10f, 1.0f);

        objPigCoin.GetComponent<Transform>().DOMove(endPos, 1.0f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            objPigCoin.SetActive(false);
            //SOUND.I.Play(DEF.SND.balance_up);
            if (complete != null) complete();
            //USER.I.onUpdateUserInfo();
        });
    }

    public void PlayCoins(GameObject start, GameObject end, System.Action complete)
    {
        SOUND.I.Play(DEF.SND.balance_flow);

        //타겟의 포지션을 월드좌표에서 ViewPort좌표로 변환하고 다시 ViewPort좌표를 NGUI월드좌표로 변환합니다.
        Vector3 startPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(start.GetComponent<Transform>().position));
        startPos.z = 90f;
        //Debug.Log("FX start pos : " + startPos);

        Vector3 endPos = Main.I.MainCamera.ViewportToWorldPoint(Main.I.MainCamera.WorldToViewportPoint(end.GetComponent<Transform>().position));
        endPos.z = 90f;
        endPos.y -= 4.5f;  // 코인 중앙으로 조절..
        //Debug.Log("FX end pos : " + endPos);
        //_objCoinBang.GetComponent<Transform>().position = endPos;
        
        _objCoin.GetComponent<Transform>().position = startPos;
        _objCoin.SetActive(true);

        _objCoin.GetComponent<Transform>().DOMove(endPos, 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            _objCoin.SetActive(false);
            SOUND.I.Play(DEF.SND.balance_up);
            //_objCoinBang.SetActive(true);
            Lobby.I._TopMenu.PlayCoinEffect();
            if (complete != null) complete();
            USER.I.onUpdateUserInfo();
        });
    }


    public Vector2 test(GameObject canvas, Vector3 target)
    {
        //    first you need the RectTransform component of your canvas
        RectTransform CanvasRect = canvas.GetComponent<RectTransform>();
        /*
            then you calculate the position of the UI element, 0,0 for the canvas is at the center of
            the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this,
            you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        */
        Vector2 ViewportPosition = Main.I.MainCamera.WorldToViewportPoint(target);
        // clamp the grenade to the screen boundaries
        ViewportPosition.x = Mathf.Clamp01(ViewportPosition.x);
        ViewportPosition.y = Mathf.Clamp01(ViewportPosition.y);

        Vector2 WorldObject_ScreenPosition = new Vector2(((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                                                                ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));
        //    now you can set the position of the ui element
        //this.rectTransform.anchoredPosition = WorldObject_ScreenPosition;
        return WorldObject_ScreenPosition;
    }
}

/// <summary>
/// 앵커 포인트 또는 피벗의 위치에 관계없이 캔바스의 크기를 조절하거나 렌더링하는 방법에 관계없이
/// 이 코드 조각은 항상 RectTransform의 사각형을 화면 좌표로 반환합니다.
/// </summary>
public static class RectTransformExtension
{
    public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
    {

        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }
}