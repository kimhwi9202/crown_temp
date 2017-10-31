using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace xLIB
{
    public enum ePlatform
    {
        None,
        Windows,
        OSX,
        Android,
        iOS,
        WSAPlayer,
        WSAPhone,
    }
    public enum eCountryCode
    {
        kor, eng, chn, jpn, twn,
    }
    public enum eLocalization
    {
        korean, english, chinese, japan, taiwan,
    }
    public enum eNetworkMode
    {
        Local, DevNetwork, RealNetwork,
    }
    public enum eConfigLoadMode
    {
        Local, Network,
    }
    public enum eBundleLoadMode
    {
        Local, Network,
    }
    public enum eNetworkState
    {
        None, Connect, Disconnect,
    }
    public enum eTutorial
    {
        off, on,
    }

    /// <summary>
    /// 프로젝트 런타임 실행환경 및 버젼정보 
    /// </summary>
    public class CONFIG 
    {
        #region Config Struct
        public class STVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Revision { get; set; }
            public STVersion() { }
            public STVersion(int Major, int Minor, int Revision)
            {
                this.Major = Major;
                this.Minor = Minor;
                this.Revision = Revision;
            }
            public string GetVersion()
            {
                return this.Major + "." + this.Minor + "." + this.Revision;
            }
        }
        public class STServer
        {
            public string ServerOn { get; set; }
            public string DevServerIP { get; set; }
            public string DevServerPORT { get; set; }
            public string DevBundleURL { get; set; }
            public string RealServerIP { get; set; }
            public string RealServerPORT { get; set; }
            public string RealBundleURL { get; set; }
        }
        public class STConfig
        {
            public STServer[] Server { get; set; }
            public STVersion[] Version { get; set; }
            public string Param1 { get; set; }
            public string Param2 { get; set; }
            public string Param3 { get; set; }
            public string Param4 { get; set; }
        }
        public class STSystemInfo
        {
            public static string configVer { get; set; }
            public static string clientVer { get; set; }
            public static string platform { get; set; }
            public static string server { get; set; }
            public static string deviceModel { get; set; }
            public static string deviceName { get; set; }
            public static string deviceType { get; set; }
            //public static string deviceUniqueIdentifier { get; set; }
            //public static int graphicsDeviceID { get; set; }
            /*
            public static string graphicsDeviceName { get; set; }
            public static string graphicsDeviceType { get; set; }
            public static string graphicsDeviceVersion { get; set; }
            public static int graphicsMemorySize { get; set; }
            public static int maxTextureSize { get; set; }
            public static int systemMemorySize { get; set; }
            public static string operatingSystem { get; set; }
            public static string operatingSystemFamily { get; set; }
            public static int processorCount { get; set; }
            public static int processorFrequency { get; set; }
            public static string processorType { get; set; }
            */
            public STSystemInfo()
            {
                configVer = GetCurrentConfigVersion().GetVersion();
                clientVer = GetBuildVersion();
                platform = CurrentPlatform.ToString();
                server = GetCurrentConfigServerIP() + ":" + GetCurrentConfigServerPORT().ToString();
                deviceModel = SystemInfo.deviceModel;
                deviceName = SystemInfo.deviceName;
                deviceType = SystemInfo.deviceType.ToString();
                //deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
                //graphicsDeviceID = SystemInfo.graphicsDeviceID;
                /*
                graphicsDeviceName = SystemInfo.graphicsDeviceName;
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString();
                graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
                graphicsMemorySize = SystemInfo.graphicsMemorySize;
                systemMemorySize = SystemInfo.systemMemorySize;
                maxTextureSize = SystemInfo.maxTextureSize;
                operatingSystem = SystemInfo.operatingSystem;
                operatingSystemFamily = SystemInfo.operatingSystemFamily.ToString();
                processorCount = SystemInfo.processorCount;
                processorFrequency = SystemInfo.processorFrequency;
                processorType = SystemInfo.processorType;
                */
            }

            public string ToString()
            {
                StringBuilder bd = new StringBuilder();
                bd.Append(configVer + "|");
                bd.Append(clientVer + "|");
                bd.Append(platform + "|");
                bd.Append(server + "|");
                bd.Append(deviceModel + "|");
                bd.Append(deviceName + "|");
                bd.Append(deviceType + "|");
                //bd.Append(deviceUniqueIdentifier + "|");
                //bd.Append(graphicsDeviceID + "|");
                /*
                bd.Append(graphicsDeviceName + "|");
                bd.Append(graphicsDeviceType + "|");
                bd.Append(graphicsDeviceVersion + "|");
                bd.Append(graphicsMemorySize + "|");
                bd.Append(systemMemorySize + "|");
                bd.Append(maxTextureSize + "|");
                bd.Append(operatingSystem + "|");
                bd.Append(operatingSystemFamily + "|");
                bd.Append(processorCount + "|");
                bd.Append(processorFrequency + "|");
                bd.Append(processorType);
                */
                return bd.ToString();
            }
        }
        #endregion

        public delegate void InitConfigDelegate(bool ok, string msg);
        #region ReadOnlyVariable
        private static STVersion _ClientVersion = new STVersion(1, 0, 0);
        private static readonly string _ConfigFileName = "config.json";
        #endregion
        private static eTutorial _Tutorial = eTutorial.off;
        private static eNetworkMode _NetworkMode = eNetworkMode.Local;
        private static ePlatform _Platform = ePlatform.None;
        private static eLocalization _Localization = eLocalization.korean;
        private static eConfigLoadMode _ConfigLoadMode = eConfigLoadMode.Local;
        private static eBundleLoadMode _BundleLoadMode = eBundleLoadMode.Local;
        private static eNetworkState _NetworkState = eNetworkState.None;
        private static string _ConfigURL = "www.ip.com";
        public static STConfig _ConfigInfo;
        public static int TutorialStep = 0;

        public static void Initialize()
        {
            ePlatform run_platform = GetRuntimePlatform();
            UnityEngine.Debug.Log("\n============Runtime Config Setting State================");
            if (run_platform == _Platform)
                UnityEngine.Debug.Log("<color=#00ff00> Platform Matching OK!... Runtime Platform = " + GetRuntimePlatform().ToString() + " / " + "Using Platform = " + _Platform.ToString() + "</Color>");
            else
                UnityEngine.Debug.Log("<color=#ff0000> Faild! Platform Matching... Runtime Platform = " + GetRuntimePlatform().ToString() + " / " + "Using Platform = " + _Platform.ToString() + "</Color>");

            Debug.Log("<color=#FFC000>Network Mode : " + _NetworkMode.ToString() + "</Color>");
            Debug.Log("<color=#FFC000>Localization : " + _Localization.ToString() + "</Color>");
            Debug.Log("<color=#FFC000>Config Load Mode : " + _ConfigLoadMode.ToString() + "</Color>");
            Debug.Log("<color=#FFC000>Bundle Load Mode : " + CurrentBundleLoadMode.ToString() + "</Color>");
            UnityEngine.Debug.Log("======================================================== \n");
        }
        public static string GetBuildVersion() { return _ClientVersion.GetVersion();  }
        public static string GetXMLConfigSystemInfo() { return LitJson.JsonMapper.ToJson(new STSystemInfo()); }
        public static string GetConfigSystemInfoToString() {
                STSystemInfo info = new STSystemInfo();
                return info.ToString();
            }
        public static string GetConfigFileName() { return _ConfigFileName; }
        public static string CurrentConfigURL { get { return _ConfigURL; } set { _ConfigURL = value; } }
        public static eBundleLoadMode CurrentBundleLoadMode { get { return _BundleLoadMode; } set { _BundleLoadMode = value; } }
        public static eConfigLoadMode CurrentConfigLoadMode { get { return _ConfigLoadMode; } set { _ConfigLoadMode = value; } }
        public static eNetworkMode CurrentNetworkMode { get { return _NetworkMode; } set { _NetworkMode = value; } }
        public static eNetworkState CurrentNetworkState { get { return _NetworkState; } set { _NetworkState = value; } }
        public static eLocalization CurrentLocalization { get { return _Localization; } set { _Localization = value; } }
        public static eTutorial CurrentTutorial { get { return _Tutorial; } set { _Tutorial = value; } }

        #region Platform Function
        public static ePlatform CurrentPlatform { get{ return _Platform; } set{ _Platform = value; } }
        public static bool IsRuningPlatform(ePlatform platform) { return CurrentPlatform == platform ? true : false; }
        public static bool IsRunningWindows() { return CurrentPlatform == ePlatform.Windows; }
        public static bool IsRunningOSX() { return CurrentPlatform == ePlatform.OSX; }
        public static bool IsRunningiOS() { return CurrentPlatform == ePlatform.iOS; }
        public static bool IsRunningAndroid() { return CurrentPlatform == ePlatform.Android; }
        public static bool IsRunningWSAPlayer() { return CurrentPlatform == ePlatform.WSAPlayer; }
        public static ePlatform GetRuntimePlatform()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:               return ePlatform.Android;
#if UNITY_5
                case BuildTarget.iOS:                   return ePlatform.iOS;
#else
                case BuildTarget.iPhone:                return ePlatform.iOS;
#endif
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:   return ePlatform.Windows;
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:return ePlatform.OSX;
#if UNITY_5
                case BuildTarget.WSAPlayer:             return ePlatform.WSAPlayer;
#else
#endif
                default:        return ePlatform.None;
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.Android:           return ePlatform.Android;
                case RuntimePlatform.IPhonePlayer:      return ePlatform.iOS;
                case RuntimePlatform.WindowsPlayer:     return ePlatform.Windows;
                case RuntimePlatform.OSXPlayer:         return ePlatform.OSX;
#if UNITY_5
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:      
                case RuntimePlatform.WSAPlayerARM:      return ePlatform.WSAPlayer;
#endif
                default:        return ePlatform.None;
            }
