#include "Precompiled.h"
#include "MapBoxMeshBuilder.h"
#include "StringBuilder.h"

MapBoxMeshBuilder::MapBoxMeshBuilder(PointCache* pointCache,ObjectCache<MapBoxVertex>* vertexCache,ObjectCache<MapBoxEdge>* edgeCache)
: _pointCache(pointCache), _vertexCache(vertexCache), _edgeCache(edgeCache),
_extrudeHeight(0)
{

}

MapBoxMeshBuilder::~MapBoxMeshBuilder()
{

}

void MapBoxMeshBuilder::reset()
{
    _pointCache->reset();

    for (int i = 0; i < 3; ++i)
    {
        _inputPaths[i].clear();
    }
    _index_list.clear();
    //_point_map.clear();

    for (std::vector<MapBoxVertex*>::iterator it = _point_list.begin(); it != _point_list.end(); ++it)
    {
        _vertexCache->push(*it);
    }

    _point_list.clear();

    for (std::vector<MapBoxEdge*>::iterator it = _edge_list.begin(); it != _edge_list.end(); ++it)
    {
        _edgeCache->push(*it);
    }
    
    _edge_list.clear();
    _edge_map.clear();
    _extrudeHeight = 0;
}

int MapBoxMeshBuilder::buildMesh()
{
    try
    {
        buildMultiSlot();

        if (_extrudeHeight > 0)
        {
            extrude();
        }

        return 0;
    }
    catch (const std::runtime_error& e)
    {

#if defined(JNI_LOGGER) // 임시코드
        Logger::logRAW(null, LOG_TYPE_ERROR, e.what());
#else
        Logger::logRAW(LOG_TYPE_ERROR, e.what());
#endif
        

        outputDebugInfo();

        reset();

        return 1;
    }
    catch (...)
    {
        outputDebugInfo();

        reset();

        return 2;
    }


    //for (int i = 0; i < _point_list.size(); ++i)
    //{
    //    MapBoxVertex* v = _point_list[i];
    //    Logger::logInfo("%d:%d.%d.%d", v->index, v->nx, v->ny, v->nz);
    //}
}

/*
public bool IsClockwise(IList<Vector> vertices)
{
    double sum = 0.0;
    for (int i = 0; i < vertices.Count; i++) {
        Vector v1 = vertices[i];
        Vector v2 = vertices[(i + 1) % vertices.Count];
        sum += (v2.X - v1.X) * (v2.Y + v1.Y);
    }
    return sum > 0.0;
}
*/

bool MapBoxMeshBuilder::isClockwise(DSClipperLib::Path& path)
{
    int sum = 0;
    for (int i = 0; i < path.size(); ++i)
    {
        DSClipperLib::IntPoint& pt1 = path[i];
        DSClipperLib::IntPoint& pt2 = path[(i + 1) % path.size()];
    
        sum += (pt2.X - pt1.X) * (pt2.Y + pt1.Y);
    }

    return sum > 0;
}

