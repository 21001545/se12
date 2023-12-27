#include "Precompiled.h"
#include "MapBoxPolygonBuilder.h"
#include "earcut.hpp"

namespace mapbox {
    namespace util {

        template <>
        struct nth<0, MapBoxPolygonBuilder::Point> {
            inline static DSClipperLib::cInt get(const MapBoxPolygonBuilder::Point& t) {
                return t.X;
            };
        };
        template <>
        struct nth<1, MapBoxPolygonBuilder::Point> {
            inline static DSClipperLib::cInt get(const MapBoxPolygonBuilder::Point& t) {
                return t.Y;
            };
        };

    } // namespace util
} // namespace mapbox

MapBoxPolygonBuilder::MapBoxPolygonBuilder(ObjectCache<MapBoxVertex>* vertexCache)
    : _vertexCache(vertexCache), _extrudeHeight(0)
{

}

MapBoxPolygonBuilder::~MapBoxPolygonBuilder()
{

}

void MapBoxPolygonBuilder::reset()
{
    for (MapBoxVertex* v : _pointList)
    {
        _vertexCache->push(v);
    }
    _pointList.clear();
    _indexList.clear();
    _inputPolygon.clear();
    _extrudeHeight = 0;
}

void MapBoxPolygonBuilder::addRing(short* pointArray, int pointCount)
{
	Ring ring;
	for (int i = 0; i < pointCount; ++i)
	{
		ring.emplace_back(Point(pointArray[i * 2 + 0], pointArray[i * 2 + 1]));
	}
	
	_inputPolygon.emplace_back(std::move(ring));
}

int MapBoxPolygonBuilder::build(int extrudeHeight)
{
    _extrudeHeight = extrudeHeight;

    try
    {
        if (_extrudeHeight == 0)
        {
            buildInner();
        }
        else
        {
            buildInnerExtrude();
        }

        return 0;
    }
    catch (...)
    {
        return 1;
    }
}

void MapBoxPolygonBuilder::buildInner()
{
    PolygonList classifiedRings = classifyRings(_inputPolygon);

    for (Polygon& polygon : classifiedRings)
    {
        // limitHoles(polygon, 500);

        std::size_t pointStartIndex = _pointList.size();

        for (Ring& ring : polygon)
        {
            for (Point& pt : ring)
            {
                _pointList.push_back(allocVertex(pt,0, 0, 0, 1000));
            }
        }

        std::vector<uint32_t> indices = mapbox::earcut(polygon);
        size_t indexCount = indices.size();
        for (size_t i = 0; i < indexCount; i ++)
        {
            _indexList.push_back( (uint32_t)(pointStartIndex + indices[i]));
        }
    }
}

void MapBoxPolygonBuilder::buildInnerExtrude()
{
    PolygonList classifiedRings = classifyRings(_inputPolygon);

    for (Polygon& polygon : classifiedRings)
    {
        // limitHoles(polygon, 500);

        std::vector<uint32_t> flatIndices;

        uint32_t triangleIndex = (uint32_t)_pointList.size();

        for (Ring& ring : polygon)
        {
            for (std::size_t i = 0; i < ring.size(); ++i)
            {
                Point& pt1 = ring[i];

                _pointList.push_back(allocVertex(pt1, _extrudeHeight, 0, 0, 1000));
                flatIndices.emplace_back(triangleIndex);
                triangleIndex++;

                if (i != 0)
                {
                    Point& pt2 = ring[i - 1];

                    Point perp = calcPerp(pt1, pt2);

                    _pointList.push_back(allocVertex(pt1, 0, perp.X, perp.Y, 0));
                    _pointList.push_back(allocVertex(pt1, _extrudeHeight, perp.X, perp.Y, 0));
                
                    _pointList.push_back(allocVertex(pt2, 0, perp.X, perp.Y, 0));
                    _pointList.push_back(allocVertex(pt2, _extrudeHeight, perp.X, perp.Y, 0));

                    _indexList.emplace_back(triangleIndex);
                    _indexList.emplace_back(triangleIndex + 1);
                    _indexList.emplace_back(triangleIndex + 2);
                    _indexList.emplace_back(triangleIndex + 1);
                    _indexList.emplace_back(triangleIndex + 3);
                    _indexList.emplace_back(triangleIndex + 2);

                    triangleIndex += 4;
                }
                else
                {
                    Point& pt2 = ring[ring.size() - 1];

                    Point perp = calcPerp(pt1, pt2);

                    _pointList.push_back(allocVertex(pt1, 0, perp.X, perp.Y, 0));
                    _pointList.push_back(allocVertex(pt1, _extrudeHeight, perp.X, perp.Y, 0));

                    _pointList.push_back(allocVertex(pt2, 0, perp.X, perp.Y, 0));
                    _pointList.push_back(allocVertex(pt2, _extrudeHeight, perp.X, perp.Y, 0));

                    _indexList.emplace_back(triangleIndex);
                    _indexList.emplace_back(triangleIndex + 1);
                    _indexList.emplace_back(triangleIndex + 2);
                    _indexList.emplace_back(triangleIndex + 1);
                    _indexList.emplace_back(triangleIndex + 3);
                    _indexList.emplace_back(triangleIndex + 2);

                    triangleIndex += 4;
                }
            }
        }

        std::vector<uint32_t> indices = mapbox::earcut(polygon);
        size_t indexCount = indices.size();
        for (size_t i = 0; i < indexCount; i++)
        {
            _indexList.emplace_back(flatIndices[indices[i]]);
        }
    }
}

MapBoxPolygonBuilder::PolygonList MapBoxPolygonBuilder::classifyRings(Polygon& rings)
{
    PolygonList polygonList;
    std::size_t len = rings.size();

    if (len <= 1)
    {
        polygonList.emplace_back(Polygon(rings));
        return polygonList;
    }

    Polygon polygon;
    int8_t ccw = 0;

    for (Ring& ring : rings) {
        double area = signedArea(ring);
        if (area == 0)
        {
            continue;
        }

        if (ccw == 0)
        {
            ccw = (area < 0 ? -1 : 1);
        }

        if (ccw == (area < 0 ? -1 : 1) && !polygon.empty()) {
            polygonList.emplace_back(std::move(polygon));
            polygon = Polygon();
        }

        polygon.emplace_back(ring);
    }

    if (!polygon.empty())
    {
        polygonList.emplace_back(std::move(polygon));
    }

    return polygonList;
}

double MapBoxPolygonBuilder::signedArea(Ring& ring)
{
	double sum = 0;

	for (std::size_t i = 0, len = ring.size(), j = len - 1; i < len; j = i++) {
		const Point& p1 = ring[i];
		const Point& p2 = ring[j];
		sum += (p2.X - p1.X) * (p1.Y + p2.Y);
	}

	return sum;
}
