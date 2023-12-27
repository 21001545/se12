#include "Precompiled.h"
#include "MapBoxContext.h"

MapBoxContext::MapBoxContext()
	:_vertexCache(2),_edgeCache(2),_meshBuilder(&_pointCache,&_vertexCache,&_edgeCache),_polygonBuilder(&_vertexCache)
{
	

}

MapBoxContext::~MapBoxContext()
{


}
