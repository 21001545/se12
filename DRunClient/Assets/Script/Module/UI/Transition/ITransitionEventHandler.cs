using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.UI
{
	public class TransitionEventType
	{
		public const int start_open = 0;
		public const int end_open = 1;
		public const int start_close = 2;
		public const int end_close = 3;
		public const int openImmediately = 4;
	}

	public interface ITransitionEventHandler
	{
		void onTransitionEvent(int type);
	}
}
