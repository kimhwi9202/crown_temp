
using UnityEngine;
using System.Collections;

using UnityEditor;
using xLIB.Editor;
using xLIB;
#if UNITY_5
using UnityEngine.SceneManagement;
#endif


[CustomEditor(typeof(Main))]
public class EditorMain : Editor
{
    protected bool IsStartScene = false;
    protected Main m_base;
    protected bool mInitialized = false;
    protected bool _configEditor = false;
    protected bool _configCreate = false;
    protected string[,] grid = null;
    protected int SceneIdx = 0;
    private void LoadSettingFile()
    {
        string fileFullPath = Application.dataPath + "/xLIB/main_setting.csv";
        string text = string.Empty;
        System.IO.FileInfo fi = new System.IO.FileInfo(fileFullPath);
        string strFileName = fi.Name.Replace(fi.Extension, "");
        text = xCSVParser.LoadFile(fileFullPath);
        grid = xCSVParser.SplitCsvGrid(text);
        for (int i = 0; i < grid.GetUpperBound(0); i++)
        {
            if (grid[i, 0].Length > 2)
            {
                if(grid[i, 0] == "PlatformCode") CONFIG.CurrentPlatform = (ePlatform)System.Convert.ToInt32(grid[i, SceneIdx+1]);
                else if (grid[i, 0] == "NetworkMode") CONFIG.CurrentNetworkMode = (eNetworkMode)System.Convert.ToInt32(grid[i, SceneIdx+1]);
                else if (grid[i, 0] == "Localization") CONFIG.CurrentLocalization = (eLocalization)System.Convert.ToInt32(grid[i, SceneIdx+1]);
                else if (grid[i, 0] == "ConfigLoadMode") CONFIG.CurrentConfigLoadMode = (eConfigLoadMode)System.Convert.ToInt32(grid[i, SceneIdx+1]);
                else if (grid[i, 0] == "BundleLoadMode") CONFIG.CurrentBundleLoadMode = (eBundleLoadMode)System.Convert.ToInt32(grid[i, SceneIdx+1]);
                Debug.Log(i + " = " + grid[i, 0] + " > " + grid[i, 1]);
            }
        }
    }

    private void SaveSettingFile()
    {
        string textOutput = "";
        for (int y = 0; y < grid.GetUpperBound(1); y++)
        {
            bool addline = false; 
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                if (string.IsNullOrEmpty(grid[x, y]) == false)
                {
                    textOutput += grid[x, y];
                    textOutput += ",";
                    addline = true;
                }
            }
            if (addline)
                textOutput += "\r\n";
        }

