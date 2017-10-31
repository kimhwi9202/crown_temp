using UnityEngine;
using UnityEngine.UI;
using xLIB;


public class SetLocalization : MonoBehaviour {
#if UNITY_EDITOR // 작업시에 즉시 언어를 변경시 적용키 위해..빌드에선 불필요한 낭비다.
    protected eLocalization local;
#endif
    public string key;
	// Use this for initialization
	void Start () {
#if UNITY_EDITOR
        local = CONFIG.CurrentLocalization;
#endif
        gameObject.GetComponent<Text>().text = Localization.Get(key);
    }
#if UNITY_EDITOR    
    void OnEnable()
    {
        if(local != CONFIG.CurrentLocalization)
        {
            gameObject.GetComponent<Text>().text = Localization.Get(key);
        }
    }
#endif
}
