using UnityEngine;
using System.Collections;

using xLIB;


/// <summary>
/// 안드로이드 전용 싱글톤 함수 
/// </summary>
public class PlatformAndroid : Singleton<PlatformAndroid>, IPlatform
{
    /*
#if UNITY_ANDROID && !UNITY_EDITOR
    //init static class --save memory/space
    private static AndroidJavaClass agent;
    private static AndroidJavaClass unityClass;

    private static string JAVA_CLASS = "com.sdk.migame.payment";
    private static string UNITY_CLASS = "com.unity3d.player.UnityPlayer";

    public static void AttachCurrentThread()
    {
        AndroidJNI.AttachCurrentThread();
    }

    public static void DetachCurrentThread()
    {
        AndroidJNI.DetachCurrentThread();
    }
#endif
*/
    public void Initialize()
    {
        /*
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
*/
    }
    public void OpenURL_Terms()
    {
        Application.OpenURL("https://d3kjdk8bsa0don.cloudfront.net/Terms_of_Service.html?ver=1.0");
    }
    public void OpenURL_Rate()
    {
        OpenURL_Terms();
    }
    public void OpenURL_TalkToNancy(string id, string name, string mail)
    {
        //https://www.sloticagames.com/Contact_us.html?id=1536764736337487&name=Kim HyunJung&mail=aoneplus@nate.com
        string url = "https://www.sloticagames.com/Contact_us.html?id=" + id.ToString() + "&name=" + name + "&mail=" + mail;
        Application.OpenURL(url);
    }
    public void OpenURL_AppsStorePackageDownload()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.crown.mobile.sloticamobile");
    }
}

