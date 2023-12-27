using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module;

namespace Festa.GoogleMap
{
	public class GoogleMapPosition
	{
		private DoubleVector2 _LngLat;

		public double Latitude => _LngLat.y;
		public double Longitude => _LngLat.x;

		public static GoogleMapPosition create(double lng, double lat)
		{
			return create(new DoubleVector2(lng,lat));
		}

		public static GoogleMapPosition create(DoubleVector2 LngLat)
		{
			GoogleMapPosition pos = new GoogleMapPosition();
			pos.init(LngLat);
			return pos;
		}

		private void init(DoubleVector2 LngLat)
		{
			_LngLat = LngLat;
		}

		public GoogleMapPosition moveByPixel(long offset_x,long offset_y,int zoom)
		{
			GoogleMapAPIProjection projection = new GoogleMapAPIProjection(zoom);

			DoubleVector2 pixelPos = projection.FromCoordinatesToPixel( new DoubleVector2(_LngLat.y, _LngLat.x));

			pixelPos.x += offset_x;
			pixelPos.y += offset_y;

			DoubleVector2 newLngLat = projection.FromPixelToCoordinates(pixelPos);

			return create(newLngLat.x, newLngLat.y);
		}

	}
}
