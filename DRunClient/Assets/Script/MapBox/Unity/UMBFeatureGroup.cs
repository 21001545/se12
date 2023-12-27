using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBFeatureGroup
	{
		private MBTileCoordinate _centerTilePos;
		private MBBound			_bound;
		private List<MBFeature> _featureList;
		private int _extrudeHeight;
		private Color _color;

		public Color getColor()
		{
			return _color;
		}

		public MBBound getBound()
		{
			return _bound;
		}

		public List<MBFeature> getFeatureList()
		{
			return _featureList;
		}

		public static UMBFeatureGroup create(MBFeature feature,MBTileCoordinate centerTilePos,Color color)
		{
			UMBFeatureGroup group = new UMBFeatureGroup();
			group.init(feature,centerTilePos, color);
			return group;
		}

		private void init(MBFeature feature,MBTileCoordinate centerTilePos,Color color)
		{
			_centerTilePos = centerTilePos;

			_color = color;
			_bound = getOffsetBound(feature);

			_featureList = new List<MBFeature>();
			_featureList.Add(feature);

			_extrudeHeight = getExtrudeHeight(feature);
		}


		private int getExtrudeHeight(MBFeature feature)
		{
			if ((string)feature.get(MBPropertyKey.extrude, "false") == "true")
			{
				return (int)((double)feature.get(MBPropertyKey.height, 0.0));
			}

			return 0;
		}

		//public bool isOverlap(MBBound test)
		public bool isOverlap(MBFeature feature,Color color)
		{
			int extrudeHeight = getExtrudeHeight(feature);
			if (extrudeHeight != _extrudeHeight)
			{
				return false;
			}

			if (_color != color)
			{
				return false;
			}

			return true;

			//if( feature.id != 0 && feature.id != _featureList[ 0].id)
			//{
			//	return false;
			//}

			//Vector2Int offset = feature._tile._tilePos.offsetFrom(_centerTilePos);
			//return MBBound.checkOverlapWithOffset(_bound, feature.getBound(), offset * 4096);
		}

		public void addFeature(MBFeature feature)
		{
			_featureList.Add(feature);

			_bound.merge(getOffsetBound( feature));
		}

		private MBBound getOffsetBound(MBFeature feature)
		{
			return MBBound.fromBoundWithOffset(feature.getBound(), feature._tile._tilePos.offsetFrom(_centerTilePos) * 4096);
		}
	}
}
