using UnityEngine;
using System.Collections;

using xLIB;
using xLIB.Interface;

public class SamplePlatformAndroid : Singleton<SamplePlatformAndroid>, SampleIPlatform
{
#if UNITY_ANDROID
    //init static class --save memory/space
    protected static AndroidJavaClass agent;
    protected static AndroidJavaClass unityClass;

    protected static string JAVA_CLASS = "com.sdk.migame.payment";
    protected static string UNITY_CLASS = "com.unity3d.player.UnityPlayer";

    public static void AttachCurrentThread()
    {
        AndroidJNI.AttachCurrentThread();
    }

    public static void DetachCurrentThread()
    {
        AndroidJNI.DetachCurrentThread();
    }
#endif
    public void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass(UNITY_CLASS))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("Initialize", "2882303761517502080", "5281750258080");
                jo.Call("SetReceiver", "Singleton_PlatformAndroid");
            }
        }
#endif
    }
    public void Login()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass(UNITY_CLASS))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("Login");
            }
        }
#endif
    }

    public void OnLoginSuccess(string session)
    {
        Debug.Log("PlatformAndroid::LoginSuccess - session" + session);
        string[] array = session.Split(':');
        if (array.Length > 1)
        {
//            Main.Instance.m_SceneTitleLogin.OnMiLogin(array[0], array[0]);
        }
    }
    public void OnLoginFailed(string msg)
    {
        Debug.Log("PlatformAndroid::OnLoginFailed - msg" + msg);
//        Main.Instance.m_SceneTitleLogin.OnLoginError(msg);
    }
}

