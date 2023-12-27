#include "Precompiled.h"
#include "PluginInterface_CSharp.h"
#include "MapBoxContextCache.h"

int MB_createContext()
{
	MapBoxContext* context = MapBoxContextCache::createContext();
	context->_lineBoundClipper.reset();
	context->_polygonBuilder.reset();
	return context->_instance_id;
}

void MB_releaseContext(int context_id)
{
	MapBoxContextCache::releaseContext(context_id);
}

void MB_startMeshBuilder(int context_id,int extrudeHeight)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return;

	context->_pointCache.reset();
	context->_meshBuilder.reset();
	context->_meshBuilder.setExtrudeHeight(extrudeHeight);
}

int MB_beginPath(int context_id,int slot)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.beginPath(slot);
}

void MB_addPathPoint(int context_id, int slot,int path_id, int x, int y)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return;

	context->_meshBuilder.addPathPoint(slot,path_id, x, y);
}

int MB_build(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.buildMesh();
}

int MB_getVertexCount(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getVertexCount();
}

int MB_getIndexCount(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getIndexCount();
}

int MB_getVertexX(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getVertexX(index);
}

int MB_getVertexY(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getVertexY(index);
}

int MB_getVertexZ(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;
	
	return context->_meshBuilder.getVertexZ(index);
}

int MB_getNormalX(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getNormalX(index);
}

int MB_getNormalY(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getNormalY(index);
}

int MB_getNormalZ(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getNormalZ(index);
}


int MB_getIndex(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_meshBuilder.getIndex(index);
}

int MB_LineBoundClip_beginPath(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_lineBoundClipper.beginPath();
}

void MB_LineBoundClip_addPathPoint(int context_id,int path_id,int x,int y)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return;

	return context->_lineBoundClipper.addPathPoint( path_id, x, y);
}

void MB_LineBoundClip_build(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return;

	return context->_lineBoundClipper.clip();
}

int MB_LineBoundClip_getResultPathCount(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_lineBoundClipper.getResultPathCount();
}

int MB_LineBoundClip_getResultPathPointCount(int context_id,int path_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_lineBoundClipper.getResultPathPointCount(path_id);
}

int MB_LineBoundClip_getResultPathX(int context_id,int path_id,int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_lineBoundClipper.getResultPathX(path_id,index);
}

int MB_LineBoundClip_getResultPathY(int context_id,int path_id,int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
		return 0;

	return context->_lineBoundClipper.getResultPathY(path_id, index);
}

void MB_Polygon_AddRing(int context_id, short* pointArray, int pointCount)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return;
	}

	context->_polygonBuilder.addRing(pointArray, pointCount);
}

int MB_Polygon_Build(int context_id,int extrudeHeight)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.build(extrudeHeight);
}

int MB_Polygon_getVertexCount(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getVertexCount();
}

int MB_Polygon_getIndexCount(int context_id)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getIndexCount();
}

int MB_Polygon_getVertexX(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getVertexX(index);
}

int MB_Polygon_getVertexY(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getVertexY(index);
}

int MB_Polygon_getVertexZ(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getVertexZ(index);
}

int MB_Polygon_getNormalX(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getNormalX(index);
}

int MB_Polygon_getNormalY(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getNormalY(index);
}

int MB_Polygon_getNormalZ(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getNormalZ(index);
}

int MB_Polygon_getIndex(int context_id, int index)
{
	MapBoxContext* context = MapBoxContextCache::getContext(context_id);
	if (context == null)
	{
		return 0;
	}

	return context->_polygonBuilder.getIndex(index);
}
