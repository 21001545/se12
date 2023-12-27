using Festa.Client.Module;

namespace DRun.Client.ViewModel
{
	public class RankingViewModel : AbstractViewModel
	{



		public static RankingViewModel create()
		{
			RankingViewModel viewModel = new RankingViewModel();
			viewModel.init();
			return viewModel;
		}

		protected override void init()
		{
			base.init();
		}

	}
}
