using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class CountryPhoneNumber
	{
		public string regionCode; // 국가 코드
		public int countryCode;	// 국제 전화 번호
		public string countryName; // 국가명 sorting용

		public string getFlagURL()
		{
			return $"https://festastorage.blob.core.windows.net/countryflag/{regionCode.ToLower()}.png";
		}
	}
}
