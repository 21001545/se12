using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public struct MBTileCoordinateDouble
	{
		public int zoom;
		public DoubleVector2 tile_pos;

		public double tile_x
		{
			get
			{
				return tile_pos.x;
			}
			set
			{
				tile_pos.x = value;
			}
		}

		public double tile_y
		{
			get
			{
				return tile_pos.y;
			}
			set
			{
				tile_pos.y = value;
			}
		}

		public bool isLeftEdge()
		{
			return (int)tile_pos.x == 0;
		}

		public bool isRightEdge()
		{
			int max = MapBoxUtil.maxTile(zoom);
			return (int)tile_pos.x == (max - 1);
		}

		public MBTileCoordinateDouble(int zoom,double tile_x,double tile_y)
		{
			this.zoom = zoom;
			this.tile_pos = new DoubleVector2(tile_x, tile_y);
		}

		public void validateByWrap()
		{
			tile_pos.x = MapBoxUtil.wrapTile(tile_pos.x, zoom);
			tile_pos.y = MapBoxUtil.wrapTile(tile_pos.y, zoom);
		}

		public bool isValid()
		{
			int max = MapBoxUtil.maxTile(zoom);
			if( tile_x < 0 || tile_x > max)
			{
				return false;
			}
			if( tile_y < 0 || tile_y > max)
			{
				return false;
			}

			return true;
		}

		public bool isEqualInteger(MBTileCoordinateDouble b)
		{
			if( zoom != b.zoom)
			{
				return false;
			}

			if( (int)tile_x != (int)b.tile_x ||
				(int)tile_y != (int)b.tile_y)
			{
				return false;
			}

			return true;
		}

		public MBTileCoordinate toInteger()
		{
			return new MBTileCoordinate(zoom, (int)tile_x, (int)tile_y);
		}

		public override string ToString()
		{
			return string.Format("(zoom[{0}],tile_x[{1}],tile_y[{2}])", zoom, tile_x, tile_y);
		}

		public MBLongLatCoordinate toLongLat()
		{
			return MBLongLatCoordinate.fromTilePos(this);
		}

		public static MBTileCoordinateDouble fromLonLat(MBLongLatCoordinate pos, int zoom)
		{
			return fromLonLat(pos.lon, pos.lat, zoom);
		}

		public static MBTileCoordinateDouble fromInteger(MBTileCoordinate pos)
		{
			MBTileCoordinateDouble c = new MBTileCoordinateDouble();
			c.zoom = pos.zoom;
			c.tile_x = pos.tile_x;
			c.tile_y = pos.tile_y;
			return c;
		}

		public static MBTileCoordinateDouble fromLonLat(double lon, double lat, int zoom)
		{
			MBTileCoordinateDouble c = new MBTileCoordinateDouble();

			double dx;
			double dy;

			MapBoxUtil.getTileXY(lon, lat, zoom, out dx, out dy);
			c.tile_x = dx;
			c.tile_y = dy;
			c.zoom = zoom;

			return c;
		}

	}
}
