using UnityEngine;

namespace Festa.Client.MapBox
{
	public static class MapBoxDefine
	{
		public const int tile_extent = 4096;
		//public static float scale_pivot = 0.07f;
		//public const float scale_pivot = 0.035f;
		public const float scale_pivot = 0.125f;

		public static Vector2Int tile_zoom_range = new Vector2Int(1, 16);
		public static Vector2Int control_zoom_range = new Vector2Int(1, 22);

		//
		public const int landTileGridZoom = 16;
		public const int landTileGridSize = 256;
		public const int landTileGridCount = 16;

		public static int makeLandTileGridKey(int grid_x,int grid_y)
		{
			return grid_x * 100 + grid_y;
		}

		public static int clampTileZoom(int zoom)
		{
			if( zoom < tile_zoom_range.x)
			{
				zoom = tile_zoom_range.x;
			}
			else if( zoom > tile_zoom_range.y)
			{
				zoom = tile_zoom_range.y;
			}
			return zoom;
		}

		public static int clampControlZoom(int zoom)
		{
			if (zoom < control_zoom_range.x)
			{
				zoom = control_zoom_range.x;
			}
			else if (zoom > control_zoom_range.y)
			{
				zoom = control_zoom_range.y;
			}
			return zoom;
		}

		public static float clampControlZoomFloat(float zoom)
		{
			return Mathf.Clamp(zoom, control_zoom_range.x, control_zoom_range.y);
		}

		public class ColliderType
		{
			public const int icon = 1;
			public const int text_box = 2;
			public const int text_along_line = 3;
		}
	}
}
