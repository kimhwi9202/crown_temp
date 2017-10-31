using UnityEngine;
using System.Collections;

namespace xLIB
{
    // http://www.gamedev.net/page/resources/_/technical/math-and-physics/unrav_bezier

    // 3 Basis functions for a cubic bezier spline
    // 2차 (Quadratic) 베지어 곡선..
    static public class Bezier2
    {
        static public float B1(float t) { return (t * t); }             //First Derived function from Bernsteins basis
        static public float B2(float t) { return (2 * t * (1 - t)); }   //Second Derived function
        static public float B3(float t) { return ((1 - t) * (1 - t)); } //Third Derived Function
    }

    // 4 Basis functions for a cubic bezier spline
    // 3차 (Cubic) 베지어 곡선..
    static public class Bezier3
    {
        static public float B1(float t) { return (t * t * t); }
        static public float B2(float t) { return (3 * t * t * (1 - t)); }
        static public float B3(float t) { return (3 * t * (1 - t) * (1 - t)); }
        static public float B4(float t) { return ((1 - t) * (1 - t) * (1 - t)); }
    }

    // 5 Basis functions for a cubic bezier spline
    // 4차 베지어 곡선..
    static public class Bezier4
    {
        static public float B1(float t) { return (t * t * t * t); }
        static public float B2(float t) { return (4 * t * t * t * (1 - t)); }
        static public float B3(float t) { return (6 * t * t * (1 - t) * (1 - t)); }
        static public float B4(float t) { return (4 * t * (1 - t) * (1 - t) * (1 - t)); }
        static public float B5(float t) { return ((1 - t) * (1 - t) * (1 - t) * (1 - t)); }
    }


    public class BezierSplines //: MonoBehaviour
    {
        public enum BType { Bezier_2, Bezier_3, Bezier_4, }
        public BType type = BType.Bezier_2;
        public bool m_bDrawGizmos;
        public Vector3[] m_controlPoint = new Vector3[5];
        public int m_iCurveLineCount = 1; // how many points on the spline
        private ArrayList array_CurvePoint = new ArrayList();
        private float m_fDistance; //how long are we going to travel each loop

        private void Make_BezierCurveSplines2()
        {
            array_CurvePoint.Clear();
            m_fDistance = 1f / m_iCurveLineCount;

            float i = 0;
            Vector3 cp0 = m_controlPoint[0];
            Vector3 cp1 = m_controlPoint[1];
            Vector3 cp2 = m_controlPoint[4];
            do
            {
                float x = cp0.x * Bezier2.B1(i) + cp1.x * Bezier2.B2(i) + cp2.x * Bezier2.B3(i);
                float y = cp0.y * Bezier2.B1(i) + cp1.y * Bezier2.B2(i) + cp2.y * Bezier2.B3(i);
                float z = cp0.z * Bezier2.B1(i) + cp1.z * Bezier2.B2(i) + cp2.z * Bezier2.B3(i);
                array_CurvePoint.Add(new Vector3(x, y, z));
                i = i + m_fDistance; // Recompute Distance
            } while (i <= 1);
            array_CurvePoint.Reverse();
        }

        private void Make_BezierCurveSplines3()
        {
            array_CurvePoint.Clear();
            m_fDistance = 1f / m_iCurveLineCount;

            float i = 0;
            Vector3 cp0 = m_controlPoint[0];
            Vector3 cp1 = m_controlPoint[1];
            Vector3 cp2 = m_controlPoint[2];
            Vector3 cp3 = m_controlPoint[4];
            do
            {
                float x = cp0.x * Bezier3.B1(i) + cp1.x * Bezier3.B2(i) + cp2.x * Bezier3.B3(i) + cp3.x * Bezier3.B4(i);
                float y = cp0.y * Bezier3.B1(i) + cp1.y * Bezier3.B2(i) + cp2.y * Bezier3.B3(i) + cp3.y * Bezier3.B4(i);
                float z = cp0.z * Bezier3.B1(i) + cp1.z * Bezier3.B2(i) + cp2.z * Bezier3.B3(i) + cp3.z * Bezier3.B4(i);
                array_CurvePoint.Add(new Vector3(x, y, z));
                i = i + m_fDistance; // Recompute Distance
            } while (i <= 1);
            array_CurvePoint.Reverse();
        }

