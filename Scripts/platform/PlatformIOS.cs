using UnityEngine;
using System.Collections;

using xLIB;

/// <summary>
/// IOS 전용 싱글톤 함수 
/// </summary>
public class PlatformIOS : Singleton<PlatformIOS>, IPlatform
{
    public void Initialize()
    {
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
        string url = "https://www.sloticagames.com/Contact_us.html?id=" + id.ToString() + "&name=" + name + "&mail=" + mail;
        url.Replace(" ", "%20");
        Application.OpenURL(WWW.EscapeURL(url));
        //Application.OpenURL("https://d3kjdk8bsa0don.cloudfront.net/Terms_of_Service.html?ver=1.0");
    }
    public void OpenURL_AppsStorePackageDownload()
    {
        //Application.OpenURL("http://itunes.apple.com/<country>/app/<app–name>/id<app-ID>?mt=8");
    }
}
