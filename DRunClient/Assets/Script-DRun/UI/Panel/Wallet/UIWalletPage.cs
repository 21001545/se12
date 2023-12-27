using Festa.Client;
using Festa.Client.Module.UI;
using UnityEngine;

namespace DRun.Client
{
	public abstract class UIWalletPage : MonoBehaviour
	{

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public virtual void initialize()
		{

		}
		
		public virtual void onShow()
		{

		}

		public virtual void resetBinding(UIBindingManager bindingManager)
		{

		}

		public virtual void update()
		{

		}
	}
}
