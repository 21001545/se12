using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public abstract class AbstractLocaleDevice
	{
		public abstract void init();
		public abstract string getCountryCode(); // ISO3199-2 (2글자 국가코드 KR,US,GB...)
	
		// 언어는 native로 구현하기 곤란하다
		public virtual int getLanguageType()
		{
			switch( Application.systemLanguage)
			{
				case SystemLanguage.English:
					return LanguageType.en;
				case SystemLanguage.Korean:
					return LanguageType.ko;
				case SystemLanguage.ChineseTraditional:
					return LanguageType.zhTW;
				case SystemLanguage.ChineseSimplified:
					return LanguageType.zhCN;
				case SystemLanguage.German:
					return LanguageType.de;
				case SystemLanguage.French:
					return LanguageType.fr;
				case SystemLanguage.Portuguese:
					return LanguageType.ptPT;
				case SystemLanguage.Spanish:
					return LanguageType.esES;
				case SystemLanguage.Russian:
					return LanguageType.ru;
			}

			Debug.LogWarning($"unsupport language : {Application.systemLanguage}");

			// fallback, config로 해주는게 좋을듯
			return LanguageType.ko;
		}
	}
}
