using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ItemPageIcon : MonoBehaviour
{
    public Image _icon;
    public GameObject _prefabsEffect;
    public GameObject _effect;

    void Awake()
    {
        //_effect = xLIB.xHelper.AddChild(this.gameObject, _prefabsEffect);
        //_effect.gameObject.GetComponent<Transform>().localScale = new Vector3(100f, 100f, 100f);
    }

    public void SetSelect(bool sel)
    {
        if (sel)
        {
            _icon.color = Color.white;
            //_effect.gameObject.SetActive(true);
        }
        else
        {
            _icon.color = new Color32(104, 104, 104, 255);
            //_effect.gameObject.SetActive(false);
        }
    }
}
