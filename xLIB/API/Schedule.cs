#if UNITY_EDITOR
//#define IS_LOG   // 로그인 추적을 원하면 주석을 해제
#endif

#region Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion


namespace xLIB
{
    public class Schedule : MonoBehaviour
    {
        //==========================
        #region value
        protected bool __wating = false;
        protected float __wait_time = 0;
        protected float __wait_elapsed_time = 0;

        protected List<Hashtable> __uc_array = new List<Hashtable>();
        protected int __uc_ref_count = 0;

        protected System.Action<Hashtable> eventParserCommand;
        virtual public void virFixedUpdate() { }

#if IS_LOG
        protected System.DateTime mResetTime = System.DateTime.Now;
        protected System.DateTime mNowTime;
        protected System.TimeSpan mCompareTime;

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

        public bool isLog = false;

#endregion // value
        //==========================

        //==========================
#region funcs
        protected string GenerateID()
        {
            return System.Guid.NewGuid().ToString();
        }

        protected Hashtable CleanArgs(Hashtable args)
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

        protected Hashtable Hash(params object[] args)
        {
            Hashtable hashTable = new Hashtable(args.Length / 2);
            if (args.Length % 2 != 0)
            {
                Debug.LogError("Schedule::Error: Hash requires an even number of arguments!");
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

        protected void __removeAt(int code)
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
        protected void __removeAt(string code)
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

        protected void __remove(int code, Hashtable obj)
        {
            if (code == obj["id"].GetHashCode())
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if IS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::__remove id : " + code.ToString() + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed! Schedule::__remove( id: " + obj["id"] + " ) => not matching id ( " + code.ToString() + " )");
        }
        protected void __remove(string code, Hashtable obj)
        {
            if (code == obj["id"].ToString())
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if IS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::__remove id : " + code.ToString() + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed! Schedule::__remove( id: " + obj["id"] + " ) => not matching id ( " + code.ToString() + " )");
        }
#endregion // funcs
        //==========================

        //==========================
#region Update
        protected bool UpdateWaitTime()
        {
            if (__wating)
            {
                __wait_elapsed_time += Time.deltaTime;
                if (__wait_elapsed_time > __wait_time)
                {
                    __wait_elapsed_time = 0;
                    __wating = false;
                }
            }

            return __wating;
        }
        protected void UpdateQueue()
        {
            if (__uc_array.Count > 0 && __uc_ref_count == 0)
            {
                Hashtable obj;
                lock (this)
                {
                    ++__uc_ref_count;
                    obj = (Hashtable)__uc_array[0];
#if IS_LOG
                    if (log_realtime_check)
                    {
                        mNowTime = System.DateTime.Now;
                        mCompareTime = (mResetTime - mNowTime);
                        log_last_get_time = mCompareTime.Days + "d_" + mCompareTime.Hours + "h_" + mCompareTime.Minutes + "m_" + mCompareTime.Seconds + "s_" + mCompareTime.Milliseconds;
                        Debug.Log(name + "::Update > " + log_last_get_time + " key= " + obj.Keys + " GetHashCode = " + obj.GetHashCode() + " count = " + __uc_array.Count);
                    }
#endif
                    if (eventParserCommand != null)
                    {
                        eventParserCommand(obj);
                    }
                }
            }

#if IS_LOG
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
        }
#endregion
        //==========================

