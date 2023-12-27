using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public class LocaleDevice_Android : AbstractLocaleDevice
	{
		AndroidJavaObject _objActivity;

		public override void init()
		{
			AndroidJavaClass classActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			_objActivity = classActivity.GetStatic<AndroidJavaObject>("currentActivity");
		}

		public override string getCountryCode()
		{
			try
			{
				using (AndroidJavaObject objResources = _objActivity.Call<AndroidJavaObject>("getResources"))
				{
					using (AndroidJavaObject objConfiguration = objResources.Call<AndroidJavaObject>("getConfiguration"))
					{
						using (AndroidJavaObject objLocale = objConfiguration.Get<AndroidJavaObject>("locale"))
						{
							string cc = objLocale.Call<string>("getCountry");
							return cc;
						}
					}
				}

			}
			catch(Exception e)
			{
				Debug.LogError(e);
				return "KR";
			}


			//TelephonyManager tm = (TelephonyManager)this.getSystemService(Context.TELEPHONY_SERVICE);
			//String countryCodeValue = tm.getNetworkCountryIso();

			//String locale = context.getResources().getConfiguration().locale.getCountry();

		}
	}
}