        string fileFullPath = Application.dataPath + "/xLIB/main_setting.csv";
        System.IO.File.WriteAllText(fileFullPath, textOutput, System.Text.Encoding.UTF8);
    }

    void OnEnable()
    {
        /*
        // apk 빌드시에 에디터 메모리로 세팅되는 문제점 발견 일단 보류
        Scene info = SceneManager.GetActiveScene();
        // 빌드 씬에 포함된 씬별 상태값 로컬에 기록해서 사용
        if (info.buildIndex >= 0)
        {
            IsStartScene = true;
            SceneIdx = info.buildIndex;
            LoadSettingFile();
            //Debug.Log(info.buildIndex + " index " + info.name + " ------ OnEnable ------- " + CONFIG.CurrentPlatform.ToString());
        }
        else IsStartScene = false;
        */
    }

    public override void OnInspectorGUI()
    {
        m_base = target as Main;
        
        if (!mInitialized)
        {
            mInitialized = true;
            SceneView.RepaintAll();
            EditorUtility.SetDirty(m_base);
        }

        EditorTools.DrawSeparator();

        GUILayout.BeginHorizontal();

#if UNITY_5_3_OR_NEWER
        if(GUILayout.Button("Clean Bundle PlayerPrefs"))
        {
            BUNDLE.DeleteBundleHashPrefs();
        }
#endif
        if (GUILayout.Button("Clean All PlayerPrefs"))
        {
            PlayerPrefs.DeleteAll();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Runtime Platform", GUILayout.Width(150));
        GUI.contentColor = Color.magenta;
        EditorGUILayout.LabelField(CONFIG.GetRuntimePlatform().ToString());
        GUI.contentColor = Color.white;
        EditorGUILayout.EndHorizontal();

        ePlatform selPlatform = (ePlatform)EditorTools.EnumPopup("Platform Code", CONFIG.CurrentPlatform, Color.cyan);
        if (CONFIG.CurrentPlatform != selPlatform) {
            m_base._Platform = selPlatform;
            CONFIG.CurrentPlatform = selPlatform;
            if (IsStartScene)
            {
                grid[0, SceneIdx+1] = System.Convert.ToString((int)CONFIG.CurrentPlatform);
                SaveSettingFile();
            }
            if (Main.eventChangePlatform != null)
                Main.eventChangePlatform();
        }
        eNetworkMode selNetworkMode = (eNetworkMode)EditorTools.EnumPopup("Network Mode", CONFIG.CurrentNetworkMode, Color.red);
        if (CONFIG.CurrentNetworkMode != selNetworkMode)
        {
            m_base._NetworkMode = selNetworkMode;
            CONFIG.CurrentNetworkMode = selNetworkMode;
            if (IsStartScene)
            {
                grid[1, SceneIdx+1] = System.Convert.ToString((int)CONFIG.CurrentNetworkMode);
                SaveSettingFile();
            }
        }
        eLocalization selLocalization = (eLocalization)EditorTools.EnumPopup("Select Localization", CONFIG.CurrentLocalization, Color.yellow);
        if (CONFIG.CurrentLocalization != selLocalization)
        {
            m_base._Localization = selLocalization;
            CONFIG.CurrentLocalization = selLocalization;
            if (IsStartScene)
            {
                grid[2, SceneIdx+1] = System.Convert.ToString((int)CONFIG.CurrentLocalization);
                SaveSettingFile();
            }
            if (Main.eventChangeLocalization != null)
                Main.eventChangeLocalization();
        }
        eConfigLoadMode selConfigLoadMode = (eConfigLoadMode)EditorTools.EnumPopup("Config Load Mode", CONFIG.CurrentConfigLoadMode, Color.green);
        if (CONFIG.CurrentConfigLoadMode != selConfigLoadMode)
        {
            m_base._ConfigLoadMode = selConfigLoadMode;
            CONFIG.CurrentConfigLoadMode = selConfigLoadMode;
            if (IsStartScene)
            {
                grid[3, SceneIdx+1] = System.Convert.ToString((int)CONFIG.CurrentConfigLoadMode);
                SaveSettingFile();
            }
        }
        eBundleLoadMode selBundleLoadMode = (eBundleLoadMode)EditorTools.EnumPopup("Bundle Load Mode", CONFIG.CurrentBundleLoadMode, Color.green);
        if (CONFIG.CurrentBundleLoadMode != selBundleLoadMode)
        {
            m_base._BundleLoadMode = selBundleLoadMode;
            CONFIG.CurrentBundleLoadMode = selBundleLoadMode;
            if (IsStartScene)
            {
                grid[4, SceneIdx+1] = System.Convert.ToString((int)CONFIG.CurrentBundleLoadMode);
                SaveSettingFile();
            }
        }

        eTutorial selTutorial = (eTutorial)EditorTools.EnumPopup("Tutorial", CONFIG.CurrentTutorial, Color.yellow);
        if (CONFIG.CurrentTutorial != selTutorial)
        {
            m_base._Tutorial = selTutorial;
            CONFIG.CurrentTutorial = selTutorial;
            if (IsStartScene)
            {
                grid[5, SceneIdx + 1] = System.Convert.ToString((int)CONFIG.CurrentTutorial);
                SaveSettingFile();
            }
        }

        if (CONFIG.CurrentConfigLoadMode == eConfigLoadMode.Network)
        {
            EditorGUILayout.BeginHorizontal();
            m_base._ConfigURL = EditorGUILayout.TextField("Config URL", CONFIG.CurrentConfigURL);
            CONFIG.CurrentConfigURL = m_base._ConfigURL;
            EditorGUILayout.EndHorizontal();
        }

        EditorTools.DrawSeparator();

        if (GUILayout.Button("Create Default Config File"))
        {
            if(CONFIG.EditorCreateConfigFile())
            {
                CONFIG.EditorSaveConfig(CONFIG.GetConfigFileName());
                Debug.Log("Successfly! Create Default Config Fils.." + CONFIG.GetConfigFileName());
            }
        }

        if (_configEditor) GUI.contentColor = Color.yellow;

        if (GUILayout.Button("Config Editor"))
        {
            if(CONFIG.EditorLoadConfig())
            {
                Debug.Log("Successfly! Load Config Fils.." + CONFIG.GetConfigFileName());
                _configEditor = !_configEditor;
            }
        }

        if(_configEditor && CONFIG.GetConfigInfo() != null)
        {
            for(int i=0; i<2; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (i == 0)
                {
                    GUI.contentColor = Color.cyan;
                    GUILayout.Label("Android Ver", GUILayout.Width(150));
                }
                else
                {
                    GUI.contentColor = Color.magenta;
                    GUILayout.Label("iOS Ver", GUILayout.Width(150));
                }
                CONFIG.GetCurrentConfigVersion(i).Major = EditorGUILayout.IntField(CONFIG.GetCurrentConfigVersion(i).Major);
                CONFIG.GetCurrentConfigVersion(i).Minor = EditorGUILayout.IntField(CONFIG.GetCurrentConfigVersion(i).Minor);
                CONFIG.GetCurrentConfigVersion(i).Revision = EditorGUILayout.IntField(CONFIG.GetCurrentConfigVersion(i).Revision);
                EditorGUILayout.EndHorizontal();
                GUI.contentColor = Color.white;

                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).ServerOn = EditorGUILayout.TextField("Server On", CONFIG.GetCurrentConfigServer(i).ServerOn);
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).DevServerIP = EditorGUILayout.TextField("DevServer IP", CONFIG.GetCurrentConfigServer(i).DevServerIP);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).DevServerPORT = EditorGUILayout.TextField("DevServer PORT", CONFIG.GetCurrentConfigServer(i).DevServerPORT);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).DevBundleURL = EditorGUILayout.TextField("DevBundle URL", CONFIG.GetCurrentConfigServer(i).DevBundleURL);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).RealServerIP = EditorGUILayout.TextField("RealServer IP", CONFIG.GetCurrentConfigServer(i).RealServerIP);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).RealServerPORT = EditorGUILayout.TextField("RealServer PORT", CONFIG.GetCurrentConfigServer(i).RealServerPORT);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                CONFIG.GetCurrentConfigServer(i).RealBundleURL = EditorGUILayout.TextField("Bundle URL", CONFIG.GetCurrentConfigServer(i).RealBundleURL);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            CONFIG.GetConfigInfo().Param1 = EditorGUILayout.TextField("Param 1", CONFIG.GetConfigInfo().Param1);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            CONFIG.GetConfigInfo().Param2 = EditorGUILayout.TextField("Param 2", CONFIG.GetConfigInfo().Param2);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            CONFIG.GetConfigInfo().Param3 = EditorGUILayout.TextField("Param 3", CONFIG.GetConfigInfo().Param3);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            CONFIG.GetConfigInfo().Param4 = EditorGUILayout.TextField("Param 4", CONFIG.GetConfigInfo().Param4);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Config Save"))
            {
                CONFIG.EditorSaveConfig(CONFIG.GetConfigFileName());
            }
        }

        GUI.contentColor = Color.white;

        EditorTools.DrawSeparator();
        DrawDefaultInspector();  // 멤버변수 Show
    }
}

