using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientPushNotificationConfig
	{
		public int config_id;
		public int status;

		public class Status
		{
			public const int disable = 0;
			public const int enable = 1;
		}

		public class ConfigID
		{
			public static int social = EncryptUtil.makeHashCode("setting.push.social");
			public static int moment = EncryptUtil.makeHashCode("setting.push.moment");
			public static int message = EncryptUtil.makeHashCode("setting.push.message");
			public static int place = EncryptUtil.makeHashCode("setting.push.setting.push.place");

			// 2022.04.19 정소현 - 오타 수정했어요!!
			public static int activity = EncryptUtil.makeHashCode("setting.push.activity");
		}
	}
}
