#include "Precompiled.h"
#include "LandBuilder.h"
#include "../collision/PolygonShape.h"
#include "../collision/CollisionTest.h"

LandBuilder::LandBuilder()
    :_vertexCache(2), _edgeCache(2), _areaCache(2)
{

}

LandBuilder::~LandBuilder()
{

}

void LandBuilder::reset()
{
    _clipper.Clear();
    _polyTree.Clear();
    
    _pointCache.reset();

    _index_list.clear();
    for(std::vector<MapBoxVertex*>::iterator it = _point_list.begin(); it != _point_list.end(); ++it)
    {
        _vertexCache.push(*it);
    }

    _point_list.clear();

    for (std::vector<MapBoxEdge*>::iterator it = _edge_list.begin(); it != _edge_list.end(); ++it)
    {
        _edgeCache.push(*it);
    }

    _edge_list.clear();

    for (std::vector<LandArea*>::iterator it = _area_list.begin(); it != _area_list.end(); ++it)
    {
        _areaCache.push(*it);
    }

    _area_list.clear();
    _edge_map.clear();
    _triAreaList.clear();
    _grid_key_list.clear();
    _grid_value_list.clear();
}

void LandBuilder::beginPath()
{
    _tempPath.clear();
}

void LandBuilder::addPathPoint(int x,int y)
{
    _tempPath.push_back(DSClipperLib::IntPoint(x,y));
}

void LandBuilder::endPath(int poly_type)
{
    _clipper.AddPath(_tempPath,(DSClipperLib::PolyType)poly_type, true);
}

void LandBuilder::addTileBound(DSClipperLib::PolyType polyType)
{
    DSClipperLib::Path boundPath;
    boundPath.push_back(DSClipperLib::IntPoint(0, 0));
    boundPath.push_back(DSClipperLib::IntPoint(0, 4096));
    boundPath.push_back(DSClipperLib::IntPoint(4096, 4096));
    boundPath.push_back(DSClipperLib::IntPoint(4096, 0));

    _clipper.AddPath(boundPath, polyType, true);
}

