using Festa.Client.Module;
using System;
using UnityEngine;

namespace Festa.Client.Data
{
	public class StartupContext
	{
		public string firebase_id;
		public string phone_number;
		public string provider_id;
		public string device_id;
		public long timezone_offset;

		//
		public bool is_newaccount;
		public bool is_first_run;

		public string public_ip;	// 나의 공인 IP

		public static StartupContext create()
		{
			StartupContext ctx = new StartupContext();
			ctx.init();
			return ctx;
		}

		private void init()
		{
#if UNITY_EDITOR
			initEditor();
#else
			initDevice();
#endif
		}
		
		private void initEditor()
		{
			string editor_device_id = SystemInfo.deviceUniqueIdentifier;

			device_id = editor_device_id + EncryptUtil.makeHashCode(Application.dataPath).ToString();
			firebase_id = "UnityEditor_" + device_id;
			phone_number = "";
			provider_id = "";
			timezone_offset = TimeUtil.timezoneOffset();
		}

		private void initDevice()
		{
			device_id = SystemInfo.deviceUniqueIdentifier;
			timezone_offset = TimeUtil.timezoneOffset();
		}
	}
}
