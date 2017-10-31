using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
    public class UILineRenderer : MaskableGraphic
    {
        private enum SegmentType
        {
            Start,
            Middle,
            End,
        }

        public enum JoinType
        {
            Bevel,
            Miter
        }

        private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

        // A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
        // quad to connect the endpoints. This improves the look of textured and transparent lines, since
        // there is no overlapping.
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

        private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;
        private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0, 1);
        private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
        private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
        private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1, 0);
        private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1, 1);

        private static readonly Vector2[] startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };
        private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
        private static readonly Vector2[] endUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };


        [SerializeField]
        Texture m_Texture;
        [SerializeField]
        Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

        public float LineThickness = 2;
        public bool UseMargins;
        public Vector2 Margin;
        public Vector2[] Points;
        public bool relativeSize;

        public bool LineList = false;
        public bool LineCaps = false;
        public JoinType LineJoins = JoinType.Bevel;

        public override Texture mainTexture
        {
            get
            {
                return m_Texture == null ? s_WhiteTexture : m_Texture;
            }
        }

        /// <summary>
        /// Texture to be used.
        /// </summary>
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                    return;

                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// UV rectangle used by the texture.
        /// </summary>
        public Rect uvRect
        {
            get
            {
                return m_UVRect;
            }
            set
            {
                if (m_UVRect == value)
                    return;
                m_UVRect = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (Points == null)
                return;

            var sizeX = rectTransform.rect.width;
            var sizeY = rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

            // don't want to scale based on the size of the rect, so this is switchable now
            if (!relativeSize)
            {
                sizeX = 1;
                sizeY = 1;
            }

            if (UseMargins)
            {
                sizeX -= Margin.x;
                sizeY -= Margin.y;
                offsetX += Margin.x / 2f;
                offsetY += Margin.y / 2f;
            }

            vh.Clear();

            // Generate the quads that make up the wide line
            var segments = new List<UIVertex[]>();
            if (LineList)
            {
                for (var i = 1; i < Points.Length; i += 2)
                {
                    var start = Points[i - 1];
                    var end = Points[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (LineCaps)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    segments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (LineCaps)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }
            else {
                for (var i = 1; i < Points.Length; i++)
                {
                    var start = Points[i - 1];
                    var end = Points[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (LineCaps && i == 1)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    segments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (LineCaps && i == Points.Length - 1)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }

            // Add the line segments to the vertex helper, creating any joins as needed
            for (var i = 0; i < segments.Count; i++)
            {
                if (!LineList && i < segments.Count - 1)
                {
                    var vec1 = segments[i][1].position - segments[i][2].position;
                    var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                    var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                    // Positive sign means the line is turning in a 'clockwise' direction
                    var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                    // Calculate the miter point
                    var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
                    var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
                    var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;

                    var joinType = LineJoins;
                    if (joinType == JoinType.Miter)
                    {
                        // Make sure we can make a miter join without too many artifacts.
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                            segments[i + 1][1].position = miterPointA;
                        }
                        else {
                            joinType = JoinType.Bevel;
                        }
                    }

                    if (joinType == JoinType.Bevel)
                    {
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                        {
                            if (sign < 0)
                            {
                                segments[i][2].position = miterPointA;
                                segments[i + 1][1].position = miterPointA;
                            }
                            else {
                                segments[i][3].position = miterPointB;
                                segments[i + 1][0].position = miterPointB;
                            }
                        }

                        var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
                        vh.AddUIVertexQuad(join);
                    }
                }
                vh.AddUIVertexQuad(segments[i]);
            }
        }

        private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
        {
            if (type == SegmentType.Start)
            {
                var capStart = start - ((end - start).normalized * LineThickness / 2);
                return CreateLineSegment(capStart, start, SegmentType.Start);
            }
            else if (type == SegmentType.End)
            {
                var capEnd = end + ((end - start).normalized * LineThickness / 2);
                return CreateLineSegment(end, capEnd, SegmentType.End);
            }

            Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
            return null;
        }

        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type)
        {
            List<UIVertex> points = new List<UIVertex>();

            var uvs = middleUvs;
            if (type == SegmentType.Start)
                uvs = startUvs;
            else if (type == SegmentType.End)
                uvs = endUvs;

            Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2;
            var v1 = start - offset;
            var v2 = start + offset;
            var v3 = end + offset;
            var v4 = end - offset;
            return SetVbo(new[] { v1, v2, v3, v4 }, uvs);
        }

        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }
    }
}

