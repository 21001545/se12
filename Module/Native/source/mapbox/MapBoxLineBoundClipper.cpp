#include "Precompiled.h"
#include "MapBoxLineBoundClipper.h"
#include "StringBuilder.h"

MapBoxLineBoundClipper::MapBoxLineBoundClipper()
{
    _boundPath.push_back(DSClipperLib::IntPoint(0, 0));
    _boundPath.push_back(DSClipperLib::IntPoint(0, 4096));
    _boundPath.push_back(DSClipperLib::IntPoint(4096, 4096));
    _boundPath.push_back(DSClipperLib::IntPoint(4096, 0));
}

MapBoxLineBoundClipper::~MapBoxLineBoundClipper()
{


}

void MapBoxLineBoundClipper::reset()
{
    _inputPaths.clear();
    _outputPaths.clear();
}

			////
			//Clipper clipper = new Clipper();
			//clipper.AddPaths(paths, PolyType.ptSubject, false);

			//List<IntPoint> ptBound = new List<IntPoint>();
			//ptBound.Add(new IntPoint(0, 0));
			//ptBound.Add(new IntPoint(0, 4096));
			//ptBound.Add(new IntPoint(4096, 4096));
			//ptBound.Add(new IntPoint(4096, 0));

			//clipper.AddPath(ptBound, PolyType.ptClip, true);

			//PolyTree polyTree = new PolyTree();
			//clipper.Execute(ClipType.ctIntersection, polyTree, PolyFillType.pftNonZero);

			//_clippedPathList = Clipper.PolyTreeToPaths( polyTree);

			
			//Clipper.SimplifyPolygons(_clippedPathList, PolyFillType.pftNonZero);
			//Clipper.CleanPolygons(_clippedPathList);

void MapBoxLineBoundClipper::clip()
{
    DSClipperLib::Clipper clipper;
	DSClipperLib::PolyTree polyTree;

    clipper.AddPaths(_inputPaths, DSClipperLib::PolyType::ptSubject, false);
    clipper.AddPath(_boundPath, DSClipperLib::PolyType::ptClip, true);
    
    clipper.Execute(DSClipperLib::ClipType::ctIntersection, polyTree, DSClipperLib::PolyFillType::pftNonZero, DSClipperLib::PolyFillType::pftNonZero);

	DSClipperLib::PolyTreeToPaths(polyTree, _outputPaths);
//    DSClipperLib::SimplifyPolygons(_outputPaths, DSClipperLib::PolyFillType::pftNonZero);
//    DSClipperLib::CleanPolygons(_outputPaths);
}
