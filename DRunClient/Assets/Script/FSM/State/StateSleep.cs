using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateSleep : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.sleep;
		}
	}
}
