using Festa.Client.Module;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Festa.Client.MapBox
{
	public class MBPolygonMeshBuilder : MBMeshBuilder
	{
		private bool _isExtrusion = false;

		public void setExtrusion(bool b)
		{
			_isExtrusion = b;
		}

		private void meshFromNative(int context_id)
		{
			int vertex_count = NativeModule.MB_Polygon_getVertexCount(context_id);
			int index_count = NativeModule.MB_Polygon_getIndexCount(context_id);

			for (int i = 0; i < vertex_count; ++i)
			{
				int x = NativeModule.MB_Polygon_getVertexX(context_id, i);
				int y = NativeModule.MB_Polygon_getVertexY(context_id, i);
				int z = NativeModule.MB_Polygon_getVertexZ(context_id, i);

				int nx = NativeModule.MB_Polygon_getNormalX(context_id, i);
				int ny = NativeModule.MB_Polygon_getNormalY(context_id, i);
				int nz = NativeModule.MB_Polygon_getNormalZ(context_id, i);

				Vector3 vert = new Vector3(x, -y, -z * 10.0f);
				vert.x /= 4096.0f;
				vert.y /= 4096.0f;
				vert.z /= 4096.0f;

				if( nx == 0 && ny == 0 && nz == 0)
				{
					nx = 0;
					ny = 0;
					nz = 1000;
					//Debug.Log("normal is zero");
				}

				_vertexList.Add(vert);
				//_vertexList.Add(new Vector3(x, -y, -z * 10.0f));

				Vector3 normal = new Vector3(nx, -ny, -nz);

				_normalList.Add(normal / 1000.0f);
			}

			for (int i = 0; i < index_count; ++i)
			{
				_indexList.Add((ushort)NativeModule.MB_Polygon_getIndex(context_id, i));
			}



			//int vertex_count = NativeModule.MB_getVertexCount(context_id);
			//int index_count = NativeModule.MB_getIndexCount(context_id);

			//for (int i = 0; i < vertex_count; ++i)
			//{
			//	int x = NativeModule.MB_getVertexX(context_id, i);
			//	int y = NativeModule.MB_getVertexY(context_id, i);
			//	int z = NativeModule.MB_getVertexZ(context_id, i);

			//	int nx = NativeModule.MB_getNormalX(context_id, i);
			//	int ny = NativeModule.MB_getNormalY(context_id, i);
			//	int nz = NativeModule.MB_getNormalZ(context_id, i);

			//	Vector3 vert = new Vector3(x, -y, -z * 10.0f);
			//	vert.x /= 4096.0f;
			//	vert.y /= 4096.0f;
			//	vert.z /= 4096.0f;

			//	_vertexList.Add(vert);
			//	//_vertexList.Add(new Vector3(x, -y, -z * 10.0f));

			//	Vector3 normal = new Vector3(nx, -ny, -nz);

			//	_normalList.Add( normal / 1000.0f);
			//}

			//for (int i = 0; i < index_count; ++i)
			//{
			//	_indexList.Add((ushort)NativeModule.MB_getIndex(context_id, i));
			//}
		}

		private void setupPaths(int context_id,MBFeature feature)
		{
			for(int i = 0; i < feature.pathList.Count; ++i)
			{
				NativeModule.MB_Polygon_AddRing(context_id, feature.pathList[i], feature.pathList[i].Length / 2);
			}


			//for (int i = 0; i < feature.pathList.Count; i++)
			//{
			//	short[] path = feature.pathList[i];
			//	int slot = feature.getMeshBuildSlot(i);

			//	int path_id = NativeModule.MB_beginPath(context_id, slot);
			//	int point_count = path.Length / 2;
			//	for (int p = 0; p < point_count; ++p)
			//	{
			//		int x = (int)path[p * 2 + 0] + offset_x;
			//		int y = (int)path[p * 2 + 1] + offset_y;

			//		NativeModule.MB_addPathPoint(context_id, slot, path_id, x, y);
			//	}
			//}
		}

		public override void build(MBFeature feature,Color color,MBLayerRenderData layer,MBStyleExpressionContext ctx)
		{
			_feature = feature;
			reset();

			int context_id = NativeModule.MB_createContextSafe();

			setupPaths(context_id, feature);

			int extrudeHeight = 0;
			if (_isExtrusion)
			{
				extrudeHeight = getExtrudeHeight(feature);
			}

			NativeModule.MB_Polygon_Build(context_id, extrudeHeight);

			meshFromNative(context_id);


			//int extrudeHeight = 0;
			//if(_isExtrusion)
			//{
			//	extrudeHeight = getExtrudeHeight(feature);
			//}

			//NativeModule.MB_startMeshBuilder(context_id, extrudeHeight);

			//setupPaths(context_id, feature, offset_x, offset_y);

			//int buildResult = NativeModule.MB_build(context_id);
			//if( buildResult != 0)
			//{
			//	Debug.LogError($"buildPolygonMesh fail: TilePos[{feature._tile._tilePos.ToString()}] Layer[{feature._layer.name}] Feature[{feature.id}]");
			//}

			//meshFromNative(context_id);

			////_feature.setMesh(_vertexList.ToArray(), _indexList.ToArray());

			NativeModule.MB_releaseContextSafe(context_id);

			makeColors(color);

		}

		private int getExtrudeHeight(MBFeature feature)
		{
			int extrudeHeight = 0;
			if ((string)feature.get(MBPropertyKey.extrude, "false") == "true")
			{
				extrudeHeight = (int)((double)feature.get(MBPropertyKey.height, 0.0));
				
				//if( extrudeHeight < 20)
				//{
				//	extrudeHeight = 20;
				//}
			}

			return extrudeHeight;
		}

		//public void build(List<MBFeature> feature_list,MBTileCoordinate centerTilePos,Color color)
		//{
		//	reset();

		//	int context_id = NativeModule.MB_createContextSafe();

		//	int extrudeHeight = getExtrudeHeight(feature_list[0]);

		//	NativeModule.MB_startMeshBuilder(context_id, extrudeHeight);
		//	foreach (MBFeature feature in feature_list)
		//	{

		//		MBTileCoordinate offsetTilePos = feature._tile._tilePos;
		//		int offset_x = (offsetTilePos.tile_x - centerTilePos.tile_x) * 4096;
		//		int offset_y = (offsetTilePos.tile_y - centerTilePos.tile_y) * 4096;

		//		setupPaths(context_id, feature, offset_x, offset_y);
		//	}

		//	NativeModule.MB_build(context_id);

		//	meshFromNative(context_id);

		//	NativeModule.MB_releaseContextSafe(context_id);

		//	makeColors( color);
		//}
	}
}
