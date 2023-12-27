using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBSymbolPlacementLineCenter : UMBSymbolPlacement
	{
		protected override void createPoints()
		{
			if (_feature.type != MBFeatureType.linestring && _feature.type != MBFeatureType.polygon)
			{
				Debug.LogWarning($"fesature type must linestring or polygon : id[{_feature.id}]");
				return;
			}

			List<MBAnchors> anchors_list = _feature.getAnchors();
			if(anchors_list == null)
			{
				anchors_list = _feature.buildAnchors();
			}

			foreach(MBAnchors anchors in anchors_list)
			{
				MBAnchorCursor cursor = anchors.getCursor(anchors.getLength() / 2);
				Vector2Int pos = cursor.getPositionInt();

				_symbolPoints.Add(UMBSymbolPoint.create(this,tilePosFromLocalPos(pos), cursor));
			}
		}
	}
}
