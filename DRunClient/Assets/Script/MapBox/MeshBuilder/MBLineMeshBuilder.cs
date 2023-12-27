using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBLineMeshBuilder : MBMeshBuilder
	{
		private static float COS_HALF_SHARP_CORNER = Mathf.Cos(75.0f / 2.0f * (Mathf.PI / 180.0f));
		private static float SHARP_CORNER_OFFSET = 15.0f;
		private static float DEG_PER_TRIANGLE = 20.0f;

		private static int LINE_DISTANCE_BUFFER_BITS = 14;
		private static float LINE_DISTANCE_SCALE = 1.0f / 2.0f;
		private static float MAX_LINE_DISTANCE = Mathf.Pow(2, LINE_DISTANCE_BUFFER_BITS) / LINE_DISTANCE_SCALE;

		private static Vector2Int NullCoordinate = new Vector2Int(Int32.MaxValue, Int32.MaxValue);
		private static Vector2 NullNormal = Vector2.zero;

		private List<Vector2Int> _inputLine;
		//private float _overscaling = 0;
		private int e1, e2, e3;

		public struct TriangleElement
		{
			public int a;
			public int b;
			public int c;

			public TriangleElement(int a,int b,int c)
			{
				this.a = a;
				this.b = b;
				this.c = c;
			}
		}

		public class Distances
		{
			private float clipStart;
			private float clipEnd;
			private float total;

			public Distances(float clipStart,float clipEnd,float total)
			{
				this.clipStart = clipStart;
				this.clipEnd = clipEnd;
				this.total = total;
			}

			public float scaleToMaxLineDistance(float tileDistance)
			{
				float relativeTileDistance = tileDistance / total;
				return (relativeTileDistance * (clipEnd - clipStart) + clipStart) * (MAX_LINE_DISTANCE - 1);
			}
		}

		protected override void init()
		{
			base.init();
			_inputLine = new List<Vector2Int>();
		}

		public override void build(MBFeature feature, Color color, MBLayerRenderData layer, MBStyleExpressionContext ctx)
		{
			reset();

			foreach (short[] path in feature.pathList)
			{
				//int startVertex = _vertexList.Count;
				//int startIndex = _indexList.Count;

				toLine(path, _inputLine, feature.type == MBFeatureType.polygon);
				addGeometry(_inputLine, feature, layer, ctx);

				//int endVertex = _vertexList.Count;
				//int endIndex = _indexList.Count;

				//Debug.Log($"buildLineMesh: src[{path.Length/2}] vertex[{endVertex - startVertex}] index[{endIndex - startIndex}]");
			}

			makeColors(layer.getLayerStyle().evaluateLineColor(ctx));
		}
		
		private void toLine(short[] path,List<Vector2Int> line,bool close_end)
		{
			line.Clear();
			for(int i = 0; i < path.Length/2; ++i)
			{
				line.Add(new Vector2Int(path[i * 2 + 0], path[i * 2 + 1]));
			}

			if( close_end)
			{
				line.Add(new Vector2Int(path[0], path[1]));
			}
		}

		private void addGeometry(List<Vector2Int> line,MBFeature feature,MBLayerRenderData layer, MBStyleExpressionContext ctx)
		{
			MBStyleLayer styleLayer = layer.getLayerStyle();

			int type = feature.type;
			int len = calcLineLength(line);
			int first = calcLineFirst(line, len);

			// Ignore invalid geometry
			if( len < (type == MBFeatureType.polygon ? 3 : 2))
			{
				return;
			}

			Distances lineDistances = null;

			if( feature.has( MBPropertyKey.mapbox_clip_start) &&
				feature.has( MBPropertyKey.mapbox_clip_end))
			{
				float total_length = 0;
				for(int i = first; i < len - 1; ++i)
				{
					total_length += Vector2Int.Distance(line[i], line[i + 1]);
				}

				float clipStart = (float)feature.get(MBPropertyKey.mapbox_clip_start);
				float clipEnd = (float)feature.get(MBPropertyKey.mapbox_clip_end);
				lineDistances = new Distances(clipStart, clipEnd, total_length);
			}

			int joinType = styleLayer.evaluateLineJointType(ctx);
			float miterLimit = joinType == MBStyleDefine.LineJoinType.bevel ? 1.05f : (float)styleLayer.evaluateLineMiterLimit(ctx);
			double sharpCorenrOffset = SHARP_CORNER_OFFSET;
			//const double sharpCornerOffset =
			//	overscaling == 0
			//		? SHARP_CORNER_OFFSET * (float(util::EXTENT) / util::tileSize)
			//		: (overscaling <= 16.0 ? SHARP_CORNER_OFFSET * (float(util::EXTENT) / (util::tileSize * overscaling))
			//							   : 0.0f);

			Vector2Int firstCoordinate = line[first];
			int beginCap = styleLayer.evaluateLineCap(ctx);
			int endCap = type == MBFeatureType.polygon ? MBStyleDefine.LineCapType.butt : beginCap;

			double distance = 0;
			bool startOfLine = true;
			Vector2Int currentCoordinate = NullCoordinate;
			Vector2Int prevCoordinate = NullCoordinate;
			Vector2Int nextCoordinate = NullCoordinate;
			Vector2 prevNormal = NullNormal;
			Vector2 nextNormal = NullNormal;

			e1 = e2 = e3 = -1;

			if( type == MBFeatureType.polygon)
			{
				currentCoordinate = line[len - 2];
				nextNormal = perpDir(firstCoordinate, currentCoordinate);
			}

			int startVertex = _vertexList.Count;
			List<TriangleElement> triangleStore = new List<TriangleElement>();

			for(int i = first; i < len; ++i)
			{
				if( type == MBFeatureType.polygon && i == len - 1)
				{
					// if the line is closed, we treat the last verx like the first
					nextCoordinate = line[first + 1];
				}
				else if( i + 1 < len)
				{
					nextCoordinate = line[i + 1];
				}
				else
				{
					nextCoordinate = NullCoordinate;
				}

				// if two consecutive vertices exist, skip the current one
				if ( nextCoordinate != NullCoordinate && line[i] == nextCoordinate)
				{
					continue;
				}

				if( nextNormal != NullNormal)
				{
					prevNormal = nextNormal;
				}

				if( currentCoordinate != NullCoordinate)
				{
					prevCoordinate = currentCoordinate;
				}

				currentCoordinate = line[i];

				// Calculate the normal towards the next vertex in this line. In case
				// there is no next vertex, pretend that the line is continuing straight,
				// meaning that we are just using the previous normal.
				nextNormal = nextCoordinate != NullCoordinate ? perpDir(nextCoordinate, currentCoordinate) : prevNormal;

				// If we still don't have a previous normal, this is the beginning of a
				// non-closed line, so we're doing a straight "join".
				if ( prevNormal == NullNormal)
				{
					prevNormal = nextNormal;
				}

				// Determine the normal of the join extrusion. It is the angle bisector
				// of the segments between the previous line and the next line.
				// In the case of 180° angles, the prev and next normals cancel each other out:
				// prevNormal + nextNormal = (0, 0), its magnitude is 0, so the unit vector would be
				// undefined. In that case, we're keeping the joinNormal at (0, 0), so that the cosHalfAngle
				// below will also become 0 and miterLength will become Infinity.
				Vector2 joinNormal = prevNormal + nextNormal;
				if( joinNormal != Vector2.zero)
				{
					joinNormal.Normalize();
				}

				/*  joinNormal     prevNormal
				 *             ↖      ↑
				 *                .________. prevVertex
				 *                |
				 * nextNormal  ←  |  currentVertex
				 *                |
				 *     nextVertex !
				 *
				 */

				double cosAngle = prevNormal.x * nextNormal.x + prevNormal.y * nextNormal.y;
				double cosHalfAngle = joinNormal.x * nextNormal.x + joinNormal.y * nextNormal.y;

				// Calculate the length of the miter (the ratio of the miter to the width)
				// as the inverse of cosine of the angle between next and join normals.
				double miterLength = cosHalfAngle != 0 ? 1 / cosHalfAngle : double.PositiveInfinity;

				// Approximate angle from cosine.
				double approxAngle = 2 * System.Math.Sqrt(2 - 2 * cosHalfAngle);
				bool isSharpCorner = cosHalfAngle < COS_HALF_SHARP_CORNER && prevCoordinate != NullCoordinate && nextCoordinate != NullCoordinate;

				if(isSharpCorner && i > first)
				{
					double prevSegmentLength = Vector2Int.Distance(currentCoordinate, prevCoordinate);
					if( prevSegmentLength > 2.0 * sharpCorenrOffset)
					{
						Vector2 diff = currentCoordinate - prevCoordinate;
						diff *= (float)(sharpCorenrOffset / prevSegmentLength);
						Vector2Int newPrevVertex = new Vector2Int( currentCoordinate.x + Mathf.RoundToInt(diff.x), currentCoordinate.y + Mathf.RoundToInt(diff.y));
						distance += Vector2.Distance(newPrevVertex, prevCoordinate);

						addCurrentVertex(newPrevVertex, ref distance, prevNormal, 0, 0, false, startVertex, triangleStore, lineDistances);
						prevCoordinate = newPrevVertex;
					}
				}

				// The join if a middle vertex, otherwise the cap
				bool middleVertex = prevCoordinate != NullCoordinate && nextCoordinate != NullCoordinate;
				int currentJoin = joinType;
				int currentCap = nextCoordinate != NullCoordinate ? beginCap : endCap;

				if( middleVertex)
				{
					if( currentJoin == MBStyleDefine.LineJoinType.round)
					{
						if( miterLength < styleLayer.getLineRoundLimit(ctx))
						{
							currentJoin = MBStyleDefine.LineJoinType.miter;
						}
						else if( miterLength <= 2)
						{
							currentJoin = MBStyleDefine.LineJoinType.fake_round;
						}
					}

					if( currentJoin == MBStyleDefine.LineJoinType.miter && miterLength > miterLimit)
					{
						currentJoin = MBStyleDefine.LineJoinType.bevel;
					}

					if( currentJoin == MBStyleDefine.LineJoinType.bevel)
					{
						// The maximum extrude length is 128 / 63 = 2 times the width of the line
						// so if miterLength >= 2 we need to draw a different type of bevel here.
						if( miterLength > 2)
						{
							currentJoin = MBStyleDefine.LineJoinType.flip_bevel;
						}

						// If the miterLength is really small and the line bevel wouldn't be visible,
						// just draw a miter join to save a triangle.
						if(miterLength < miterLimit)
						{
							currentJoin = MBStyleDefine.LineJoinType.miter;
						}
					}
				}

				if(prevCoordinate != NullCoordinate)
				{
					distance += Vector2.Distance(currentCoordinate, prevCoordinate);
				}

				if(middleVertex && currentJoin == MBStyleDefine.LineJoinType.miter)
				{
					joinNormal = joinNormal * (float)miterLength;
					addCurrentVertex(currentCoordinate, ref distance, joinNormal, 0, 0, false, startVertex,
									 triangleStore, lineDistances);
				}
				else if( middleVertex && currentJoin == MBStyleDefine.LineJoinType.flip_bevel)
				{
					// miter is too big, flip the direction to make a beveled join
					
					if( miterLength > 100)
					{
						// Almost parallel lines
						joinNormal = nextNormal * -1.0f;
					}
					else
					{
						double direction = (prevNormal.x * nextNormal.y - prevNormal.y * nextNormal.x) > 0 ? -1 : 1;
						double bevelLength = miterLength * (prevNormal + nextNormal).magnitude / (prevNormal - nextNormal).magnitude;

						joinNormal = Vector2.Perpendicular(joinNormal) * (float)(bevelLength * direction);
					}

					addCurrentVertex(currentCoordinate, ref distance, joinNormal, 0, 0, false, startVertex,
									 triangleStore, lineDistances);

					addCurrentVertex(currentCoordinate, ref distance, joinNormal * -1.0f, 0, 0, false, startVertex,
									 triangleStore, lineDistances);
				}
				else if( middleVertex && (currentJoin == MBStyleDefine.LineJoinType.bevel || currentJoin == MBStyleDefine.LineJoinType.fake_round))
				{
					bool lineTurnsLeft = (prevNormal.x * nextNormal.y - prevNormal.y * nextNormal.x) > 0;
					float offset = -(float)System.Math.Sqrt(miterLength * miterLength - 1);
					float offsetA;
					float offsetB;

					if(lineTurnsLeft)
					{
						offsetB = 0;
						offsetA = offset;
					}
					else
					{
						offsetA = 0;
						offsetB = offset;
					}

					// Close previous segement with bevel
					if(!startOfLine)
					{
						addCurrentVertex(currentCoordinate, ref distance, prevNormal, offsetA, offsetB, false,
										 startVertex, triangleStore, lineDistances);
					}

					if(currentJoin == MBStyleDefine.LineJoinType.fake_round)
					{
						// The join angle is sharp enough that a round join would be visible.
						// Bevel joins fill the gap between segments with a single pie slice triangle.
						// Create a round join by adding multiple pie slices. The join isn't actually round, but
						// it looks like it is at the sizes we render lines at.

						// Pick the number of triangles for approximating round join by based on the angle between normals.

						int n = Mathf.RoundToInt( (float)(approxAngle * 180 / Mathf.PI) / DEG_PER_TRIANGLE);
					
						for(int m = 1; m < n; ++m)
						{
							double t = (double)m / (double)n;
							if( t != 0.5)
							{
								// approximate spherical interpolation https://observablehq.com/@mourner/approximating-geometric-slerp
								double t2 = t - 0.5;
								double A = 1.0904 + cosAngle * (-3.2452 + cosAngle * (3.55645 - cosAngle * 1.43519));
								double B = 0.848013 + cosAngle * (-1.06021 + cosAngle * 0.215638);
								t = t + t * t2 * (t - 1) * (A * t2 * t2 + B);
							}

							Vector2 approxFractionalNormal = (prevNormal * (1.0f - (float)t) + nextNormal * (float)t).normalized;
							addPieSliceVertex(currentCoordinate, ref distance, approxFractionalNormal, lineTurnsLeft, startVertex, triangleStore, lineDistances);
						}
					}

					// Start next segment
					if (nextCoordinate != NullCoordinate)
					{
						addCurrentVertex(currentCoordinate, ref distance, nextNormal, -offsetA, -offsetB,
										 false, startVertex, triangleStore, lineDistances);

					}
				}
				else if( !middleVertex && currentCap == MBStyleDefine.LineCapType.butt)
				{
					if(!startOfLine)
					{
						// Close previous segment with a butt
						addCurrentVertex(currentCoordinate, ref distance, prevNormal, 0, 0, false,
										 startVertex, triangleStore, lineDistances);

					}

					// Start next segment with a butt
					if(nextCoordinate != NullCoordinate)
					{
						addCurrentVertex(currentCoordinate, ref distance, nextNormal, 0, 0, false,
										 startVertex, triangleStore, lineDistances);

					}
				}
				else if( !middleVertex && currentCap == MBStyleDefine.LineCapType.square)
				{
					if(!startOfLine)
					{
						// Close previous segment with a square cap
						addCurrentVertex(currentCoordinate, ref distance, prevNormal, 1, 1, false,
										 startVertex, triangleStore, lineDistances);

						// The segment is done. Unset vertices to disconnect segments.
						e1 = e2 = -1;
					}

					// Start next segment
					if (nextCoordinate != NullCoordinate)
					{
						addCurrentVertex(currentCoordinate, ref distance, nextNormal, -1, -1, false,
										 startVertex, triangleStore, lineDistances);
					}
				}
				else if( middleVertex ? currentJoin == MBStyleDefine.LineJoinType.round : currentCap == MBStyleDefine.LineCapType.round)
				{
					if(!startOfLine)
					{
						// Close previous segment with a butt
						addCurrentVertex(currentCoordinate, ref distance, prevNormal, 0, 0, false,
										 startVertex, triangleStore, lineDistances);

						// Add round cap or linejoin at end of segment
						addCurrentVertex(currentCoordinate, ref distance, prevNormal, 1, 1, true, startVertex,
										 triangleStore, lineDistances);

						// The segment is done. Unset vertices to disconnect segments.
						e1 = e2 = -1;
					}

					// Start next segment with a butt
					if (nextCoordinate != NullCoordinate)
					{
						// Add round cap before first segment
						addCurrentVertex(currentCoordinate, ref distance, nextNormal, -1, -1, true,
										 startVertex, triangleStore, lineDistances);

						addCurrentVertex(currentCoordinate, ref distance, nextNormal, 0, 0, false,
										 startVertex, triangleStore, lineDistances);
					}
				}

				if(isSharpCorner && i < len - 1)
				{
					double nextSegmentLength = Vector2Int.Distance(currentCoordinate, nextCoordinate);
					if( nextSegmentLength > 2 * sharpCorenrOffset)
					{
						Vector2 diff = nextCoordinate - currentCoordinate;
						diff *= (float)(sharpCorenrOffset / nextSegmentLength);

						Vector2Int newCurrentVertex = new Vector2Int(currentCoordinate.x + Mathf.RoundToInt(diff.x), currentCoordinate.y + Mathf.RoundToInt(diff.y));
						distance += Vector2.Distance(newCurrentVertex, currentCoordinate);
						addCurrentVertex(newCurrentVertex, ref distance, nextNormal, 0, 0, false, startVertex, triangleStore, lineDistances);
						currentCoordinate = newCurrentVertex;
					}
				}

				startOfLine = false;
			}

			int endVertex = _vertexList.Count;
			int vertexCount = endVertex - startVertex;

			foreach(TriangleElement tri in triangleStore)
			{
				_indexList.Add((ushort)(startVertex + tri.a));
				_indexList.Add((ushort)(startVertex + tri.b));
				_indexList.Add((ushort)(startVertex + tri.c));
			}

			//if (segments.empty() || segments.back().vertexLength + vertexCount > std::numeric_limits < uint16_t >::max())
			//{
			//	segments.emplace_back(startVertex, triangles.elements());
			//}

			//auto & segment = segments.back();
			//assert(segment.vertexLength <= std::numeric_limits < uint16_t >::max());
			//uint16_t index = segment.vertexLength;

			//for (const auto&triangle : triangleStore) {
			//	triangles.emplace_back(index + triangle.a, index + triangle.b, index + triangle.c);
			//}

			//segment.vertexLength += vertexCount;
			//segment.indexLength += triangleStore.size() * 3;
		}

		private void addCurrentVertex(Vector2Int currentCoordinate, ref double distance,Vector2 normal,double endLeft,double endRight,bool round,int startVertex,List<TriangleElement> triangleStore,Distances lineDistances)
		{
			Vector2 extrude = normal;
			double scaledDistance = lineDistances != null ? lineDistances.scaleToMaxLineDistance((float)distance) : distance;

			if(endLeft != 0)
			{
				extrude = extrude - (Vector2.Perpendicular(normal) * (float)endLeft);
			}
			addVertex(currentCoordinate, extrude, round, false, (int)endLeft, (int)(scaledDistance * LINE_DISTANCE_SCALE));
			e3 = _vertexList.Count - 1 - startVertex;
			if( e1 >= 0 && e2 >= 0)
			{
				triangleStore.Add(new TriangleElement(e1, e2, e3));
			}
			e1 = e2;
			e2 = e3;

			extrude = normal * -1.0f;
			if(endRight != 0)
			{
				extrude = extrude - (Vector2.Perpendicular(normal) * (float)endRight);
			}
			addVertex(currentCoordinate, extrude, round, true, (int)-endRight, (int)(scaledDistance * LINE_DISTANCE_SCALE));
			e3 = _vertexList.Count - 1 - startVertex;
			if( e1 >= 0 && e2 >= 0)
			{
				triangleStore.Add(new TriangleElement(e3, e2, e1));	// 한번 뒤집음
			}
			e1 = e2;
			e2 = e3;

			// There is a maximum "distance along the line" that we can store in the buffers.
			// When we get close to the distance, reset it to zero and add the vertex again with
			// a distance of zero. The max distance is determined by the number of bits we allocate
			// to `linesofar`.
			if( distance > MAX_LINE_DISTANCE / 2.0f && lineDistances == null)
			{
				distance = 0;
				addCurrentVertex(currentCoordinate, ref distance, normal, endLeft, endRight, round, startVertex, triangleStore, lineDistances);
			}
		}

		private void addPieSliceVertex(Vector2Int currentVertex,ref double distance,Vector2 extrude,bool lineTurnsLeft,int startVertex,List<TriangleElement> triangleStore,Distances lineDistances)
		{
			Vector2 flippedExtrude = extrude * (lineTurnsLeft ? -1.0f : 1.0f);
			if( lineDistances != null)
			{
				distance = lineDistances.scaleToMaxLineDistance((float)distance);
			}

			addVertex(currentVertex, flippedExtrude, false, lineTurnsLeft, 0, (int)(distance * LINE_DISTANCE_SCALE));
			e3 = _vertexList.Count - 1 - startVertex;
			if( e1 >= 0 && e2 >= 0)
			{
				triangleStore.Add(new TriangleElement(e1, e2, e3));
			}

			if( lineTurnsLeft)
			{
				e2 = e3;
			}
			else
			{
				e1 = e3;
			}
		}

		//static LayoutVertex layoutVertex(Point<int16_t> p, Point<double> e, bool round, bool up, int8_t dir, int32_t linesofar = 0)
		//{
		//	return LayoutVertex {
		//		{
		//			{
		//				static_cast<int16_t>((p.x * 2) | (round ? 1 : 0)),
		//              static_cast<int16_t>((p.y * 2) | (up ? 1 : 0))

		//	}
		//		},
		//          {
		//			{
		//				// add 128 to store a byte in an unsigned byte
		//				static_cast<uint8_t>(::round(extrudeScale * e.x) + 128),
		//              static_cast<uint8_t>(::round(extrudeScale * e.y) + 128),

		//              // Encode the -1/0/1 direction value into the first two bits of .z of a_data.
		//              // Combine it with the lower 6 bits of `linesofar` (shifted by 2 bites to make
		//              // room for the direction value). The upper 8 bits of `linesofar` are placed in
		//              // the `w` component. `linesofar` is scaled down by `LINE_DISTANCE_SCALE` so that
		//              // we can store longer distances while sacrificing precision.

		//              // Encode the -1/0/1 direction value into .zw coordinates of a_data, which is normally covered
		//              // by linesofar, so we need to merge them.
		//              // The z component's first bit, as well as the sign bit is reserved for the direction,
		//              // so we need to shift the linesofar.
		//              static_cast<uint8_t>(((dir == 0 ? 0 : (dir < 0 ? -1 : 1)) + 1) | ((linesofar & 0x3F) << 2)),
		//              static_cast<uint8_t>(linesofar >> 6)

		//	}
		//		}
		//	};
		//}


		/*


			// the distance over which the line edge fades out.
			// Retina devices need a smaller distance to avoid aliasing.
			float ANTIALIASING = 1.0 / u_device_pixel_ratio / 2.0;

			vec2 a_extrude = a_data.xy - 128.0;
			float a_direction = mod(a_data.z, 4.0) - 1.0;
			vec2 pos = floor(a_pos_normal * 0.5);

			// x is 1 if it's a round cap, 0 otherwise
			// y is 1 if the normal points up, and -1 if it points down
			// We store these in the least significant bit of a_pos_normal
			mediump vec2 normal = a_pos_normal - 2.0 * pos;
			normal.y = normal.y * 2.0 - 1.0;
			v_normal = normal;

			// these transformations used to be applied in the JS and native code bases.
			// moved them into the shader for clarity and simplicity.
			gapwidth = gapwidth / 2.0;
			float halfwidth = width / 2.0;
			offset = -1.0 * offset;

			float inset = gapwidth + (gapwidth > 0.0 ? ANTIALIASING : 0.0);
			float outset = gapwidth + halfwidth * (gapwidth > 0.0 ? 2.0 : 1.0) + (halfwidth == 0.0 ? 0.0 : ANTIALIASING);

			// Scale the extrusion vector down to a normal and then up by the line width
			// of this vertex.
			mediump vec2 dist = outset * a_extrude * EXTRUDE_SCALE;

			// Calculate the offset when drawing a line that is to the side of the actual line.
			// We do this by creating a vector that points towards the extrude, but rotate
			// it when we're drawing round end points (a_direction = -1 or 1) since their
			// extrude vector points in another direction.
			mediump float u = 0.5 * a_direction;
			mediump float t = 1.0 - abs(u);
			mediump vec2 offset2 = offset * a_extrude * EXTRUDE_SCALE * normal.y * mat2(t, -u, u, t);

			vec4 projected_extrude = u_matrix * vec4(dist * u_pixels_to_tile_units, 0.0, 0.0);
			gl_Position = u_matrix * vec4(pos + offset2 * u_pixels_to_tile_units, 0.0, 1.0) + projected_extrude;


		 */

		private void addVertex(Vector2Int p,Vector2 e,bool round,bool up,int dir,int linesofar = 0)
		{
			Vector3 pos = new Vector3(p.x, -p.y, 0);
			pos.x /= 4096.0f;
			pos.y /= 4096.0f;

			Vector3 normal;
			//normal.x = e.x * 2 + (round ? 1 : 0);
			//normal.y = e.y * 2 + (up ? 1 : 0);
			normal.x = e.x;
			normal.y = -e.y;
			normal.z = 0;

			Vector2 uv;
			uv.x = (((dir == 0 ? 0 : (dir < 0 ? -1 : 1)) + 1) | ((linesofar & 0x3F) << 2));
			uv.y = (linesofar >> 6);

			_vertexList.Add(pos);
			_normalList.Add(normal);
			_uvList.Add(uv);
		}

		// If the line has duplicate vertices at the end, adjust length to remove them.
		private int calcLineLength(List<Vector2Int> line)
		{
			int len = line.Count;

			while(len >= 2 && line[ len - 1] == line[ len - 2])
			{
				len--;
			}

			return len;
		}

		// If the line has duplicate vertices at the start, adjust index to remove them.
		private int calcLineFirst(List<Vector2Int> line,int len)
		{
			int i = 0;
			while( i < len - 1 && line[ i] == line[ i + 1])
			{
				i++;
			}

			return i;
		}

		private Vector2 perpDir(Vector2Int p0,Vector2Int p1)
		{
			Vector2 diff = new Vector2(p0.x - p1.x, p0.y - p1.y);
			diff.Normalize();
			return Vector2.Perpendicular(diff);
		}
	}
}
