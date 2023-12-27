#ifndef _MAPBOXCONTEXT_H
#define _MAPBOXCONTEXT_H

#include "PointCache.h"
#include "MapBoxVertex.h"
#include "MapBoxMeshBuilder.h"
#include "MapBoxLineBoundClipper.h"
#include "MapBoxPolygonBuilder.h"

class MapBoxContext : public ResuableObject<MapBoxContext>
{
public:
    MapBoxContext();
    ~MapBoxContext();

public:
    PointCache              _pointCache;
    ObjectCache<MapBoxVertex>   _vertexCache;
    ObjectCache<MapBoxEdge> _edgeCache;
    MapBoxMeshBuilder       _meshBuilder;
    MapBoxLineBoundClipper  _lineBoundClipper;
    MapBoxPolygonBuilder    _polygonBuilder;
};

#endif