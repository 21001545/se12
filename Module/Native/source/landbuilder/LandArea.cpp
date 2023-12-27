#include "Precompiled.h"
#include "LandArea.h"

void LandArea::reset()
{
    _edge_set.clear();
    _vertex_list.clear();
    _index_list.clear();
    _area = 0;
}

void LandArea::addTriangle(int p0, int p1, int p2,int area)
{
    addEdge(p0, p1);
    addEdge(p1, p2);
    addEdge(p2, p0);

    int p[3];
    p[0] = addPoint(p0);
    p[1] = addPoint(p1);
    p[2] = addPoint(p2);

    _index_list.push_back(p[0]);
    _index_list.push_back(p[1]);
    _index_list.push_back(p[2]);

    _area += area;
}

void LandArea::addEdge(int p0, int p1)
{
    KEY_TYPE key = makeEdgeKey(p0, p1);

    if (_edge_set.find(key) == _edge_set.end())
    {
        _edge_set.insert(key);
    }
}

int LandArea::addPoint(int p0)
{
    for (int i = 0; i < _vertex_list.size(); ++i)
    {
        if (_vertex_list[i] == p0)
        {
            return i;
        }
    }

    _vertex_list.push_back(p0);
    return (int)_vertex_list.size() - 1;
}


bool LandArea::isEdgeShared(int p0, int p1, int p2)
{
    KEY_TYPE edge0 = makeEdgeKey(p0, p1);
    KEY_TYPE edge1 = makeEdgeKey(p1, p2);
    KEY_TYPE edge2 = makeEdgeKey(p2, p0);

    if (_edge_set.find(edge0) != _edge_set.end())
    {
        return true;
    }

    if (_edge_set.find(edge1) != _edge_set.end())
    {
        return true;
    }

    if (_edge_set.find(edge2) != _edge_set.end())
    {
        return true;
    }

    return false;
}

void LandArea::setBound(DSClipperLib::IntPoint& min, DSClipperLib::IntPoint& max)
{
    _min = min;
    _max = max;
}

//void LandArea::buildBound()
//{
//    DSClipperLib::IntPoint min;
//    DSClipperLib::IntPoint max;
//
//    for (int i = 0; i < _vertex_list.size() / 2; ++i)
//    {
//        int x = _vertex_list[i * 2 + 0];
//        int y = _vertex_list[i * 2 + 1];
//
//        if (i == 0)
//        {
//            min.X = max.X = x;
//            min.Y = max.Y = y;
//        }
//        else
//        {
//            if (x < min.X)
//            {
//                min.X = x;
//            }
//
//            if (x > max.X)
//            {
//                max.X = x;
//            }
//
//            if (y < min.Y)
//            {
//                min.Y = y;
//            }
//
//            if (y > max.Y)
//            {
//                max.Y = y;
//            }
//        }
//    }
//
//    _min = min;
//    _max = max;
//}
