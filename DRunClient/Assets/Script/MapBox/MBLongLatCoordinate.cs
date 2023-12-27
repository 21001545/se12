using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public struct MBLongLatCoordinate : IEquatable<MBLongLatCoordinate>
	{
		public DoubleVector2 pos;

		public double lon => pos.x;
		public double lat => pos.y;

		public MBLongLatCoordinate(double lon,double lat)
		{
			pos.x = lon;
			pos.y = lat;
		}

		public MBLongLatCoordinate(DoubleVector2 pos)
		{
			this.pos = pos;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals((MBLongLatCoordinate)obj);
		}

		public bool Equals(MBLongLatCoordinate obj)
		{
			return pos == obj.pos;
		}

		public override string ToString()
		{
			return string.Format("(lon[{0}],lat[{1}])", lon, lat);
		}

		public bool isZero()
		{
			return pos.x == 0 && pos.y == 0;
		}

		public double distanceFrom(MBLongLatCoordinate from)
		{
			return MapBoxUtil.distance(pos.x, pos.y, from.pos.x, from.pos.y);
		}

		public static MBLongLatCoordinate zero = new MBLongLatCoordinate(0, 0);

		public static MBLongLatCoordinate fromTileXY(int zoom,double tile_x,double tile_y)
		{
			double lon;
			double lat;
			MapBoxUtil.fromTileXY(tile_x, tile_y, zoom, out lon, out lat);

			return new MBLongLatCoordinate(lon, lat);
		}

		public static MBLongLatCoordinate fromTileXY(int zoom, DoubleVector2 tilePos)
		{
			double lon;
			double lat;
			MapBoxUtil.fromTileXY(tilePos.x, tilePos.y, zoom, out lon, out lat);

			return new MBLongLatCoordinate(lon, lat);
		}

		public static MBLongLatCoordinate fromTilePos(MBTileCoordinateDouble tilePos)
		{
			return fromTileXY(tilePos.zoom, tilePos.tile_x, tilePos.tile_y);
		}

		public static MBLongLatCoordinate fromJson(JsonObject data)
		{
			MBLongLatCoordinate c = new MBLongLatCoordinate();
			c.pos.x = data.getDouble("longitude");
			c.pos.y = data.getDouble("latitude");
			return c;
		}

		public JsonObject toJson()
		{
			JsonObject json = new JsonObject();
			json.put("longitude", pos.x);
			json.put("latitude", pos.y);
			return json;
		}

	}
}