void MapBoxMeshBuilder::buildMultiSlot()
{
    DSClipperLib::Clipper clipper;

    DSClipperLib::PolyTree* polyTree = new DSClipperLib::PolyTree();
    _lastPolyTree = polyTree;

    //if (_extrudeHeight > 0)
    //{
    //    Logger::log(0, "slot[0] - %d", _inputPaths[0].size());
    //    Logger::log(0, "slot[1] - %d", _inputPaths[1].size());
    //    Logger::log(0, "slot[2] - %d", _inputPaths[2].size());
    //}

    DSClipperLib::Paths tempPaths;

    if (_inputPaths[1].size() > 0) // union with clip
    {
        for (int i = 0; i < _inputPaths[0].size(); ++i)
        {
            DSClipperLib::Path& path = _inputPaths[0][i];
            clipper.AddPath(path, DSClipperLib::PolyType::ptSubject, true);
        }

        for (int i = 0; i < _inputPaths[1].size(); ++i)
        {
            DSClipperLib::Path& path = _inputPaths[1][i];
            clipper.AddPath(path, DSClipperLib::PolyType::ptClip, true);
        }

        clipper.Execute(DSClipperLib::ClipType::ctDifference, tempPaths, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

        DSClipperLib::SimplifyPolygons(tempPaths, DSClipperLib::PolyFillType::pftNonZero);
        DSClipperLib::CleanPolygons(tempPaths);
    }
    else // just union paths
    {
        for (int i = 0; i < _inputPaths[0].size(); ++i)
        {
            DSClipperLib::Path& path = _inputPaths[0][i];
            clipper.AddPath(path, DSClipperLib::PolyType::ptSubject, true);
        }

        clipper.Execute(DSClipperLib::ClipType::ctUnion, tempPaths, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

        DSClipperLib::SimplifyPolygons(tempPaths, DSClipperLib::PolyFillType::pftNonZero);
        DSClipperLib::CleanPolygons(tempPaths);
    }
    
    // clip by tilebounds
    clipper.Clear();

    clipper.AddPaths(tempPaths, DSClipperLib::PolyType::ptSubject,true);

    DSClipperLib::Path boundPath;
    boundPath.push_back(DSClipperLib::IntPoint(0, 0));
    boundPath.push_back(DSClipperLib::IntPoint(0, 4096));
    boundPath.push_back(DSClipperLib::IntPoint(4096, 4096));
    boundPath.push_back(DSClipperLib::IntPoint(4096, 0));
    clipper.AddPath(boundPath, DSClipperLib::PolyType::ptClip, true);

    tempPaths.clear();
    clipper.Execute(DSClipperLib::ClipType::ctIntersection, tempPaths, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

    DSClipperLib::SimplifyPolygons(tempPaths, DSClipperLib::PolyFillType::pftNonZero);
    DSClipperLib::CleanPolygons(tempPaths);

    //
    clipper.Clear();
    clipper.AddPaths(tempPaths, DSClipperLib::PolyType::ptSubject, true);
    clipper.Execute(DSClipperLib::ClipType::ctUnion, *polyTree, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

    buildMeshFromPolyTree(*polyTree);

    delete polyTree;
}

//void MapBoxMeshBuilder::buildMeshSingle()
//{
//    DSClipperLib::Clipper clipper;
//
//    clipper.AddPath(_inputPaths[0], DSClipperLib::PolyType::ptSubject, true);
//
//    DSClipperLib::PolyTree polyTree;
//    clipper.Execute(DSClipperLib::ClipType::ctUnion, polyTree, DSClipperLib::PolyFillType::pftPositive);
//    
//    buildMesh(polyTree);
//}

void MapBoxMeshBuilder::buildMeshFromPolyTree(DSClipperLib::PolyTree& polyTree)
{
    //////////////////////////////

    CDT_MAP cdtMap;

    int index = 0;
    double hole_noise = 0;

    DSClipperLib::PolyNode* curNode = polyTree.GetFirst();
    while (curNode != null)
    {
        POINT_LIST pt_list;

        if (curNode->IsHole())
        {
            hole_noise += 0.0001f;
        }

        createPointList(curNode, pt_list, hole_noise);

        //if (_extrudeHeight > 0)
        //{
        //    Logger::log(0, "index[%d] pts[%d] is_hole[%d]", index, pt_list.size(), curNode->IsHole());
        //}

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
#if defined(JNI_LOGGER) // 임시코드
#else
                Logger::log(1, "index[%d] can't find hole's parent", index);
#endif                
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

    //////////////////////////////
    CDT_MAP::iterator it;
    for (it = cdtMap.begin(); it != cdtMap.end(); ++it)
    {
        p2t::CDT* cdt = it->second;
        _lastCDT = cdt;

        cdt->Triangulate();

        _lastCDT = null;
    }

    for (it = cdtMap.begin(); it != cdtMap.end(); ++it)
    {
        addTriangles(it->second);

        delete it->second;
    }
    cdtMap.clear();
}


/*
    2021.7.1

        ���� ��ġ�� ����Ʈ�� �����ؼ� ���� ��� triangluation���� crash�� ����

*/

//void MapBoxMeshBuilder::createPointList(DSClipperLib::Path& path, POINT_LIST& point_list)
//{
//    DSClipperLib::IntPoint last_pt;
//
//    for (int i = 0; i < path.size(); ++i)
//    {
//        DSClipperLib::IntPoint& path_pt = path[i];
//
//        if (i > 0)
//        {
//            if (path_pt == last_pt)
//            {
//                continue;
//            }
//        }
//
//        last_pt.X = path_pt.X;
//        last_pt.Y = path_pt.Y;
//
//        p2t::Point* pt = _pointCache->getFree();
//        pt->x = path_pt.X;
//        pt->y = path_pt.Y;
//        point_list.push_back(pt);
//    }
//}

void MapBoxMeshBuilder::createPointList(DSClipperLib::PolyNode* node, POINT_LIST& point_list,double hole_noise)
{
    bool isHole = node->IsHole();

    // ���ۿ� noise�� �� �ش�

    for (int i = 0; i < node->Contour.size(); ++i)
    {
        DSClipperLib::IntPoint& iPt = node->Contour[i];

        p2t::Point* pt = _pointCache->getFree();
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

void MapBoxMeshBuilder::addTriangles(p2t::CDT* cdt)
{
    std::vector<p2t::Triangle*>& tri_list = cdt->GetTriangles();
    for (int i = 0; i < tri_list.size(); ++i)
    {
        p2t::Triangle* tri = tri_list[i];
        addTriangle(tri);
    }
}

void MapBoxMeshBuilder::addTriangle(p2t::Triangle* triangle)
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
}

void MapBoxMeshBuilder::addEdge(int p0, int p1)
{
    KEY_TYPE key;
    
    if( p0 < p1)
    {
        key = makeEdgeKey(p0, p1);
    }
    else
    {
        key = makeEdgeKey(p1, p0);
    }

    std::map<unsigned long, MapBoxEdge*>::iterator it = _edge_map.find(key);

    if ( it != _edge_map.end())
    {
        it->second->shared_count += 1;
    }
    else
    {
        MapBoxEdge* edge = _edgeCache->pop();
        edge->p0 = _point_list[p0];
        edge->p1 = _point_list[p1];
        edge->shared_count = 1;

        _edge_list.push_back(edge);
        _edge_map.insert(std::make_pair(key, edge));
    }
}

int MapBoxMeshBuilder::addPoint(p2t::Point* pt)
{
    int x = (int)pt->x;
    int y = (int)pt->y;

    MapBoxVertex* vert = _vertexCache->pop();
    vert->x = x;
    vert->y = y;
    vert->z = _extrudeHeight;
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
        _vertexCache->push(vert);
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

int MapBoxMeshBuilder::findVertex(MapBoxVertex* v)
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


void MapBoxMeshBuilder::extrude()
{
    std::map<KEY_TYPE, MapBoxEdge*>::iterator it;
    for (it = _edge_map.begin(); it != _edge_map.end(); ++it)
    {
        MapBoxEdge* edge = it->second;
        if (edge->shared_count > 1)
        {
            continue;
        }
        
        edge->makeExtrudeNormal();
        
        MapBoxVertex* p[4];
        p[0] = _vertexCache->pop();
        p[1] = _vertexCache->pop();
        p[2] = _vertexCache->pop();
        p[3] = _vertexCache->pop();

        p[0]->copyFrom(edge->p0);
        p[1]->copyFrom(edge->p1);
        p[2]->copyFrom(edge->p0);
        p[3]->copyFrom(edge->p1);

        p[2]->z = 0;
        p[3]->z = 0;

        int base_index = (int)_point_list.size();

        for (int i = 0; i < 4; ++i)
        {
            p[i]->nx = edge->nx;
            p[i]->ny = edge->ny;
            p[i]->nz = edge->nz;
            p[i]->index = base_index + i;
            _point_list.push_back(p[i]);
        }

        _index_list.push_back(base_index + 2);
        _index_list.push_back(base_index + 1);
        _index_list.push_back(base_index + 0);

        _index_list.push_back(base_index + 3);
        _index_list.push_back(base_index + 1);
        _index_list.push_back(base_index + 2);
        /*
        int p0_index = edge->p0->index;
        int p1_index = edge->p1->index;
        int p0_base_index;
        int p1_base_index;

        MapBoxVertex* p0 = edge->p0;
        MapBoxVertex* p1 = edge->p1;

        MapBoxVertex* p0_base = _vertexCache->pop();
        MapBoxVertex* p1_base = _vertexCache->pop();

        p0_base->x = p0->x;
        p0_base->y = p0->y;
        p0_base->z = 0;
        p0_base->index = p0_base_index = (int)_point_list.size();

        _point_list.push_back(p0_base);

        p1_base->x = p1->x;
        p1_base->y = p1->y;
        p1_base->z = 0;
        p1_base->index = p1_base_index = (int)_point_list.size();

        _point_list.push_back(p1_base);

        _index_list.push_back(p1_index);
        _index_list.push_back(p0_index);
        _index_list.push_back(p0_base_index);

        _index_list.push_back(p1_base_index);
        _index_list.push_back(p1_index);
        _index_list.push_back(p0_base_index);
        */
    }
}

void MapBoxMeshBuilder::outputDebugInfo()
{
    StringBuilder sb;

    sb.appendFormat("{\"log\":\"exception in MapBoxMeshBuilder!!\",\n");

    sb.appendFormat("\"input\":[\n");

    for (int i = 0; i < 3; ++i)
    {
        DSClipperLib::Paths& paths = _inputPaths[i];

        sb.appendFormat("[\n");

        for (int j = 0; j < paths.size(); ++j)
        {
            DSClipperLib::Path& path = paths[j];

            sb.appendFormat("[\n");

            for (int m = 0; m < path.size(); ++m)
            {
                DSClipperLib::IntPoint& pt = path[m];

                if (m < path.size() - 1)
                {
                    sb.appendFormat("{\"x\":%d,\"y\":%d},\n", pt.X, pt.Y);
                }
                else
                {
                    sb.appendFormat("{\"x\":%d,\"y\":%d}\n", pt.X, pt.Y);
                }
            }

            if (j < paths.size() - 1)
            {
                sb.appendFormat("],\n");
            }
            else
            {
                sb.appendFormat("]\n");
            }
        }

        sb.appendFormat("],\n");
    }

    sb.appendFormat("],\n");

    sb.appendFormat("\"inputCDT\":[\n");

    {
        int index = 0;
        double hole_noise = 0;

        DSClipperLib::PolyNode* curNode = _lastPolyTree->GetFirst();
        while (curNode != null)
        {
            POINT_LIST pt_list;

            if (curNode->IsHole())
            {
                hole_noise += 0.0001f;
            }

            createPointList(curNode, pt_list, hole_noise);

            //Logger::log( 0, "index[%d] pts[%d] is_hole[%d]", index, pt_list.size(), curNode->IsHole());
            bool is_hole = curNode->IsHole();

            sb.appendFormat("{\n\"is_hole\":%s,\n", is_hole ? "true": "false");
            sb.appendFormat("\"points\":[\n");

            for (int i = 0; i < pt_list.size(); ++i)
            {
                p2t::Point* pt = pt_list[i];

                //sb.appendFormat("input_cdt:[%d][%d] (%f,%f)\n", is_hole ? 1 : 0, i, pt->x, pt->y);
                if (i < pt_list.size() - 1)
                {
                    sb.appendFormat("{\"x\":%f,\"y\":%f},\n", pt->x, pt->y);
                }
                else
                {
                    sb.appendFormat("{\"x\":%f,\"y\":%f}\n", pt->x, pt->y);
                }
            }

            sb.appendFormat("]\n},\n");

            curNode = curNode->GetNext();
            index++;
        }
    }

    sb.appendFormat("]}\n");

    //if (_lastCDT != null)
    //{
    //    POINT_LIST& point_list = _lastCDT->getPoints();
    //    for (int i = 0; i < point_list.size(); ++i)
    //    {
    //        p2t::Point* pt = point_list[i];

    //        sb.appendFormat("last_cdt:[%d] (%f,%f)\n", i, pt->x, pt->y);
    //    }
    //}

#if defined(JNI_LOGGER) // 임시코드
    Logger::logRAW(null, LOG_TYPE_ERROR, sb.toString());
#else
    Logger::logRAW(LOG_TYPE_ERROR, sb.toString());
#endif

}