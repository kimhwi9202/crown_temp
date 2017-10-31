using UnityEngine;
using CatchCo; // My namespace to avoid conflicts



/// <summary>
/// http://rapapa.net/?p=2550
/// Reflection 적용 예제
/// </summary>
public class TestMonoBehaviour : MonoBehaviour
{
    // This is our fancy attribute. Easy no?
    [ExposeMethodInEditor]
    public void DoThePublicThing()
    {
        Debug.Log("DoThePublicThing");
    }

    // We'll make it work on private methods too
    [ExposeMethodInEditor]
    private void DoThePrivateThing()
    {
        Debug.Log("Thing done in private");
    }
}
