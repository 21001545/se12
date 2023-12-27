#ifndef __LANDBUILDER_H
#define __LANDBUILDER_H

#include <map>
#include <set>

#include "../clipper/clipper.hpp"
#include "../poly2tri/poly2tri.h"
#include "PointCache.h"
#include "MapBoxVertex.h"
#include "MapBoxEdge.h"
#include "ObjectCache.h"
#include "LandArea.h"

class LandBuilder : public ResuableObject<LandBuilder>
{
public:
    LandBuilder();
    ~LandBuilder();

protected:
    typedef std::map<DSClipperLib::PolyNode*, p2t::CDT*>	CDT_MAP;
    typedef std::vector<p2t::Point*>	POINT_LIST;
    typedef unsigned long    KEY_TYPE;

    DSClipperLib::Clipper _clipper;
    DSClipperLib::PolyTree _polyTree;

    DSClipperLib::Path _tempPath;

    PointCache  _pointCache;
    ObjectCache<MapBoxVertex> _vertexCache;
    ObjectCache<MapBoxEdge> _edgeCache;
    ObjectCache<LandArea> _areaCache;

    std::vector<MapBoxVertex*> _point_list;
    std::vector<MapBoxEdge*> _edge_list;
    std::vector<int> _index_list;
    std::vector<int> _triAreaList;
    
    std::map<KEY_TYPE, MapBoxEdge*> _edge_map;    

    std::vector<LandArea*> _area_list;
    std::vector<int> _grid_key_list;
    std::vector<std::vector<int> > _grid_value_list;

public:
    void reset();

    void beginPath();
    void addPathPoint(int x, int y);
    void endPath(int poly_type);

    void build(JNIENV_PARAM_SINGLE);

    __inline int getAreaCount()
    {
        return (int)_area_list.size();
    }

    __inline int getAreaVertexCount(int area_id)
    {
        return _area_list[area_id]->getVertexCount();
    }

    __inline int getAreaIndexCount(int area_id)
    {
        return _area_list[area_id]->getIndexCount();
    }

    __inline int getAreaVertexX(int area_id,int vertex_id)
    {
        int vi = _area_list[area_id]->getVertx(vertex_id);
        return _point_list[vi]->x;
    }

    __inline int getAreaVertexY(int area_id, int vertex_id)
    {
        int vi = _area_list[area_id]->getVertx(vertex_id);
        return _point_list[vi]->y;
    }

    __inline int getAreaGridCount()
    {
        return (int)_grid_key_list.size();
    }

    __inline int getAreaGridKey(int index_id)
    {
        return _grid_key_list[index_id];
    }

    __inline int getAreaGridValueCount(int index_id)
    {
        return (int)_grid_value_list[index_id].size();
    }

    __inline int getAreaGridValue(int index_id, int value_id)
    {
        return (_grid_value_list[index_id])[value_id];
    }

public:
    __inline int getAreaIndex(int area_id, int index_id)
    {
        return _area_list[area_id]->getIndex(index_id);
    }

    __inline int getAreaArea(int area_id)
    {
        return _area_list[area_id]->getArea();
    }


private:
    void addTileBound(DSClipperLib::PolyType polyType);
    void buildMeshFromPolyTree();
    void createPointList(DSClipperLib::PolyNode* node, POINT_LIST& point_list, double hole_noise);
    void addTriangles(p2t::CDT* cdt);
    void addTriangle(p2t::Triangle* triangle);
    int addPoint(p2t::Point* pt);
    void addEdge(int p0, int p1);
    int findVertex(MapBoxVertex* v);

    inline KEY_TYPE makeEdgeKey(int p0, int p1)
    {
        return ((KEY_TYPE)p0 << 20) + p1;
    }

    int calcTriangleArea(p2t::Triangle* triangle);

    void buildArea();
    void buildAreaBound();
    void filterArea();
    void buildAreaGrid(JNIENV_PARAM_SINGLE);
    void buildAreaGrid(JNIENV_PARAM LandArea* area);
    void buildAreaGrid(JNIENV_PARAM LandArea* area, int grid_x, int grid_y);

    void makeGridVertList(int grid_x, int grid_y, std::vector<p2t::Point>& vertList);
    void addAreaToGrid(LandArea* area, int grid_x, int grid_y);
};

#endif
