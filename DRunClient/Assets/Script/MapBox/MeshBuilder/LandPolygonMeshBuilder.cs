using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class LandPolygonMeshBuilder : MBMeshBuilder
	{
		//private LandTileFeature _feature;

		public override void build(MBFeature feature, Color color, MBLayerRenderData layer, MBStyleExpressionContext ctx)
		{
			throw new NotImplementedException();
		}

		public void build(LandTileFeature feature,Color color)
		{
			//_feature = feature;
			reset();

			for(int i = 0; i < feature.vertexList.Length / 2; ++i)
			{
				int x = feature.vertexList[i * 2 + 0];
				int y = feature.vertexList[i * 2 + 1];

				Vector3 v = new Vector3(x, -y, 0);
				v /= 4096.0f;

				_vertexList.Add(v);
				_normalList.Add(Vector3.forward);
			}

			for(int i = 0; i < feature.indexList.Length; ++i)
			{
				_indexList.Add( (ushort)feature.indexList[i]);
			}

			makeColors(color);

			//int context_id = NativeModule.MB_createContextSafe();

			//NativeModule.MB_startMeshBuilder(context_id, 0);

			//setupPath(context_id, offset_x, offset_y);

			//NativeModule.MB_build(context_id);

			//meshFromNative(context_id);

			//NativeModule.MB_releaseContextSafe(context_id);

			//makeColors(color);
		}

		//private void setupPath(int context_id,int offset_x,int offset_y)
		//{
		//	int path_id = NativeModule.MB_beginPath(context_id, 0);
		//	int point_count = _feature.path.Length / 2;
		//	for(int i = 0; i < point_count; ++i)
		//	{
		//		int x = _feature.path[i * 2 + 0] + offset_x;
		//		int y = _feature.path[i * 2 + 1] + offset_y;
		//		NativeModule.MB_addPathPoint(context_id, 0, path_id, x, y);
		//	}
		//}

		//private void meshFromNative(int context_id)
		//{
		//	int vertex_count = NativeModule.MB_getVertexCount(context_id);
		//	int index_count = NativeModule.MB_getIndexCount(context_id);

		//	for (int i = 0; i < vertex_count; ++i)
		//	{
		//		int x = NativeModule.MB_getVertexX(context_id, i);
		//		int y = NativeModule.MB_getVertexY(context_id, i);
		//		int z = NativeModule.MB_getVertexZ(context_id, i);

		//		int nx = NativeModule.MB_getNormalX(context_id, i);
		//		int ny = NativeModule.MB_getNormalY(context_id, i);
		//		int nz = NativeModule.MB_getNormalZ(context_id, i);

		//		Vector3 vert = new Vector3(x, -y, -z * 10.0f);
		//		vert.x /= 4096.0f;
		//		vert.y /= 4096.0f;
		//		vert.z /= 4096.0f;

		//		_vertexList.Add(vert);
		//		//_vertexList.Add(new Vector3(x, -y, -z * 10.0f));

		//		Vector3 normal = new Vector3(nx, -ny, -nz);

		//		_normalList.Add(normal / 1000.0f);
		//	}

		//	for (int i = 0; i < index_count; ++i)
		//	{
		//		_indexList.Add((ushort)NativeModule.MB_getIndex(context_id, i));
		//	}
		//}
	}
}
