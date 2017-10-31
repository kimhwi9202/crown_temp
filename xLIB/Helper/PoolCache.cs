using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace xLIB
{
    public class PoolCache
    {
        public int count;
        public GameObject prefab;
        public List<GameObject> buffer = new List<GameObject>();    // 버퍼..
        private Transform _parent;

        public void remove_all()
        {
            count = 0;
            prefab = null;
            buffer.Clear();
        }

        private void _create()
        {
            GameObject obj = GameObject.Instantiate(prefab) as GameObject;
            obj.transform.SetParent(_parent);
            obj.SetActive(false);
            buffer.Add(obj);
        }

        public void CreateBuffer(Transform parent, GameObject _prefab, int _count, int _layer = 0)
        {
            _parent = parent;
            count = 0;
            prefab = _prefab;
            xHelper.SetLayerRecursively(prefab, _layer);
            for (int i = 0; i < _count; i++) _create();
        }

        // Attach(Transform) 사용시 자원 회수시에 이용
        public void Detach(GameObject obj)
        {
            if (obj)
            {
                obj.transform.SetParent(_parent);
                obj.SetActive(false);
                buffer.Add(obj);
            }
        }
        // 특정 오브젝트로 이동해서 사용시
        public GameObject Attach(Transform parent)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                GameObject obj = (GameObject)buffer[i];
                if (obj != null && obj.activeSelf == false)
                {
                    obj.transform.SetParent(parent);
                    obj.SetActive(true);
                    buffer.RemoveAt(i);
                    return obj;
                }
            }

            _create();
            int idx = buffer.Count - 1;

            GameObject new_obj = (GameObject)buffer[idx];
            new_obj.transform.SetParent(parent);
            new_obj.SetActive(true);
            buffer.RemoveAt(idx);
            return new_obj;
        }

        public GameObject Attach()
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                GameObject obj = (GameObject)buffer[i];
                if (obj != null && obj.activeSelf == false)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            _create();
            int idx = buffer.Count - 1;

            GameObject new_obj = (GameObject)buffer[idx];
            new_obj.SetActive(true);

            return new_obj;
        }

        public GameObject Attach(Vector3 _pos) { return Attach(_pos, Quaternion.identity); }
        public GameObject Attach(Vector3 _pos, Quaternion _quat)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                GameObject obj = (GameObject)buffer[i];
                if (obj != null && obj.activeSelf == false)
                {
                    obj.transform.position = _pos;
                    obj.transform.localRotation = _quat;
                    obj.SetActive(true);
                    return obj;
                }
            }

            _create();
            int idx = buffer.Count - 1;

            GameObject new_obj = (GameObject)buffer[idx];
            new_obj.transform.position = _pos;
            new_obj.transform.localRotation = _quat;
            new_obj.SetActive(true);

            return new_obj;
        }
        public GameObject LocalPosAttach(Vector3 _pos)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                GameObject obj = (GameObject)buffer[i];
                if (obj != null && obj.activeSelf == false)
                {
                    obj.transform.localPosition = _pos;
                    obj.SetActive(true);
                    return obj;
                }
            }

            _create();
            int idx = buffer.Count - 1;

            GameObject new_obj = (GameObject)buffer[idx];
            new_obj.transform.localPosition = _pos;
            new_obj.SetActive(true);

            return new_obj;
        }

    }
}
