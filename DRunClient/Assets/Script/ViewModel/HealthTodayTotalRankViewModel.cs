using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class HealthTodayTotalRankViewModel : AbstractViewModel
	{
		private int _rank;			// 0 : 1등
		private int _count;
		private int _average;
		private float _rankRatio;

		public int Rank
		{
			get
			{
				return _rank;
			}
			set
			{
				Set(ref _rank, value);
			}
		}

		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				Set(ref _count, value);
			}
		}

		public int Average
		{
			get
			{
				return _average;
			}
			set
			{
				Set(ref _average, value);
			}
		}

		public float RankRatio
		{
			get
			{
				return _rankRatio;
			}
			set
			{
				Set(ref _rankRatio, value);
			}
		}

		public static HealthTodayTotalRankViewModel create()
		{
			HealthTodayTotalRankViewModel model = new HealthTodayTotalRankViewModel();
			model.init();
			return model;
		}

		public void updateRankRatio()
		{
			if( _count == 0)
			{
				RankRatio = 1.0f;
			}
			else
			{
				RankRatio = (float)_rank / (float)_count;
			}
		}
	}
}
