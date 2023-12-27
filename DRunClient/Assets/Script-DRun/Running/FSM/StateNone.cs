using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.Running
{
	public class StateNone : RecorderStateBehaviour
	{
		public override int getType()
		{
			return StateType.none;
		}
	}
}
