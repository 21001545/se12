using Festa.Client;
using Festa.Client.RefData;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace DRun.Client.Module
{
    public static class StringUtil
    {
        // 한글/영문/숫자만 가능
        public static bool checkForNickname(string name)
        {
//			2~10글자
            //		한글 / 영문 / 숫자사용 가능(특수문자, 이모티콘 사용 불가)

            if (name.Length < 2 || name.Length > 10)
            {
                return false;
            }

            Regex check = new Regex(@"[0-9a-zA-Zㄱ-힣]");
            if (check.IsMatch(name) == false)
            {
                return false;
            }

            return true;
        }

        public static bool checkEMail(string email)
        {
			bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
            return valid;
        }

        public static bool checkPassword_1(string password)
        {
            if (password.Length < 8 || password.Length > 15)
            {
                return false;
            }

            // 이건 필요없을것 같은데
            Regex check = new Regex(@"[0-9a-zA-Z]");
            if (check.IsMatch(password) == false)
            {
                return false;
            }

            return true;
        }

        public static bool checkPassword_2(string password)
        {
            //Regex eLower = new Regex(@"[a-z]");
            //Regex eUpper = new Regex(@"[A-Z]");

            Regex eEnglish = new Regex(@"[a-zA-Z]");
            Regex eNumber = new Regex(@"[0-9]");
            Regex eSpecial = new Regex(@"[~!@\#$%^&*\()\=+|\\/:;?""<>']");

            int match_count = 0;

            if (eEnglish.IsMatch(password))
            {
                match_count++;
            }

            if (eNumber.IsMatch(password))
            {
                match_count++;
            }

            if (eSpecial.IsMatch(password))
            {
                match_count++;
            }

            return match_count >= 2;
        }

        public static double toDRN(long peb)
        {
            return GlobalRefDataContainer.getRefDataHelper().toDRN(peb);
        }

        public static string toDRNStringDefault(long peb)
        {
            return toDRNStringGrouped(peb);
        }

        public static string toDRNStringAll(long peb)
        {
            return toDRN(peb).ToString();
        }

        public static string toDRNStringGrouped(long peb)
        {
            double drn = toDRN(peb);
            drn = Math.Ceiling(drn * 10000.0) / 10000.0;

            // 백만
            if (drn > 1000000.0)
            {
                return (drn / 1000000.0).ToString("F4").TrimAllZeroWithinFloatingPoints() + "M";
            }
            else if (drn > 1000.0)
            {
                return (drn / 1000.0).ToString("F4").TrimAllZeroWithinFloatingPoints() + "K";
            }
            else
            {
                return drn.ToString("F4").TrimAllZeroWithinFloatingPoints();
            }
        }

        public static string toStaminaString(int stamina)
        {
            double d_stamina = (double)stamina / 100.0;
            d_stamina = System.Math.Floor(d_stamina * 100.0) / 100.0;
            return d_stamina.ToString();
        }

        // 반올린되는 이슈가 있어서
        public static string toDistanceString(double km)
        {
            double distance = System.Math.Floor(km * 100.0) / 100.0;
            return distance.ToString("N2");
        }

        public static string toCaloriesString(double cal)
        {
            return cal.ToString("N0");
        }

        public static string toStatDistanceString(double km)
        {
            return toDistanceString(km).TrimAllZeroWithinFloatingPoints();
        }

        public static string toRunningTimeString(TimeSpan time)
        {
            return $"{time.Hours.ToString("D2")}:{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}";
        }

        public static string toRecordTime(DateTime time)
        {
            return $"{time.Year}.{time.Month}.{time.Day} {time.Hour.ToString("D2")}:{time.Minute.ToString("D2")}";
        }

        // 소수점 자릿수 세는 함수
        public static int GetSignificantNumberOfDecimalPlaces(double d)
        {
            //string inputStr = d.ToString(CultureInfo.InvariantCulture);
            //string inputStr = string.Format("{0:F}", d);
            string inputStr = d.ToString("F20").TrimAllZeroWithinFloatingPoints();

            Debug.Log(inputStr);

            int decimalIndex = inputStr.IndexOf(".") + 1;
            if (decimalIndex == 0)
            {
                return 0;
            }
            return inputStr.Substring(decimalIndex).TrimEnd(new[] { '0' }).Length;
        }

        public static string ColorifyString(string str, string color = "red") => $"<color={color}>{str}</color>";

		/// <summary>
		/// drn-345
		/// 소수점 자리가 0으로 끝날 때, 안 보이게 할 수 있을까요?
		///  소수
		///  ex. 12.00 → 12 (소수점에 0밖에 없어서 . 포함해서 0 모두 제거)
		///      12.10 → 12.1
		///      232323.80 -> 232323.8 (마지막 0 만 자름)
		///      223.11 -> 223.11 (0 없어서 처리 없음)
		///
		///  자연수
		///  ex. 20000 -> 20000
		///      20 -> 20
		///      000123 -> 000123
		///      11223344 -> 11223344
		///      151525 -> 151525
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static string TrimAllZeroWithinFloatingPoints(this string self)
		{
			if (self.Length == 0)
				return string.Empty;

            if( self.Contains('.') == false)
            {
                return self;
            }

            return self.TrimEnd('0').TrimEnd(".");

			//int deleteCount = 0;
			//var reversed = new string(self.Reverse().ToArray());
			//using var ch = reversed.GetEnumerator();
			//int dotCount = 0;

			//while (ch.MoveNext())
			//{
			//	if (ch.Current == '.')
			//	{
			//		++dotCount;
			//		break;
			//	}

			//	if (ch.Current != '0')
			//		break;

			//	++deleteCount;
			//}

			//// FIXED: 2023-01-13 윤상
			//// . 이 없으면 자연수로 인식하고 trim 안함!
			//// float 수가 자연수 일 때, 뒤의 모든 0 을 trim 하는 에러 가 있었음..
			//// 다음 부터는 edge case 확인을 더 꼼꼼히..
			//if (dotCount == 0)
			//	return self;

			//string removed = reversed.Remove(0, deleteCount);
			//if (string.IsNullOrEmpty(removed))
			//	return string.Empty;

			//// 마지막 문자가 . -> 부동소수점 숫자가 없음.
			//if (removed[0] == '.')
			//	removed = removed.Remove(0, 1);

			//self = new string(removed.Reverse().ToArray());

			//return self;
		}
	}
}