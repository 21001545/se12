#ifndef _MAPBOXMESHBUILDER_H
#define _MAPBOXMESHBUILDER_H

#include <map>
#include <set>

#include "clipper.hpp"
#include "PointCache.h"
#include "../clipper/clipper.hpp"
#include "../poly2tri/poly2tri.h"
#include "MapBoxVertex.h"
#include "MapBoxEdge.h"
#include "ObjectCache.h"

class MapBoxMeshBuilder
{
private:
    typedef std::map<DSClipperLib::PolyNode*, p2t::CDT*>	CDT_MAP;
    typedef std::vector<p2t::Point*>	POINT_LIST;
    typedef unsigned long    KEY_TYPE;

    PointCache* _pointCache;
    ObjectCache<MapBoxVertex>*    _vertexCache;
    ObjectCache<MapBoxEdge>*    _edgeCache;

    DSClipperLib::Paths     _inputPaths[3];

    std::vector<MapBoxVertex*>    _point_list;
    std::vector<MapBoxEdge*>    _edge_list;
    std::vector<int>        _index_list;
    
    std::map<KEY_TYPE, MapBoxEdge*> _edge_map;

    int _extrudeHeight;

// ������
private:
    p2t::CDT* _lastCDT;
    DSClipperLib::PolyTree* _lastPolyTree;

public:
    MapBoxMeshBuilder(PointCache* pointCache,ObjectCache<MapBoxVertex>* vertexCache,ObjectCache<MapBoxEdge>* edgeCache);
    ~MapBoxMeshBuilder();

    inline int beginPath(int slot)
    {
        int id = (int)_inputPaths[slot].size();
        _inputPaths[slot].push_back(DSClipperLib::Path());
        return id;
    }

    inline void addPathPoint(int slot,int path_id, int x, int y)
    {
        DSClipperLib::Path& path = _inputPaths[slot][path_id];

        path.push_back(DSClipperLib::IntPoint(x, y));
    }

    inline void setExtrudeHeight(int height)
    {
        _extrudeHeight = height;
    }

    void reset();
    int buildMesh();

    inline int getVertexCount() { return (int)_point_list.size(); }
    inline int getIndexCount() { return (int)_index_list.size(); }

    inline int getVertexX(int index)
    {
        return _point_list[index]->x;
    }
    inline int getVertexY(int index)
    {
        return _point_list[index]->y;
    }
    inline int getVertexZ(int index)
    {
        return _point_list[index]->z;
    }
    inline int getNormalX(int index)
    {
        return _point_list[index]->nx;
    }
    inline int getNormalY(int index)
    {
        return _point_list[index]->ny;
    }
    inline int getNormalZ(int index)
    {
        return _point_list[index]->nz;
    }

    inline int getIndex(int index)
    {
        return _index_list[index];
    }


private:
    void buildMultiSlot();

    void buildMeshFromPolyTree(DSClipperLib::PolyTree& polyTree);
    //void createPointList(DSClipperLib::Path& path, POINT_LIST& point_list);
    void createPointList(DSClipperLib::PolyNode* node, POINT_LIST& point_list,double hole_noise);
    void addTriangles(p2t::CDT* cdt);
    void addTriangle(p2t::Triangle* triangle);
    int addPoint(p2t::Point* pt);
    void addEdge(int p0, int p1);

    bool isClockwise(DSClipperLib::Path& path);
    void extrude();
    int findVertex(MapBoxVertex* v);

    inline KEY_TYPE makeEdgeKey(int p0,int p1)
    {
        return ((KEY_TYPE)p0 << 20) + p1;
    }

    void outputDebugInfo();



};

#endif