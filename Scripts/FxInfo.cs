using UnityEngine;
using System.Collections;

public class FxInfo : MonoBehaviour {
    public ParticleSystem PS;

    public bool OnlyDeactivate;

    void OnEnable()
    {
        StartCoroutine("CheckIfAlive");
    }

    IEnumerator CheckIfAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!PS.IsAlive(true))
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
}
