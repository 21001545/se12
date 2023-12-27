using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Festa.Client.Module
{
	public class LocaleDevice_iOS : AbstractLocaleDevice
	{
#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern string IOSgetPhoneCountryCode();
#endif

		public override void init()
		{
			
		}

		public override string getCountryCode()
		{
#if UNITY_IOS
			return IOSgetPhoneCountryCode();
#else
			return "KR";
#endif
		}
	}
}
