using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// SDK v4.6.2
public class AppsFlyerTrackerCallbacks : MonoBehaviour
{
    readonly string APPSFLAYER_DEV_KEY = "GWXTmhCem5XCeNbchrvYEc";
    readonly string APP_PACKAGE_NAME = "com.crown.mobile.sloticamobile";

    // Use this for initialization
    void Start () {
		print ("@@@@@ AppsFlyerTrackerCallbacks on Start @@@@@");

#if UNITY_IOS
        AppsFlyer.setAppsFlyerKey (APPSFLAYER_DEV_KEY);
        AppsFlyer.setAppID (APP_PACKAGE_NAME);
        // For detailed logging     
        // AppsFlyer.setIsDebug (true);
        // For getting the conversion data will be triggered on AppsFlyerTrackerCallbacks.cs file     
        AppsFlyer.getConversionData ();               
        // For testing validate in app purchase (test against Apple's sandbox environment     
        //AppsFlyer.setIsSandbox(true);                   
        AppsFlyer.trackAppLaunch (); 
#elif UNITY_ANDROID
        AppsFlyer.setAppsFlyerKey(APPSFLAYER_DEV_KEY);
        AppsFlyer.setAppID (APP_PACKAGE_NAME);
        //AppsFlyer.setIsDebug (true);     
        //AppsFlyer.createValidateInAppListener ("AppsFlyerTrackerCallbacks", "onInAppBillingSuccess", "onInAppBillingFailure");     
        //AppsFlyer.loadConversionData("AppsFlyerTrackerCallbacks","didReceiveConversionData", "didReceiveConversionDataWithError"); 
#endif

        DontDestroyOnLoad(this.gameObject);
    }
	
    public void didReceiveConversionData(string conversionData) {
		print ("AppsFlyerTrackerCallbacks:: got conversion data = " + conversionData);
	}
	
	public void didReceiveConversionDataWithError(string error) {
		print ("AppsFlyerTrackerCallbacks:: got conversion data error = " + error);
	}
	
	public void didFinishValidateReceipt(string validateResult) {
		print ("AppsFlyerTrackerCallbacks:: got didFinishValidateReceipt  = " + validateResult);
		
	}
	
	public void didFinishValidateReceiptWithError (string error) {
		print ("AppsFlyerTrackerCallbacks:: got idFinishValidateReceiptWithError error = " + error);
		
	}
	
	public void onAppOpenAttribution(string validateResult) {
		print ("AppsFlyerTrackerCallbacks:: got onAppOpenAttribution  = " + validateResult);
		
	}
	
	public void onAppOpenAttributionFailure (string error) {
		print ("AppsFlyerTrackerCallbacks:: got onAppOpenAttributionFailure error = " + error);
		
	}
	
	public void onInAppBillingSuccess () {
		print ("AppsFlyerTrackerCallbacks:: got onInAppBillingSuccess succcess");
		
	}
	public void onInAppBillingFailure (string error) {
		print ("AppsFlyerTrackerCallbacks:: got onInAppBillingFailure error = " + error);
		
	}
}
