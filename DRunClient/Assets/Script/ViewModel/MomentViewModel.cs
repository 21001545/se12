using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System.Collections.Generic;

namespace Festa.Client.ViewModel
{
	public class MomentViewModel : AbstractViewModel
	{
		private List<ClientMoment> _momentList;
		private int _begin;
		private int _end;

		public int Begin => _begin;
		public int End => _end;
		public List<ClientMoment> MomentList => _momentList;

		public static MomentViewModel create()
		{
			MomentViewModel vm = new MomentViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_momentList = new List<ClientMoment>();
			_end = _begin = 0;
		}

		public override void updateFromAck(MapPacket ack)
		{
			List<ClientMoment> list = ack.getList<ClientMoment>("moment");

			foreach(ClientMoment moment in list)
			{
				_momentList.Add(moment);

				_begin = System.Math.Min(moment.id, _begin);
				_end = System.Math.Max(moment.id, _end);
			}

			_momentList.Sort((a, b) => {
				if (a.id > b.id)
				{
					return -1;
				}
				else if( a.id < b.id)
				{
					return 1;
				}
				return 0;
			});
		}

		// 모먼트를 삭제하면 viewmodel에서도 지워준다
		public void delete(int moment_id)
		{
			foreach(ClientMoment moment in _momentList)
			{
				if( moment.id == moment_id)
				{
					_momentList.Remove(moment);
					return;
				}
			}
		}
	}
}
