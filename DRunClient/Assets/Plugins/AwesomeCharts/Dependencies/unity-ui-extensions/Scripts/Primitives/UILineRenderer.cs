/// Credit jack.sydorenko, firagon
/// Sourced from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
/// Updated/Refactored from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/#post-2528050

using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
    [RequireComponent(typeof(RectTransform))]
    public class UILineRenderer : UIPrimitiveBase
	{
		private enum SegmentType
		{
			Start,
            Middle,
            End,
            Full,
		}

		public enum JoinType
		{
			Bevel,
            Miter
		}

		public enum BezierType
		{
			None,
            Quick,
            Basic,
            Improved,
            Catenary,
        }

		private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

		// A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
		// quad to connect the endpoints. This improves the look of textured and transparent lines, since
		// there is no overlapping.
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

		private static Vector2 UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_TOP_CENTER_LEFT, UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_RIGHT, UV_BOTTOM_RIGHT;
		private static Vector2[] startUvs, middleUvs, endUvs, fullUvs;

        [SerializeField, Tooltip("Points to draw lines between\n Can be improved using the Resolution Option")]
        internal Vector2[] m_points;

        [SerializeField, Tooltip("Thickness of the line")]
        internal float lineThickness = 2;
        [SerializeField, Tooltip("Use the relative bounds of the Rect Transform (0,0 -> 0,1) or screen space coordinates")]
        internal bool relativeSize;
        [SerializeField, Tooltip("Do the points identify a single line or split pairs of lines")]
        internal bool lineList;
        [SerializeField, Tooltip("Add end caps to each line\nMultiple caps when used with Line List")]
        internal bool lineCaps;
        [SerializeField, Tooltip("Add end caps to each line\nMultiple caps when used with Line List")]
        internal int lineCapVertexCount = 1;
        [SerializeField, Tooltip("Resolution of the Bezier curve, different to line Resolution")]
        internal int bezierSegmentsPerCurve = 10;

        public float LineThickness
        {
            get { return lineThickness; }
            set { lineThickness = value; SetAllDirty(); }
        }

        public bool RelativeSize
        {
            get { return relativeSize; }
            set { relativeSize = value; SetAllDirty(); }
        }

        public bool LineList
        {
            get { return lineList; }
            set { lineList = value; SetAllDirty(); }
        }

        public bool LineCaps
        {
            get { return lineCaps; }
            set { lineCaps = value; SetAllDirty(); }
        }

        [Tooltip("The type of Join used between lines, Square/Mitre or Curved/Bevel")]
		public JoinType LineJoins = JoinType.Bevel;

        [Tooltip("Bezier method to apply to line, see docs for options\nCan't be used in conjunction with Resolution as Bezier already changes the resolution")]
        public BezierType BezierMode = BezierType.None;

        public int BezierSegmentsPerCurve
        {
            get { return bezierSegmentsPerCurve; }
            set { bezierSegmentsPerCurve = value; }
        }

        [HideInInspector]
        public bool drivenExternally = false;


		/// <summary>
		/// Points to be drawn in the line.
		/// </summary>
        public Vector2[] Points
		{
			get
			{
				return m_points;
			}

			set
			{
				if (m_points == value)
					return;
				m_points = value;
				SetAllDirty();
			}
		}

        protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (m_points == null)
				return;
            GeneratedUVs();
			Vector2[] pointsToDraw = m_points;
			//If Bezier is desired, pick the implementation
            if (BezierMode != BezierType.None && BezierMode != BezierType.Catenary && m_points.Length > 3)
			{
				BezierPath bezierPath = new BezierPath();

				bezierPath.SetControlPoints(pointsToDraw);
				bezierPath.SegmentsPerCurve = bezierSegmentsPerCurve;
				List<Vector2> drawingPoints;
				switch (BezierMode)
				{
					case BezierType.Basic:
                        drawingPoints = bezierPath.GetDrawingPoints0();
					break;
					case BezierType.Improved:
                        drawingPoints = bezierPath.GetDrawingPoints1();
					break;
					default:
                        drawingPoints = bezierPath.GetDrawingPoints2();
					break;
				}

				pointsToDraw = drawingPoints.ToArray();
			}
            if (BezierMode == BezierType.Catenary && m_points.Length == 2)
            {
                CableCurve cable = new CableCurve(pointsToDraw);
                cable.slack = Resoloution;
                cable.steps = BezierSegmentsPerCurve;
                pointsToDraw = cable.Points();
            }

            if (ImproveResolution != ResolutionMode.None)
            {
                pointsToDraw = IncreaseResolution(pointsToDraw);
            }

            // scale based on the size of the rect or use absolute, this is switchable
            var sizeX = !relativeSize ? 1 : rectTransform.rect.width;
            var sizeY = !relativeSize ? 1 : rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * sizeX;
            var offsetY = -rectTransform.pivot.y * sizeY;

			vh.Clear();

			// Generate the quads that make up the wide line
            var segments = new List<UIVertex[]>();
            var caps = new List<UIVertex[]>();
            if (lineList)
			{
				for (var i = 1; i < pointsToDraw.Length; i += 2)
				{
					var start = pointsToDraw[i - 1];
					var end = pointsToDraw[i];
					start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
					end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

					if (lineCaps)
					{
						//segments.Add(CreateLineCap(start, end, SegmentType.Start));
					}

					//segments.Add(CreateLineSegment(start, end, SegmentType.Full));
					segments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (lineCaps)
					{
						//segments.Add(CreateLineCap(start, end, SegmentType.End));
					}
				}
			}
			else
			{
				for (var i = 1; i < pointsToDraw.Length; i++)
				{
					var start = pointsToDraw[i - 1];
					var end = pointsToDraw[i];
					start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
					end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

					if (lineCaps && i == 1)
					{
                        var list = CreateLineCap(start, end, SegmentType.Start);
                        for(int z = 0; z< list.Length; z+=4)
                        {
                            UIVertex[] v = new UIVertex[4];
                            v[0] = list[z + 0];
                            v[1] = list[z + 1];
                            v[2] = list[z + 2];
                            v[3] = list[z + 3];
                            caps.Add(v);
                        }
					}

					segments.Add(CreateLineSegment(start, end, SegmentType.Middle));
					//segments.Add(CreateLineSegment(start, end, SegmentType.Full));

					if (lineCaps && i == pointsToDraw.Length - 1)
                    {
                        var list = CreateLineCap(start, end, SegmentType.End);
                        for (int z = 0; z < list.Length; z += 4)
                        {
                            UIVertex[] v = new UIVertex[4];
                            v[0] = list[z + 0];
                            v[1] = list[z + 1];
                            v[2] = list[z + 2];
                            v[3] = list[z + 3];
                            caps.Add(v);
                        }
                    }
				}
			}

            // Add the line segments to the vertex helper, creating any joins as needed
            for (var i = 0; i < segments.Count; i++)
            {
                if (segments[i].Length == 4)
                {
                    if (!lineList && i < segments.Count - 1)
                    {
                        var vec1 = segments[i][1].position - segments[i][2].position;
                        var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                        var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                        // Positive sign means the line is turning in a 'clockwise' direction
                        var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                        // Calculate the miter point
                        var miterDistance = lineThickness / (2 * Mathf.Tan(angle / 2));
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
                            else
                            {
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
                                else
                                {
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

            //caps를 그려보자.

            if (caps.Count > 0 )
            {
                for (int i = 0; i < caps.Count; ++i)
                {
                    vh.AddUIVertexQuad(caps[i]);
                }

            }
            if (vh.currentVertCount > 64000)
            {
                Debug.LogError("Max Verticies size is 64000, current mesh vertcies count is [" + vh.currentVertCount + "] - Cannot Draw");
                vh.Clear();
                return;
            }
        }

		private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
		{
			if (type == SegmentType.Start)
            {
				var capStart = start - ((end - start).normalized * lineThickness / 2);
                var diff = (capStart - start);
                var normal = diff.normalized;
				var length = diff.magnitude;
                			
                // 꼭지점을 1개로 하면? 삼각형이 2개인 이등변삼각형.. Normalize 방향을 기준으로 -90~+90도로 해서 만들어보자.
                // 2개면 삼각형이 2개네

                float normalAngle = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(normal, Vector2.up));
                normalAngle = normal.x >= 0.0f ? normalAngle : -normalAngle;

                float minAngle = normalAngle - 90.0f;
                float maxAngle = normalAngle + 90.0f;

				int vertexCount = lineCapVertexCount;

				float angleOffset = 180.0f / (vertexCount+1);
             
                Vector3 origin = start;
                // 삼각형을 quad로..해야한다공..? 
                // origin - v1 - v2 - origin으로 구성하자. 나중에 triangle steam으로 넘기자.
                List<UIVertex> list = new List<UIVertex>();
                for ( int i = 0; i < vertexCount+1; ++i)
                {
                    // v2를 구해봅시다.
                    float a = minAngle + angleOffset * i;
                    float x = Mathf.Sin(Mathf.Deg2Rad * a) * length;
                    float y = Mathf.Cos(Mathf.Deg2Rad * a) * length;
                    var vert = UIVertex.simpleVert;
                    vert.position = origin;
                    vert.uv0 = startUvs[0];
                    vert.color = color;
                    list.Add(vert);
                    Vector2 v1 = new Vector2(start.x + x, start.y + y);

                    vert.position = v1;
                    vert.uv0 = startUvs[1];
                    list.Add(vert);
                    a = minAngle + angleOffset * (i+1);
                    x = Mathf.Sin(Mathf.Deg2Rad * a) * length;
                    y = Mathf.Cos(Mathf.Deg2Rad * a) * length;

                    Vector2 v2 = new Vector2(start.x + x, start.y + y);
                    vert.position = v2;
                    vert.uv0 = startUvs[2];
                    list.Add(vert);
                    vert.position = origin;
                    vert.uv0 = startUvs[3];
                    list.Add(vert);
                }
                return list.ToArray();
			}
			else if (type == SegmentType.End)
			{
				var capEnd = end + ((end - start).normalized * lineThickness / 2);
                var diff = (capEnd - end);
                var normal = diff.normalized;
                var length = diff.magnitude;


                float normalAngle = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(normal, Vector2.up));
                normalAngle = normal.x >= 0.0f ? normalAngle : -normalAngle;

                float minAngle = normalAngle - 90.0f;
                float maxAngle = normalAngle + 90.0f;

                int vertexCount = lineCapVertexCount;

                float angleOffset = 180.0f / (vertexCount + 1);

                Vector3 origin = end;
                // 삼각형을 quad로..해야한다공..? 
                // origin - v1 - v2 - origin으로 구성하자. 나중에 triangle steam으로 넘기자.
                List<UIVertex> list = new List<UIVertex>();
                for (int i = 0; i < vertexCount + 1; ++i)
                {
                    // v2를 구해봅시다.
                    float a = minAngle + angleOffset * i;
                    float x = Mathf.Sin(Mathf.Deg2Rad * a) * length;
                    float y = Mathf.Cos(Mathf.Deg2Rad * a) * length;
                    var vert = UIVertex.simpleVert;
                    vert.position = origin;
                    vert.uv0 = startUvs[0];
                    vert.color = color;
                    list.Add(vert);
                    Vector2 v1 = new Vector2(origin.x + x, origin.y + y);

                    vert.position = v1;
                    vert.uv0 = startUvs[1];
                    list.Add(vert);
                    a = minAngle + angleOffset * (i + 1);
                    x = Mathf.Sin(Mathf.Deg2Rad * a) * length;
                    y = Mathf.Cos(Mathf.Deg2Rad * a) * length;

                    Vector2 v2 = new Vector2(origin.x + x, origin.y + y);
                    vert.position = v2;
                    vert.uv0 = startUvs[2];
                    list.Add(vert);
                    vert.position = origin;
                    vert.uv0 = startUvs[3];
                    list.Add(vert);
                }
                return list.ToArray();

            }

            Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
			return null;
		}

		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type)
		{
			Vector2 offset = new Vector2((start.y - end.y), end.x - start.x).normalized * lineThickness / 2;

			var v1 = start - offset;
			var v2 = start + offset;
			var v3 = end + offset;
			var v4 = end - offset;
            //Return the VDO with the correct uvs
            switch (type)
            {
                case SegmentType.Start:
                    return SetVbo(new[] { v1, v2, v3, v4 }, startUvs);
                case SegmentType.End:
                    return SetVbo(new[] { v1, v2, v3, v4 }, endUvs);
                case SegmentType.Full:
                    return SetVbo(new[] { v1, v2, v3, v4 }, fullUvs);
                default:
                    return SetVbo(new[] { v1, v2, v3, v4 }, middleUvs);
            }
		}

        protected override void GeneratedUVs()
        {
            if (activeSprite != null)
            {
                var outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                var inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                UV_TOP_LEFT = new Vector2(outer.x, outer.y);
                UV_BOTTOM_LEFT = new Vector2(outer.x, outer.w);
                UV_TOP_CENTER_LEFT = new Vector2(inner.x, inner.y);
                UV_TOP_CENTER_RIGHT = new Vector2(inner.z, inner.y);
                UV_BOTTOM_CENTER_LEFT = new Vector2(inner.x, inner.w);
                UV_BOTTOM_CENTER_RIGHT = new Vector2(inner.z, inner.w);
                UV_TOP_RIGHT = new Vector2(outer.z, outer.y);
                UV_BOTTOM_RIGHT = new Vector2(outer.z, outer.w);
            }
            else
            {
                UV_TOP_LEFT = Vector2.zero;
                UV_BOTTOM_LEFT = new Vector2(0, 1);
                UV_TOP_CENTER_LEFT = new Vector2(0.5f, 0);
                UV_TOP_CENTER_RIGHT = new Vector2(0.5f, 0);
                UV_BOTTOM_CENTER_LEFT = new Vector2(0.5f, 1);
                UV_BOTTOM_CENTER_RIGHT = new Vector2(0.5f, 1);
                UV_TOP_RIGHT = new Vector2(1, 0);
                UV_BOTTOM_RIGHT = Vector2.one;
            }


            startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER_LEFT, UV_TOP_CENTER_LEFT };
            middleUvs = new[] { UV_TOP_CENTER_LEFT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_CENTER_RIGHT };
            endUvs = new[] { UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_RIGHT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
            fullUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
        }

        protected override void ResolutionToNativeSize(float distance)
        {
            if (UseNativeSize)
            {
                m_Resolution = distance / (activeSprite.rect.width / pixelsPerUnit);
                lineThickness = activeSprite.rect.height / pixelsPerUnit;
            }
        }
    }
}