#include "Precompiled.h"
#include "CollisionTest.h"

float CollisionTest::findAxisLeastPenetration(int* faceIndex, PolygonShape& A, PolygonShape& B)
{
    float bestDistance = -1000000.0f;
    int bestIndex = -1;

    for (int i = 0; i < A._vertices.size(); ++i)
    {
        // Retrieve a face normal from A
        p2t::Point& n = A._normals[i];
        //Vector2 nw = n;

        //// Transform face normal into B's model space
        //Mat2 buT = B->u.Transpose();
        //n = buT * nw;

        // Retrieve support point from B along -n
        p2t::Point& s = *B.getSupport(-n);

        // Retrieve vertex on face from A, transform into
        // B's model space
        p2t::Point& v = A._vertices[i];
        //v = A->u * v + body_a->position;
        //v -= body_b->position;
        //v = buT * v;

        // Compute penetration distance (in B's model space)
        float d = (float)p2t::Dot(n, s - v);

        // Store greatest distance
        if (d > bestDistance)
        {
            bestDistance = d;
            bestIndex = i;
        }
    }

    *faceIndex = bestIndex;
    return bestDistance;
}

bool CollisionTest::polygonToPolygon(PolygonShape& a, PolygonShape& b)
{
    int faceIndex = 0;
    float penetrationA = findAxisLeastPenetration(&faceIndex, a, b);
    if (penetrationA >= 0)
    {
        return false;
    }

    float penetrationB = findAxisLeastPenetration(&faceIndex, b, a);
    if (penetrationB >= 0)
    {
        return false;
    }

    return true;
}
