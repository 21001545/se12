using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.UI
{
	public class UIFormatter
	{
		public static string timePeroid(TimeSpan span)
		{
			if( span.TotalHours < 1.0f)
			{
				return string.Format("{0}:{1}", span.Minutes.ToString("D2"), span.Seconds.ToString("D2"));
			}
			else if( span.TotalDays < 1.0f)
			{
				return string.Format("{0}:{1}", span.Hours.ToString("D2"), span.Seconds.ToString("D2"));
			}
			else
			{
				return string.Format("{0} {1}:{2}", span.Days.ToString("D0"), span.Hours.ToString("D2"), span.Seconds.ToString("D2"));
			}
		}

		public static string timePeroidUnit(TimeSpan span)
		{
			if( span.TotalHours < 1.0f)
			{
				return "sec";
			}
			else if( span.TotalDays < 1.0f)
			{
				return "min";
			}
			else
			{
				return "min";
			}
		}

		public static string distance(double distance)
		{
			if( distance < 1.0)
			{
				return ((int)(distance * 100)).ToString("D0");
			}
			else
			{
				return distance.ToString("F1");
			}
		}

		public static string distanceUnit(double distance)
		{
			if( distance < 1.0)
			{
				return "m";
			}
			else
			{
				return "km";
			}
		}

		public static string deltaN0(int value)
		{
			if( value <= 0)
			{
				return value.ToString("N0");
			}
			else
			{
				return string.Format("+{0}", value.ToString("N0"));
			}
		}
	}
}
