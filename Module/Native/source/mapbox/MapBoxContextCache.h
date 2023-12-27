#ifndef _MAPBOXCONTEXTCACHE_H
#define _MAPBOXCONTEXTCACHE_H

#include "MapBoxContext.h"

class MapBoxContextCache
{
private:
    static ObjectCache<MapBoxContext>   _context_cache;

public:
    static void init();
    static MapBoxContext* createContext();
    static void releaseContext(int id);
    static MapBoxContext* getContext(int id);
};

#endif