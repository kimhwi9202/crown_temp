#if UNITY_EDITOR && UNITY_5

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xLIB.Editor;
using xLIB;
using System.IO;

public class AssetBundleWindow : EditorWindow
{
    public static AssetBundleWindow instance;

    protected Vector2 mScroll = Vector2.zero;
    //Vector2 mScrollLocal = Vector2.zero;
    //Vector2 mScrollFtp = Vector2.zero;
    protected string msg_log = string.Empty;
    protected string sel_rename = string.Empty;
    protected int mTab = 0;
    protected List<FTP.STFTPFiles> files = null;
    protected FTP.FileStruct[] ftp_filelist = null;
    protected bool ftpFiles = false;

    public Dictionary<string, string> config = new Dictionary<string, string>();



    [MenuItem("xLIB/AssetBundleWindow", false, 1)]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<AssetBundleWindow>(false, "AB Maker", true).Show();
    }
    [MenuItem("xLIB/UnUsedBundleName", false, 2)]
    public static void CleanUnUsedBundleName()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    void Start()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            EditorABSetting.emBuildTarget = EditorABSetting.eBUILDTARGET.Android;
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            EditorABSetting.emBuildTarget = EditorABSetting.eBUILDTARGET.iOS;
    }

    void OnEnable()
    {
        config.Add("test", "t1");
        instance = this;
        mScroll = Vector2.zero;
        msg_log = string.Empty;
        sel_rename = string.Empty;
        EditorABSetting.LoadPrefs();
    }
    void OnDisable()
    {
        instance = null;
        EditorABSetting.SavePrefs();
    }

    public bool DrawABList(int idx, string text, int count, bool forceOn)
    {
        string key = text;
        bool state = EditorPrefs.GetBool(key, false);

        if (!forceOn && !state) GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (!state) // pading off
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(22f)))
            {
                AssetDatabase.RemoveAssetBundleName(text, true);
                msg_log = "Remove Bundle : " + text;
            }
            GUI.backgroundColor = Color.white;
        }

        if(state) GUI.backgroundColor = Color.green;
        text = "<b><size=12>" + text + " ( " + count.ToString() + " ) " + "</size></b>";
        if (state) text = " \u25BC  " + text;
        else text = " \u25BA  " + text;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;

        GUI.backgroundColor = Color.white;

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    public void DrawBundleLocalFiles()
    {
        // 번들 리스트 채크.
        DirectoryInfo dir = new DirectoryInfo(EditorABSetting.GetBuildExportFullPath());
        if (dir.Exists)
        {
            List<FileInfo> inFileList = new List<FileInfo>(dir.GetFiles("*.*", SearchOption.AllDirectories));
            if(inFileList.Count > 0)
            {
                for (int i = 0; i < inFileList.Count; i++)
                {
                    GUILayout.Space(-1f);
                    int highlight = (i % 2);// (selected == j);
                    GUI.backgroundColor = highlight == 1 ? Color.white : new Color(0.6f, 0.6f, 0.6f);

                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                    GUI.backgroundColor = Color.white;
                    GUILayout.Label(i.ToString(), GUILayout.Width(24f));
                    GUILayout.Label(inFileList[i].Name, GUILayout.Width(250f));
                    float size = inFileList[i].Length / 1000f;
                    int fileSize = (size > 1f) ? (int)size : 1;
                    GUILayout.Label(fileSize + " KB", GUILayout.Width(100f));
                    GUILayout.Label(inFileList[i].LastWriteTime.ToString(), GUILayout.Width(180f));
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
    public void DrawBundleFTPFileDetails(FTP.FileStruct[] list)
    {
        int i = 0;
        foreach (FTP.FileStruct thisstruct in list)
        {
            if (thisstruct.IsDirectory)
            {
                GUILayout.Space(-1f);
                int highlight = (i % 2);// (selected == j);
                GUI.backgroundColor = highlight == 1 ? Color.white : new Color(0.6f, 0.6f, 0.6f);

                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                GUILayout.Label("<"+thisstruct.Name+">", GUILayout.Width(400f));
                GUILayout.Label(" ", GUILayout.Width(20f));
                GUILayout.Label(thisstruct.Flags, GUILayout.Width(50f));
                GUILayout.Label(thisstruct.time, GUILayout.Width(150f));

                GUILayout.EndHorizontal();
                i++;
            }
        }
        foreach (FTP.FileStruct thisstruct in list)
        {
            if (!thisstruct.IsDirectory)
            {
                GUILayout.Space(-1f);
                int highlight = (i % 2);
                GUI.backgroundColor = highlight == 1 ? Color.white : new Color(0.6f, 0.6f, 0.6f);

                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                GUILayout.Label(thisstruct.Name, GUILayout.Width(400f));
                GUILayout.Label(string.Format("{0:N0}", thisstruct.Size, GUILayout.Width(20f)));
                GUILayout.Label(thisstruct.Flags, GUILayout.Width(50f));
                GUILayout.Label(thisstruct.time, GUILayout.Width(150f));

                GUILayout.EndHorizontal();
                i++;
            }
        }
    }


    void DrawPathInfo(string title, ref string value, string def)
    {
        bool sel = false;
        if (string.Compare(title, sel_rename) == 0) sel = true;

        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(80f));
        GUILayout.Space(5f);

        if(sel)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = string.Empty;
                GUI.FocusControl("R");
            }
            else value = EditorGUILayout.TextField(value);
            if (GUILayout.Button("Default", GUILayout.Width(60f))) // 초기화.
            {
                value = def;
                GUI.FocusControl("Default");
            }
        }
        else 
        {
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = title;
                GUI.FocusControl("R");
            }
            GUILayout.Label(value);
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }

    void DrawFTPLogin(string title, ref string value, bool password=false)
    {
        bool sel = false;
        if (string.Compare(title, sel_rename) == 0) sel = true;

        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(80f));
        GUILayout.Space(5f);

        if (sel)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = string.Empty;
                GUI.FocusControl("R");
            }
            else if(password)   value = EditorGUILayout.PasswordField(value);
            else value = EditorGUILayout.TextField(value);
        }
        else
        {
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = title;
                GUI.FocusControl("R");
            }
            if (password) GUILayout.PasswordField(value, '*');
            else GUILayout.Label(value);
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }

    void DrawSettingItem(string title, ref string value, string def)
    {
        bool sel = false;
        if (string.Compare(title, sel_rename) == 0) sel = true;

        GUILayout.BeginHorizontal();
        GUILayout.Label(title, GUILayout.Width(200f));
        GUILayout.Space(5f);

        if (sel)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = string.Empty;
                GUI.FocusControl("R");
            }
            value = EditorGUILayout.TextField(value);
        }
        else
        {
            if (GUILayout.Button("R", GUILayout.Width(22f)))
            {
                sel_rename = title;
                GUI.FocusControl("R");
            }
            GUILayout.Label(value);
        }

        if(GUILayout.Button("Default", GUILayout.Width(60f)))
        {
            value = def;
            GUILayout.Label(value);
            GUI.FocusControl("Default");
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }


    public void SetMsgLog(string txt)
    {
        msg_log = txt;
    }


    void InfoHeaderGUI()
    {
        EditorTools.DrawHeader("Info", true);
        EditorTools.BeginContents(false);

        GUI.contentColor = Color.white;
        GUILayout.Label("\u25BA Absolute Project Path >  " + EditorABSetting.GetProjectPath());
        GUILayout.Label("\u25BA Current Build Setting Platform >  " + EditorUserBuildSettings.activeBuildTarget.ToString());
        GUILayout.Label("\u25BA Selected Bundle Extenstion >  " + EditorABSetting.GetBundleExtenstion());
        GUILayout.Label("\u25BA BundleExport FullPath >  " + EditorABSetting.GetBuildExportFullPath());
        GUI.contentColor = Color.white;

        EditorTools.EndContents();
    }


    void TabMakerGUI()
    {
        InfoHeaderGUI();

        //-----------------------------
        EditorTools.DrawHeader("Build Bundle", true);
        EditorTools.BeginContents(false);

        DrawSettingItem("Build Export Folder Name", ref EditorABSetting.strExportPath, EditorABSetting.DEF_EXPORT_PATH);

        GUILayout.BeginHorizontal();
        BuildAssetBundleOptions emOpt = (BuildAssetBundleOptions)EditorTools.EnumPopup("BuildOptions", EditorABSetting.emBuildOptions, Color.green, false);
        if (emOpt != EditorABSetting.emBuildOptions)
        {
            EditorABSetting.emBuildOptions = emOpt;
        }
        EditorABSetting.eBUILDTARGET emTarget = (EditorABSetting.eBUILDTARGET)EditorTools.EnumPopup("BuildTarget", EditorABSetting.emBuildTarget, Color.green, false);
        if (emTarget != EditorABSetting.emBuildTarget)
        {
            EditorABSetting.emBuildTarget = emTarget;
        }
        GUILayout.EndHorizontal();
        EditorTools.EndContents();

        //-----------------------------
        EditorTools.DrawHeader("AssetBundle Maker", true);
        EditorTools.BeginContents(false);

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        GUILayout.Space(20f);
        if (GUILayout.Button("Add AssetBundle Folder"))
        {
            msg_log = EditorABManager.MarkFolderInFiles(EditorUtility.OpenFolderPanel("Add a Assets", EditorABSetting.GetProjectPath()+"/Assets", "Select Folder"));
            GUILayout.Space(20f);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            return;
        }
        if (GUILayout.Button("Add AssetBundle File"))
        {
            msg_log = EditorABManager.MarkInFile(EditorUtility.OpenFilePanel("Add a Assets", EditorABSetting.GetProjectPath() + "/Assets", "*.*"));
            GUILayout.Space(20f);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            return;
        }
        GUILayout.Space(20f);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        EditorTools.EndContents();

        // 번들 리스트 채크.
        string[] all = AssetDatabase.GetAllAssetBundleNames();

        EditorTools.DrawHeader("AssetBundle Build", true);
        EditorTools.BeginContents(false);

        if (all.Length > 0)
        {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.magenta;
            GUILayout.Space(20f);
            if (GUILayout.Button("Export AssetBundles"))
            {
                EditorABSetting.CreateExoprtFolder();
                CleanUnUsedBundleName();
                BuildPipeline.BuildAssetBundles(EditorABSetting.GetBuildPipelineOutPath(), 
                    EditorABSetting.GetBuildPipelineOptions(), 
                    EditorABSetting.GetBuildPipelineTarget());
                EditorABSetting.SaveConfigFile();
                return;
            }
            GUILayout.Space(20f);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        EditorTools.EndContents();


        // 번들 리스트 채크.
        if (all.Length > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);
            GUILayout.BeginVertical();

            mScroll = GUILayout.BeginScrollView(mScroll);
            for (int i = 0; i < all.Length; i++)
            {
                string[] childs = AssetDatabase.GetAssetPathsFromAssetBundle(all[i]);
                if (DrawABList(i, all[i], childs.Length, false))
                {
                    for (int j = 0; j < childs.Length; j++)
                    {
                        GUILayout.Space(-1f);
                        int highlight = (j % 2);// (selected == j);
                        GUI.backgroundColor = highlight == 1 ? Color.white : new Color(0.6f, 0.6f, 0.6f);

                        GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                        GUI.backgroundColor = Color.white;
                        GUILayout.Label(j.ToString(), GUILayout.Width(24f));
                        if (GUILayout.Button(childs[j], "OL TextField", GUILayout.Height(20f)))
                        {
                            msg_log = childs[j];
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }
    }

    void TabFTPGUI()
    {
        EditorTools.DrawHeader("Info", true);
        EditorTools.BeginContents(false);
        GUILayout.Label("< Local Target Directory > " + EditorABSetting.GetBuildExportFullPath());
        GUILayout.Label("< FTP   Target Directory > " + FTP.GetAddress() + EditorABSetting.strExportPath + "/" + EditorABSetting.emBuildTarget.ToString() + "/");
        EditorTools.EndContents();

        EditorTools.DrawHeader("FTP Login", true);
        string tempAddress = FTP.ftpAddress;
        DrawFTPLogin("FTP Address", ref FTP.ftpAddress);
        if(tempAddress != FTP.ftpAddress) FTP.SaveLoinInfo();

        string tempID = FTP.Id;
        DrawFTPLogin("ID", ref FTP.Id);
        if (tempID != FTP.Id) FTP.SaveLoinInfo();

        GUILayout.BeginHorizontal();
        GUILayout.Label("PASSWORD", GUILayout.Width(80f));
        GUILayout.Space(5f);
        string tempPwd = EditorGUILayout.PasswordField(FTP.Password);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        if(tempPwd != FTP.Password)
        {
            FTP.Password = tempPwd;
            FTP.SaveLoinInfo();
        }


        EditorTools.BeginContents(false);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        GUILayout.Space(20f);

        if (GUILayout.Button("AssetBundle Upload to FTP"))
        {
            if (FTP.ftpAddress.Length < 10) msg_log = "FtpServer Name - A minimum of 20 characters or more !";
            else if (FTP.Id.Length < 2) msg_log = "FtpId Name - A minimum of 2 characters or more !";
            else if (FTP.Password.Length < 2) msg_log = "FtpPassworkd Name - A minimum of 2 characters or more !";
            else
            {
                if(!FTP.FindFileFormFTPDirectory(EditorABSetting.strExportPath))
                {
                    FTP.MakeDirectoryFromFTP(EditorABSetting.strExportPath);
                }
                if (!FTP.FindFileFormFTPDirectory(EditorABSetting.emBuildTarget.ToString(), EditorABSetting.strExportPath + "/"))
                {
                    FTP.MakeDirectoryFromFTP(EditorABSetting.strExportPath + "/" + EditorABSetting.emBuildTarget.ToString());
                }

                FTP.UploadtoLocalDirectory(EditorABSetting.GetBuildExportFullPath(), EditorABSetting.strExportPath + "/" + EditorABSetting.emBuildTarget.ToString() + "/");
            }
        }

        if (GUILayout.Button("Directory FTP"))
        {
            if (!FTP.FindFileFormFTPDirectory(EditorABSetting.strExportPath))
            {
                FTP.MakeDirectoryFromFTP(EditorABSetting.strExportPath);
            }
            if (!FTP.FindFileFormFTPDirectory(EditorABSetting.emBuildTarget.ToString(), EditorABSetting.strExportPath + "/"))
            {
                FTP.MakeDirectoryFromFTP(EditorABSetting.strExportPath + "/" + EditorABSetting.emBuildTarget.ToString());
            }

            ftp_filelist = FTP.FileListFormFTPDirectoryDetails2(EditorABSetting.GetBuildPipelineOutPath());
            if (ftp_filelist != null) ftpFiles = true;
            else ftpFiles = false;
        }

        GUILayout.Space(20f);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        EditorTools.EndContents();


        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);
        GUILayout.BeginVertical();

        mScroll = GUILayout.BeginScrollView(mScroll);

        if (EditorTools.DrawHeader("AssetBundle Local Files", true))
        {
            DrawBundleLocalFiles();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        GUILayout.EndHorizontal();

        
        if (ftp_filelist != null && EditorTools.DrawHeader("FTP Upload Files", false))
        {
            DrawBundleFTPFileDetails(ftp_filelist);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
    }


    void TabSettingGUI()
    {
        InfoHeaderGUI();

        //-----------------------------
        EditorTools.DrawHeader("Maker Bundle", true);
        EditorTools.BeginContents(false);

        DrawSettingItem("Extenstion Name \u25BA " + EditorABSetting.eBUILDTARGET.Windows.ToString(), ref EditorABSetting.strArrayBuildExtension[0], EditorABSetting.DEF_BUNDLE_EXTENSION[0]);
        DrawSettingItem("Extenstion Name \u25BA " + EditorABSetting.eBUILDTARGET.iOS.ToString(), ref EditorABSetting.strArrayBuildExtension[1], EditorABSetting.DEF_BUNDLE_EXTENSION[1]);
        DrawSettingItem("Extenstion Name \u25BA " + EditorABSetting.eBUILDTARGET.Android.ToString(), ref EditorABSetting.strArrayBuildExtension[2], EditorABSetting.DEF_BUNDLE_EXTENSION[2]);
        DrawSettingItem("Extenstion Name \u25BA " + EditorABSetting.eBUILDTARGET.WSAPlayer.ToString(), ref EditorABSetting.strArrayBuildExtension[3], EditorABSetting.DEF_BUNDLE_EXTENSION[3]);

        EditorTools.EndContents();

        EditorTools.DrawHeader("Config Info", true);
        EditorTools.BeginContents(false);
        foreach (KeyValuePair<string, string> pair in config)
        {
            if(pair.Key != string.Empty)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(pair.Key);
                GUILayout.Label(pair.Value);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(" ");
                EditorGUILayout.TextField(" ");
                GUILayout.EndHorizontal();
            }
        }

        EditorTools.EndContents();
    }

    void OnGUI()
    {
        EditorTools.SetLabelWidth(84f);
        GUILayout.Space(3f);

        int curTab = mTab;

        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(curTab == 0, "AB Maker", "ButtonLeft")) curTab = 0;
        if (GUILayout.Toggle(curTab == 1, "AB FTP", "ButtonLeft")) curTab = 1;
        if (GUILayout.Toggle(curTab == 2, "Setting", "ButtonRight")) curTab = 2;
        GUILayout.EndHorizontal();

        if (mTab != curTab)
        {
            sel_rename = "";  // 
            mTab = curTab;

            if (curTab == 1)
            {
                FTP.LoadLoinInfo();
            }
        }

        if (curTab == 0) TabMakerGUI();
        else if (curTab == 1) TabFTPGUI();
        else if (curTab == 2) TabSettingGUI();
    }
}

public class EditorABManager : MonoBehaviour
{
    struct STAssetFiles
    {
        public string folder;
        public string path;
        public string name;
    }

    public readonly static string DEF_PATH_ASSETS = "Assets/";
    public readonly static string DEF_PATH_EXPORT = "AssetBundles";
    public readonly static string DEF_BUNDLE_EXTENSION = ".ad";

    public static string PATH_ASSETS = DEF_PATH_ASSETS;
    //public static string PATH_EXPORT = DEF_PATH_EXPORT;
    //public static string BUNDLE_EXTENSION = DEF_BUNDLE_EXTENSION;

    private static List<FileInfo> GetRemoveMetaFileList(DirectoryInfo dir, SearchOption option = SearchOption.TopDirectoryOnly)
    {
        List<FileInfo> inFileList = new List<FileInfo>(dir.GetFiles("*.*", option));
        inFileList.RemoveAll(inFile => inFile.Extension == ".meta");

        return inFileList;
    }
    private static List<FileInfo> GetDirInFiles(string childPath, SearchOption option = SearchOption.TopDirectoryOnly)
    {
        string dirPath = PATH_ASSETS + childPath;
        DirectoryInfo dir = new DirectoryInfo(dirPath);

        return GetRemoveMetaFileList(dir, option);
    }
    private static List<STAssetFiles> GetAssetDirInFiles(string childPath, SearchOption option = SearchOption.TopDirectoryOnly)
    {
        DirectoryInfo dir = new DirectoryInfo(childPath);
        List<FileInfo> inFileList = new List<FileInfo>(dir.GetFiles("*.*", option));
        inFileList.RemoveAll(inFile => inFile.Extension == ".unity");
        inFileList.RemoveAll(inFile => inFile.Extension == ".meta");
        inFileList.RemoveAll(inFile => inFile.Extension == ".cs");
        inFileList.RemoveAll(inFile => inFile.Extension == ".js");

        List<STAssetFiles> files = new List<STAssetFiles>();

        for (int i = 0; i < inFileList.Count; i++)
        {
            STAssetFiles data = new STAssetFiles();
            data.folder = dir.Name;
            data.path = inFileList[i].DirectoryName;
            data.name = inFileList[i].Name;
            data.path = data.path.Substring(inFileList[i].DirectoryName.IndexOf("Assets", 0));
            data.path.Replace("\\", "/");
            files.Add(data);
        }
        return files;
    }
    private static IEnumerator OnProgress(string title, List<STAssetFiles> files, int current, int totalAmount)
    {
        while (current < totalAmount)
        {
            EditorUtility.DisplayProgressBar(title, "Progress : " + files[current].path, current / (float)totalAmount);
            current++;
            yield return null;
        }

        EditorUtility.ClearProgressBar();
    }

    public static string MarkFolderInFiles(string dirPath)
    {
        if (string.IsNullOrEmpty(dirPath)) return "";

        string msg = string.Empty;
        List<STAssetFiles> tableFiles = GetAssetDirInFiles(dirPath, SearchOption.AllDirectories);
        AssetImporter importer;

        string folder = string.Empty;
        int totalAmount = tableFiles.Count;
        if (totalAmount > 0)
        {
            folder = tableFiles[0].folder;
            IEnumerator progressShow = OnProgress(folder, tableFiles, 0, totalAmount - 1);
            for (int i = 0; i < totalAmount; i++)
            {
                importer = AssetImporter.GetAtPath(tableFiles[i].path + "/" + tableFiles[i].name);
                if (importer) importer.assetBundleName = folder + "." + EditorABSetting.GetBundleExtenstion();
                else Debug.Log("Reject -> " + tableFiles[i].path);
                progressShow.MoveNext();
            }
            msg = "Marking Complete Folder : " + folder;
        }

        return msg;
    }
    public static string MarkInFile(string dirPath)
    {
        if (string.IsNullOrEmpty(dirPath)) return "";

        FileInfo fi = new FileInfo(dirPath);

        string path = dirPath.Substring(fi.DirectoryName.IndexOf("Assets", 0));

        string markingName = fi.Name.Replace(fi.Extension, "");// string.Format(PARTS_MATERIALS_PREFIX, );

        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer) importer.assetBundleName = markingName + "." + EditorABSetting.GetBundleExtenstion();
        else Debug.Log("Reject -> " + path);

        string msg = "Marking Complete File : " + fi.Name;

        return msg;
    }
}

public class EditorABSetting
{
    public enum eBUNDLE_EXTENSION
    {
        unity3d,
        ios,
        ad,
        wsa,
    }

    public enum eBUILDTARGET
    {
        Windows,
        iOS,
        Android,
        WSAPlayer,  // windows 10 
    }
    private readonly static string TAG_PATH = "ABSetExportPath";
    private readonly static string TAG_EXT = "ABSetExtenstion";
    public static BuildAssetBundleOptions emBuildOptions = BuildAssetBundleOptions.None;
    public static eBUILDTARGET emBuildTarget = eBUILDTARGET.Android;
    public readonly static string DEF_EXPORT_PATH = "AssetBundles";
    public readonly static string[] DEF_BUNDLE_EXTENSION = { "unity3d", "ios", "ad", "wsa" };
    public static string strExportPath = DEF_EXPORT_PATH;
    public static string[] strArrayBuildExtension = { "unity3d", "ios", "ad", "wsa" };


    public static void SavePrefs()
    {
        PlayerPrefs.SetString(TAG_PATH, strExportPath);
        PlayerPrefs.SetString(TAG_EXT + eBUILDTARGET.Windows.ToString(), strArrayBuildExtension[0].ToString());
        PlayerPrefs.SetString(TAG_EXT + eBUILDTARGET.iOS.ToString(), strArrayBuildExtension[1].ToString());
        PlayerPrefs.SetString(TAG_EXT + eBUILDTARGET.Android.ToString(), strArrayBuildExtension[2].ToString());
        PlayerPrefs.SetString(TAG_EXT + eBUILDTARGET.WSAPlayer.ToString(), strArrayBuildExtension[3].ToString());
    }

    public static void LoadPrefs()
    {
        strExportPath = PlayerPrefs.GetString(TAG_PATH);
        if (strExportPath == string.Empty) strExportPath = DEF_EXPORT_PATH;
        strArrayBuildExtension[0] = PlayerPrefs.GetString(TAG_EXT + eBUILDTARGET.Windows.ToString(), DEF_BUNDLE_EXTENSION[0]);
        strArrayBuildExtension[1] = PlayerPrefs.GetString(TAG_EXT + eBUILDTARGET.iOS.ToString(), DEF_BUNDLE_EXTENSION[1]);
        strArrayBuildExtension[2] = PlayerPrefs.GetString(TAG_EXT + eBUILDTARGET.Android.ToString(), DEF_BUNDLE_EXTENSION[2]);
        strArrayBuildExtension[3] = PlayerPrefs.GetString(TAG_EXT + eBUILDTARGET.WSAPlayer.ToString(), DEF_BUNDLE_EXTENSION[3]);
    }

    public static void DeletePrefs()
    {
        PlayerPrefs.DeleteKey(TAG_PATH);
        PlayerPrefs.DeleteKey(TAG_EXT + eBUILDTARGET.Windows.ToString());
        PlayerPrefs.DeleteKey(TAG_EXT + eBUILDTARGET.iOS.ToString());
        PlayerPrefs.DeleteKey(TAG_EXT + eBUILDTARGET.Android.ToString());
        PlayerPrefs.DeleteKey(TAG_EXT + eBUILDTARGET.WSAPlayer.ToString());
    }

    public static void CreateExoprtFolder()
    {
        string AssetFolder = GetBuildExportPath();
        string PlatformFolder = GetBuildExportFullPath();

        DirectoryInfo dir = new DirectoryInfo(AssetFolder);
        if (dir.Exists == false)
        {
            dir = Directory.CreateDirectory(AssetFolder);
            if (!dir.Exists) Debug.LogError("Fail - Create Directory : " + AssetFolder);
        }
        DirectoryInfo dir2 = new DirectoryInfo(PlatformFolder);
        if (dir2.Exists == false)
        {
            dir2 = Directory.CreateDirectory(PlatformFolder);
            if (!dir2.Exists) Debug.LogError("Fail - Create Directory : " + PlatformFolder);
        }
    }

    public static void SaveConfigFile()
    {
        //CONFIG.SaveConfig
    }

    public static string GetProjectPath()
    {
        return xSystem.GetPlatformPath();
    }

    public static string GetBuildExportPath()
    {
        return xSystem.GetPlatformPath() + "/" + strExportPath + "/";
    }
    public static string GetBuildExportFullPath()
    {
        return xSystem.GetPlatformPath() + "/" + strExportPath + "/" + emBuildTarget.ToString() + "/";
    }
    public static BuildTarget GetBuildTarget(eBUILDTARGET target)
    {
        switch (target)
        {
            case eBUILDTARGET.iOS: return BuildTarget.iOS;
            case eBUILDTARGET.Android: return BuildTarget.Android;
            case eBUILDTARGET.Windows: return BuildTarget.StandaloneWindows;
            case eBUILDTARGET.WSAPlayer: return BuildTarget.WSAPlayer;
        }

        return BuildTarget.Android;
    }

    public static string GetBundleExtenstion()
    {
        return strArrayBuildExtension[(int)emBuildTarget];
    }

    public static string GetBuildPipelineOutPath()
    {
        return strExportPath + "/" + emBuildTarget.ToString() + "/";
    }
    public static BuildAssetBundleOptions GetBuildPipelineOptions()
    {
        return emBuildOptions;
    }
    public static BuildTarget GetBuildPipelineTarget()
    {
        return GetBuildTarget(emBuildTarget);
    }
}


public class PedtClearCaching
{

    [MenuItem("xLIB/CleanCache")]
    public static void CleanCache()
    {
        if (Caching.CleanCache())
        {
            EditorUtility.DisplayDialog("알림", "캐시가 삭제되었습니다.", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("오류", "캐시 삭제에 실패했습니다.", "확인");
        }
    }
}


#endif