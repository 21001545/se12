#include "Precompiled.h"
#include "PluginInterface_JNI.h"
#include "../landbuilder/LandBuilderCache.h"

JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeMain_init
  (JNIEnv *, jclass)
{
  LandBuilderCache::init();
}

