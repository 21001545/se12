#ifndef PluginInterface_CSharp_H
#define PluginInterface_CSharp_H

#include "PluginInterface_CSharp_Config.h"

extern "C" {

    PLUGIN_API void init(LogCallback callback);

	PLUGIN_API bool BSDiff(const char* old_file_path,const char* new_file_path,const char* patch_file_path);
	PLUGIN_API bool BSPatch(const char* old_file_path,const char* patch_file_path,const char* new_file_path);

	PLUGIN_API int MB_createContext();
	PLUGIN_API void MB_releaseContext(int context_id);
	PLUGIN_API void MB_startMeshBuilder(int context_id,int extrudeHeight);
	PLUGIN_API int MB_beginPath(int context_id,int slot);
	PLUGIN_API void MB_addPathPoint(int context_id, int slot,int path_id, int x, int y);
	PLUGIN_API int MB_build(int context_id);
	PLUGIN_API int MB_getVertexCount(int context_id);
	PLUGIN_API int MB_getIndexCount(int context_id);
	PLUGIN_API int MB_getVertexX(int context_id,int index);
	PLUGIN_API int MB_getVertexY(int context_id,int index);
	PLUGIN_API int MB_getVertexZ(int context_id,int index);
	PLUGIN_API int MB_getNormalX(int context_id, int index);
	PLUGIN_API int MB_getNormalY(int context_id, int index);
	PLUGIN_API int MB_getNormalZ(int context_id, int index);
	PLUGIN_API int MB_getIndex(int context_id, int index);

	PLUGIN_API int MB_LineBoundClip_beginPath(int context_id);
	PLUGIN_API void MB_LineBoundClip_addPathPoint(int context_id,int path_id,int x,int y);
	PLUGIN_API void MB_LineBoundClip_build(int context_id);
	PLUGIN_API int MB_LineBoundClip_getResultPathCount(int context_id);
	PLUGIN_API int MB_LineBoundClip_getResultPathPointCount(int context_id,int path_id);
	PLUGIN_API int MB_LineBoundClip_getResultPathX(int context_id,int path_id,int index);
	PLUGIN_API int MB_LineBoundClip_getResultPathY(int context_id,int path_id,int index);

	PLUGIN_API void MB_Polygon_AddRing(int context_id,short* pointArray, int pointCount);
	PLUGIN_API int MB_Polygon_Build(int context_id,int extrudeHeight);
	PLUGIN_API int MB_Polygon_getVertexCount(int context_id);
	PLUGIN_API int MB_Polygon_getIndexCount(int context_id);
	PLUGIN_API int MB_Polygon_getVertexX(int context_id, int index);
	PLUGIN_API int MB_Polygon_getVertexY(int context_id, int index);
	PLUGIN_API int MB_Polygon_getVertexZ(int context_id, int index);
	PLUGIN_API int MB_Polygon_getNormalX(int context_id, int index);
	PLUGIN_API int MB_Polygon_getNormalY(int context_id, int index);
	PLUGIN_API int MB_Polygon_getNormalZ(int context_id, int index);
	PLUGIN_API int MB_Polygon_getIndex(int context_id, int index);
}

#endif