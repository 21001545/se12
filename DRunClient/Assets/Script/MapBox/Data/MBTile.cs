using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBTile
	{
		public List<MBLayer> layers;

		[SerializeOption(SerializeOption.NONE)]
		public MBTileCoordinate _tilePos;

		[SerializeOption(SerializeOption.NONE)]
		public Dictionary<int, MBLayer> _layerMap;

		[SerializeOption(SerializeOption.NONE)]
		public Dictionary<MBStyle, List<MBLayerRenderData>> _dicStyleRenderData;

		public void postProcess()
		{
			_layerMap = new Dictionary<int, MBLayer>();

			foreach(MBLayer layer in layers)
			{
				layer._tile = this;
				layer.buildHasedValueList();

				foreach (MBFeature feature in layer.featureList)
				{
					feature._tile = this;
					feature._layer = layer;

					feature.buildProperties(layer);

					//if( is_building)
					//{
					//	feature.buildBound();
					//}
				}

				_layerMap.Add(EncryptUtil.makeHashCode(layer.name), layer);
			}

			_dicStyleRenderData = new Dictionary<MBStyle, List<MBLayerRenderData>>();
		}

		public MBLayer getLayer(int nameCode)
		{
			MBLayer layer;
			if( _layerMap.TryGetValue( nameCode, out layer))
			{
				return layer;
			}
			return null;
		}

		public void mergeFrom(MBTile tile)
		{
			foreach(MBLayer layer in tile.layers)
			{
				if(getLayer( EncryptUtil.makeHashCode(layer.name)) == null)
				{
					layers.Add(layer);
				}
			}

			postProcess();
		}
	}
}
