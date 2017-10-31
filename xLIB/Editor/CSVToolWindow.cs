#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using xLIB.Editor;
using xLIB;
using System.IO;
using System.Text;


public class CSVToolWindow : EditorWindow
{
    public static CSVToolWindow instance;

    protected Vector2 mScroll = Vector2.zero;
    protected Vector2 mScrollLocal = Vector2.zero;
    protected Vector2 mScrollFtp = Vector2.zero;
    protected string sel_rename = string.Empty;
    protected int mTab = 0;
    protected string sel_key = string.Empty;
    protected string sel_state = string.Empty;


    [MenuItem("xLIB/CSVToolWindow")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<CSVToolWindow>(false, "CSVToolWindow Editor", true).Show();
    }
    void OnEnable()
    {
        instance = this;
        mScroll = Vector2.zero;
        sel_rename = string.Empty;
        DBTableSetting.LoadPrefs();
    }
    void OnDisable()
    {
        instance = null;
        DBTableSetting.SavePrefs();
    }

    void OnGUI()
    {
        TabFileGUI();
    }

    public bool DrawItemList(int idx, string text, bool forceOn)
    {
        string key = text;
        bool state = EditorPrefs.GetBool(key, false);

        if (!forceOn && !state) GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (!state) // pading off
        {
            GUI.backgroundColor = Color.red;
            GUI.backgroundColor = Color.white;
        }

        if (state) GUI.backgroundColor = Color.green;
        text = "<b><size=12>" + text + "</size></b>";
        if (state) text = " \u25BC  " + text;
        else text = " \u25BA  " + text;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;

        GUI.backgroundColor = Color.white;

        if (GUI.changed)
        {
            sel_key = key;
            EditorPrefs.SetBool(key, state);
        }

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    void DrawValueInfo(string type, string title, ref string value, string def)
    {
        bool sel = false;
        if (title != string.Empty && string.Compare(title, sel_rename) == 0) sel = true;

        GUILayout.BeginHorizontal();
        GUILayout.Label(type, GUILayout.Width(50));
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
            if( value != def) GUILayout.Label(def);
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }


    void ShowListItems(string[,] grid)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);
        GUILayout.BeginVertical();

        mScroll = GUILayout.BeginScrollView(mScroll);
        int begin = DBTableSetting.GetPageBeginIndex();
        int end = DBTableSetting.GetPageEndIndex();

