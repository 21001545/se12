#ifndef _MAPBOXLINEMESHBUILDER_H
#define _MAPBOXLINEMESHBUILDER_H

#include <map>
#include <set>

#include "clipper.hpp"

class MapBoxLineBoundClipper
{
private:
    DSClipperLib::Paths _inputPaths;
    DSClipperLib::Paths _outputPaths;
    DSClipperLib::Path _boundPath;

public:
    MapBoxLineBoundClipper();
    ~MapBoxLineBoundClipper();

    inline int beginPath()
    {
        int id = (int)_inputPaths.size();
        _inputPaths.push_back(DSClipperLib::Path());
        return id;
    }

    inline void addPathPoint(int path_id, int x, int y)
    {
        DSClipperLib::Path& path = _inputPaths[path_id];
        path.push_back(DSClipperLib::IntPoint(x, y));
    }

    void reset();
    void clip();

    inline int getResultPathCount()
    {
        return (int)_outputPaths.size();
    }

    inline int getResultPathPointCount(int path_id)
    {
        return (int)_outputPaths[path_id].size();
    }

    inline int getResultPathX(int path_id, int index)
    {
        return _outputPaths[path_id][index].X;
    }

    inline int getResultPathY(int path_id, int index)
    {
        return _outputPaths[path_id][index].Y;
    }

};

#endif