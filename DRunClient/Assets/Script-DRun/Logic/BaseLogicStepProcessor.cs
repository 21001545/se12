using Festa.Client;
using Festa.Client.Module;

namespace DRun.Client.Logic
{
	public abstract class BaseLogicStepProcessor : BaseStepProcessor
	{
		protected ClientNetwork Network => ClientMain.instance.getNetwork();
		protected ClientViewModel ViewModel => ClientMain.instance.getViewModel();

	}
}
