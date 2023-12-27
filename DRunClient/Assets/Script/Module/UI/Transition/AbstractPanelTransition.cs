using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client.Module.UI
{
	public abstract class AbstractPanelTransition : MonoBehaviour
	{
		public abstract float getDuration();
		public abstract void init(ITransitionEventHandler eventHandler);
		public abstract float startOpen();
		public abstract float startClose();
		public abstract float openImmediately();
		public abstract float closeImmediately(float duration);
		public abstract void update();
		public abstract bool isActive();
	}
}
