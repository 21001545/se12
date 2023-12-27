using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public static class UnitDefine
	{
		public static class DistanceType
		{
			public const int unknown = 0;
			public static int cm = 1;
			public static int m = 2;
			public static int km = 3;
			public static int inch = 11;
			public static int ft = 12;
			public static int mil = 13;
		}

		public static class WeightType
		{
			public const int unknown = 0;
			public const int kg = 1;
			public const int lb = 2;
			public const int st = 3;
		}

		public static class TemperatureType
		{
			public const int unknown = 0;
			public const int c = 1;
			public const int f = 2;
		}

		public static double km_2_mil(double km)
		{
			return km / 1.609344f;
		}

		// meter to feet
		public static double m_2_f(double m)
		{
			return m * 3.28084;
		}

		public static double c_to_f(double c)
		{
			return (c * 1.8) + 32;
		}
	}
}
