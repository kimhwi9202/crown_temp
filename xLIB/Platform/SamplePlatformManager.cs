using UnityEngine;
using System.Collections.Generic;

using xLIB;
using xLIB.Interface;
public class SamplePlatformManager : Singleton<SamplePlatformManager> , SampleIPlatform
{
    public delegate void delegateCall(string error);
    public static delegateCall DelegateCall = null;

    private SampleIPlatform curPlatform;
    public void Initialize()
    {
    }
}

