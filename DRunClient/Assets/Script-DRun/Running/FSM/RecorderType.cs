using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.Running
{
	public static class StateType
	{
		public static int none = 1;
		public static int init = 2;
		public static int wait_gps = 3;
		public static int wait_first_move = 4;
		public static int tracking = 5;
		public static int paused = 6;
		public static int end = 7;
		public static int continue_from_localdata = 8;
		public static int write_running_log = 9;
		public static int fail_write_running_log = 10;
	}
}
