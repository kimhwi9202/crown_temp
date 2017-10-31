using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(ParticleSystem))]
public class FxControl : MonoBehaviour
{
    private ParticleSystem _PS = null;
    private Renderer _Render = null;

    //public SortingLayer layer = SortingLayer.layers.;

    public bool OnlyDeactivate;
    public bool test = false;

    void Awake()
    {
        _PS = this.GetComponent<ParticleSystem>();
        _Render = _PS.GetComponent<Renderer>();
    }

    void Start()
    {
        if(_Render) _Render.sortingLayerName = "effect";
    }

    void OnEnable()
    {
        StartCoroutine("CheckIfAlive");
    }

    IEnumerator CheckIfAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!_PS.IsAlive(true))
            {
                if (OnlyDeactivate)
                {
#if UNITY_3_5
						this.gameObject.SetActiveRecursively(false);
#else
                    this.gameObject.SetActive(false);
#endif
                }
                else
                    GameObject.Destroy(this.gameObject);
                break;
            }
        }
    }

    void Update()
    {
        if (test == true)
        {
            test = false;
            //GetComponent<DOTweenPath>().DOPlay();
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(554f, -34f);
            this.gameObject.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-556f, -33f), 10f);
        }
    }
}
