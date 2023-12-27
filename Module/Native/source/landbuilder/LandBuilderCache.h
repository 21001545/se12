#ifndef __LANDBUILDERCACHE_H
#define __LANDBUILDERCACHE_H

#include "LandBuilder.h"

class LandBuilderCache
{
private:
	static ObjectCache<LandBuilder>	_cache;

public:
	static void init();
	static LandBuilder* createBuilder();
	static void releaseBuilder(int id);
	static LandBuilder* getBuilder(int id);
};

#endif