#ifndef __POLYGON_SHAPE_H
#define __POLYGON_SHAPE_H

#include "../poly2tri/common/shapes.h"

class PolygonShape
{
public:
	std::vector<p2t::Point>	_vertices;
	std::vector<p2t::Point>	_normals;

	void reset();
	void setBox(double hw, double hh);
	void setPolygon(std::vector<p2t::Point>& verts);
	p2t::Point* getSupport(const p2t::Point& dir);
};


#endif