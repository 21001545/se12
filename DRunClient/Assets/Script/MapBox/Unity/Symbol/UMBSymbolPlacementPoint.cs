using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBSymbolPlacementPoint : UMBSymbolPlacement
	{
		protected override void createPoints()
		{
			if( _feature.type == MBFeatureType.point)
			{
				if( _feature.pathList.Count > 0)
				{
					Vector2Int localPos = Vector2Int.zero;
					localPos = new Vector2Int(_feature.pathList[0][0],_feature.pathList[0][1]);

					UMBSymbolPoint point = UMBSymbolPoint.create(this, tilePosFromLocalPos(localPos), null);
					_symbolPoints.Add(point);
				}
			}
			else
			{
				List<MBAnchors> anchors_list = _feature.getAnchors();
				if( anchors_list == null)
				{
					anchors_list = _feature.buildAnchors();
				}
				
				foreach(MBAnchors anchors in anchors_list)
				{
					MBAnchorCursor cursor = anchors.getCursor(anchors.getLength() / 2.0f);

					UMBSymbolPoint point = UMBSymbolPoint.create(this, tilePosFromLocalPos(cursor.getPosition()), null);
					_symbolPoints.Add(point);
				}
			}

		}
	}
}
