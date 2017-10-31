using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace xLIB.Editor
{
    public class EditorHelper : ScriptableWizard
    {
        public delegate void OnSelectionCallback(Object obj);

        System.Type mType = null;
        string mTitle;
        OnSelectionCallback mCallback;
        Object[] mObjects;
        public bool mSearched = false;
        //Vector2 mScroll = Vector2.zero;
        string[] mExtensions = null;

        static string GetName(System.Type t)
        {
            string s = t.ToString();
            s = s.Replace("UnityEngine.", "");
            if (s.StartsWith("UI")) s = s.Substring(2);
            return s;
        }


        /// <summary>
        /// Search the entire project for required assets.
        /// </summary>

        void Search()
        {
            mSearched = true;

            if (mExtensions != null)
            {
                string[] paths = AssetDatabase.GetAllAssetPaths();
                bool isComponent = mType.IsSubclassOf(typeof(Component));
                List<Object> list = new List<Object>();

                for (int i = 0; i < mObjects.Length; ++i)
                    if (mObjects[i] != null)
                        list.Add(mObjects[i]);

                for (int i = 0; i < paths.Length; ++i)
                {
                    string path = paths[i];

                    bool valid = false;

                    for (int b = 0; b < mExtensions.Length; ++b)
                    {
                        if (path.EndsWith(mExtensions[b], System.StringComparison.OrdinalIgnoreCase))
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (!valid) continue;

                    EditorUtility.DisplayProgressBar("Loading", "Searching assets, please wait...", (float)i / paths.Length);
                    Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                    if (obj == null || list.Contains(obj)) continue;

                    if (!isComponent)
                    {
                        System.Type t = obj.GetType();
                        if (t == mType || t.IsSubclassOf(mType) && !list.Contains(obj))
                            list.Add(obj);
                    }
                    else if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                    {
                        Object t = (obj as GameObject).GetComponent(mType);
                        if (t != null && !list.Contains(t)) list.Add(t);
                    }
                }
                list.Sort(delegate (Object a, Object b) { return a.name.CompareTo(b.name); });
                mObjects = list.ToArray();
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Draw details about the specified object in column format.
        /// </summary>

        bool DrawObject(Object obj)
        {
            if (obj == null) return false;
            bool retVal = false;
            Component comp = obj as Component;

            GUILayout.BeginHorizontal();
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (string.IsNullOrEmpty(path))
                {
                    path = "[Embedded]";
                    GUI.contentColor = new Color(0.7f, 0.7f, 0.7f);
                }
                else if (comp != null && EditorUtility.IsPersistent(comp.gameObject))
                    GUI.contentColor = new Color(0.6f, 0.8f, 1f);

                retVal |= GUILayout.Button(obj.name, "AS TextArea", GUILayout.Width(160f), GUILayout.Height(20f));
                retVal |= GUILayout.Button(path.Replace("Assets/", ""), "AS TextArea", GUILayout.Height(20f));
                GUI.contentColor = Color.white;

                retVal |= GUILayout.Button("Select", "ButtonLeft", GUILayout.Width(60f), GUILayout.Height(16f));
            }
            GUILayout.EndHorizontal();
            return retVal;
        }
    }
}