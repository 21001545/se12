using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module
{
	public class LocaleManager
	{
		private AbstractLocaleDevice _device;

		public static LocaleManager create()
		{
			LocaleManager locale = new LocaleManager();
			locale.init();
			return locale;
		}

		private void init()
		{
			createDevice();
		}

		private void createDevice()
		{
#if UNITY_EDITOR
			_device = new LocaleDevice_Editor();
#else

#if UNITY_IOS
			_device = new LocaleDevice_iOS();
#elif UNITY_ANDROID
			_device = new LocaleDevice_Android();
#endif

#endif
			_device.init();
		}

		public string getCountryCode()
		{
			return _device.getCountryCode();
		}
	}
}
