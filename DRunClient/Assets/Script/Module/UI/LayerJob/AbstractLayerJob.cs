using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public abstract class AbstractLayerJob
	{
		protected UILayer	_owner;
		protected UIPanelOpenParam _openParam;

		public abstract void start();
		public abstract bool run();

	}
}

