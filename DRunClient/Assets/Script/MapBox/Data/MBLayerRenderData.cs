using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBLayerRenderData
	{
		private MBTile _tile;
		private MBStyleLayer	_layerStyle;
		private List<MBFeature> _featureList;

		private MBMesh _mergedMesh;
		private bool _isDynamicOpacity;

		public static int id_hillShade = EncryptUtil.makeHashCodePositive("hillshade");

		public MBStyleLayer getLayerStyle()
		{
			return _layerStyle;
		}

		public bool isEmpty()
		{
			return _featureList.Count == 0;
		}

		public List<MBFeature> getFeatureList()
		{
			return _featureList;
		}

		public void setMergedMesh(MBMesh mesh)
		{
			_mergedMesh = mesh;
		}

		public MBMesh getMergedMesh()
		{
			return _mergedMesh;
		}

		public bool hasValidMergedMesh()
		{
			return _mergedMesh != null && _mergedMesh.isEmpty() == false;
		}

		//public List<MBFeature> getOutlinePolygonList()
		//{
		//	return _outlinePolygonList;
		//}

		public bool isDynamicOpacity()
		{
			return _isDynamicOpacity;
		}

		public static MBLayerRenderData create(MBTile tile,MBStyleLayer layer_style, MBLayer source_layer,MBStyleExpressionContext ctx)
		{
			MBLayerRenderData data = new MBLayerRenderData();
			data.init(tile, layer_style, source_layer,ctx);
			return data;
		}

		private void init(MBTile tile,MBStyleLayer layer_style,MBLayer source_layer, MBStyleExpressionContext ctx)
		{
			_tile = tile;
			_layerStyle = layer_style;
			_featureList = new List<MBFeature>();

			ctx._zoom = source_layer._tile._tilePos.zoom;
			foreach(MBFeature feature in source_layer.featureList)
			{
				ctx._feature = feature;

				if( _layerStyle.evaluateFilter(ctx) == false)
				{
					continue;
				}

				_featureList.Add(feature);
			}

			checkDynamicOpacity();
		}

		private void checkDynamicOpacity()
		{
			_isDynamicOpacity = _layerStyle.getIDHashCode() == id_hillShade;
		}
	}
}
