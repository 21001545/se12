using System.Collections.Generic;
using UnityEngine;
using System.Text;
using ClipperLib;
using Festa.Client.Module;

// 2021.11.15 line-gap-width -> Road의 아웃라인 렌더링 용으로 사용

//float RenderLineLayer::getLineWidth(const GeometryTileFeature& feature, const float zoom,
//                                    const FeatureState& featureState) const {
//    const auto& evaluated = static_cast<const LineLayerProperties&>(*evaluatedProperties).evaluated;
//    float lineWidth =
//        evaluated.get<style::LineWidth>().evaluate(feature, zoom, featureState, style::LineWidth::defaultValue());
//    float gapWidth =
//        evaluated.get<style::LineGapWidth>().evaluate(feature, zoom, featureState, style::LineGapWidth::defaultValue());
//    if (gapWidth) {
//        return gapWidth + 2 * lineWidth;
//    } else {
//        return lineWidth;
//    }
//}

namespace Festa.Client.MapBox
{
	public class MBLineStringMeshBuilder : MBMeshBuilder
	{
		private List<Vector3> _tempPosList;
		private List<Vector3> _tempDirList;

		protected override void init()
		{
			base.init();
			_tempPosList = new List<Vector3>();
			_tempDirList = new List<Vector3>();
		}

		protected override void reset()
		{
			base.reset();
			_tempPosList.Clear();
			_tempDirList.Clear();
		}

		public override void build(MBFeature feature,Color color, MBLayerRenderData layer, MBStyleExpressionContext ctx)
		{
			build(feature, 10.0f,color);
		}

		public void build(MBFeature feature,float thickness, Color color)
		{
			reset();

			_feature = feature;

			thickness *= 2.0f;
			thickness /= 4096.0f;

			//buildClippedPathList();
			//if( _clippedPathList.Count == 0)
			//{
			//	return;
			//}

			bool auto_close = feature.type == MBFeatureType.polygon;
			foreach (short[] path in feature.pathList)
			{
				buildLineMesh(path, thickness, false, auto_close);
			}

			//foreach (short[] path in _feature.pathList)
			//{
			//	if( feature.type == MBFeatureType.polygon)
			//	{
			//		buildLineMesh(path, thickness, false, true);
			//	}
			//	else
			//	{
			//		buildLineMesh(path, thickness, false, false);
			//	}
			//}

			//_feature.setMesh(_vertexList.ToArray(), _indexList.ToArray());

			makeColors(color);
		}

		//protected void buildClippedPathList()
		//{

		//	//			bool auto_close = _feature.type == MBFeatureType.polygon;

		//	int context_id = NativeModule.MB_createContextSafe();

		//	// input
		//	foreach (short[] path in _feature.pathList)
		//	{
		//		int path_id = NativeModule.MB_LineBoundClip_beginPath(context_id);

		//		for(int i = 0; i < path.Length/2; ++i)
		//		{
		//			NativeModule.MB_LineBoundClip_addPathPoint(context_id, path_id, path[i * 2 + 0], path[i * 2 + 1]);
		//		}
		//	}

		//	//build
		//	NativeModule.MB_LineBoundClip_build(context_id);

		//	//
		//	int resultPathCount = NativeModule.MB_LineBoundClip_getResultPathCount(context_id);
		//	for(int i = 0; i < resultPathCount; ++i)
		//	{
		//		List<IntPoint> path = new List<IntPoint>();
		//		int pointCount = NativeModule.MB_LineBoundClip_getResultPathPointCount(context_id, i);

		//		for(int j = 0; j < pointCount; ++j)
		//		{
		//			int x = NativeModule.MB_LineBoundClip_getResultPathX(context_id, i, j);
		//			int y = NativeModule.MB_LineBoundClip_getResultPathY(context_id, i, j);

		//			path.Add(new IntPoint(x, y));
		//		}

		//		_clippedPathList.Add(path);
		//	}

		//	NativeModule.MB_releaseContextSafe(context_id);

		//}

		//protected void buildLineMesh(short[] path, float thinkness,bool normal_as_thickness,bool auto_close)
		protected void buildLineMesh(short[] path, float thinkness, bool normal_as_thickness, bool auto_close)
		{
			_tempPosList.Clear();
			_tempDirList.Clear();

			for (int i = 0; i < path.Length / 2; ++i)
			{
				int x = (int)path[i * 2 + 0];
				int y = (int)path[i * 2 + 1];

				Vector3 pos = new Vector3(x, -y, 0);
				pos.x /= 4096;
				pos.y /= 4096;

				_tempPosList.Add(pos);

				if (i > 0)
				{
					Vector3 dir = pos - _tempPosList[i - 1];
					dir.Normalize();
					_tempDirList.Add(dir);
				}

				//if( i == (path.Count - 1) && auto_close)
				//{
				//	Vector3 pos_first = new Vector3(path[0].X, -path[0].Y, 0);
				//	pos_first /= 4096;

				//	_tempPosList.Add(pos_first);

				//	Vector3 dir = pos_first - pos;
				//	dir.Normalize();
				//	_tempDirList.Add(dir);
				//}
			}

			for (int i = 0; i < _tempDirList.Count; ++i)
			{
				Vector3 dir = _tempDirList[i];
				if (i - 1 >= 0)
				{
					dir = (dir + _tempDirList[i - 1]) / 2.0f;
				}

				Vector3 right = Vector3.Cross(dir, Vector3.forward);
				Vector3 begin = _tempPosList[i];
				Vector3 end = _tempPosList[i + 1];

				int vindex = _vertexList.Count;

				_vertexList.Add(begin - right * thinkness);
				_vertexList.Add(begin + right * thinkness);
				if(normal_as_thickness)
				{
					_normalList.Add(-right);
					_normalList.Add(right);
				}
				else
				{
					_normalList.Add(-Vector3.forward);
					_normalList.Add(-Vector3.forward);
				}

				if (i == _tempDirList.Count - 1)
				{
					_vertexList.Add(end - right * thinkness);
					_vertexList.Add(end + right * thinkness);

					if (normal_as_thickness)
					{
						_normalList.Add(-right);
						_normalList.Add(right);
					}
					else
					{
						_normalList.Add(-Vector3.forward);
						_normalList.Add(-Vector3.forward);
					}
				}

				_indexList.Add((ushort)(vindex + 0));
				_indexList.Add((ushort)(vindex + 2));
				_indexList.Add((ushort)(vindex + 1));

				_indexList.Add((ushort)(vindex + 1));
				_indexList.Add((ushort)(vindex + 2));
				_indexList.Add((ushort)(vindex + 3));
			}
		}
	}
}
