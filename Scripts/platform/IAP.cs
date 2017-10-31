using UnityEngine;
using System.Collections;
using xLIB;
using System;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

/// <summary>
/// 
/// </summary>
public class IAP : MonoBehaviour , IStoreListener
{
    private static IStoreController m_StoreController; // Reference to the Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // Reference to store-specific Purchasing
                                                                // Use this for initialization
    // Google Play Store Public Key ( IAP Obfuscate secrets memnu )
    private static string MyLicenseKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkJuTUG6lpXSc4LHa14LoOvU/oS4CGXWpDBZ1SKBWZ2WhTnpRnbKrltdv6P8CByAKfIB81J5jfgd1k7wsi0fvilJorqwJFvnihQbfVonbzvMDNhH5oxM5fVF5DM3RksJSh1V+XtxfxEmbCEL8HRNpArgRlntws256ePa7rvZrkbepMvA+gCjMCjZoyUYXEC8yrFc4bXhraLdxsVfW9cJ7CkKQ/8T8Gl+SkAHHHMm5y61IM8uqyPe60Cpm0M4uC/7+RrKpNzC5zCUNDeImOsDVUctH4jTGWDzS+wseA70SGee/38tSL9wAlR+G0nelnsSL/VPu/RbzLFVIHY48iMvLuwIDAQAB";

    protected System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> _onComplete = null;
    protected string strProductId = "power_item";
    protected DEF.IAPData iapData;

    public DEF.IAPData GetIAPData() { return iapData; }

    /*
    // 결제된 심플 
    string receipt = "{\"Store\":\"GooglePlay\",\"TransactionID\":\"GPA.3336-4764-6030-10363\",\"Payload\":\"{\"json\":\"{\\\"orderId\\\":\\\"GPA.3336-4764-6030-10363\\\",\\\"packageName\\\":\\\"com.crown.mobile.sloticamobile\\\",\\\"productId\\\":\\\"125\\\",\\\"purchaseTime\\\":1494557221480,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"apgdhagjkpjfildegpeoneac.AO-J1Ozm6ziGOiwrvj0jhueuMrnGZSqSvzua9ftxZoGc0DQeDmlhWwGxCK_0D_C-NCH5Erjd8xuFmhYN8qblsABRZaPbe93oAclh57Ldt-vldv8cedo3DAbXvddfyjlLGyL-GsJll_wg\\\"}\",\"signature\":\"VqESYS1qVVIkAWkqM4Lv8MPn9STcXEDQLxNy7N+T19PhE2wPk\\/b9f+z2N50rMj0CLx9rKXJ\\/1s8eCgtJHN5OMFpndtBncQ3PVnRe1cvx7WQ\\/NCvuzK6cLvdO4TDMj+LlRHBrktkxoULYMDY82rhCA7ffy9h2uKACCA9kGyomt0Mdwv2eJApcj7b14Kv9iTfswB\\/EtVFuuWwmdd\\/pwMTnSKgCQcnmbJNTPl8q9jVq\\/KfuVEdP0u8X8Kuxl2evX7ETx2EvVYarm0eSvc8CkKedu6tELkZfeESUk9xLDfZQApdn05thM2wHdtD+51mOfYvaf584SM5lP3pd9U\\/xGOYSjA==\"}\"}";
    */

    public void Initialize()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
        else
        {
            Debug.Log("Start : not null");
        }
    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.Configure<IGooglePlayConfiguration>().SetPublicKey(MyLicenseKey);

        //Debug.Log("INIT COMPLETE :: MODULE!!!!");

        // 구글플레이 및 iOS 개발센터에 등록된 상품코드 매칭용 
        // 상품 코드 변경시 같이 변경할것.
        for (int i = 1; i <= 252; i++)
        {
            builder.AddProduct(i.ToString(), ProductType.Consumable);
        }

        Debug.Log("INIT IAP Add Product ( 0 ~ 252 ) COMPLETE!!! ");

        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");
        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        if (_onComplete != null) _onComplete("failed", null, null);
    }

    /// <summary>
    /// 구매성공 Processes the purchase.
    /// </summary>
    /// <param name="args">The <see cref="PurchaseEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        bool validPurchase = true; // Presume valid for platforms with no R.V.

        // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // Prepare the validator with the secrets we prepared in the Editor
        // obfuscation window.
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.bundleIdentifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(args.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);

                GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                if (null != google)
                {
                    // This is Google's Order ID.
                    // Note that it is null when testing in the sandbox
                    // because Google's sandbox does not provide Order IDs.
                    Debug.Log(google.transactionID);
                    Debug.Log(google.purchaseState);
                    Debug.Log(google.purchaseToken);
                }

                AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
                if (null != apple)
                {
                    Debug.Log(apple.originalTransactionIdentifier);
                    Debug.Log(apple.subscriptionExpirationDate);
                    Debug.Log(apple.cancellationDate);
                    Debug.Log(apple.quantity);
                }

                if (_onComplete != null) _onComplete("ok", google, apple);
            }
        }
        catch (IAPSecurityException)
        {
            Debug.Log("Invalid receipt, not unlocking content");
            validPurchase = false;
        }
#endif

        if (validPurchase)
        {
            if (_onComplete != null) _onComplete("failed", null, null);
            // Unlock the appropriate content here.
        }
        /*
        // A consumable product has been purchased by this user.
        UnityEngine.Debug.Log("## ProcessPurchase >> receipt = " + args.purchasedProduct.receipt);

        if (String.Equals(args.purchasedProduct.definition.id, strProductId, StringComparison.Ordinal))
        {
            UnityEngine.Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            if (_onComplete != null) _onComplete(args.purchasedProduct.definition.id, args.purchasedProduct.receipt);
        }
        else if (_onComplete != null)
        {
            _onComplete("failed", "Not Equals ProductId : " + strProductId);
        }
        // Return a flag indicating wither this product has completely been received, or if the application needs to be reminded of this purchase at next app launch. Is useful when saving purchased products to the cloud, and when that save is delayed.
        */
        return PurchaseProcessingResult.Complete;
    }


    /// <summary>
    /// 구매실패 Called when [purchase failed].
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="failureReason">The failure reason.</param>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        if (_onComplete != null) _onComplete("failed", null, null);
    }


    /// <summary>
    /// 구매 Buys the product identifier.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    public void BuyProductID(DEF.IAPData _itemInfo, System.Action<string, GooglePlayReceipt, AppleInAppPurchaseReceipt> Complete)
    {
#if UNITY_EDITOR
        if (Complete != null)
        {
            Complete("failed", null, null);
            return;
        }
#endif

        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            iapData = _itemInfo;
            //strProductId = productId;

            _onComplete = Complete;
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(iapData.id.ToString());

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                if (_onComplete != null) _onComplete("failed", null, null);
                // ... report the product look-up failure situation  
                Debug.Log("Buy ProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                UI.I.ShowMsgBox("Buy ProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            if (_onComplete != null) _onComplete("failed", null, null);
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("Buy ProductID FAIL. Not initialized.");
            UI.I.ShowMsgBox("Buy ProductID FAIL. Not initialized.");
        }
    }
}


