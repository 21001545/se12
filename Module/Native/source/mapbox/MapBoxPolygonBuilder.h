#ifndef _MAPBOXPOLYGONBUILDER_H
#define _MAPBOXPOLYGONBUILDER_H

#include <map>
#include <set>
#include <cmath>

#include "PointCache.h"
#include "../clipper/clipper.hpp"
#include "MapBoxVertex.h"
#include "MapBoxEdge.h"
#include "ObjectCache.h"

class MapBoxPolygonBuilder
{
public:
    typedef DSClipperLib::IntPoint Point;
    typedef std::vector<Point> Ring;
    typedef std::vector<Ring> Polygon;
    typedef std::vector<Polygon> PolygonList;

private:
    Polygon _inputPolygon;

    ObjectCache<MapBoxVertex>* _vertexCache;
    std::vector<MapBoxVertex*>   _pointList;
    std::vector<uint32_t>        _indexList;
    int _extrudeHeight;

public:
    MapBoxPolygonBuilder(ObjectCache<MapBoxVertex>* vertexCache);
    ~MapBoxPolygonBuilder();

    void reset();
    void addRing(short* pointArray, int pointCount);
    int build(int extrudeHeight);

    inline int getVertexCount() { return (int)_pointList.size(); }
    inline int getIndexCount() { return (int)_indexList.size(); }

    inline int getVertexX(int index) {
        return _pointList[index]->x;
    }

    inline int getVertexY(int index) {
        return _pointList[index]->y;
    }

    inline int getVertexZ(int index) {
        return _pointList[index]->z;
    }

    inline int getNormalX(int index) {
        return _pointList[index]->nx;
    }

    inline int getNormalY(int index) {
        return _pointList[index]->ny;
    }

    inline int getNormalZ(int index) {
        return _pointList[index]->nz;
    }

    inline int getIndex(int index) {
        return _indexList[index];
    }

protected:
    void buildInner();
    void buildInnerExtrude();
    PolygonList classifyRings(Polygon& polygon);
    double signedArea(Ring& ring);

    inline MapBoxVertex* allocVertex(Point& pt, int z, int nx, int ny, int nz)
    {
        MapBoxVertex* vert = _vertexCache->pop();
        vert->set(pt.X, pt.Y, z, nx, ny, nz, 0);
        return vert;
    }

    inline Point calcPerp(Point& p1, Point& p2)
    {
        double dx = -(p2.Y - p2.Y);
        double dy = p1.X - p2.X;

        double length = std::sqrt(dx * dx + dy * dy);
        if (length > 0)
        {
            dx /= length;
            dy /= length;
        }

        Point result;
        result.X = (int)(dx * 1000);
        result.Y = (int)(dy * 1000);
        return result;
    }
};

#endif