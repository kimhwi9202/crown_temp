#if UNITY_EDITOR
//#define ISS_LOG
#endif

#region Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion


/// <summary>
/// 
/// </summary>
namespace xLIB
{
    public class SingletonSchedule<T> : Schedule where T : Schedule
    {
        #region Singleton define
        private static T m_Instance = null;
        public static T I
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = FindObjectOfType(typeof(T)) as T;
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject("Schedule_" + typeof(T).ToString());
                    m_Instance = obj.AddComponent(typeof(T)) as T;
                }
                return m_Instance;
            }
        }
        private void OnApplicationQuit()
        {
            m_Instance = null;
        }
        #endregion
        /*
        //==========================
        #region value
        private bool __wating = false;
        private float __wait_time = 0;
        private float __wait_elapsed_time = 0;

        private List<Hashtable> __uc_array = new List<Hashtable>();
        private int __uc_ref_count = 0;

        public delegate void delegateParserCommand(Hashtable hash);
        public event delegateParserCommand eventParserCommand;

#if ISS_LOG
        private System.DateTime mResetTime = System.DateTime.Now;
        private System.DateTime mNowTime;
        private System.TimeSpan mCompareTime;

        public bool log_realtime_check = false;
        public GameObject log_target = null;
        public string log_func;
        public string log_wating;
        public float log_elapsed_time = 0;
        public int log_total_count = 0;
        public int log_ref_count = 0;
        public string log_current_id;
        public int log_current_hashcode;
        public string log_last_get_time;
#endif
*/
        void Awake()
        {
            DontDestroyOnLoad(this);
            virAwake();
        }

        virtual public void virAwake() { }
        //virtual public void virFixedUpdate() { }

        void FixedUpdate()
        {
            if (UpdateWaitTime()) { virFixedUpdate(); return; }
            UpdateQueue();
            virFixedUpdate();
        }

        /*
        #endregion
        //==========================

        //==========================
        #region private funcs
        private string GenerateID()
        {
            return System.Guid.NewGuid().ToString();
        }

        private Hashtable CleanArgs(Hashtable args)
        {
            Hashtable argsCopy = new Hashtable(args.Count);
            Hashtable argsCaseUnified = new Hashtable(args.Count);

            foreach (DictionaryEntry item in args)
            {
                argsCopy.Add(item.Key, item.Value);
            }

            foreach (DictionaryEntry item in argsCopy)
            {
                if (item.Value.GetType() == typeof(System.Int32))
                {
                    int original = (int)item.Value;
                    float casted = (float)original;
                    args[item.Key] = casted;
                }
                if (item.Value.GetType() == typeof(System.Double))
                {
                    double original = (double)item.Value;
                    float casted = (float)original;
                    args[item.Key] = casted;
                }
            }

            //unify parameter case:
            foreach (DictionaryEntry item in args)
            {
                argsCaseUnified.Add(item.Key.ToString().ToLower(), item.Value);
            }

            //swap back case unification:
            args = argsCaseUnified;

            return args;
        }

        private Hashtable Hash(params object[] args)
        {
            Hashtable hashTable = new Hashtable(args.Length / 2);
            if (args.Length % 2 != 0)
            {
                Debug.LogError(name+"::Error: Hash requires an even number of arguments!");
                return null;
            }
            else
            {
                int i = 0;
                while (i < args.Length - 1)
                {
                    hashTable.Add(args[i], args[i + 1]);
                    i += 2;
                }
                return hashTable;
            }
        }
        private void __removeAt(int code)
        {
            for (int i = 0; i < __uc_array.Count; i++)
            {
                Hashtable obj = (Hashtable)__uc_array[i];
                if (code == obj["id"].GetHashCode())
                {
                    __uc_array.Remove(obj);
                    if (i == 0) --__uc_ref_count;
                    break;
                }
            }
        }
        private void __removeAt(string code)
        {
            for (int i = 0; i < __uc_array.Count; i++)
            {
                Hashtable obj = (Hashtable)__uc_array[i];
                if (code == obj["id"].ToString())
                {
                    __uc_array.Remove(obj);
                    if (i == 0) --__uc_ref_count;
                    break;
                }
            }
        }

        private void __remove(int code, Hashtable obj)
        {
            if (code == obj["id"].GetHashCode())
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if ISS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::__remove id : " + code.ToString() + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed! " + name + "::__remove( id: " + obj["id"] + " ) => not matching id ( " + code.ToString() + " )");
        }
        private void __remove(string code, Hashtable obj)
        {
            if (code == obj["id"].ToString())
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if ISS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::__remove id : " + code.ToString() + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed! " + name + "::__remove( id: " + obj["id"] + " ) => not matching id ( " + code.ToString() + " )");
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //Debug.Log("FixedUUpdate : " + Time.fixedTime);
            if (__wating)
            {
                __wait_elapsed_time += Time.deltaTime;
                if (__wait_elapsed_time > __wait_time)
                {
                    Debug.Log("__wait_elapsed_time : " + __wait_elapsed_time);
                    __wait_elapsed_time = 0;
                    __wating = false;
                }
                else
                {
                    virFixedUpdate();
                    return;
                }
            }

            if (__uc_array.Count > 0 && __uc_ref_count == 0)
            {
                Hashtable obj;
                lock (this)
                {
                    ++__uc_ref_count;
                    obj = (Hashtable)__uc_array[0];
#if ISS_LOG 
                    if (log_realtime_check)
                    {
                        mNowTime = System.DateTime.Now;
                        mCompareTime = (mResetTime - mNowTime);
                        log_last_get_time = mCompareTime.Days + "d_" + mCompareTime.Hours + "h_" + mCompareTime.Minutes + "m_" + mCompareTime.Seconds + "s_" + mCompareTime.Milliseconds;
                        Debug.Log(name + "::Update > " + log_last_get_time + " key= " + obj.Keys + " GetHashCode = " + obj.GetHashCode() + " count = " + __uc_array.Count);
                    }
#endif
                    if (eventParserCommand != null) eventParserCommand(obj);
                }
            }

#if ISS_LOG
            if (log_realtime_check)
            {
                log_wating = __wating.ToString();
                log_elapsed_time = __wait_elapsed_time;
                log_ref_count = __uc_ref_count;
                log_total_count = __uc_array.Count;
                if (log_total_count > 0)
                {
                    Hashtable ref_obj = (Hashtable)__uc_array[0];
                    log_current_id = ref_obj["id"].ToString();
                    log_current_hashcode = ref_obj["id"].GetHashCode();
                }
            }
#endif
            virFixedUpdate();
        }

        #endregion // private funcs
        //==========================

        //==========================
        #region public Interface 
        public void remove(Hashtable args)
        {
            Hashtable obj = (Hashtable)__uc_array[0];
            if (args["id"] == obj["id"] && args["guid"] == obj["guid"])
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if ISS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::remove id : " + obj["id"] + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed!" + name + "::remove( id: " + obj["id"] + " ) => not matching id ( " + args["id"] + " )");
        }
        public void removeAt(System.Enum eid) { __removeAt(eid.GetHashCode()); }
        public void removeAt(int nid) { __removeAt(nid.GetHashCode()); }
        public void removeAt(string sid) { __removeAt(sid); }
        public void remove(System.Enum eid)
        {
            Hashtable obj = (Hashtable)__uc_array[0];
            __remove(eid.GetHashCode(), obj);
        }
        public void remove(int nid)
        {
            Hashtable obj = (Hashtable)__uc_array[0];
            __remove(nid.GetHashCode(), obj);
        }
        public void remove(string sid)
        {
            Hashtable obj = (Hashtable)__uc_array[0];
            __remove(sid, obj);
        }

        public void removeAll()
        {
            if (__uc_array.Count > 0)
            {
                lock (this)
                {
                    __uc_ref_count = 0;
                    __uc_array.Clear();
                }
            }
        }

        public void SetDalayWating(float time)
        {
            __wait_time = time;
            __wating = true;
        }
        #endregion
        //==========================
        
        #region Has Convert Api
        public int HasToInt(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return System.Convert.ToInt32(has[key].ToString());
            return -1;
        }
        public long HasToLong(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return System.Convert.ToInt64(has[key].ToString());
            return -1;
        }
        public double HasToDouble(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return System.Convert.ToDouble(has[key].ToString());
            return -1f;
        }
        public bool HasToBoolean(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return System.Convert.ToBoolean(has[key].ToString());
            return false;
        }
        public string HasToString(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return has[key].ToString();
            return string.Empty;
        }
        #endregion //Has Convert Api

        #region Add Commnad Api
        /// <summary>
        /// AddMessage enum type id 
        /// </summary>
        public void AddMessage(System.Enum eid, params object[] args) { AddMessage(eid.GetHashCode(), args); }
        public void AddMessage(System.Enum eid, Hashtable hash) { AddMessage(eid.GetHashCode(), hash); }
        public void AddMessage(System.Enum eid) { AddMessage(eid.GetHashCode()); }
        // ex) AddMessage=>(IDs, "a", 1, "b", 2 ); Parser=>(has["id"], "a=" + has["a"] + " , b=" + has["b"]);
        /// <summary>
        /// AddMessage int type id 
        /// </summary>
        public void AddMessage(int nid, params object[] args)
        {
            Hashtable hashArgs = CleanArgs(Hash(args));
            if (!hashArgs.Contains("guid")) hashArgs["guid"] = GenerateID();
            hashArgs["id"] = nid;
            __uc_array.Add(hashArgs);
        }
        public void AddMessage(int nid, Hashtable args)
        {
            args = CleanArgs(args);
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = nid;
            __uc_array.Add(args);
        }
        public void AddMessage(int nid)
        {
            Hashtable args = CleanArgs(Hash("id", nid));
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = nid;
            __uc_array.Add(args);
        }
        /// <summary>
        /// AddMessage string type id 
        /// </summary>
        public void AddMessage(string sid, params object[] args)
        {
            Hashtable hashArgs = CleanArgs(Hash(args));
            if (!hashArgs.Contains("guid")) hashArgs["guid"] = GenerateID();
            hashArgs["id"] = sid;
            __uc_array.Add(hashArgs);
        }
        public void AddMessage(string sid, Hashtable args)
        {
            args = CleanArgs(args);
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = sid;
            __uc_array.Add(args);
        }
        public void AddMessage(string sid)
        {
            Hashtable args = CleanArgs(Hash("id", sid));
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = sid;
            __uc_array.Add(args);
        }
        #endregion
    */
    }
}