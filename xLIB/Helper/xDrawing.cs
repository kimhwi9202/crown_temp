using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace xLIB
{
    public class xDrawing
    {
        //****************************************************************************************************
        //  static function DrawLine(rect : Rect) : void
        //  static function DrawLine(rect : Rect, color : Color) : void
        //  static function DrawLine(rect : Rect, width : float) : void
        //  static function DrawLine(rect : Rect, color : Color, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
        //  
        //  Draws a GUI line on the screen.
        //  
        //  DrawLine makes up for the severe lack of 2D line rendering in the Unity runtime GUI system.
        //  This function works by drawing a 1x1 texture filled with a color, which is then scaled
        //   and rotated by altering the GUI matrix.  The matrix is restored afterwards.
        //****************************************************************************************************

        private static Texture2D _GUILineTex;

        private static Material _GLLineMaterial;

        public static void GUILine(Rect rect) { GUILine(rect, GUI.contentColor, 1.0f); }
        public static void GUILine(Rect rect, Color color) { GUILine(rect, color, 1.0f); }
        public static void GUILine(Rect rect, float width) { GUILine(rect, GUI.contentColor, width); }
        public static void GUILine(Rect rect, Color color, float width) { GUILine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
        public static void GUILine(Vector2 pointA, Vector2 pointB) { GUILine(pointA, pointB, GUI.contentColor, 1.0f); }
        public static void GUILine(Vector2 pointA, Vector2 pointB, Color color) { GUILine(pointA, pointB, color, 1.0f); }
        public static void GUILine(Vector2 pointA, Vector2 pointB, float width) { GUILine(pointA, pointB, GUI.contentColor, width); }
        public static void GUILine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            // Save the current GUI matrix, since we're going to make changes to it.
            Matrix4x4 matrix = GUI.matrix;

            // Generate a single pixel texture if it doesn't exist
            if (!_GUILineTex) { _GUILineTex = new Texture2D(1, 1); }

            // Store current GUI color, so we can switch it back later,
            // and set the GUI color to the color parameter
            Color savedColor = GUI.color;
            GUI.color = color;

            // Determine the angle of the line.
            float angle = Vector3.Angle(pointB - pointA, Vector2.right);

            // Vector3.Angle always returns a positive number.
            // If pointB is above pointA, then angle needs to be negative.
            if (pointA.y > pointB.y) { angle = -angle; }

            // Use ScaleAroundPivot to adjust the size of the line.
            // We could do this when we draw the texture, but by scaling it here we can use
            //  non-integer values for the width and length (such as sub 1 pixel widths).
            // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
            //  is centered on the origin at pointA.
            GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));

            // Set the rotation for the line.
            //  The angle was calculated with pointA as the origin.
            GUIUtility.RotateAroundPivot(angle, pointA);

            // Finally, draw the actual line.
            // We're really only drawing a 1x1 texture from pointA.
            // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
            //  render with the proper width, length, and angle.
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), _GUILineTex);

            // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
            GUI.matrix = matrix;
            GUI.color = savedColor;
        }

        public static void DrawCurves(Rect wr, Rect wr2)
        {
#if UNITY_EDITOR
            Color color = new Color(0.4f, 0.4f, 0.5f);
            Vector3 startPos = new Vector3(wr.x + wr.width, wr.y + 3 + wr.height / 2, 0);
            Vector3 endPos = new Vector3(wr2.x, wr2.y + wr2.height / 2, 0);
            Vector3 startTangent = startPos + Vector3.right * 50.0f;
            Vector3 endTangent = endPos - Vector3.left * 50.0f;
            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, _GUILineTex, 5f);
#endif
        }


        public static void GLLineRender(Material mat, Vector3 s, Vector3 e)
        {
            if (!mat)
            {
                Debug.LogError("Please Assign a material on the inspector");
                return;
            }
            mat.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            GL.Color(Color.green);

            GL.Vertex(new Vector3(s.x / Screen.width, s.y / Screen.height, 0));
            GL.Vertex(new Vector3(e.x / Screen.width, e.y / Screen.height, 0));
            //GL.Vertex(new Vector3(e.x, e.y, 0));


            GL.End();
            GL.PopMatrix();
        }

        static void _CreateGLLineMatrial()
        {
            if (!_GLLineMaterial)
            {
                /*
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                                            "SubShader { Pass { " +
                                            "    Blend SrcAlpha OneMinusSrcAlpha " +
                                            "    ZWrite Off Cull Off Fog { Mode Off } " +
                                            "    BindChannels {" +
                                            "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                            "} } }");
                */
                _GLLineMaterial = new Material(Shader.Find("UI/Default"));
                _GLLineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _GLLineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }


        public static void GL2DLine(Camera cam, Vector3 p1, Vector3 p2, Color color)
        {
            if (!cam) return;
            Vector3 s = cam.WorldToScreenPoint(p1);
            Vector3 e = cam.WorldToScreenPoint(p2);
            GL2DLine(new Vector3(s.x / Screen.width, s.y / Screen.height, 0), new Vector3(e.x / Screen.width, e.y / Screen.height, 0), color);
        }

        public static void GL2DLine(Vector3 p1, Vector3 p2, Color color)
        {
            _CreateGLLineMatrial();

            _GLLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(p1);
            GL.Vertex(p2);
            GL.End();
            GL.PopMatrix();
        }

        public static void GL3DLine(Vector3 p1, Vector3 p2, Color color)
        {
            _CreateGLLineMatrial();

            _GLLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(p1.x, p1.y, p1.z);
            GL.Vertex3(p2.x, p2.y, p2.z);
            GL.End();
            GL.PopMatrix();
        }
    }
}