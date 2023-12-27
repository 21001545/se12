#include "Precompiled.h"
#include "PluginInterface_CSharp.h"
#include "MapBoxContextCache.h"
#include "LandBuilderCache.h"

void init(LogCallback callback)
{
    Logger::setLogCallback(callback);

    MapBoxContextCache::init();
    LandBuilderCache::init();
}
