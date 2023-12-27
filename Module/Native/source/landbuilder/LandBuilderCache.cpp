#include "Precompiled.h"
#include "LandBuilderCache.h"

ObjectCache<LandBuilder> LandBuilderCache::_cache;

void LandBuilderCache::init()
{
	_cache.init(2);
}

LandBuilder* LandBuilderCache::createBuilder()
{
	LandBuilder* builder = _cache.pop();
	builder->reset();
	return builder;
}

void LandBuilderCache::releaseBuilder(int id)
{
	LandBuilder* builder = getBuilder(id);
	if (builder != null)
	{
		_cache.push(builder);
	}
}

LandBuilder* LandBuilderCache::getBuilder(int id)
{
	return _cache.fromID(id);
}


