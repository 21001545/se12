using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public static class Haversine
	{
		private static double toRadian = Math.PI / 180.0;

		public static double calc(double lat1, double lng1, double lat2, double lng2)
		{
			int r = 6371; // average radius of the earth in km
			double dLat = toRadian * (lat2 - lat1);
			double dLon = toRadian * (lng2 - lng1);
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
			   Math.Cos(toRadian * lat1) * Math.Cos(toRadian * lat2)
			  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			double d = r * c;
			return d;
		}
	}
}
