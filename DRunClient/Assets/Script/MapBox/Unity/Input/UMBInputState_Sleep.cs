using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public class UMBInputState_Sleep : UMBInputStateBehaviour
	{
		public override int getType()
		{
			return UMBInputStateType.sleep;
		}
	}
}