void LandBuilder::build(JNIENV_PARAM_SINGLE)
{
    addTileBound(DSClipperLib::PolyType::ptSubject);

    DSClipperLib::Paths tempPaths;

    _clipper.Execute(DSClipperLib::ClipType::ctDifference, tempPaths,  DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

    DSClipperLib::SimplifyPolygons(tempPaths, DSClipperLib::PolyFillType::pftNonZero);
    DSClipperLib::CleanPolygons(tempPaths);

    ////
    _clipper.Clear();

    addTileBound(DSClipperLib::PolyType::ptSubject);
    _clipper.AddPaths(tempPaths, DSClipperLib::PolyType::ptClip, true);

    _clipper.Execute(DSClipperLib::ClipType::ctIntersection, _polyTree, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

    //// check outbound vertex
    //DSClipperLib::PolyNode* curNode = _polyTree.GetFirst();
    //while (curNode != null)
    //{
    //    for (int i = 0; i < curNode->Contour.size(); ++i)
    //    {
    //        DSClipperLib::IntPoint& pt = curNode->Contour[i];

    //        if (pt.X < 0 || pt.Y < 0 || pt.X > 4096 || pt.Y > 4096)
    //        {
    //            Logger::logInfo(JNIENV_CALL_PARAM "weired vert[(%d,%d)] is_hole[%d]", pt.X, pt.Y, curNode->IsHole() ? 1 : 0);
    //        }
    //    }

    //    curNode = curNode->GetNext();
    //}

    buildMeshFromPolyTree();

    buildArea();
    buildAreaBound();
    buildAreaGrid(JNIENV_CALL_PARAM_SINGLE);
}

void LandBuilder::createPointList(DSClipperLib::PolyNode* node, POINT_LIST& point_list, double hole_noise)
{
    bool isHole = node->IsHole();

    for (int i = 0; i < node->Contour.size(); ++i)
    {
        DSClipperLib::IntPoint& iPt = node->Contour[i];

        p2t::Point* pt = _pointCache.getFree();
        pt->x = iPt.X;
        pt->y = iPt.Y;

        if (isHole)
        {
            pt->x += hole_noise;
            pt->y += hole_noise;
        }

        bool exists = false;
        for (int j = 0; j < point_list.size(); ++j)
        {
            if (pt->x == point_list[j]->x &&
                pt->y == point_list[j]->y)
            {
                exists = true;
                break;
            }
        }

        if (exists)
        {
            continue;
        }

        point_list.push_back(pt);
    }
}

void LandBuilder::addTriangles(p2t::CDT* cdt)
{
    std::vector<p2t::Triangle*>& tri_list = cdt->GetTriangles();
    for (int i = 0; i < tri_list.size(); ++i)
    {
        p2t::Triangle* tri = tri_list[i];
        addTriangle(tri);
    }
}

int LandBuilder::calcTriangleArea(p2t::Triangle* triangle)
{
    p2t::Point* p0 = triangle->GetPoint(0);
    p2t::Point* p1 = triangle->GetPoint(1);
    p2t::Point* p2 = triangle->GetPoint(2);

    double dX0 = p0->x;
    double dX1 = p1->x;
    double dX2 = p2->x;
    double dY0 = p0->y;
    double dY1 = p1->y;
    double dY2 = p2->y;

    double dArea = ((dX1 - dX0) * (dY2 - dY0) - (dX2 - dX0) * (dY1 - dY0)) / 2.0;
    if (dArea > 0.0)
    {
        return (int)dArea;
    }
    else
    {
        return (int)-dArea;
    }
}


void LandBuilder::addTriangle(p2t::Triangle* triangle)
{
    int p[3];

    p[0] = addPoint(triangle->GetPoint(0));
    p[1] = addPoint(triangle->GetPoint(1));
    p[2] = addPoint(triangle->GetPoint(2));

    addEdge(p[0], p[1]);
    addEdge(p[1], p[2]);
    addEdge(p[2], p[0]);

    _index_list.push_back(p[0]);
    _index_list.push_back(p[1]);
    _index_list.push_back(p[2]);

    _triAreaList.push_back(calcTriangleArea(triangle));

/*
    float num = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
    if (num < 1E-15f)
    {
        return 0f;
    }

    float num2 = Mathf.Clamp(Dot(from, to) / num, -1f, 1f);
    return (float)Math.Acos(num2) * 57.29578f;
*/

}

void LandBuilder::addEdge(int p0, int p1)
{
    KEY_TYPE key;

    if (p0 < p1)
    {
        key = makeEdgeKey(p0, p1);
    }
    else
    {
        key = makeEdgeKey(p1, p0);
    }

    std::map<unsigned long, MapBoxEdge*>::iterator it = _edge_map.find(key);

    if (it != _edge_map.end())
    {
        it->second->shared_count += 1;
    }
    else
    {
        MapBoxEdge* edge = _edgeCache.pop();
        edge->p0 = _point_list[p0];
        edge->p1 = _point_list[p1];
        edge->shared_count = 1;

        _edge_list.push_back(edge);
        _edge_map.insert(std::make_pair(key, edge));
    }
}

int LandBuilder::addPoint(p2t::Point* pt)
{
    int x = (int)pt->x;
    int y = (int)pt->y;

    MapBoxVertex* vert = _vertexCache.pop();
    vert->x = x;
    vert->y = y;
    vert->z = 0;
    vert->nx = 0;
    vert->ny = 0;
    vert->nz = 1000;

    vert->index = findVertex(vert);
    if (vert->index == -1)
    {
        vert->index = (int)_point_list.size();
        _point_list.push_back(vert);
    }
    else
    {
        _vertexCache.push(vert);
    }

    return vert->index;

    //KEY_TYPE key = makeVertexKey(x, y, _extrudeHeight);

    //std::map<unsigned __int64, MapBoxVertex*>::iterator it = _point_map.find(key);
    //if (it != _point_map.end())
    //{
    //    return it->second->index;
    //}
    //else
    //{
    //    MapBoxVertex* vert = _vertexCache->pop();
    //    vert->x = x;
    //    vert->y = y;
    //    vert->z = _extrudeHeight;
    //    vert->index = (int)_point_list.size();
    //    _point_list.push_back(vert);
    //    _point_map.insert(std::make_pair(key, vert));

    //    return vert->index;
    //}
}

int LandBuilder::findVertex(MapBoxVertex* v)
{
    for (int i = 0; i < _point_list.size(); ++i)
    {
        if (_point_list[i]->isEqual(v))
        {
            return i;
        }
    }

    return -1;
}

void LandBuilder::buildMeshFromPolyTree()
{
    CDT_MAP cdtMap;

    int index = 0;
    double hole_noise = 0;

    DSClipperLib::PolyNode* curNode = _polyTree.GetFirst();
    while (curNode != null)
    {
        POINT_LIST pt_list;

        if (curNode->IsHole())
        {
            hole_noise += 0.0001f;
        }

        createPointList(curNode, pt_list, hole_noise);

        if (curNode->IsHole())
        {
            typename CDT_MAP::iterator it = cdtMap.find(curNode->Parent);
            if (it != cdtMap.end())
            {
                p2t::CDT* cdt = (*it).second;
                cdt->AddHole(pt_list);
            }
            else
            {

            }
        }
        else
        {
            p2t::CDT* cdt = new p2t::CDT(pt_list);
            cdtMap.insert(std::make_pair(curNode, cdt));
        }

        curNode = curNode->GetNext();
        index++;
    }

    CDT_MAP::iterator it;
    for (it = cdtMap.begin(); it != cdtMap.end(); ++it)
    {
        p2t::CDT* cdt = it->second;
        cdt->Triangulate();
    }

    for (it = cdtMap.begin(); it != cdtMap.end(); ++it)
    {
        addTriangles(it->second);

        delete it->second;
    }
    cdtMap.clear();
}

void LandBuilder::buildArea()
{
    int tri_count = (int)_index_list.size() / 3;
    for (int i = 0; i < tri_count; ++i)
    {
        int p0 = _index_list[i * 3 + 0];
        int p1 = _index_list[i * 3 + 1];
        int p2 = _index_list[i * 3 + 2];

        LandArea* areaShared = NULL;

        for (int j = 0; j < _area_list.size(); ++j)
        {
            LandArea* area = _area_list[j];

            if (area->isEdgeShared(p0, p1, p2))
            {
                areaShared = area;
                break;
            }
        }

        if (areaShared == null)
        {
            areaShared = _areaCache.pop();
            areaShared->reset();

            areaShared->setListIndex((int)_area_list.size());
            _area_list.push_back(areaShared);
        }

        areaShared->addTriangle(p0, p1, p2, _triAreaList[i]);
    }
}

void LandBuilder::buildAreaBound()
{
    for (int i = 0; i < _area_list.size(); ++i)
    {
        LandArea* area = _area_list[i];
        int area_id = area->getListIndex();
        int vertex_count = getAreaVertexCount(area_id);

        DSClipperLib::IntPoint min;
        DSClipperLib::IntPoint max;

        for (int j = 0; j < vertex_count; ++j)
        {
            int x = getAreaVertexX(area_id, j);
            int y = getAreaVertexY(area_id, j);
            
            if (j == 0)
            {
                min.X = max.X = x;
                min.Y = max.Y = y;
            }
            else
            {
                if (x < min.X)
                {
                    min.X = x;
                }
            
                if (x > max.X)
                {
                    max.X = x;
                }
            
                if (y < min.Y)
                {
                    min.Y = y;
                }
            
                if (y > max.Y)
                {
                    max.Y = y;
                }
            }
        }

        area->setBound(min, max);
    }
}

void LandBuilder::filterArea()
{
    std::vector<LandArea*> tempList;
    tempList.swap(_area_list);

    for (int i = 0; i < tempList.size(); ++i)
    {
        LandArea* area = tempList[i];
        if (area->getArea() < 200 && area->isTouchOutline() == false)
        {
            _areaCache.push(area);
            continue;
        }

        _area_list.push_back(area);
    }
}

void LandBuilder::buildAreaGrid(JNIENV_PARAM_SINGLE)
{
    for (int i = 0; i < _area_list.size(); ++i)
    {
        LandArea* area = _area_list[i];

        buildAreaGrid(JNIENV_CALL_PARAM area);
    }
}

void LandBuilder::buildAreaGrid(JNIENV_PARAM LandArea* area)
{
    DSClipperLib::IntPoint begin;
    DSClipperLib::IntPoint end;

    begin.X = area->getBoundMin().X / 256;
    begin.Y = area->getBoundMin().Y / 256;
    end.X = area->getBoundMax().X / 256;
    end.Y = area->getBoundMax().Y / 256;

    //Logger::logInfo(JNIENV_CALL_PARAM "area[%d] begin[%d,%d] end[%d,%d]", area->getListIndex(), 
    //    area->getBoundMin().X, area->getBoundMin().Y, area->getBoundMax().X, area->getBoundMax().Y);

    for (int x = begin.X; x <= end.X; ++x)
    {
        for (int y = begin.Y; y <= end.Y; ++y)
        {
            buildAreaGrid(JNIENV_CALL_PARAM area, x, y);
        }
    }
}

void LandBuilder::buildAreaGrid(JNIENV_PARAM LandArea* area, int grid_x, int grid_y)
{
    std::vector<p2t::Point> vertList;

    PolygonShape gridShape;
    PolygonShape triShape;

    makeGridVertList(grid_x, grid_y, vertList);
    gridShape.setPolygon(vertList);

    for (int i = 0; i < area->getIndexCount() / 3; ++i)
    {
        vertList.clear();

        for (int p = 0; p < 3; ++p)
        {
            int v_index = area->getIndex(i * 3 + p);
            int x = getAreaVertexX(area->getListIndex(), v_index);
            int y = getAreaVertexY(area->getListIndex(), v_index);

            vertList.push_back(p2t::Point(x, y));
        }

        triShape.setPolygon(vertList);

        if (CollisionTest::polygonToPolygon(triShape, gridShape))
        {
            addAreaToGrid(area, grid_x, grid_y);
            break;
        }
    }

}

void LandBuilder::makeGridVertList(int grid_x, int grid_y, std::vector<p2t::Point>& vertList)
{
    vertList.clear();

    p2t::Point min(grid_x * 256, grid_y * 256);
    p2t::Point max((grid_x + 1) * 256, (grid_y + 1) * 256);

    vertList.push_back(min);
    vertList.push_back(p2t::Point(max.x, min.y));
    vertList.push_back(max);
    vertList.push_back(p2t::Point(min.x, max.y));
}

void LandBuilder::addAreaToGrid(LandArea* area, int grid_x, int grid_y)
{
    int key = grid_x * 100 + grid_y;

    int exist_index = -1;
    for (int i = 0; i < _grid_key_list.size(); ++i)
    {
        if (_grid_key_list[i] == key)
        {
            exist_index = i;
            break;
        }
    }

    if (exist_index == -1)
    {
        exist_index = (int)_grid_key_list.size();

        _grid_key_list.push_back(key);
        _grid_value_list.push_back(std::vector<int>());

    }

    _grid_value_list[exist_index].push_back(area->getListIndex());
}
