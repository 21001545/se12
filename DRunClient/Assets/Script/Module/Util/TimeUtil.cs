using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace Festa.Client.Module
{
	public static class TimeUtil
	{
		private static DateTime timestamp_origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0);

		public static long msSecond = 1000;
		public static long msMinute = 60 * msSecond;
		public static long msHour = 60 * msMinute;
		public static long msDay = 24 * msHour;
		public static long msWeek = 7 * msDay;

		/*
			1970년 1월 1일은 목요일
			목요일 부터 일요일까지의 시간값 = 4일
		*/
		public static long msFirstWeekMod = 3 * msDay;

		public static DateTime dateTimeFromUnixTimestamp(long timestamp)
		{
			return timestamp_origin.AddMilliseconds( timestamp);
		}

		public static long unixTimestampFromDateTime(DateTime date)
		{
			TimeSpan diff = date - timestamp_origin;
			return (long)System.Math.Floor( diff.TotalMilliseconds);
		}

		public static DateTime futureFromNow(long period)
		{
			return DateTime.UtcNow.AddMilliseconds( period);
		}

		public static long unixTimestampUtcNow()
		{
			return unixTimestampFromDateTime( DateTime.UtcNow);
		}

		public static DateTime todayBegin()
		{
			DateTime dtNow = DateTime.Now;
			DateTime todayBegin = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

			return todayBegin;
		}

		public static DateTime todayBeginUTC()
		{
			DateTime dtNow = DateTime.Now;
			DateTime todayBegin = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 0, 0);

			return todayBegin.ToUniversalTime();
		}

		public static DateTime tommorowBegin()
		{
			return todayBegin().AddDays(1);
		}

		public static DateTime tommorowBeginUTC()
		{
			return todayBeginUTC().AddDays(1);
		}

		public static long timezoneOffset()
		{
			TimeSpan offset = TimeZoneInfo.Local.BaseUtcOffset;
			return (long)offset.TotalMilliseconds;
		}

		public static long todayDayCount()
		{
			return (unixTimestampUtcNow() + timezoneOffset()) / msDay;
		}

		public static long thisWeekCount()
		{
			return (unixTimestampUtcNow() + timezoneOffset() - msFirstWeekMod) / msWeek;
		}

		public static long thisWeekEndTime()
		{
			return nowUTC_BeginWeek(timezoneOffset()) + msWeek;
		}

		public static long thisMonthCount()
		{
			DateTime dateTime = dateTimeFromUnixTimestamp(unixTimestampUtcNow() + timezoneOffset());
			return (dateTime.Year - 1970) * 12 + dateTime.Month - 1;
		}

		public static long thisYearCount()
		{
			DateTime dateTime = dateTimeFromUnixTimestamp(unixTimestampUtcNow() + timezoneOffset());
			return dateTime.Year - 1970;
		}

		public static long nowUTC_Begin(long timezone_offset,long unit_time)
		{
			long local = unixTimestampUtcNow() + timezone_offset;
			local -= local % unit_time;
			return local - timezone_offset;
		}

		public static long nowUTC_BeginWeek(long timezone_offset)
		{
			return nowUTC_Begin(timezone_offset - msFirstWeekMod, msWeek);
		}

		public static DateTime dateFromDayCountUTC(long day,long timezone_offset)
		{
			return dateTimeFromUnixTimestamp(day * msDay - timezone_offset);
		}

		public static DateTime dateFromWeekCountUTC(long week,long timezone_offset)
		{
			long utc = week * msWeek + msFirstWeekMod - timezone_offset;
			return dateTimeFromUnixTimestamp(utc);
		}

		public static DateTime dateFromMonthCountUTC(long monthCount,long timezone_offset)
		{
			int month = (int)((monthCount % 12) + 1);
			int year = (int)((monthCount / 12) + 1970);

			return (new DateTime(year, month, 1)).AddMilliseconds(-timezone_offset);
		}

		public static DateTime dateFromYearCountUTC(long yearCount,long timezone_offset)
		{
			return (new DateTime((int)yearCount + 1970, 1, 1)).AddMilliseconds(-timezone_offset);
		}

		public static long dayCountFromDate(DateTime dtTime,long timezone_offset)
		{
			return (unixTimestampFromDateTime(dtTime) + timezone_offset) / msDay;
		}

		static readonly string[] formats = {
			"yyyy-MM-ddTHH:mm:ss.fff",
			"yyyy-MM-ddTHH:mm:ss.fffffff",
			// Basic formats
			"yyyyMMddTHHmmsszzz",
			"yyyyMMddTHHmmsszz",
			"yyyyMMddTHHmmssZ",
			// Extended formats
			"yyyy-MM-ddTHH:mm:sszzz",
			"yyyy-MM-ddTHH:mm:sszz",
			"yyyy-MM-ddTHH:mm:ssZ",
			// All of the above with reduced accuracy
			"yyyyMMddTHHmmzzz",
			"yyyyMMddTHHmmzz",
			"yyyyMMddTHHmmZ",
			"yyyy-MM-ddTHH:mmzzz",
			"yyyy-MM-ddTHH:mmzz",
			"yyyy-MM-ddTHH:mmZ",
			// Accuracy reduced to hours
			"yyyyMMddTHHzzz",
			"yyyyMMddTHHzz",
			"yyyyMMddTHHZ",
			"yyyy-MM-ddTHHzzz",
			"yyyy-MM-ddTHHzz",
			"yyyy-MM-ddTHHZ"
			
			};
		

		public static bool tryParseISO8601(string str,out DateTime result)
		{
			return DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
		}
	}

}