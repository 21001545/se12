using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{

	public class PolygonShape
	{
		public List<Vector2> _vertices = new List<Vector2>();
		public List<Vector2> _normals = new List<Vector2>();

		public void reset()
		{
			_vertices.Clear();
			_normals.Clear();
		}

		public void setBox(float hw,float hh)
		{
			reset();

			_vertices.Add( new Vector2(-hw, -hh));
			_vertices.Add( new Vector2(hw, -hh));
			_vertices.Add( new Vector2(hw, hh));
			_vertices.Add( new Vector2(-hw, hh));
		
			_normals.Add(new Vector2(0.0f,-1.0f));
			_normals.Add(new Vector2(1.0f,0.0f));
			_normals.Add(new Vector2(0.0f,1.0f));
			_normals.Add(new Vector2(-1.0f,0.0f));
		}

		public void setPolygon(List<Vector2> verts)
		{
			reset();

			// Find the right most point on the hull
			int rightMost = 0;
			float highestXCoord = verts[0].x;
			for (int i = 1; i < verts.Count; ++i)
			{
				float x = verts[i].x;
				if (x > highestXCoord)
				{
					highestXCoord = x;
					rightMost = i;
				}

				// If matching x then take farthest negative y
				else if (x == highestXCoord)
					if (verts[i].y < verts[rightMost].y)
						rightMost = i;
			}

			List<int> hull = new List<int>();
		
			int outCount = 0;
			int indexHull = rightMost;

			int vertexCount = 0;

			for (; ; )
			{
				hull.Add(indexHull);

				// Search for next index that wraps around the hull
				// by computing cross products to find the most counter-clockwise
				// vertex in the set, given the previos hull index
				int nextHullIndex = 0;
				for (int i = 1; i < (int)verts.Count; ++i)
				{
					// Skip if same coordinate as we need three unique
					// points in the set to perform a cross product
					if (nextHullIndex == indexHull)
					{
						nextHullIndex = i;
						continue;
					}

					// Cross every set of three unique vertices
					// Record each counter clockwise third vertex and add
					// to the output hull
					// See : http://www.oocities.org/pcgpe/math2d.html
					Vector2 e1 = verts[nextHullIndex] - verts[hull[outCount]];
					Vector2 e2 = verts[i] - verts[hull[outCount]];
					float c = e1.x * e2.y - e1.y * e2.x;
					if (c < 0)
						nextHullIndex = i;

					// Cross product is Fixed64::Zero then e vectors are on same line
					// therefor want to record vertex farthest along that line
					if (c == 0 && e2.sqrMagnitude > e1.sqrMagnitude)
						nextHullIndex = i;
				}

				++outCount;
				indexHull = nextHullIndex;

				// Conclude algorithm upon wrap-around
				if (nextHullIndex == rightMost)
				{
					vertexCount = outCount;
					break;
				}
			}

			// Copy vertices into shape's vertices
			for (int i = 0; i < vertexCount; ++i)
				_vertices.Add(verts[hull[i]]);

			// Compute face normals
			for (int i1 = 0; i1 < vertexCount; ++i1)
			{
				int i2 = i1 + 1 < vertexCount ? i1 + 1 : 0;
				Vector2 face = _vertices[i2] - _vertices[i1];

				//// Ensure no Fixed64::Zero-length edges, because that's bad
				//assert(face.LenSqr() > Fixed64::Sq(Fixed64::Epsilon));

				// Calculate normal with 2D cross product between vector and scalar
				_normals.Add( (new Vector2(face.y, -face.x)).normalized);
			}
		}

		// The extreme point along a direction within a polygon
		public Vector2 getSupport(Vector2 dir )
		{
			float bestProjection = float.MinValue;
			Vector2 bestVertex = Vector2.zero;

			for(int i = 0; i < _vertices.Count; ++i)
			{
				Vector2 v = _vertices[i];
				float projection = Vector2.Dot(v, dir);

				if(projection > bestProjection)
				{
					bestVertex = v;
					bestProjection = projection;
				}
			}

			return bestVertex;
		}
	}


}
