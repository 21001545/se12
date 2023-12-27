using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{

    public class CollisionTest
    {
        public static float findAxisLeastPenetration(ref int faceIndex, PolygonShape A, PolygonShape B)
        {
            float bestDistance = float.MinValue;
            int bestIndex = -1;

            for (int i = 0; i < A._vertices.Count; ++i)
            {
                // Retrieve a face normal from A
                Vector2 n = A._normals[i];
                //Vector2 nw = n;

                //// Transform face normal into B's model space
                //Mat2 buT = B->u.Transpose();
                //n = buT * nw;

                // Retrieve support point from B along -n
                Vector2 s = B.getSupport(-n);

                // Retrieve vertex on face from A, transform into
                // B's model space
                Vector2 v = A._vertices[i];
                //v = A->u * v + body_a->position;
                //v -= body_b->position;
                //v = buT * v;

                // Compute penetration distance (in B's model space)
                float d = Vector2.Dot(n, s - v);

                // Store greatest distance
                if (d > bestDistance)
                {
                    bestDistance = d;
                    bestIndex = i;
                }
            }

            faceIndex = bestIndex;
            return bestDistance;
        }

        public static bool polygonToPolygon(PolygonShape a,PolygonShape b)
	    {
            int faceIndex = 0;
            float penetrationA = findAxisLeastPenetration(ref faceIndex, a, b);
            if( penetrationA >= 0)
		    {
                return false;
		    }

            float penetrationB = findAxisLeastPenetration(ref faceIndex, b, a);
            if( penetrationB >= 0)
		    {
                return false;
		    }

            return true;
        }
    }


}
