using UnityEngine;
using System.Collections;

namespace xLIB
{
    // Mathematics
    public class xMath: MonoBehaviour
    {
        public static bool IsTouch(Camera _camera, Transform _target, Vector3 _pos, float _maxDistance=10f)
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(_pos);
            if (Physics.Raycast(ray, out hit, _maxDistance))
            {
                if (_target == hit.transform) return true;
            }
            return false;
        }

        // 최대공약수 구하기
        public static int gcd(int p, int q)
        {
            if (q == 0) return p;
            return gcd(q, p % q);
        }

        // 나의 위치와 타겟의 포인터 의 각도 
        public static float Get2DTargetAngle(float _x, float _y, float _tx, float _ty)
        {
            return (Mathf.Atan2(_tx - _x, _y - _ty) * Mathf.Rad2Deg) + 180f;
        }


        public static float Texture2DRotation(GameObject _obj, float _targetPosX, float _targetPosY, float _objAnchorsAngle=0)
        {
            // _x, _y 는 2D화면 좌표값.
            // 현재 메쉬의 지역좌표계 기준으로 월드 좌표를 지역 좌표로 변환해줌
            Vector3 Relative = _obj.transform.InverseTransformPoint(new Vector3(_targetPosX, _targetPosY, 0));
            float angle = Mathf.Atan2(Relative.y, Relative.x) * Mathf.Rad2Deg;
            // 기준각도 0도는 3시방향 즉 90도에 위치한경우다.. 12시방향 0도에 기준이미지일경우는 angle-90 해줘야 한다.
            _obj.transform.Rotate(new Vector3(0, 0, angle- _objAnchorsAngle));

            return angle;
        }
    }
}
