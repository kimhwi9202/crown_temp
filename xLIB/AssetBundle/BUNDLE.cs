
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace xLIB
{
#if UNITY_5_3_OR_NEWER

    /// <summary>
    /// 로컬에 일괄 바이너리 형태를 그대로 다운 저장후 이용방식이다.
    /// 네트워크에 접속시 버젼정보가 틀린경우 해당 파일만 다시 다운받아서 로컬에 덮어씌운다.
    /// 장점 : 번들이 자주 업데이트를 하지 않는다면 네트워크 다운로드 자제하므로 이점이 있다.
    /// 단덤 : 로컬에 저장후 다시 캐싱로딩 해야 하므로 두번작업을 한다. (로컬 용량도 두배로 먹는다)
    /// </summary>
    /// <seealso cref="xLIB.Singleton{xLIB.BUNDLE}" />
    public class BUNDLE : Singleton<BUNDLE>
    {
        private static uint CACHING_LIMIT = 1500;

        #region ReadOnlyVariable
        private static readonly string PREFS_KEY = "BundleManager";
        private static string EXPORT_ASSETBUNDLES_NAME = "AssetBundles";
        private static readonly string[] DEF_BUNDLE_EXTENSION = { "unity3d", "ios", "ad", "wsa" };
        #endregion

        #region BundleRefer
        public class STLoadedBundle
        {
            public AssetBundle bundle;
            public int referencedCount;
            public STLoadedBundle(AssetBundle bundle)
            {
                this.bundle = bundle;
                this.referencedCount = 1;
            }
        }
        #endregion

        #region ProgressDelegate    
        public delegate void OnProgressByRatio(float value);
        public delegate void OnError(string pErrorMsg);
        public delegate void OnCompleteBytes(byte[] pResult);
        public delegate void OnCompleteCaching(string pBundleName);
        public delegate void OnComplete();
        #endregion

        #region Varaiable
        private Dictionary<string, STLoadedBundle> loadedBundles = new Dictionary<string, STLoadedBundle>();
        private Dictionary<string, string> localBundleHash;
        private AssetBundleManifest serverBundleHash;
        #endregion

        #region BundleLoadAtAsset
        public void UnLoadBundle(string bundleName, bool unloadAllLoadedObjects=false)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
                loadedBundles[bundleName].bundle.Unload(unloadAllLoadedObjects);
                loadedBundles.Remove(bundleName);
            }
        }

        public bool IsLocalFileCached(string bundleName)
        {
            if (this.localBundleHash.ContainsKey(bundleName))
            {
                return true;
            }
            return false;
        }

        public bool IsBundleCachingVersionCheck(string bundleName)
        {
            if (this.localBundleHash.ContainsKey(bundleName))
            {
                string hash = this.serverBundleHash.GetAssetBundleHash(bundleName).ToString();
                if (this.localBundleHash[bundleName].Equals(hash))
                    return true;
            }
            return false;
        }


        public AssetBundle GetBundle(string bundleName)
        {
            if(loadedBundles.ContainsKey(bundleName))
            {
                return loadedBundles[bundleName].bundle;
            }
            return null;
        }

        public T LoadAsset<T>(string value) where T : Object
        {
            T result = null;

            foreach (string key in loadedBundles.Keys)
            {
                if (loadedBundles[key].bundle.Contains(value))
                {
                    result = loadedBundles[key].bundle.LoadAsset<T>(value);
                    break;
                }
            }

            if (result == null)
                Debug.LogWarning("################# Not Found Assetbundle :" + value);

            return result;
        }

        public AssetBundleRequest LoadAssetAsync<T>(string value) where T : Object
        {
            AssetBundleRequest result = null;
            foreach (string key in loadedBundles.Keys)
            {
                if (loadedBundles[key].bundle.Contains(value))
                {
                    result = loadedBundles[key].bundle.LoadAssetAsync<T>(value);
                    break;
                }
            }

            if (result == null)
                Debug.LogWarning("################# Not Found Assetbundle :" + value);

            return result;
        }
        #endregion

        #region VersionCheck
        public IEnumerator BeginVersionCheck(OnComplete pCallback, OnError pOnError)
        {
            yield return StartCoroutine(LoadAssetBundleMainfast((errorMsg) =>
            {
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    if(pOnError != null) pOnError(errorMsg);
                    /*
                    ShowErrorMsg("ERROR_NET_INSTABILITY", () =>
                    {
                        StartCoroutine(BeginVersionCheck(pCallback, pOnError));
                    }, () =>
                    {
                        Application.Quit();
                    });*/
                }
            }));

            this.localBundleHash = GetBundleHashPrefs(PREFS_KEY);
            yield return null;

            if (pCallback != null)
                pCallback();
        }
        #endregion

        #region AssetBundleHashDownLoad
        private IEnumerator LoadAssetBundleMainfast(OnError pOnCallback)
        {
            string versionAssetPath = GetAssetBundleMainfestPath();
            Caching.maximumAvailableDiskSpace = CACHING_LIMIT * 1024 * 1024;

            int remainMB = (int)(Caching.spaceFree / 1024 / 1024);
            UnityEngine.Debug.Log("##### Caching Info : max = " + (Caching.maximumAvailableDiskSpace/1024/1024) + " MB / remain = " + (Caching.spaceFree/1024/1024) + " MB #####");
            if (remainMB < 1)
                Caching.CleanCache();

            while (!Caching.enabled)
                yield return null;

            while (!Caching.ready)
                yield return null;

            using (WWW versionLoader = new WWW(versionAssetPath))
            {
                while (!versionLoader.isDone && string.IsNullOrEmpty(versionLoader.error))
                {
                    yield return null;
                }

                if (string.IsNullOrEmpty(versionLoader.error))
                {
                    AssetBundle versionBundle = versionLoader.assetBundle;
                    this.serverBundleHash = versionBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
#if UNITY_EDITOR
                    Debug.Log("Server Hash Down Complete : " + versionAssetPath);
#endif
                }
                else
                {
                    Debug.LogError("########## DownLoadMainFest Error@@@@!! : " + versionLoader.error + " Path : " + versionAssetPath);
                }

                pOnCallback(versionLoader.error);
            }
        }
        #endregion

        #region LoadFromCacheOrDownload
        public IEnumerator DownloadUpdateFromServer(string pBundleName, OnProgressByRatio onProgress = null, OnComplete pOnCompleted = null, OnError pOnError = null)
        {
            yield return new WaitForEndOfFrame();
            string pSavePath = GetAssetBundleFilePath(pBundleName);
            Debug.Log(pSavePath);

            if (IsBundleCachingVersionCheck(pBundleName))
            {
                onProgress = null;
                if (GetBundle(pBundleName))
                {
                    if (pOnCompleted != null)
                        pOnCompleted();
                    yield break;
                }
            }

            using (WWW caching = WWW.LoadFromCacheOrDownload(pSavePath, this.serverBundleHash.GetAssetBundleHash(pBundleName)))
            {
                if (onProgress != null)
                {
                    while (!caching.isDone && string.IsNullOrEmpty(caching.error))
                    {
                        //yield return new WaitForSeconds(2f);
                        //if (caching != null) { caching.Dispose(); }

                        if (onProgress != null)
                        {
                            //Debug.Log(pBundleName + " DownloadUpdateFromServer = " + caching.progress);
                            onProgress(caching.progress);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    //if (caching != null) { caching.Dispose(); }

                    yield return caching;
                }

                if (string.IsNullOrEmpty(caching.error))
                {
                    this.loadedBundles.Add(pBundleName, new STLoadedBundle(caching.assetBundle));

                    Debug.Log("Complete DownloadUpdateFromServer = " + caching.progress);

                    // add hash bundle write complete file
                    if (this.localBundleHash.ContainsKey(pBundleName))
                        this.localBundleHash.Remove(pBundleName);

                    // update playerpref hash file
                    this.localBundleHash.Add(pBundleName, this.serverBundleHash.GetAssetBundleHash(pBundleName).ToString());

                    SaveBundleHashPrefs(PREFS_KEY, this.localBundleHash);

                    if (onProgress != null)
                        onProgress(caching.progress);

                    if (pOnCompleted != null)
                        pOnCompleted();
                }
                else
                {
                    Debug.Log("DownloadUpdateFromServer:" + caching.error);
                    if(pOnError != null) pOnError(caching.error);
                }
            }
        }
        #endregion

        #region HelperFunction
        /// <summary>
        /// 번들 다운후 로컬기록후에 해쉬정보 PlayerPrefs 에 기록 (용량은 1메가를 초과할수 없다)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The object.</param>
        public static void SaveBundleHashPrefs(string key, Dictionary<string, string> obj)
        {
            try
            {
                string serialize = string.Empty;
                foreach (KeyValuePair<string, string> pair in obj)
                {
                    serialize += pair.Key.ToString() + ",";
                    serialize += pair.Value.ToString() + ",";
                }
                //Debug.Log("serialize => " + serialize);
                PlayerPrefs.SetString(key, serialize);
                PlayerPrefs.Save();
            }
            catch (PlayerPrefsException exception)
            {
                Debug.LogError("SaveBundleHashPrefs Exception : " + exception);
            }
        }
        /// <summary>
        /// PlayerPrefs에 기록된 BundleHas 정보 제거 (제거되면 처음부터 다시 받는다)
        /// </summary>
        public static void DeleteBundleHashPrefs()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            Caching.CleanCache();
        }
        private Dictionary<string, string> GetBundleHashPrefs(string key)
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                string deserializeData = PlayerPrefs.GetString(key, "");
                //Debug.Log("deSerialize => " + deserializeData);
                if (string.IsNullOrEmpty(deserializeData))
                    return result;

                string[,] grid = xCSVParser.SplitCsvGrid(deserializeData);

                for (int x = 0; x < grid.GetUpperBound(0); x += 2)
                {
                    result.Add(grid[x, 0], grid[x + 1, 0]);
                }

                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError("GetBundleHashPrefs Exception : " + e);
                return null;
            }
        }


        public static void SetExportAssetbundlesName(string name)
        {
            EXPORT_ASSETBUNDLES_NAME = name;
        }


        public static string GetAssetBundleBaseURL()
        {
            return CONFIG.GetBundleURL() + EXPORT_ASSETBUNDLES_NAME + "/" + CONFIG.CurrentPlatform.ToString();
        }
        public static string GetAssetBundleMainfestPath()
        {
            return GetAssetBundleBaseURL() + "/" + CONFIG.CurrentPlatform.ToString();
        }
        public static string GetAssetBundleFilePath(string bundleName)
        {
            return GetAssetBundleBaseURL() + "/" + bundleName;
        }
        /// <summary>
        /// 번들 확장명을 얻는다. 메인에서 플랫폼 선택과 연동된다.
        /// </summary>
        /// <returns></returns>
        public static string GetBundleExtenstion()
        {
            switch (CONFIG.CurrentPlatform)
            {
                case ePlatform.Windows: return DEF_BUNDLE_EXTENSION[0];
                case ePlatform.iOS:     return DEF_BUNDLE_EXTENSION[1];
                case ePlatform.Android: return DEF_BUNDLE_EXTENSION[2];
                case ePlatform.WSAPhone:return DEF_BUNDLE_EXTENSION[3];
            }
            return "";
        }
        #endregion //Bundle Function

        // Occur Error
        private void ShowErrorMsg(string pMsg, System.Action pCallbackOk, System.Action pCallbackCancle)
        {
            /*
            UISceneController.Instance.AlertBox(true, new AlertBox.AlertParam(AlertBox.AlertType.SELECTABLE, new Vector2(800, 450), NetTable.GetSystemTxt(pMsg)), (pResult) => {
                if (pResult == AlertBox.UserSelect.OK)
                    pCallbackOk();
                else {
                    pCallbackCancle();
                }
            });
            */
        }
    }

#endif
}


