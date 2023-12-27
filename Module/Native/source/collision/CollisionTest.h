#ifndef __COLLISION_TEST_H
#define __COLLISION_TEST_H

#include "PolygonShape.h"

class CollisionTest
{
public:
	static float findAxisLeastPenetration(int* faceIndex, PolygonShape& A, PolygonShape& B);
	static bool polygonToPolygon(PolygonShape& a, PolygonShape& b);

};

#endif