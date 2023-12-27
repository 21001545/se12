#include "Precompiled.h"
#include "MapBoxContextCache.h"

ObjectCache<MapBoxContext> MapBoxContextCache::_context_cache;

void MapBoxContextCache::init()
{
    _context_cache.init(2);
}

MapBoxContext* MapBoxContextCache::createContext()
{
    return _context_cache.pop();
}

void MapBoxContextCache::releaseContext(int id)
{
    MapBoxContext* context = getContext(id);
    if( context != null)
    {
        _context_cache.push(context);
    }
}

MapBoxContext* MapBoxContextCache::getContext(int id)
{
    return _context_cache.fromID(id);
}