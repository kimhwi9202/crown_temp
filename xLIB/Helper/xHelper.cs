using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace xLIB
{
    public class xHelper : MonoBehaviour
    {
        //-----------------------------------------------------------------
        // 서버 타임과 동기화.. 
        //-----------------------------------------------------------------
        // 서버머신 시작타임..
        private static long TM_2008_01_01_00_00_00 = 1199113200;//1469936378444;//1199113200;
        private static long BASE_TIME(long _time) { return ((_time - TM_2008_01_01_00_00_00) % 86400); }
        private static long BASE_TIME_REMAIN(long _time) { return (_time % 86400); }

        // 이전시간과 지금 시간을 비교시 하루가 지났는가?
        static public bool IS_NEXT_DAY(long _before, long _now)
        {
            return (((_before - TM_2008_01_01_00_00_00) / 86400) < (((_now) - TM_2008_01_01_00_00_00) / 86400));
        }
        // 서버기준 시간..
        static public long GET_DAY(long _time) { return ((_time - TM_2008_01_01_00_00_00) / 86400); }
        static public long GET_HOUR(long _time) { return (BASE_TIME(_time) / 3600); }
        static public long GET_MINUTE(long _time) { return ((BASE_TIME(_time) - (GET_HOUR(_time) * 3600)) / 60); }
        static public long GET_SEC(long _time) { return (BASE_TIME(_time) - ((GET_HOUR(_time) * 3600) + (GET_MINUTE(_time) * 60))); }
        // 시간..
        static public long GET_DAY_REMAIN(long _time) { return (_time / 86400); }
        static public long GET_HOUR_REMAIN(long _time) { return (BASE_TIME_REMAIN(_time) / 3600); }
        static public long GET_MINUTE_REMAIN(long _time) { return ((BASE_TIME_REMAIN(_time) - (GET_HOUR_REMAIN(_time) * 3600)) / 60); }
        static public long GET_SEC_REMAIN(long _time) { return (BASE_TIME_REMAIN(_time) - ((GET_HOUR_REMAIN(_time) * 3600) + (GET_MINUTE_REMAIN(_time) * 60))); }
        //-----------------------------------------------------------------

        static public string GetTimeKorToString(long _time, bool _ignoreSecond = false)
        {
            long temp;
            string str = "";
            if ((temp = GET_DAY_REMAIN(_time)) > 0)
            {
                str = temp + "일 " + GET_HOUR_REMAIN(_time) + "시간";
            }
            else if ((temp = GET_HOUR_REMAIN(_time)) > 0)
            {
                str = temp + "시간 " + GET_MINUTE_REMAIN(_time) + "분";
            }
            else if ((temp = GET_MINUTE_REMAIN(_time)) > 0)
            {
                if (_ignoreSecond == false)
                    str = temp + "분 " + GET_SEC_REMAIN(_time) + "초";
                else
                    str = temp + "분";
            }
            else if (_ignoreSecond == false)
            {
                str = GET_SEC_REMAIN(_time) + "초";
            }
            return str;
        }
        static public string GetTimeChineseToString(long _time, bool _ignoreSecond = false)
        {
            long temp;
            string str = "";
            if ((temp = GET_DAY_REMAIN(_time)) > 0)
            {
                str = temp + "天 " + GET_HOUR_REMAIN(_time) + "时间";
            }
            else if ((temp = GET_HOUR_REMAIN(_time)) > 0)
            {
                str = temp + "时间 " + GET_MINUTE_REMAIN(_time) + "分钟";
            }
            else if ((temp = GET_MINUTE_REMAIN(_time)) > 0)
            {
                if (_ignoreSecond == false)
                    str = temp + "分钟 " + GET_SEC_REMAIN(_time) + "第二";
                else
                    str = temp + "分钟";
            }
            else if (_ignoreSecond == false)
            {
                str = GET_SEC_REMAIN(_time) + "第二";
            }
            return str;
        }

        static public string GetTimeEngToString(long _time, bool _ignoreSecond = false)
        {
            long temp;
            string str = "";
            if ((temp = GET_DAY_REMAIN(_time)) > 0)
            {
                str = temp + "d " + GET_HOUR_REMAIN(_time) + "h";
            }
            else if ((temp = GET_HOUR_REMAIN(_time)) > 0)
            {
                str = temp + "h " + GET_MINUTE_REMAIN(_time) + "m";
            }
            else if ((temp = GET_MINUTE_REMAIN(_time)) > 0)
            {
                if (_ignoreSecond == false)
                    str = temp + "m " + GET_SEC_REMAIN(_time) + "s";
                else
                    str = temp + "m ";
            }
            else if (_ignoreSecond == false)
            {
                str = GET_SEC_REMAIN(_time) + "s";
            }
            return str;
        }


#if UNITY_EDITOR
        static protected System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
#endif
        //모든 자식오브젝트도 레이어 적용 - 레이어 변경 때 사용 SetLayerRecursively(오브젝트, LayerMask.NameToLayer("이름") );
        static public void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
                return;

            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        static public void StartStopwatch()
        {
#if UNITY_EDITOR
            _sw.Reset();
            _sw.Start();
#endif
        }
        static public void RestStopwatch()
        {
#if UNITY_EDITOR
            if (_sw.IsRunning)
                _sw.Reset();
#endif
        }
        // 3000 msec 이상이면 실패.
        static public string StopStopwatch()
        {
            string msec = "";
#if UNITY_EDITOR
            if (_sw.IsRunning)
            {
                _sw.Stop();
                msec = _sw.ElapsedMilliseconds.ToString() + " msec";
            }
#endif
            return msec;
        }

        static public GameObject AddChild(GameObject parent, GameObject prefab)
        {
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
            if (go != null && parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = parent.layer;
            }
            return go;
        }

    }


    public class Json
    {
        static public bool IsKey(LitJson.JsonData data, string itemName)
        {
            if (0 < data.Count) return data.Keys.Contains(itemName);
            return false;
        }
        static public int GetInt(LitJson.JsonData data, string itemName)
        {
            if (IsKey(data, itemName)) return Convert.ToInt32(data[itemName].ToString());
            return -1;
        }
        static public float GetDouble(LitJson.JsonData data, string itemName)
        {
            if (IsKey(data, itemName)) return (float)Convert.ToDouble(data[itemName].ToString());
            return -1;
        }
        static public string GetString(LitJson.JsonData data, string itemName)
        {
            if (IsKey(data, itemName)) return data[itemName].ToString();
            return "";
        }
        static public bool GetBool(LitJson.JsonData data, string itemName)
        {
            if (IsKey(data, itemName)) return (data[itemName].ToString() == "Y") ? true : false;
            return false;
        }

        // { "seq": 0,"member_seq": 0,...} -> "0,0,..."
        static public string ConvertJsonToCsv<T>(T _value)
        {
            LitJson.JsonData TJsonData = LitJson.JsonMapper.ToObject(LitJson.JsonMapper.ToJson(_value));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < TJsonData.Count; i++)
            {
                if (i + 1 == TJsonData.Count)
                {
                    sb.Append(TJsonData[i].ToString());
                }
                else
                {
                    sb.Append(TJsonData[i].ToString() + ",");
                }
            }
            //Debug.Log("sb= " + sb.ToString());
            return sb.ToString();
        }

        // "0,0,..." -> { "seq": 0,"member_seq": 0,...} return T
        static public T ConvertCsvToJson<T>(string csv) where T : new()
        {
            T _value = new T();
            // csv형태의 값들을 배열로 변환
            string[] csv_array = xCSVParser.SplitCsvLine(csv);
            // T 데이터형의 멤버 스트링정보 얻기
            LitJson.JsonData TJsonData = LitJson.JsonMapper.ToObject(LitJson.JsonMapper.ToJson(_value));
            LitJson.JsonData newJsonData = new LitJson.JsonData();
            ICollection<string> col = TJsonData.Keys;
            IEnumerator e = col.GetEnumerator();
            int i = 0;
            while (e.MoveNext())
            {
                string key = e.Current.ToString(); // T 맴버 스트링

                if(TJsonData[i]==null)  // string 형의 할당이 안되어 있다..별도 처리
                {
                    newJsonData[key] = csv_array[i];
                }
                else
                {
                    if (TJsonData[i].IsString) newJsonData[key] = Convert.ToString(csv_array[i]);
                    else if (TJsonData[i].IsBoolean) newJsonData[key] = Convert.ToBoolean(csv_array[i]);
                    else if (TJsonData[i].IsDouble) newJsonData[key] = Convert.ToDouble(csv_array[i]);
                    else if (TJsonData[i].IsInt) newJsonData[key] = Convert.ToInt32(csv_array[i]);
                    else if (TJsonData[i].IsLong) newJsonData[key] = Convert.ToInt64(csv_array[i]);
                    else newJsonData[key] = csv_array[i];  // 오류방지를 위해 나머지는 스트링 처리
                }
                ++i;
            }
            //Debug.Log("ConvertCsvToJson= " + newJsonData.ToJson());
            return LitJson.JsonMapper.ToObject<T>(newJsonData.ToJson());
        }
    }
}