        //==========================
#region Remove Interface 
        public void remove(Hashtable args)
        {
            Hashtable obj = (Hashtable)__uc_array[0];
            if (args["id"] == obj["id"] && args["guid"] == obj["guid"])
            {
                __uc_array.Remove(obj);
                --__uc_ref_count;
#if IS_LOG
                if (log_realtime_check) Debug.Log("OK!" + name + "::remove id : " + obj["id"] + " => remain count : " + __uc_array.Count);
#endif
            }
            else Debug.LogError("Failed! Schedule::remove( id: " + obj["id"] + " ) => not matching id ( " + args["id"] + " )");
        }
        public void removeAt(System.Enum eid) { __removeAt(eid.GetHashCode()); }
        public void removeAt(int nid) { __removeAt(nid.GetHashCode()); }
        public void removeAt(string sid) { __removeAt(sid); }
        public void remove(System.Enum eid)
        {
            if (__uc_array.Count > 0)
            {
                Hashtable obj = (Hashtable)__uc_array[0];
                if (obj != null) __remove(eid.GetHashCode(), obj);
            }
        }
        public void remove(int nid)
        {
            if (__uc_array.Count > 0)
            {
                Hashtable obj = (Hashtable)__uc_array[0];
                if (obj != null) __remove(nid.GetHashCode(), obj);
            }
        }
        public void remove(string sid)
        {
            if (__uc_array.Count > 0)
            {
                Hashtable obj = (Hashtable)__uc_array[0];
                if (obj != null) __remove(sid, obj);
            }
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
#endregion // Remove Interface
        //==========================

        //==========================
#region Has Convert Interface
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
        public int HasToEnum(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return has[key].GetHashCode();
            return 0;
        }
        public string HasToString(string key)
        {
            Hashtable has = (Hashtable)__uc_array[0];
            if (has != null && has[key] != null) return has[key].ToString();
            return string.Empty;
        }
#endregion //Has Convert Interface
        //==========================

        //==========================
#region Add Message Interface
        /// <summary> AddMessage enum type id  </summary>
        public void AddMessage(System.Enum eid, params object[] args) {
            if(isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type enum: " + eid.ToString() + " </Color>");
            AddMessage(eid.GetHashCode(), args);
        }
        public void AddMessage(System.Enum eid, Hashtable hash) {
            if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type enum: " + eid.ToString() + " </Color>");
            AddMessage(eid.GetHashCode(), hash);
        }
        public void AddMessage(System.Enum eid) {
            if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type enum: " + eid.ToString() + " </Color>");
            AddMessage(eid.GetHashCode());
        }
        // ex) AddMessage=>(IDs, "a", 1, "b", 2 ); Parser=>(has["id"], "a=" + has["a"] + " , b=" + has["b"]);
        /// <summary> AddMessage int type id  </summary>
        public void AddMessage(int nid, params object[] args)
        {
            //if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type int: " + nid + " </Color>");
            Hashtable hashArgs = CleanArgs(Hash(args));
            if (!hashArgs.Contains("guid")) hashArgs["guid"] = GenerateID();
            hashArgs["id"] = nid;
            __uc_array.Add(hashArgs);
        }
        public void AddMessage(int nid, Hashtable args)
        {
            //if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type int: " + nid + " </Color>");
            args = CleanArgs(args);
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = nid;
            __uc_array.Add(args);
        }
        public void AddMessage(int nid)
        {
            //if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type int: " + nid + " </Color>");
            Hashtable args = CleanArgs(Hash("id", nid));
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = nid;
            __uc_array.Add(args);
        }
        /// <summary> AddMessage string type id  </summary>
        public void AddMessage(string sid, params object[] args)
        {
            if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type string: " + sid + " </Color>");
            Hashtable hashArgs = CleanArgs(Hash(args));
            if (!hashArgs.Contains("guid")) hashArgs["guid"] = GenerateID();
            hashArgs["id"] = sid;
            __uc_array.Add(hashArgs);
        }
        public void AddMessage(string sid, Hashtable args)
        {
            if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type string: " + sid + " </Color>");
            args = CleanArgs(args);
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = sid;
            __uc_array.Add(args);
        }
        public void AddMessage(string sid)
        {
            if (isLog) Debug.Log("<Color=#80FFC0> >> AddMessage - ID type string: " + sid + " </Color>");
            Hashtable args = CleanArgs(Hash("id", sid));
            if (!args.Contains("guid")) args["guid"] = GenerateID();
            args["id"] = sid;
            __uc_array.Add(args);
        }
#endregion //Add Message Interface
        //==========================


        /// <summary>
        /// 메세지 큐 받을 콜벡함수 정의
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void SetCallback_HandleMessage(System.Action<Hashtable> callback)
        {
            eventParserCommand = callback;
        }

        /// <summary>
        /// 멤버로 사용시 업데이트 함수에서 호출해라.
        /// </summary>
        public void UpdateMessage()
        {
            if (UpdateWaitTime()) { virFixedUpdate(); return; }
            UpdateQueue();
            virFixedUpdate();
        }

        /// <summary>
        /// 상속받은 오브젝트형 클래스라면 FixedUpdate 내부예약어 사용한다.
        /// </summary>
        void FixedUpdate()
        {
            UpdateMessage();
        }
    }
}