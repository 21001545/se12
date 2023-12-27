#include "Precompiled.h"
#include "PluginInterface_JNI.h"
#include "JNIHelper.h"
#include "JNIScopedContext.h"
#include "../landbuilder/LandBuilderCache.h"

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_createBuilderInternal
  (JNIEnv *env, jclass)
{
    LandBuilder* builder = LandBuilderCache::createBuilder();
    return builder->_instance_id;
}

JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_releaseBuilderInternal
  (JNIEnv *env, jclass, jint builder_id)
{
    LandBuilderCache::releaseBuilder(builder_id);
}

#define FIND_BUILDER_VOID() \
    LandBuilder* builder = LandBuilderCache::getBuilder(builder_id);\
    if (builder == null)\
    {\
        JniHelper::Throw(env, "can't find builder:%d", builder_id);\
        return;\
    }

#define FIND_BUILDER_ZERO() \
    LandBuilder* builder = LandBuilderCache::getBuilder(builder_id);\
    if (builder == null)\
    {\
        JniHelper::Throw(env, "can't find builder:%d", builder_id);\
        return 0;\
    }


JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_beginPath
  (JNIEnv *env, jclass, jint builder_id)
{
    FIND_BUILDER_VOID();

    return builder->beginPath();
}

JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_addPathPoint
  (JNIEnv *env, jclass, jint builder_id, jint x, jint y)
{
    FIND_BUILDER_VOID();

    builder->addPathPoint(x, y);
}

JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_endPath
  (JNIEnv *env, jclass, jint builder_id, jint poly_type)
{
    FIND_BUILDER_VOID();

    builder->endPath(poly_type);
}

JNIEXPORT void JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_build
  (JNIEnv *env, jclass, jint builder_id)
{
    FIND_BUILDER_VOID();

    builder->build(env);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaCount
(JNIEnv* env, jclass, jint builder_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaCount();
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaVertexCount
(JNIEnv* env, jclass, jint builder_id, jint area_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaVertexCount(area_id);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaVertexX
(JNIEnv* env, jclass, jint builder_id, jint area_id, jint vertex_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaVertexX(area_id, vertex_id);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaVertexY
(JNIEnv* env, jclass, jint builder_id, jint area_id, jint vertex_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaVertexY(area_id, vertex_id);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaIndexCount
(JNIEnv* env, jclass, jint builder_id, jint area_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaIndexCount(area_id);

}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaIndex
(JNIEnv* env, jclass, jint builder_id, jint area_id, jint index_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaIndex(area_id, index_id);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaArea
(JNIEnv* env, jclass, jint builder_id, jint area_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaArea(area_id);
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaGridCount
(JNIEnv* env, jclass, jint builder_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaGridCount();
}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaGridKey
(JNIEnv* env, jclass, jint builder_id, jint index_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaGridKey(index_id);

}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaGridValueCount
(JNIEnv* env, jclass, jint builder_id, jint index_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaGridValueCount(index_id);

}

JNIEXPORT jint JNICALL Java_com_lifefesta_drun_server_module_nativemodule_NativeLandBuilder_getAreaGridValue
(JNIEnv* env, jclass, jint builder_id, jint index_id, jint value_id)
{
    FIND_BUILDER_ZERO();

    return builder->getAreaGridValue(index_id, value_id);
}
