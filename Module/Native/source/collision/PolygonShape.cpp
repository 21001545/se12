#include "Precompiled.h"
#include "PolygonShape.h"

void PolygonShape::reset()
{
	_vertices.clear();
	_normals.clear();
}

void PolygonShape::setBox(double hw, double hh)
{
	reset();

	_vertices.push_back(p2t::Point(-hw, -hh));
	_vertices.push_back(p2t::Point(hw, -hh));
	_vertices.push_back(p2t::Point(hw, hh));
	_vertices.push_back(p2t::Point(-hw, hh));

	_normals.push_back(p2t::Point(0, -1));
	_normals.push_back(p2t::Point(1, 0));
	_normals.push_back(p2t::Point(0, 1));
	_normals.push_back(p2t::Point(-1, 0));
}

void PolygonShape::setPolygon(std::vector<p2t::Point>& verts)
{
	reset();

	// Find the right most point on the hull
	int rightMost = 0;
	float highestXCoord = (float)verts[0].x;
	for (int i = 1; i < verts.size(); ++i)
	{
		float x = (float)verts[i].x;
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

	std::vector<int> hull;

	int outCount = 0;
	int indexHull = rightMost;

	int vertexCount = 0;

	for (; ; )
	{
		hull.push_back(indexHull);

		// Search for next index that wraps around the hull
		// by computing cross products to find the most counter-clockwise
		// vertex in the set, given the previos hull index
		int nextHullIndex = 0;
		for (int i = 1; i < (int)verts.size(); ++i)
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
			p2t::Point e1 = verts[nextHullIndex] - verts[hull[outCount]];
			p2t::Point e2 = verts[i] - verts[hull[outCount]];
			float c = (float)(e1.x * e2.y - e1.y * e2.x);
			if (c < 0)
				nextHullIndex = i;

			// Cross product is Fixed64::Zero then e vectors are on same line
			// therefor want to record vertex farthest along that line
			if (c == 0 && e2.LengthSqr() > e1.LengthSqr())
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
		_vertices.push_back(verts[hull[i]]);

	// Compute face normals
	for (int i1 = 0; i1 < vertexCount; ++i1)
	{
		int i2 = i1 + 1 < vertexCount ? i1 + 1 : 0;
		p2t::Point face = _vertices[i2] - _vertices[i1];

		//// Ensure no Fixed64::Zero-length edges, because that's bad
		//assert(face.LenSqr() > Fixed64::Sq(Fixed64::Epsilon));

		// Calculate normal with 2D cross product between vector and scalar
		p2t::Point normal(face.y, -face.x);
		normal.Normalize();
		
		_normals.push_back(normal);
	}
}

p2t::Point* PolygonShape::getSupport(const p2t::Point& dir)
{
	float bestProjection = -100000.0f;
	p2t::Point* bestVertex = null;

	for (int i = 0; i < _vertices.size(); ++i)
	{
		p2t::Point& pt = _vertices[i];
		float projection = (float)p2t::Dot(pt, dir);

		if (projection > bestProjection)
		{
			bestVertex = &pt;
			bestProjection = projection;
		}
	}

	return bestVertex;
}
