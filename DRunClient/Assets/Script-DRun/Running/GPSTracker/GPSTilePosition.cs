using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using Festa.Client.NetData;

namespace DRun.Client.Running
{
	public class GPSTilePosition
	{
		public const int zoom = 18;

		public int index;
		public long time;
		public DoubleVector2 tile_pos;
		public DoubleVector2 gps_pos;
		public double gps_alt;

		public double deltaDistance; // km
		public double deltaCalorie;
		public double deltaTime;    // 초
		public double deltaSpeedKMH;
		public bool exceedSpeedLimit;

		public static implicit operator MBLongLatCoordinate(GPSTilePosition pos)
		{
			return new MBLongLatCoordinate(pos.gps_pos.x, pos.gps_pos.y);
		}

		public void pack(MessagePacker packer)
		{
			packer.packLong(time);
			packer.packDouble(gps_pos.x);
			packer.packDouble(gps_pos.y);
			packer.packDouble(gps_alt);

			packer.packDouble(deltaDistance);
			packer.packDouble(deltaCalorie);
			packer.packDouble(deltaTime);
			packer.packDouble(deltaSpeedKMH);
			packer.packBoolean(exceedSpeedLimit);
		}

		public static GPSTilePosition unpack(int index,MessageUnpacker unpacker)
		{
			long time = unpacker.unpackLong();
			double longitude = unpacker.unpackDouble();
			double latitude = unpacker.unpackDouble();
			double altitude = unpacker.unpackDouble();
			double deltaDistance = unpacker.unpackDouble();
			double deltaCalorie = unpacker.unpackDouble();
			double deltaTime = unpacker.unpackDouble();
			double deltaSpeedKMH = unpacker.unpackDouble();
			bool exceedSpeedLimit = unpacker.unpackBoolean();

			GPSTilePosition pos = create(index, time, longitude, latitude, altitude);
			pos.deltaDistance = deltaDistance;
			pos.deltaCalorie = deltaCalorie;
			pos.deltaTime = deltaTime;
			pos.deltaSpeedKMH = deltaSpeedKMH;
			pos.exceedSpeedLimit= exceedSpeedLimit;

			return pos;
		}

		public static GPSTilePosition create(int index,ClientLocationLog log)
		{
			return create(index, TimeUtil.unixTimestampFromDateTime(log.event_time), log.longitude, log.latitude, log.altitude);
		}

		public static GPSTilePosition create(int index,long time,double longitude,double latitude,double altitude)
		{
			GPSTilePosition pos = new GPSTilePosition();
			pos.index = index;
			pos.time = time;
			pos.gps_pos = new DoubleVector2(longitude, latitude);
			pos.gps_alt = altitude;

			double tile_x;
			double tile_y;

			MapBoxUtil.getTileXY(pos.gps_pos.x, pos.gps_pos.y, zoom, out tile_x, out tile_y);
			pos.tile_pos = new DoubleVector2(tile_x, tile_y);
			pos.deltaDistance = 0;
			pos.deltaCalorie = 0;
			pos.deltaTime = 0;
			pos.deltaSpeedKMH = 0;
			pos.exceedSpeedLimit = false;

			return pos;
		}

		//public GPSTilePosition(long time,double longitude,double latitude,double tile_x,double tile_y)
		//{
		//	this.index = -1;
		//	this.time = time;
		//	tile_pos = new DoubleVector2(tile_x, tile_y);
		//	gps_pos = new DoubleVector2(longitude, latitude);
		//	gps_alt = 0;

		//	deltaDistance = 0;
		//	deltaCalorie = 0;
		//	deltaTime = 0;
		//	deltaSpeedKMH = 0;
		//	exceedSpeedLimit = false;
		//}
	}
}
