using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public static class CollisionTest
	{
		public delegate bool TestFunction(Collider a, Collider b);

		private static TestFunction[] testFunctionMap = new TestFunction[]{
			circleToCircle, circleToPolygon,
			polygonToCircle, polygonToPolygon
		};

		public static bool test(Collider a, Collider b)
		{
			int index = (int)a.getShapeType() * 2 + (int)b.getShapeType();

			return testFunctionMap[ index](a,b);
		}

		public static bool circleToCircle(Collider a, Collider b)
		{
			CircleShape cA = a.getShape() as CircleShape;
			CircleShape cB = b.getShape() as CircleShape;

			Vector2 diff = a.getPosition() - b.getPosition();
			return diff.magnitude < (cA.getRadius() + cB.getRadius());
		}

		public static bool circleToPolygon(Collider a, Collider b)
		{
			CircleShape cA = a.getShape() as CircleShape;
			PolygonShape pB = b.getShape() as PolygonShape;

			// Transform circle center to Polygon model space
			Vector2 center = a.getPosition();
			center = center - b.getPosition();

			List<Vector2> vertices = pB.getVertices();
			List<Vector2> normals = pB.getNormals();

			// Find edge with minimum penetration
			// Exact concept as using support points in Polygon vs Polygon
			float separation = float.MinValue;
			int faceNormal = 0;
			for(int i = 0; i < vertices.Count; ++i)
			{
				float s = Vector2.Dot(normals[i], center - vertices[i]);

				if (s > cA.getRadius())
					return false;

				if( s > separation)
				{
					separation = s;
					faceNormal = i;
				}
			}

			// Grab face's vertices
			Vector2 v1 = vertices[faceNormal];
			int i2 = ((faceNormal + 1) < vertices.Count) ? faceNormal + 1 : 0;
			Vector2 v2 = vertices[i2];

			// Check to see if center is within polygon
			if ( separation < Mathf.Epsilon)
			{
				return true;
			}

			// Determine which voronoi region of the edge center of circle lies within
			float dot1 = Vector2.Dot(center - v1, v2 - v1);
			float dot2 = Vector2.Dot(center - v2, v1 - v2);
			float penetration = cA.getRadius() - separation;

			// pssobiel situation at fix16's precision
			if ( penetration <= Mathf.Epsilon)
			{
				return false;
			}

			// Closest to v1
			if(dot1 <= 0)
			{
				if( Vector2.Distance( center, v1) > cA.getRadius())
				{
					return false;
				}
			}
			else if( dot2 <= 0)
			{
				if( Vector2.Distance( center, v2) > cA.getRadius())
				{
					return false;
				}
			}
			else
			{
				Vector2 n = normals[faceNormal];
				if( Vector2.Dot(center - v1, n) > cA.getRadius())
				{
					return false;
				}
			}

			return true;
		}

		public static bool polygonToCircle(Collider a, Collider b)
		{
			return circleToPolygon(b, a);
		}

		private static float findAxisLeastPenetration(out int faceIndex,Collider a,Collider b,PolygonShape pA,PolygonShape pB)
		{
			float bestDistance = float.MinValue;
			int bestIndex = -1;

			for(int i = 0; i < pA.getVertices().Count; ++i)
			{
				Vector2 n = pA.getNormals()[i];

				//// Retrieve a face normal from A
				//Vec2 n = A->m_normals[i];
				//Vec2 nw = A->u * n;

				//// Transform face normal into B's model space
				//Mat2 buT = B->u.Transpose();
				//n = buT * nw;


				//// Retrieve support point from B along -n
				Vector2 s = pB.getSupport(-n);

				// Retrieve vertex on face from A, transform into
				// B's model space
				Vector2 v = pA.getVertices()[i];
				v += a.getPosition() - b.getPosition();

				float d = Vector2.Dot(n, s - v);

				if( d > bestDistance)
				{
					bestDistance = d;
					bestIndex = i;
				}
			}

			faceIndex = bestIndex;
			return bestDistance;
		}

		public static bool biasGreaterThan(float a,float b)
		{
			float biasRelative = 0.95f;
			float biasAbsolute = 0.01f;
			return a >= ((b * biasRelative) + (a * biasAbsolute));
		}

		static void findIncidentFace(ref Vector2[] v,Collider a,Collider b,PolygonShape pA,PolygonShape pB,int referenceIndex)
		{
			Vector2 referenceNormal = pA.getNormals()[referenceIndex];

			int incidentFace = 0;
			float minDot = float.MaxValue;
			for(int i = 0; i < pB.getVertices().Count; ++i)
			{
				float dot = Vector2.Dot(referenceNormal, pB.getNormals()[i]);
				if( dot < minDot)
				{
					minDot = dot;
					incidentFace = i;
				}
			}

			v[0] = pB.getVertices()[incidentFace] + b.getPosition();
			incidentFace = (incidentFace + 1) >= pB.getVertices().Count ? 0 : incidentFace + 1;
			v[1] = pB.getVertices()[incidentFace] + b.getPosition();
		}

		static int clip(Vector2 n,float c,ref Vector2[] face)
		{
			int sp = 0;
			Vector2[] vout = new Vector2[2] { face[0],face[1]};

			float d1 = Vector2.Dot(n, face[0]) - c;
			float d2 = Vector2.Dot(n, face[1]) - c;

			if (d1 <= 0) vout[sp++] = face[0];
			if (d2 <= 0) vout[sp++] = face[1];

			if(d1 * d2 < 0)
			{
				float alpha = d1 / (d1 - d2);
				vout[sp] = face[0] + alpha * (face[1] - face[0]);
				++sp;
			}

			face[0] = vout[0];
			face[1] = vout[1];

			return sp;
		}

		public static bool polygonToPolygon(Collider a, Collider b)
		{
			PolygonShape pA = a.getShape() as PolygonShape;
			PolygonShape pB = b.getShape() as PolygonShape;

			int faceA;
			float penetrationA = findAxisLeastPenetration(out faceA, a, b, pA, pB);
			if( penetrationA >= 0)
			{
				return false;
			}

			int faceB;
			float penetrationB = findAxisLeastPenetration(out faceB, b, a, pB, pA);
			if( penetrationB >= 0)
			{
				return false;
			}

			int referenceIndex;
			//bool flip;

			Collider refCollider;
			Collider incCollider;
			PolygonShape refPoly;
			PolygonShape incPoly;

			if(biasGreaterThan(penetrationA, penetrationB))
			{
				refCollider = a;
				incCollider = b;
				refPoly = pA;
				incPoly = pB;
				referenceIndex = faceA;
				//flip = false;
			}
			else
			{
				refCollider = b;
				incCollider = a;
				refPoly = pB;
				incPoly = pA;
				referenceIndex = faceB;
				//flip = true;
			}

			Vector2[] incidentFace = new Vector2[2];
			findIncidentFace(ref incidentFace, refCollider, incCollider, refPoly, incPoly, referenceIndex);

			Vector2 v1 = refPoly.getVertices()[referenceIndex];
			referenceIndex = referenceIndex + 1 == refPoly.getVertices().Count ? 0 : referenceIndex + 1;
			Vector2 v2 = refPoly.getVertices()[referenceIndex];

			v1 = v1 + refCollider.getPosition();
			v2 = v2 + refCollider.getPosition();

			Vector2 sidePlaneNormal = (v2 - v1);
			sidePlaneNormal.Normalize();

			Vector2 refFaceNormal = new Vector2(sidePlaneNormal.y, -sidePlaneNormal.x);

			float refC = Vector2.Dot(refFaceNormal, v1);
			float negSide = -Vector2.Dot(sidePlaneNormal, v1);
			float posSide = Vector2.Dot(sidePlaneNormal, v2);

			if( clip( -sidePlaneNormal, negSide, ref incidentFace) < 2)
			{
				return false;
			}

			if( clip( sidePlaneNormal, posSide, ref incidentFace) < 2)
			{
				return false;
			}

			return true;
		}
	}
}