/*
// from http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/


public class UILineRenderer : Graphic
{
    [SerializeField]
    Texture m_Texture;
    [SerializeField]
    Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

    public float LineThickness = 2;
    public bool UseMargins;
    public Vector2 Margin;
    public Vector2[] Points;
    public bool relativeSize;

    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    /// <summary>
    /// Texture to be used.
    /// </summary>
    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// UV rectangle used by the texture.
    /// </summary>
    public Rect uvRect
    {
        get
        {
            return m_UVRect;
        }
        set
        {
            if (m_UVRect == value)
                return;
            m_UVRect = value;
            SetVerticesDirty();
        }
    }

    //protected override void OnFillVBO(List<UIVertex> vbo)
    protected override void OnPopulateMesh(Mesh toFill)
    {
        // requires sets of quads
        if (Points == null || Points.Length < 2)
            Points = new[] { new Vector2(0, 0), new Vector2(1, 1) };
        var capSize = 24;
        var sizeX = rectTransform.rect.width;
        var sizeY = rectTransform.rect.height;
        var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
        var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

        // don't want to scale based on the size of the rect, so this is switchable now
        if (!relativeSize)
        {
            sizeX = 1;
            sizeY = 1;
        }
        // build a new set of points taking into account the cap sizes.
        // would be cool to support corners too, but that might be a bit tough :)
        var pointList = new List<Vector2>();
        pointList.Add(Points[0]);
        var capPoint = Points[0] + (Points[1] - Points[0]).normalized * capSize;
        pointList.Add(capPoint);

        // should bail before the last point to add another cap point
        for (int i = 1; i < Points.Length - 1; i++)
        {
            pointList.Add(Points[i]);
        }
        capPoint = Points[Points.Length - 1] - (Points[Points.Length - 1] - Points[Points.Length - 2]).normalized * capSize;
        pointList.Add(capPoint);
        pointList.Add(Points[Points.Length - 1]);

        var TempPoints = pointList.ToArray();
        if (UseMargins)
        {
            sizeX -= Margin.x;
            sizeY -= Margin.y;
            offsetX += Margin.x / 2f;
            offsetY += Margin.y / 2f;
        }

        toFill.Clear();
        var vbo = new VertexHelper(toFill);

        Vector2 prevV1 = Vector2.zero;
        Vector2 prevV2 = Vector2.zero;

        for (int i = 1; i < TempPoints.Length; i++)
        {
            var prev = TempPoints[i - 1];
            var cur = TempPoints[i];
            prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
            cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

            float angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

            var v1 = prev + new Vector2(0, -LineThickness / 2);
            var v2 = prev + new Vector2(0, +LineThickness / 2);
            var v3 = cur + new Vector2(0, +LineThickness / 2);
            var v4 = cur + new Vector2(0, -LineThickness / 2);

            v1 = RotatePointAroundPivot(v1, prev, new Vector3(0, 0, angle));
            v2 = RotatePointAroundPivot(v2, prev, new Vector3(0, 0, angle));
            v3 = RotatePointAroundPivot(v3, cur, new Vector3(0, 0, angle));
            v4 = RotatePointAroundPivot(v4, cur, new Vector3(0, 0, angle));

            Vector2 uvTopLeft = Vector2.zero;
            Vector2 uvBottomLeft = new Vector2(0, 1);

            Vector2 uvTopCenter = new Vector2(0.5f, 0);
            Vector2 uvBottomCenter = new Vector2(0.5f, 1);

            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomRight = new Vector2(1, 1);

            Vector2[] uvs = new[] { uvTopCenter, uvBottomCenter, uvBottomCenter, uvTopCenter };

            if (i > 1)
                SetVbo(vbo, new[] { prevV1, prevV2, v1, v2 }, uvs);

            if (i == 1)
                uvs = new[] { uvTopLeft, uvBottomLeft, uvBottomCenter, uvTopCenter };
            else if (i == TempPoints.Length - 1)
                uvs = new[] { uvTopCenter, uvBottomCenter, uvBottomRight, uvTopRight };

            //SetVbo(vbo, new[] { v1, v2, v3, v4 }, uvs, toFill);
            vbo.AddUIVertexQuad(SetVbo(vbo, new[] { v1, v2, v3, v4 }, uvs));
            vbo.FillMesh(toFill);

            prevV1 = v3;
            prevV2 = v4;
        }
    }

    //protected void SetVbo(UIVertex vbo, Vector2[] vertices, Vector2[] uvs)
    protected UIVertex[] SetVbo(VertexHelper vbo, Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] VboVertices = new UIVertex[4];

        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            VboVertices[i] = vert;
        }

        return VboVertices;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
*/
