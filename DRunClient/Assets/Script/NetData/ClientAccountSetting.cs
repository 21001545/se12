using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientAccountSetting
	{
		public int config_id;
		public int value;

		public class ConfigID
		{
			public static int distance_unit = EncryptUtil.makeHashCode("distance_unit");
			public static int temperature_unit = EncryptUtil.makeHashCode("temperature_unit");
		}
	}
}
