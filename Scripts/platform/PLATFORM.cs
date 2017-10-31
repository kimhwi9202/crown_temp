using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xLIB;
using UnityEngine.Purchasing.Security;

/// <summary>
/// 플랫폼간의 공통 함수를 정의
/// 추가되는 함수는 여기서부터 정의해야 한다.
/// </summary>
public interface IPlatform
{
    void Initialize();
    void OpenURL_Terms();
    void OpenURL_Rate();
    void OpenURL_TalkToNancy(string id, string name, string mail);
    void OpenURL_AppsStorePackageDownload();
}

/// <summary>
/// - 런타임시 플랫폼 전처리 상관없이 사용하기 위함 매니져는 IPlatform 를 상속하지 않았다.
///  App에서 좀더 유연하게 사용하기 위해 뺏다.
/// - 런타임시 전용 디바이스 플랫폼 관리 클래스를 생성한다. 
/// </summary>
public class PLATFORM : Singleton<PLATFORM>
{
    public delegate void delegateCall(string error);
    public static delegateCall DelegateCall = null;
    private IPlatform curPlatform = null;
    private IAP _IAP = new IAP();   // 유니티엔진 라이브러리 결제모듈
    private FCM _FCM = new FCM();   // 푸시

    public DEF.IAPData GetIAPData() { return _IAP.GetIAPData(); }

    public void Initialize()
    {
        _IAP.Initialize();
        _FCM.Initialize();

        // 화면 꺼짐 방지
#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
#if UNITY_IOS
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
        if (CONFIG.IsRunningAndroid())
        {
            PlatformAndroid.I.Initialize();
            curPlatform = PlatformAndroid.I;
        }
        else if(CONFIG.IsRunningiOS())
        {
            PlatformIOS.I.Initialize();
            curPlatform = PlatformIOS.I;
        }
#if UNITY_EDITOR  // test
        else
        {
            PlatformAndroid.I.Initialize();
            curPlatform = PlatformAndroid.I;
        }
#endif
    }

//    public void BuyPurchase(string productId, System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> Complete)
//    {
//        _IAP.BuyProductID(productId, Complete);
//    }
    public void BuyPurchase(PK.GetPurchaseItems.REDataPacks ItemInfo, System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> Complete)
    {
        Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.TRY_PURCHASE, ItemInfo.id.ToString());

        _IAP.BuyProductID(new DEF.IAPData(ItemInfo), Complete);
    }
    public void BuyPurchase(PK.GetDailyWheelShop.REDataPacks ItemInfo, System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> Complete)
    {
        Main.I.AppsFlyerEvent(AFInAppEvents.SHOP, AFInAppEvents.TRY_PURCHASE, ItemInfo.id.ToString());

        _IAP.BuyProductID(new DEF.IAPData(ItemInfo), Complete);
    }
    //public void BuyPurchase(PK.GetVaultShop.REDataPacks ItemInfo, System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> Complete)
    //{
    //    _IAP.BuyProductID(new DEF.IAPData(ItemInfo), Complete);
    //}

    public void OpenURL_Terms()
    {
        if (curPlatform != null)    curPlatform.OpenURL_Terms();
    }

    public void OpenURL_Rate()
    {
        if (curPlatform != null)    curPlatform.OpenURL_Rate();
    }

    public void OpenURL_TalkToNancy(string id, string name, string mail)
    {
        if (curPlatform != null)    curPlatform.OpenURL_TalkToNancy(id, name, mail);
    }

    public void OpenURL_AppsStorePackageDownload()
    {
        if (curPlatform != null) curPlatform.OpenURL_AppsStorePackageDownload();
    }
}

