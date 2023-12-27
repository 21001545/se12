using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public class UIFixedLayer : UILayer
	{
		public override bool isFixed()
		{
			return true;
		}

	}
}