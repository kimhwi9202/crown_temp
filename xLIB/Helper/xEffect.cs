using UnityEngine;
using System.Collections;

namespace xLIB
{
    public class xEffect : MonoBehaviour
    {
        public static void Shockwave(GameObject _target, float _continueTime = 0.5f, float _power = 10f)
        {
            float x = Random.Range(_power - 5f, _power);
            float y = Random.Range(_power - 5f, _power);
            Hashtable ht = new Hashtable();
            ht.Add("x", x);
            ht.Add("y", y);
            ht.Add("time", _continueTime);
            iTween.ShakePosition(_target, ht);
        }
    }
}