        for (int y = begin; y < end; y++)
        {
            string textOutput = "";
            textOutput += grid[0, y];
            textOutput += " / ";
            textOutput += grid[1, y];
            textOutput += " / ";
            textOutput += grid[2, y];

            if (DrawItemList(y, textOutput, false))
            {
                textOutput = "";
                for (int x = 0; x < grid.GetUpperBound(0); x++)
                {
                    textOutput = grid[x, y];
                    DrawValueInfo(grid[x, 0], grid[x, 1], ref textOutput, DBTableSetting.oldGrid[x, y]);
                    grid[x, y] = textOutput;
                }
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
    }


    void TabFileGUI()
    {
        EditorTools.DrawHeader("Info", true);
        EditorTools.BeginContents(false);

        GUI.contentColor = Color.green;
        GUILayout.Label("\u25BA Absolute Project Path >  " + DBTableSetting.GetProjectPath());
        GUILayout.Label("\u25BA Absolute Backup Path >  " + DBTableSetting.GetBackupFullPath());
        GUI.contentColor = Color.white;

        EditorTools.EndContents();


        EditorTools.DrawHeader(DBTableSetting.strFileName, true);
        EditorTools.BeginContents(false);

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        GUILayout.Space(20f);

        if (GUILayout.Button("Load CSV File"))
        {
            if (sel_key != string.Empty)
            {
                EditorPrefs.SetBool(sel_key, false); // 펼쳐진 리스트 초기화.
            }

            DBTableSetting.OpenLoadCSVFile();

            GUILayout.Space(20f);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            return;
        }

        GUILayout.Space(20f);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        EditorTools.EndContents();


        if(DBTableSetting.IsLoadFile())
        {
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.cyan;
            GUILayout.Label("\u25BA Load Path >  " + DBTableSetting.strLoadFullPath);
            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            GUILayout.Space(20f);
            if (GUILayout.Button("Save CSV File"))
            {
                DBTableSetting.SaveCSVFile();
            }
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Clean List"))
            {
                DBTableSetting.DeletePrefs();
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                return;
            }
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            GUILayout.Space(20f);
            if (GUILayout.Button("Make Struct"))
            {
                DBTableSetting.CreateStructClipboard(0);
            }
            if (GUILayout.Button("Make Load List Func"))
            {
                DBTableSetting.CreateParserListClipboard(0);
            }
            if (GUILayout.Button("Make Load Dictionary Func"))
            {
                DBTableSetting.CreateParserDictionaryClipboard(0);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();


            ShowListItems(DBTableSetting.grid);


            EditorTools.BeginContents(false);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.cyan;
            GUILayout.Space(20f);

            if (GUILayout.Button(" < ")) DBTableSetting.PagePrev();
            GUILayout.Toggle(true, DBTableSetting.GetCurrentPage(), "dragtab2", GUILayout.MaxWidth(70f));
            if (GUILayout.Button(" > ")) DBTableSetting.PageNext();

            GUILayout.Space(20f);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            EditorTools.EndContents();
        }
    }
}


public class DBTableSetting
{
    public static string[,] grid = null;
    public static string[,] oldGrid = null;
    public static string strLoadFullPath = string.Empty;
    public static string strFileName = "None";
    public static int curPage = 1;
    public static int maxList = 0;
    public static int maxPage = 1;
    public static int maxPageLine = 20;

    public static void SavePrefs()
    {
    }

    public static void LoadPrefs()
    {
        if (IsLoadFile())
        {
            LoadFile(strLoadFullPath);
        }
    }

    public static void DeletePrefs()
    {
        grid = null;
        oldGrid = null;
        strLoadFullPath = string.Empty;
        strFileName = "None";
        curPage = 1;
        maxList = 0;
        maxPage = 1;
    }
    public static bool IsLoadFile()
    {
        return (grid != null) ? true : false;
    }

    public static void OpenLoadCSVFile()
    {
        strLoadFullPath = EditorUtility.OpenFilePanel("Load CSV Files", GetAssetsPath(), "csv");
        if(strLoadFullPath.Length > 20) LoadFile(strLoadFullPath);
    }

    private static void LoadFile(string fileFullPath)
    {
        string text = string.Empty;
        FileInfo fi = new FileInfo(fileFullPath);
        strFileName = fi.Name.Replace(fi.Extension, "");
/*
        TextAsset _txtFile = (TextAsset)Resources.Load(strFileName) as TextAsset;
        text = _txtFile.text;
*/
        text = xCSVParser.LoadFile(fileFullPath);

        grid = xCSVParser.SplitCsvGrid(text);
        oldGrid = xCSVParser.SplitCsvGrid(text);
        maxList = grid.GetUpperBound(1) - 2;
        curPage = 0;
        maxPage = maxList / maxPageLine;
    }

    public static void SaveCSVFile()
    {
        string textOutput = "";
        for (int y = 0; y < grid.GetUpperBound(1); y++)
        {
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                textOutput += grid[x, y];
                textOutput += ",";
            }
            textOutput += "\r\n";
        }

        File.WriteAllText(strLoadFullPath, textOutput, Encoding.UTF8);
        // backup
        DirectoryInfo dir = new DirectoryInfo(GetBackupFullPath());
        if (dir.Exists == false)
        {
            dir = Directory.CreateDirectory(GetBackupFullPath());
            if (!dir.Exists) { Debug.LogError("Fail - Create Directory : " + GetBackupFullPath()); return; }
        }
        File.WriteAllText(GetBackupFullPath() + "/" + strFileName + ".csv", textOutput, Encoding.UTF8);
    }

    public static void CreateStructClipboard(int type)
    {
        string temp = string.Empty;
        temp = "public struct ST" + strFileName + "\r\n";
        temp += "{\r\n";
        for (int x = 0; x < grid.GetUpperBound(0); x++)
        {
            temp += "public " + grid[x,0] + " " + grid[x, 1] + ";\r\n";
            /*
            if (type==0) temp += "public string " + grid[x, 0] + ";\r\n";
            else if(type==1) temp += "public int " + grid[x, 0] + ";\r\n";
            else if (type == 2) temp += "public float " + grid[x, 0] + ";\r\n";
            */
        }
        temp += "}\r\n";

        xClipboardHelper.clipBoard = temp;
        //Debug.Log(temp);
    }
    public static void CreateParserListClipboard(int type)
    {
        string temp = string.Empty;
        string name = "ST" + strFileName;
        temp = "public void LoadTable"+name+"()\r\n";
        temp += "{\r\n";
        temp += "//public List<" + name + "> list" + name + " = new List<" + name + ">();\r\n";
        temp += "TextAsset table = BundleManager.Instance.LoadAsset<TextAsset>(\"" + strFileName + "\");\r\n";
        temp += "string[,] grid = CSVParser.SplitCsvGrid(table.text);\r\n";
        temp += "for (int y = 2; y < grid.GetUpperBound(1)-2; y++)\r\n";
        temp += "{\r\n";
        temp += "int idx = 0;\r\n";
        temp += name + " data = new " + name + "();\r\n";
        temp += "if (grid[idx, y] != null && grid[idx, y].Length > 0)\r\n";
        temp += "{\r\n";
        for (int x = 0; x < grid.GetUpperBound(0); x++)
        {
            if(grid[x,0] == "string") temp += "    data." + grid[x, 1] + " = grid[idx++, y];\r\n";
            else temp += "    data." + grid[x, 1] + " = " + grid[x, 0] + ".Parse(grid[idx++, y]);\r\n";
            /*
            if (type==0) temp += "    data." + grid[x, 0] + " = grid[idx++, y];\r\n";
            else if (type == 1) temp += "    data." + grid[x, 0] + " = int.Parse(grid[idx++, y]);\r\n";
            else if (type == 2) temp += "    data." + grid[x, 0] + " = float.Parse(grid[idx++, y]);\r\n";
            */
        }
        temp += "\r\n" + "list" + name + ".Add(data);\r\n";
        temp += "}\r\n";
        temp += "}\r\n";
        temp += "}\r\n";

        xClipboardHelper.clipBoard = temp;
        //Debug.Log(temp);
    }
    public static void CreateParserDictionaryClipboard(int type)
    {
        //public Dictionary<string, STFish_Nature> dictSTFish_Nature = new Dictionary<string, STFish_Nature>();
        string temp = string.Empty;
        string name = "ST" + strFileName;
        temp = "public void LoadTable" + name + "()\r\n";
        temp += "{\r\n";
        temp += "//public Dictionary<string," + name + "> dict" + name + " = new Dictionary<string," + name + ">();\r\n";
        temp += "TextAsset table = BundleManager.Instance.LoadAsset<TextAsset>(\"" + strFileName + "\");\r\n";
        temp += "string[,] grid = CSVParser.SplitCsvGrid(table.text);\r\n";
        temp += "for (int y = 2; y < grid.GetUpperBound(1)-2; y++)\r\n";
        temp += "{\r\n";
        temp += "int idx = 0;\r\n";
        temp += name + " data = new " + name + "();\r\n";
        temp += "if (grid[idx, y] != null && grid[idx, y].Length > 0)\r\n";
        temp += "{\r\n";
        for (int x = 0; x < grid.GetUpperBound(0); x++)
        {
            if (grid[x, 0] == "string") temp += "    data." + grid[x, 1] + " = grid[idx++, y];\r\n";
            else temp += "    data." + grid[x, 1] + " = " + grid[x, 0] + ".Parse(grid[idx++, y]);\r\n";
            /*
            if (type==0) temp += "    data." + grid[x, 0] + " = grid[idx++, y];\r\n";
            else if (type == 1) temp += "    data." + grid[x, 0] + " = int.Parse(grid[idx++, y]);\r\n";
            else if (type == 2) temp += "    data." + grid[x, 0] + " = float.Parse(grid[idx++, y]);\r\n";
            */
        }
        //dictSTFish_Nature.Add(data.id, data);
        temp += "\r\n" + "dict" + name + ".Add(data."+ grid[0, 1]+",data);\r\n";
        temp += "}\r\n";
        temp += "}\r\n";
        temp += "}\r\n";

        xClipboardHelper.clipBoard = temp;
        //Debug.Log(temp);
    }

    public static void PagePrev()
    {
        --curPage;
        if (curPage < 0) curPage = 0;
    }
    public static string GetCurrentPage()
    {
        string strPage = (curPage + 1) + " / " + (maxPage + 1);
        return strPage = "<b><size=12>" + strPage + "</size></b>";
    }
    public static void PageNext()
    {
        ++curPage;
        if (curPage > maxPage) curPage = maxPage;
    }

    public static int GetPageBeginIndex()
    {
        int begin = (curPage * maxPageLine);
        if (begin < 2) begin = 2;
        return begin;
    }
    public static int GetPageEndIndex()
    {
        int end = ((curPage + 1) * maxPageLine);
        if (end > maxList) end = maxList;
        return end;
    }

    public static string GetProjectPath()
    {
        return xSystem.GetPlatformPath();
    }

    public static string GetBackupFullPath()
    {
        return xSystem.GetPlatformPath() + "/DBBackup/";
    }
    public static string GetAssetsPath()
    {
        return xSystem.GetPlatformPath() + "/Assets";
    }

    public static string GetResourcesPath()
    {
        return xSystem.GetPlatformPath() + "/Assets/Resources";
    }
}

#endif