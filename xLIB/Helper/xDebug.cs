using UnityEngine;
using System.Collections;

namespace xLIB
{
    public class xDebug : MonoBehaviour
    {
        static public void Log(string msg, string color="#FFFFFF")
        {
            UnityEngine.Debug.Log("<Color="+ color+">" + msg + " </Color>");
        }
    }
}
