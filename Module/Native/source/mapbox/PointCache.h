#ifndef __POINTCACHE_H
#define __POINTCACHE_H

#include "../poly2tri/common/shapes.h"

#include <vector>

class PointCache
{
private:
	std::vector<p2t::Point*>	_pointList;
	int		_free_index;

public:
	__inline PointCache()
	: _free_index(0)
	{

	}

	__inline p2t::Point* getFree()
	{
		if( _free_index >= _pointList.size())
		{
			p2t::Point* newPt = new p2t::Point();
			_pointList.push_back( newPt);
			_free_index ++;
			return newPt;
		}
		else
		{
			p2t::Point* pt = _pointList[ _free_index];
			pt->edge_list.clear();
			_free_index ++;
			return pt;
		}
	}

	__inline void reset()
	{
		_free_index = 0;
	}

	__inline int getAllocSize()
	{
		return (int)_pointList.size();
	}

	__inline int getUsedSize()
	{
		return (int)_free_index;
	}
};

#endif