using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public static class LanguageType
	{
		public static int en = EncryptUtil.makeHashCode("en");
		public static int ko = EncryptUtil.makeHashCode("ko");
		public static int zhCN = EncryptUtil.makeHashCode("zhCN");
		public static int zhTW = EncryptUtil.makeHashCode("zhTW");
		public static int de = EncryptUtil.makeHashCode("de");
		public static int fr = EncryptUtil.makeHashCode("fr");
		public static int ptPT = EncryptUtil.makeHashCode("ptPT");
		public static int esES = EncryptUtil.makeHashCode("esES");
		public static int ru = EncryptUtil.makeHashCode("ru");
		public static int jp = EncryptUtil.makeHashCode("jp");

		public static string getNameKey(int type)
		{
			string key = "LanguageName.en";
			if( type == en)
			{
				key = "LanguageName.en";
			}
			else if( type == ko)
			{
				key = "LanguageName.ko";
			}
			else if (type == zhCN)
			{
				key = "LanguageName.zhCN";
			}
			else if (type == zhTW)
			{
				key = "LanguageName.zhTW";
			}
			else if (type == de)
			{
				key = "LanguageName.de";
			}
			else if (type == fr)
			{
				key = "LanguageName.fr";
			}
			else if (type == ptPT)
			{
				key = "LanguageName.ptPT";
			}
			else if (type == esES)
			{
				key = "LanguageName.esES";
			}
			else if (type == ru)
			{
				key = "LanguageName.ru";
			}
			else if (type == jp)
			{
				key = "LanguageName.jp";
			}

			return key;
		}

		// fallback도 넣어줌
		public static string getAcceptLanguageValue(int type)
		{
			string key = "";
			if (type == en)
			{
				key = "en-US";
			}
			else if (type == ko)
			{
				key = "ko-KR; en-US";
			}
			else if (type == zhCN)
			{
				key = "zh-Hans; en-US";
			}
			else if (type == zhTW)
			{
				key = "zh-Hant; en-US";
			}
			else if (type == de)
			{
				key = "de; en-US";
			}
			else if (type == fr)
			{
				key = "fr; en-US";
			}
			else if (type == ptPT)
			{
				key = "pt; en-US";
			}
			else if (type == esES)
			{
				key = "es; en-US";
			}
			else if (type == ru)
			{
				key = "ru; en-US";
			}
			else if (type == jp)
			{
				key = "ja-JP; en-US";
			}

			return key;
		}

		// 언어 타입으로 국기 URL얻어 오기
		private static string _countryFlagURL = "https://festastorage.blob.core.windows.net/countryflag/{0}.png";

		public static string getCountryFlagURLFromLanguageType(int type)
		{
			string cc = "kr";
			if (type == en)
			{
				cc = "us";
			}
			else if (type == ko)
			{
				cc = "kr";
			}
			else if (type == zhCN)
			{
				cc = "cn";
			}
			else if (type == zhTW)
			{
				cc = "cn";
			}
			else if (type == de)
			{
				cc = "de";
			}
			else if (type == fr)
			{
				cc = "fr";
			}
			else if (type == ptPT)
			{
				cc = "pt";
			}
			else if (type == esES)
			{
				cc = "es";
			}
			else if (type == ru)
			{
				cc = "ru";
			}
			else if (type == jp)
			{
				cc = "jp";
			}

			return string.Format(_countryFlagURL, cc);
		}
/*
		public static int zhCN = EncryptUtil.makeHashCode("zhCN");
		public static int zhTW = EncryptUtil.makeHashCode("zhTW");
		public static int de = EncryptUtil.makeHashCode("de");
		public static int fr = EncryptUtil.makeHashCode("fr");
		public static int ptPT = EncryptUtil.makeHashCode("ptPT");
		public static int esES = EncryptUtil.makeHashCode("esES");
		public static int ru = EncryptUtil.makeHashCode("ru");
		public static int jp = EncryptUtil.makeHashCode("jp");
*/
		public static int getLanguageTypeFromDevice()
		{
			SystemLanguage deviceLanguage = Application.systemLanguage;

			switch(deviceLanguage)
			{
				case SystemLanguage.English:
					return en;
				case SystemLanguage.Korean:
					return ko;
				case SystemLanguage.ChineseSimplified:
					return zhCN;
				case SystemLanguage.ChineseTraditional:
					return zhTW;
				case SystemLanguage.German:
					return de;
				case SystemLanguage.French:
					return fr;
				case SystemLanguage.Portuguese:
					return ptPT;
				case SystemLanguage.Spanish:
					return esES;
				case SystemLanguage.Russian:
					return ru;
				case SystemLanguage.Japanese:
					return jp;
				default:
					return en;
			}
		}
	}
}
