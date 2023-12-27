using Festa.Client.Module.UI;

namespace DRun.Client
{
	public class UIBecomeActive : UIPanel
	{
		public static UIBecomeActive spawn()
		{
			UIBecomeActive panel = UIManager.getInstance().spawnInstantPanel<UIBecomeActive>();
			return panel;
		}
	}
}
