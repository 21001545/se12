using UnityEngine;
using System.Collections.Generic;
using System;

namespace Festa.Client.MapBox
{
	public struct MBTileCoordinate : IEquatable<MBTileCoordinate>
	{
		public int zoom;
		public int tile_x;
		public int tile_y;

		public bool isEqual(MBTileCoordinate c)
		{
			return zoom == c.zoom &&
					tile_x == c.tile_x &&
					tile_y == c.tile_y;
		}

		public MBTileCoordinate(int zoom,int tile_x,int tile_y)
		{
			this.zoom = zoom;
			this.tile_x = tile_x;
			this.tile_y = tile_y;
		}

		public bool isValid()
		{
			int tileMax = MapBoxUtil.maxTile(zoom);

			return tile_x >= 0 && tile_x < tileMax &&
					tile_y >= 0 && tile_y < tileMax;
		}

		public bool isLeftEdge()
		{
			return tile_x == 0;
		}

		public bool isRightEdge()
		{
			int tileMax = MapBoxUtil.maxTile(zoom);
			return tile_x == tileMax - 1;
		}

		public MBTileCoordinate offset(int offset_x,int offset_y)
		{
			return new MBTileCoordinate(zoom, tile_x + offset_x, tile_y + offset_y);
		}

		public MBTileCoordinate getValidByWrap()
		{
			int tile_x_wrapped = MapBoxUtil.wrapTile(tile_x, zoom);
			int tile_y_wrapped = MapBoxUtil.wrapTile(tile_y, zoom);
			return new MBTileCoordinate(zoom, tile_x_wrapped, tile_y_wrapped);
		}

		public Vector2Int offsetFrom(MBTileCoordinate from)
		{
			Vector2Int offset = new Vector2Int();
			offset.x = tile_x - from.tile_x;
			offset.y = tile_y - from.tile_y;
			return offset;
		}

		public static MBTileCoordinate fromLonLat(MBLongLatCoordinate pos,int zoom)
		{
			return fromLonLat(pos.lon, pos.lat, zoom);
		}

		public static MBTileCoordinate fromLonLat(double lon,double lat,int zoom)
		{
			MBTileCoordinate c = new MBTileCoordinate();

			double dx;
			double dy;

			MapBoxUtil.getTileXY(lon, lat, zoom, out dx, out dy);
			c.tile_x = (int)dx;
			c.tile_y = (int)dy;
			c.zoom = zoom;

			return c;
		}

		public static MBTileCoordinate fromTile(MBTileCoordinate from)
		{
			MBTileCoordinate c = new MBTileCoordinate();
			c.zoom = from.zoom;
			c.tile_x = from.tile_x;
			c.tile_y = from.tile_y;
			return c;
		}

		public override string ToString()
		{
			return string.Format("(zoom[{0}],tile_x[{1}],tile_y[{2}])", zoom, tile_x, tile_y);
		}

		public bool Equals(MBTileCoordinate other)
		{
			return isEqual(other);
		}

		public static bool Equals(MBTileCoordinate a,MBTileCoordinate b)
		{
			return a.isEqual(b);
		}

		public class EqualityComparer : IEqualityComparer<MBTileCoordinate>
		{
			public bool Equals(MBTileCoordinate x, MBTileCoordinate y)
			{
				return x.isEqual(y);
			}

			public int GetHashCode(MBTileCoordinate obj)
			{
				return obj.tile_x;
			}
		}

	}
}