#endif // UNITY_EDITOR
        }
        #endregion //Platform Function

        public static bool EditorCreateConfigFile()
        {
            string fullPath = Application.dataPath + "/xLIB/def_config.json";
            FileInfo fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists == false) return false;
            string str = File.ReadAllText(fullPath, Encoding.UTF8);
            _ConfigInfo = xLitJson.JsonMapper.ToObject<STConfig>(str);
            return _ConfigInfo != null ? true : false;
        }
        public static bool EditorLoadConfig()
        {
            string fullPath = Path.Combine(Application.dataPath, GetConfigFileName());
            FileInfo fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists == false) return false;
            string str = File.ReadAllText(fullPath, Encoding.UTF8);
            _ConfigInfo = xLitJson.JsonMapper.ToObject<STConfig>(str);
            return _ConfigInfo != null ? true : false;
        }
        public static void EditorSaveConfig(string fileName)
        {
            string fullPath = Path.Combine(Application.dataPath, fileName);
            string str = xLitJson.JsonMapper.ToJson(_ConfigInfo);
            str.Replace("\n", "\\r\\n");
            using (StreamWriter outputFile = new StreamWriter(fullPath))
            {
                outputFile.WriteLine(str);
            }
        }

        public static STConfig GetConfigInfo()
        {
            return _ConfigInfo;
        }
        public static string GetConfigURL()
        {
            string url = "";
            if (eConfigLoadMode.Local == _ConfigLoadMode)
                url = "file:///" + Path.Combine(Application.dataPath, GetConfigFileName());
            else
                url = _ConfigURL + GetConfigFileName() + "?" + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Millisecond.ToString();
            return url;
        }
        public static STVersion GetCurrentConfigVersion(int idx)
        {
            return _ConfigInfo.Version[idx];
        }
        public static STVersion GetCurrentConfigVersion()
        {
            if (CONFIG.CurrentPlatform == ePlatform.Android) return _ConfigInfo.Version[0];
            else return _ConfigInfo.Version[1];
        }
        public static STServer GetCurrentConfigServer(int idx)
        {
            return _ConfigInfo.Server[idx];
        }
        public static STServer GetCurrentConfigServer()
        {
            if (CONFIG.CurrentPlatform == ePlatform.Android) return _ConfigInfo.Server[0];
            else return _ConfigInfo.Server[1];
        }
        public static string GetBundleURL()
        {
            if (_BundleLoadMode == eBundleLoadMode.Local)
                return "file:///" + xSystem.GetPlatformPath() + "/";
            else if(_NetworkMode == eNetworkMode.DevNetwork)
                return GetCurrentConfigServer().DevBundleURL;
            else 
                return GetCurrentConfigServer().RealBundleURL;
        }

        public static string GetCurrentConfigServerIP()
        {
            int idx = 0;  // 0번배열 android
            if (CurrentPlatform != ePlatform.Android) idx = 1;
            if (eNetworkMode.RealNetwork == _NetworkMode)       return _ConfigInfo.Server[idx].RealServerIP;
            else if (eNetworkMode.DevNetwork == _NetworkMode)   return _ConfigInfo.Server[idx].DevServerIP;
            return "";
        }
        public static int GetCurrentConfigServerPORT()
        {
            int idx = 0;  // 0번배열 android
            if (CurrentPlatform != ePlatform.Android) idx = 1;
            if (eNetworkMode.RealNetwork == _NetworkMode) return System.Convert.ToInt32(_ConfigInfo.Server[idx].RealServerPORT);
            else if (eNetworkMode.DevNetwork == _NetworkMode) return System.Convert.ToInt32(_ConfigInfo.Server[idx].DevServerPORT);
            return 0;
        }

        public static IEnumerator WWWLoadConfigFile(InitConfigDelegate pCallback)
        {
            WWW www = new WWW(GetConfigURL());
            yield return www;

#if UNITY_EDITOR
            Debug.Log("Config Json : " + www.text);
#endif
            if (string.IsNullOrEmpty(www.error))
            {
                _ConfigInfo = xLitJson.JsonMapper.ToObject<STConfig>(www.text);
                if (pCallback != null) pCallback(true, "<color=#00ff00>Successfully! WWW Load Config File URL : " + GetConfigURL() + "</color>");
            }
            else
            {
                if (pCallback != null) pCallback(false, "<color=#ff0000>Faild! WWW Load Config File URL : " + GetConfigURL() + "</color>");
            }
        }

        public static void CheckMatchClientVersion(InitConfigDelegate pCallBack)
        {
            bool ok = true;
            string msg = _ClientVersion.GetVersion();

            STVersion ConfigVersion = GetCurrentConfigVersion();
            if (ConfigVersion.Major != _ClientVersion.Major || ConfigVersion.Minor != _ClientVersion.Minor)
            {
                // need to show the force patch popup
                msg = "You have to patch the newest version( " + ConfigVersion.GetVersion() + " ) of the game client. (" + _ClientVersion.GetVersion() + ")";
                ok = false;
            }
            else if (ConfigVersion.Revision != _ClientVersion.Revision)
            {
                // need to show the recommend popup
                msg = "The newest version( " + ConfigVersion.GetVersion()+" ) of the game client(" + _ClientVersion.GetVersion() + ") is available.";
                ok = false;
            }

            if (pCallBack != null) pCallBack(ok, msg);
        }

        public static void BuildSetting(STVersion version, string configURL)
        {
            _ClientVersion = version;  
            CurrentConfigURL = configURL;
        }
    }
}
