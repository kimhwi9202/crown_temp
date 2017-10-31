using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace xLIB
{
    public class xSystem: MonoBehaviour
    {
        public delegate void delegateCallBack(System.Collections.Hashtable hash);
        public static delegateCallBack eventCallBack = null;

        public static string GetPlatformPath()
        {
            if (Application.isEditor)
                //return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
                return System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            else if (Application.isWebPlayer)
                return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
#if UNITY_5
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return Application.streamingAssetsPath;
#endif
            else // For standalone player.
                return "file://" + Application.streamingAssetsPath;

        }
        public static string GetPathForDocumentsFile(string filename)
        {
#if UNITY_METRO && !UNITY_EDITOR
            if (IsRunningWSA())
            {
                string path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, filename);
            }
            else
            {
                string path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, filename);
            }
#else
            
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
                string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5);  // ios 8 버젼에선 이렇게 해야 한다.
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(Path.Combine(path, "Documents"), filename);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                string path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, filename);
            }
            else
            {
                string path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                //return path +"/" + filename;
                return Path.Combine(path, filename);
            }
#endif
        }
        public static string AppAllocatePath(string pAttachLastStr = null)
        {
            //Debug.Log( Application.persistentDataPath + "||" + Application.platform );
            string basePath = string.Empty;
            string resultPath = string.Empty;

#if UNITY_EDITOR
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    basePath = Application.persistentDataPath;
                    resultPath = basePath;
                    break;
            }
#else
            switch( Application.platform ) {                
                case RuntimePlatform.Android:
                    basePath=Application.persistentDataPath;                                        
                    resultPath = basePath;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    basePath = Application.dataPath.Substring( 0, Application.dataPath.Length - 5 );
                    basePath = basePath.Substring( 0, basePath.LastIndexOf( '/' ) );
                    resultPath = Path.Combine( basePath, "Documents" );
                    break;
                default:
                    basePath = Application.dataPath;                    
                    resultPath = basePath;
                    break;
            }
#endif

            return pAttachLastStr == null ? resultPath : Path.Combine(resultPath, pAttachLastStr);
        }


        public static GameObject AddChild(GameObject parent, GameObject prefab)
        {
            if (prefab == null) return null;

            GameObject go = GameObject.Instantiate(prefab) as GameObject;

            if (go != null && parent != null)
            {
                // 부모좌표계로 이동처리..
                Vector3 pos = parent.transform.position;
                parent.transform.position = Vector3.zero;

                Transform t = go.transform;
                t.parent = parent.transform;
                t.position = Vector3.zero;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                //t.localScale = Vector3.one;

                parent.transform.position = pos;
            }
            return go;
        }

        public static void FileWrite(string savePath, string value)
        {
            File.WriteAllText(savePath, value, System.Text.Encoding.UTF8);
        }

        public static List<FileInfo> GetFiles(string pDirPaths)
        {
            DirectoryInfo dir = new DirectoryInfo(pDirPaths);
            FileInfo[] files = dir.GetFiles();

            if (files == null || files.Length == 0)
                return new List<FileInfo>();

            return new List<FileInfo>(files);
        }

        public static void RemoveFile(string pFilePath)
        {
            if (File.Exists(pFilePath))
                File.Delete(pFilePath);
        }

        public static string LoadTextFile(string fileFullPath)
        {
            TextReader _reader = null;
            FileInfo _sourceFile = new FileInfo(fileFullPath);
            if (_sourceFile != null && _sourceFile.Exists)
            {
                _reader = _sourceFile.OpenText();
            }

            if (_reader == null)
            {
                Debug.LogError("File not found or not readable : " + fileFullPath);
                return "";
            }

            string inputData = _reader.ReadLine();
            string textOutput = inputData;
            textOutput += "\n";
            while (inputData != null)
            {
                inputData = _reader.ReadLine();
                textOutput += inputData;
                textOutput += "\n";
            }

            _sourceFile = null;
            _reader.Dispose();
            _reader = null;

            return textOutput;
        }

    }
}
