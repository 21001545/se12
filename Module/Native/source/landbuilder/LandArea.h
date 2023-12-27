#ifndef __LANDAREA_H
#define __LANDAREA_H

#include <set>
#include "../clipper/clipper.hpp"

class LandArea : public ResuableObject<LandArea>
{
private:
    typedef unsigned long KEY_TYPE;

    std::set<KEY_TYPE>  _edge_set;

    std::vector<int>    _vertex_list;
    std::vector<int>    _index_list;

    int _area;
    DSClipperLib::IntPoint  _min;
    DSClipperLib::IntPoint  _max;

    int _listIndex;

public:
    void reset();
    void addTriangle(int p0, int p1, int p2,int area);
    bool isEdgeShared(int p0, int p1, int p2);

    __inline int getVertexCount()
    {
        return (int)_vertex_list.size();
    }

    __inline int getIndexCount()
    {
        return (int)_index_list.size();
    }

    __inline int getVertx(int vertex_id)
    {
        return _vertex_list[vertex_id];
    }

    __inline int getIndex(int index_id)
    {
        return _index_list[index_id];
    }

    __inline int getArea()
    {
        return _area;
    }

    __inline DSClipperLib::IntPoint& getBoundMin()
    {
        return _min;
    }

    __inline DSClipperLib::IntPoint& getBoundMax()
    {
        return _max;
    }

    __inline void setListIndex(int index)
    {
        _listIndex = index;
    }

    __inline int getListIndex()
    {
        return _listIndex;
    }

    __inline bool isTouchOutline()
    {
        if (_min.X <= 0 || _max.X >= 4096)
        {
            return true;
        }

        if (_min.Y <= 0 || _max.Y >= 4096)
        {
            return true;
        }

        return false;
    }

    void setBound(DSClipperLib::IntPoint& min, DSClipperLib::IntPoint& max);

private:
    void addEdge(int p0, int p1);
    int addPoint(int p0);

    inline KEY_TYPE makeEdgeKey(int p0, int p1)
    {
        if (p0 < p1)
        {
            return ((KEY_TYPE)p0 << 20) + p1;
        }
        else
        {
            return ((KEY_TYPE)p1 << 20) + p0;
        }
    }

};

#endif