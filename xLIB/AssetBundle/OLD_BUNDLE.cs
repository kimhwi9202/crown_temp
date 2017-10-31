
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
    public class OLD_BUNDLE : Singleton<BUNDLE>
    {

        private static uint CACHING_LIMIT = 1500;

        #region ReadOnlyVariable
        private static readonly string PREFS_KEY = "BundleManager";
        private static readonly string EXPORT_ASSETBUNDLES_NAME = "AssetBundles";
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
            string pSavePath = "file:///" + xSystem.AppAllocatePath(bundleName);
            if(Caching.IsVersionCached(pSavePath, this.serverBundleHash.GetAssetBundleHash(bundleName)))
            {
                //Debug.Log("IsCached = " + bundleName);
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
        public IEnumerator BeginVersionCheck(OnComplete pCallback)
        {
            yield return StartCoroutine(LoadAssetBundleMainfast((errorMsg) =>
            {
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    ShowErrorMsg("ERROR_NET_INSTABILITY", () =>
                    {
                        StartCoroutine(BeginVersionCheck(pCallback));
                    }, () =>
                    {
                        Application.Quit();
                    });
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

        #region BundleDownLoad
        /// <summary>
        /// 메니페스트에 있는 모든 번들 다운로드 처리
        /// </summary>
        /// <param name="pOnProgress">The p on progress.</param>
        /// <param name="pOnCompleted">The p on completed.</param>
        /// <returns></returns>
        public IEnumerator DownloadUpdateFromServer(OnProgressByRatio pOnProgress = null, OnComplete pOnCompleted = null)
        {
            float duration = Time.time;
            List<System.IO.FileInfo> needRemoveFiles = GetNeedRemoveFiles();
            for (int i = 0; i < needRemoveFiles.Count; i++)
            {
#if UNITY_EDITOR
                Debug.Log("Delete :" + needRemoveFiles[i].Name);
#endif
                xSystem.RemoveFile(needRemoveFiles[i].FullName);
                yield return null;
            }
#if UNITY_EDITOR
            Debug.Log("Total Amount : " + needRemoveFiles.Count + " Remove TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
#endif

            duration = Time.time;
            List<string> needUpdateFromServerBundles = GetNeedRequestServerBundles();
            int totalAmount = needUpdateFromServerBundles.Count;

            yield return null;

            for (int i = 0; i < totalAmount; i++)
            {
                string bundleName = needUpdateFromServerBundles[i];
                byte[] pResultBytes = null;
                // bundle download from webserver process
                yield return StartCoroutine(DownLoadBundle(bundleName, pOnProgress, (pBytes) =>
                {
                    pResultBytes = pBytes;
                }));

                while (pResultBytes == null)
                    yield return null;

                // downloaded bundle write local hard                        
                yield return StartCoroutine(WriteLocalFile(bundleName, pResultBytes, pOnProgress, (pWriteError) =>
                {
                    Debug.Log("Error Incorrect : " + pWriteError);
                    ShowErrorMsg("ERROR_STORAGE_INSUFFICIENT", () =>
                    {
                        Caching.CleanCache();
                        Application.Quit();
                    });
                }));

                // add hash bundle write complete file
                if (this.localBundleHash.ContainsKey(bundleName))
                    this.localBundleHash.Remove(bundleName);

                // update playerpref hash file
                this.localBundleHash.Add(bundleName, this.serverBundleHash.GetAssetBundleHash(bundleName).ToString());

                SaveBundleHashPrefs(PREFS_KEY, this.localBundleHash);
            }
#if UNITY_EDITOR
            Debug.Log("Total Amount : " + totalAmount + " Download & Write TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
#endif

            duration = Time.time;
            List<string> requestCachingList = new List<string>(this.serverBundleHash.GetAllAssetBundles());
            bool isErrorOccur = false;
            for (int i = 0; i < requestCachingList.Count; i++)
            {
                StartCoroutine(CachingBundle(requestCachingList[i], null, (pCompleteName) =>
                {
                    requestCachingList.Remove(pCompleteName);
                }, (pErrorMsg) =>
                {
                    Debug.Log("Caching Disk Error :" + pErrorMsg);
                    isErrorOccur = true;
                }));
            }

            int cachingtotalAmount = requestCachingList.Count;
            int cachingAmount = 0;

            while (requestCachingList.Count > 0)
            {
                if (cachingAmount != cachingtotalAmount - requestCachingList.Count)
                {
                    cachingAmount = cachingtotalAmount - requestCachingList.Count;
                    pOnProgress((float)cachingAmount / cachingtotalAmount);
                }

                if (isErrorOccur)
                {
                    Debug.Log("Error Incorrect : " + isErrorOccur);
                    //UISceneController.Instance.AlertBox(true, new AlertBox.AlertParam(AlertBox.AlertType.SELECTABLE, new Vector2(700, 400), NetTable.GetSystemTxt("ERROR_NET_SOCKET_NORESPONSE")), (pResult) => {
                    //    Application.Quit();
                    //});
                    yield break;
                }

                yield return null;
            }

            int remainMB = (int)(Caching.maximumAvailableDiskSpace / 1024 / 1024);
            if (remainMB < 1)
            {
                Debug.Log("Remain Disk : " + remainMB + " Free : " + remainMB);
                ShowErrorMsg("ERROR_STORAGE_INSUFFICIENT", () =>
                {
                    Caching.CleanCache();
                    Application.Quit();
                });
                yield break;
            }

#if UNITY_EDITOR
            //Debug.Log("Total Amount : " + totalAmount + " Caching TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
            Debug.Log(" Used :" + (Caching.spaceOccupied / 1024 / 1024) + "||" + (Caching.spaceFree / 1024 / 1024) + "||" + (Caching.maximumAvailableDiskSpace / 1024 / 1024));
#endif

            if (pOnCompleted != null)
            {
                pOnCompleted();
            }
        }
        #endregion



        #region AssetbundleCaching
        public IEnumerator DownloadUpdateFromServer(string pBundleName, OnProgressByRatio onProgress = null, OnComplete pOnCompleted = null)
        {
            yield return new WaitForEndOfFrame();
            string pSavePath = GetAssetBundleFilePath(pBundleName);
            using (WWW caching = WWW.LoadFromCacheOrDownload(pSavePath, this.serverBundleHash.GetAssetBundleHash(pBundleName)))
            {
                if (onProgress != null)
                {
                    while (!caching.isDone && string.IsNullOrEmpty(caching.error))
                    {
                        if (onProgress != null)
                        {
                            Debug.Log("DownloadUpdateFromServer = " + caching.progress);
                            onProgress(caching.progress);
                        }
                        yield return null;
                    }
                }
                else
                {
                    yield return caching;
                }

                if (string.IsNullOrEmpty(caching.error))
                {
                    this.loadedBundles.Add(pBundleName, new STLoadedBundle(caching.assetBundle));

                    if (onProgress != null)
                        onProgress(caching.progress);

                    if (pOnCompleted != null)
                        pOnCompleted();
                }
                else
                {
                    Debug.Log("DownloadUpdateFromServer:" + caching.error);
                }
            }
        }
        #endregion

        /*
        #region BundleDownLoad
        /// <summary>
        /// 개별 번들 다운로드 
        /// </summary>
        /// <param name="targetBundle">The target bundle.</param>
        /// <param name="pOnProgress">The p on progress.</param>
        /// <param name="pOnCompleted">The p on completed.</param>
        /// <returns></returns>
        public IEnumerator DownloadUpdateFromServer(string targetBundle, OnProgressByRatio pOnProgress = null, OnComplete pOnCompleted = null)
        {
            float duration = Time.time;
            List<System.IO.FileInfo> needRemoveFiles = GetNeedRemoveFiles();
            for (int i = 0; i < needRemoveFiles.Count; i++)
            {
#if UNITY_EDITOR
                Debug.Log("Delete :" + needRemoveFiles[i].Name);
#endif
                xSystem.RemoveFile(needRemoveFiles[i].FullName);
                yield return null;
            }
#if UNITY_EDITOR
            Debug.Log(targetBundle + " >>> Total Amount : " + needRemoveFiles.Count + " Remove TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
#endif

            duration = Time.time;
            List<string> needUpdateFromServerBundles = GetNeedRequestServerBundles(targetBundle);
            int totalAmount = needUpdateFromServerBundles.Count;

            yield return null;

            for (int i = 0; i < totalAmount; i++)
            {
                string bundleName = needUpdateFromServerBundles[i];
                byte[] pResultBytes = null;
                // bundle download from webserver process
                yield return StartCoroutine(DownLoadBundle(bundleName, pOnProgress, (pBytes) =>
                {
                    pResultBytes = pBytes;
                }));

                while (pResultBytes == null)
                    yield return null;

                // downloaded bundle write local hard                        
                yield return StartCoroutine(WriteLocalFile(bundleName, pResultBytes, pOnProgress, (pWriteError) =>
                {
                    Debug.Log("Error Incorrect : " + pWriteError);
                    ShowErrorMsg("ERROR_STORAGE_INSUFFICIENT", () =>
                    {
                        Caching.CleanCache();
                        Application.Quit();
                    });
                }));

                // add hash bundle write complete file
                if (this.localBundleHash.ContainsKey(bundleName))
                    this.localBundleHash.Remove(bundleName);

                // update playerpref hash file
                this.localBundleHash.Add(bundleName, this.serverBundleHash.GetAssetBundleHash(bundleName).ToString());

                SaveBundleHashPrefs(PREFS_KEY, this.localBundleHash);
            }

#if UNITY_EDITOR
            Debug.Log("Total Amount : " + totalAmount + " Download & Write TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
#endif
            //if (totalAmount > 0)
            {
                duration = Time.time;
                //List<string> requestCachingList = new List<string>(this.serverBundleHash.GetAllAssetBundles());
                //requestCachingList.Clear();
                //requestCachingList.Add(targetBundle);
                //bool isErrorOccur = false;
                //for (int i = 0; i < requestCachingList.Count; i++)
                {
                    yield return StartCoroutine(CachingBundle(targetBundle, null, (pCompleteName) =>
                    {
                        //requestCachingList.Remove(pCompleteName);
                    }, (pErrorMsg) =>
                    {
                        Debug.Log("Caching Disk Error :" + pErrorMsg);
                        //isErrorOccur = true;
                    }));
                }
            }
            int remainMB = (int)(Caching.maximumAvailableDiskSpace / 1024 / 1024);
            if (remainMB < 1)
            {
                Debug.Log("Remain Disk : " + remainMB + " Free : " + remainMB);
                ShowErrorMsg("ERROR_STORAGE_INSUFFICIENT", () =>
                {
                    Caching.CleanCache();
                    Application.Quit();
                });
                yield break;
            }

#if UNITY_EDITOR
            //Debug.Log("Total Amount : " + totalAmount + " Caching TIme :<color=#ff0000>" + (Time.time - duration) + "</color>");
            Debug.Log(" Used :" + (Caching.spaceOccupied / 1024 / 1024) + "||" + (Caching.spaceFree / 1024 / 1024) + "||" + (Caching.maximumAvailableDiskSpace / 1024 / 1024));
#endif

            if (pOnCompleted != null)
            {
                pOnCompleted();
            }
        }
        #endregion
        */

        #region BundleDownloadFromServer
        private IEnumerator DownLoadBundle(string pBundleName, OnProgressByRatio pOnProgress = null, OnCompleteBytes pOnCallbackComplete = null)
        {
            string versionAssetPath = GetAssetBundleFilePath(pBundleName);
            Debug.Log(versionAssetPath);
            using (WWW loader = new WWW(versionAssetPath))
            {
                while (!loader.isDone && string.IsNullOrEmpty(loader.error))
                {
                    if (pOnProgress != null)
                    {
                        pOnProgress(loader.progress * 0.9f);
                    }

                    yield return null;
                }

                if (!string.IsNullOrEmpty(loader.error))
                {
                    Debug.LogWarning("###### DownLoadBundle Error@@@!!! : " + loader.error + " Path : " + loader.url);
                    ShowErrorMsg("ERROR_NET_INSTABILITY", () =>
                    {
                        StartCoroutine(DownLoadBundle(pBundleName, pOnProgress, pOnCallbackComplete));
                    }, () =>
                    {
                        Application.Quit();
                    });
                    yield break;
                }
                else
                {
                    Debug.Log("DownLoad From Server Bundle Complete : " + pBundleName);
                }

                if (pOnCallbackComplete != null)
                    pOnCallbackComplete(loader.bytes);
            }
        }
        #endregion

        #region AssetbundlWriteForLocal
        private IEnumerator WriteLocalFile(string pBundleName, byte[] pBytes, OnProgressByRatio pOnProgress = null, OnError pOnError = null)
        {
            string pSavePath = xSystem.AppAllocatePath(pBundleName);
            using (System.IO.FileStream fs = new System.IO.FileStream(pSavePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(pBytes))
                {
                    int len = (int)ms.Length;
                    int read = 0;

                    byte[] buffer = new byte[1024 * 1024 * 10];

                    while ((read = ms.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        try
                        {
                            fs.Write(buffer, 0, read);

                            if (pOnProgress != null)
                                pOnProgress(read / len * 0.1f + 0.9f);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log(e.ToString());

                            ms.Close();
                            fs.Close();

                            if (pOnError != null)
                                pOnError(e.ToString());

                            yield break;
                        }
                        yield return null;
                    }

                    ms.Close();
                }
                fs.Close();
            }
            Debug.Log("Write Bundle From Local Complete : " + pBundleName);
        }
        #endregion

        #region AssetbundleCaching
        public IEnumerator CachingBundle(string pBundleName, OnProgressByRatio onProgress = null, OnCompleteCaching pCallbackComplete = null, OnError pCallbackError = null)
        {
            yield return new WaitForEndOfFrame();

            string pSavePath = "file:///" + xSystem.AppAllocatePath(pBundleName);
            //string pSavePath = GetAssetBundleFilePath(pBundleName);
            using (WWW caching = WWW.LoadFromCacheOrDownload(pSavePath, this.serverBundleHash.GetAssetBundleHash(pBundleName)))
            {
                if (onProgress != null)
                {
                    while (!caching.isDone && string.IsNullOrEmpty(caching.error))
                    {
                        if (onProgress != null)
                            onProgress(caching.progress);
                        yield return null;
                    }
                }
                else
                {
                    yield return caching;
                }

                if (string.IsNullOrEmpty(caching.error))
                {
                    //Debug.Log( "Caching Complete" );

                    this.loadedBundles.Add(pBundleName, new STLoadedBundle(caching.assetBundle));

                    if (onProgress != null)
                        onProgress(caching.progress);

                    if (pCallbackComplete != null)
                        pCallbackComplete(pBundleName);
                }
                else
                {
                    if (pCallbackError != null)
                        pCallbackError(caching.error);
                    Debug.Log("CachingBundle:" + caching.error);
                }
            }
        }
        #endregion

        #region HelperFunction
        /// <summary>
        /// 로컬에 기록된 번들외의 현재 번들해쉬 리스트에서 빠진 이전 파일들 제거..
        /// </summary>
        /// <returns></returns>
        private List<System.IO.FileInfo> GetNeedRemoveFiles()
        {
            List<System.IO.FileInfo> localFiles = xSystem.GetFiles(xSystem.AppAllocatePath());
            string[] bundles = this.serverBundleHash.GetAllAssetBundles();

            for (int i = 0; i < bundles.Length; i++)
            {
                localFiles.RemoveAll(bundleFile => bundleFile.Name.Equals(bundles[i]));
            }
            // 버젼에서 제외된 번들파일 제거
            localFiles.RemoveAll(file => !file.Name.EndsWith("."+GetBundleExtenstion()));
            return localFiles;
        }
        /// <summary>
        /// Gets the need request server bundles.
        /// 번들해쉬정보의 리스트와 로컬번들과 비교 버젼이 틀린경우와 로컬의 파일이 없다면 서버에서 다운로드용 리스트에 추가
        /// </summary>
        /// <returns></returns>
        private List<string> GetNeedRequestServerBundles(string targetBundleName="")
        {
            List<string> needRequestUpdateFromServerBundle = new List<string>();

            string[] bundles = {""};
            if (string.IsNullOrEmpty(targetBundleName)) bundles = this.serverBundleHash.GetAllAssetBundles();
            else bundles[0] = targetBundleName;

            for (int i = 0; i < bundles.Length; i++)
            {
                string bundleName = bundles[i];

                if (this.localBundleHash.ContainsKey(bundleName))
                {
                    string hash = this.serverBundleHash.GetAssetBundleHash(bundleName).ToString();

                    if (!this.localBundleHash[bundleName].Equals(hash))
                        needRequestUpdateFromServerBundle.Add(bundleName);
                    else
                    {
                        string isExistFilePath = xSystem.AppAllocatePath(bundleName);

                        bool isExist = System.IO.File.Exists(isExistFilePath);
                        if (!isExist)
                            needRequestUpdateFromServerBundle.Add(bundleName);
                    }
                }
                else
                {
                    needRequestUpdateFromServerBundle.Add(bundleName);
                }
            }

            return needRequestUpdateFromServerBundle;
        }

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
                Debug.Log("serialize => " + serialize);
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
        #endregion

        #region Bundle Function
        public static string GetAssetBundleBaseURL()
        {
            return CONFIG.GetBundleURL() + "/" + EXPORT_ASSETBUNDLES_NAME + "/" + CONFIG.CurrentPlatform.ToString();
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

        private void ShowErrorMsg(string pMsg, System.Action pCallbackOk)
        {
            /*
            UISceneController.Instance.AlertBox(true, new AlertBox.AlertParam(AlertBox.AlertType.SELECTABLE, new Vector2(800, 450), NetTable.GetSystemTxt(pMsg)), (pResult) => {
                if (pResult == AlertBox.UserSelect.OK)
                    pCallbackOk();
            });
            */
        }

        // GetAssetDownBase Path Combie

    }

#endif
}