        private void Make_BezierCurveSplines4()
        {
            array_CurvePoint.Clear();
            m_fDistance = 1f / m_iCurveLineCount;

            float i = 0;
            Vector3 cp0 = m_controlPoint[0];
            Vector3 cp1 = m_controlPoint[1];
            Vector3 cp2 = m_controlPoint[2];
            Vector3 cp3 = m_controlPoint[3];
            Vector3 cp4 = m_controlPoint[4];
            do
            {
                float x = cp0.x * Bezier4.B1(i) + cp1.x * Bezier4.B2(i) + cp2.x * Bezier4.B3(i) + cp3.x * Bezier4.B4(i) + cp4.x * Bezier4.B5(i);
                float y = cp0.y * Bezier4.B1(i) + cp1.y * Bezier4.B2(i) + cp2.y * Bezier4.B3(i) + cp3.y * Bezier4.B4(i) + cp4.y * Bezier4.B5(i);
                float z = cp0.z * Bezier4.B1(i) + cp1.z * Bezier4.B2(i) + cp2.z * Bezier4.B3(i) + cp3.z * Bezier4.B4(i) + cp4.z * Bezier4.B5(i);
                array_CurvePoint.Add(new Vector3(x, y, z));
                i = i + m_fDistance; // Recompute Distance
            } while (i <= 1);
            array_CurvePoint.Reverse();
        }

        //------- user func --------------
        public void Set_DrawGizmos(bool draw) { m_bDrawGizmos = draw; }
        public void Set_CurveLineCount(int count) { m_iCurveLineCount = count; }
        public void Set_FirstPointPos(Vector3 pos) { m_controlPoint[0] = pos; }
        public void Set_EndPointPos(Vector3 pos) { m_controlPoint[4] = pos; }
        public void Set_ControlPointPos(int index, Vector3 pos) { if (index > 0 && index < 4) m_controlPoint[index] = pos; }
        public Vector3 Get_FirstPointPos() { return m_controlPoint[0]; }
        public Vector3 Get_EndPointPos() { return m_controlPoint[4]; }
        public Vector3 Get_ControlPointPos(int index) { if (index > 0 && index < 4) return m_controlPoint[index]; return Vector3.zero; }
        public int Get_MaxCurvePoints() { return array_CurvePoint.Count; }
        public Vector3 Get_CurvePoints(int n) { Vector3 pos = Vector3.zero; if (n >= 0 && n < array_CurvePoint.Count) pos = (Vector3)array_CurvePoint[n]; return pos; }
        // 모든 세팅수치 설정후에 호출해줘야 라인 포인터가 생성된다.. 	
        public void Make_BezierSplines()
        {
            if (type == BType.Bezier_2) Make_BezierCurveSplines2();
            else if (type == BType.Bezier_3) Make_BezierCurveSplines3();
            else if (type == BType.Bezier_4) Make_BezierCurveSplines4();
        }

        //----------- gizmos -----------------
        private void DrawCross_ControlPoint(int i, float size = 1)
        {
            Vector3 x = new Vector3(m_controlPoint[i].x - 1, m_controlPoint[i].y, m_controlPoint[i].z);
            Vector3 x2 = new Vector3(m_controlPoint[i].x + 1, m_controlPoint[i].y, m_controlPoint[i].z);
            Vector3 y = new Vector3(m_controlPoint[i].x, m_controlPoint[i].y - 1, m_controlPoint[i].z);
            Vector3 y2 = new Vector3(m_controlPoint[i].x, m_controlPoint[i].y + 1, m_controlPoint[i].z);
            Vector3 z = new Vector3(m_controlPoint[i].x, m_controlPoint[i].y, m_controlPoint[i].z - 1);
            Vector3 z2 = new Vector3(m_controlPoint[i].x, m_controlPoint[i].y, m_controlPoint[i].z + 1);
            Gizmos.DrawLine(x, x2);
            Gizmos.DrawLine(y, y2);
            Gizmos.DrawLine(z, z2);
        }

        public void DrawGizmos()
        {
            if (!m_bDrawGizmos) return;

            /*	
                DrawCross_ControlPoint(0);
                DrawCross_ControlPoint(1);
                DrawCross_ControlPoint(4);

                if (type == BType.Bezier_3)	{
                    DrawCross_ControlPoint(2);
                }
                else if (type == BType.Bezier_4)	{
                    DrawCross_ControlPoint(2);
                    DrawCross_ControlPoint(3);
                }
            */
            Gizmos.color = Color.yellow;
            for (int i = 0; i < array_CurvePoint.Count; i++)
            {
                Vector3 cp = (Vector3)array_CurvePoint[i];
                Gizmos.DrawSphere(cp, 1.1f);
            }
            Gizmos.color = Color.white;
        }
    }
}