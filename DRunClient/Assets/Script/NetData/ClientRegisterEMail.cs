using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientRegisterEMail
	{
		public string email;
		public int status;

		public static class Status
		{
			public const int none = 0;
			public const int wait_confirm = 1;
			public const int confirm = 2;
		}
	}
}
