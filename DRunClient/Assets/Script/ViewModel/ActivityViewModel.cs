using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class ActivityViewModel : AbstractViewModel
	{
		private List<ClientActivity> _activityList;
		private Dictionary<int, ClientActivity> _activityMap;
		private ClientNewActivityCount _newActicityCount;
		private int _read_slot_id;

		public List<ClientActivity> ActivityList
		{
			get
			{
				return _activityList;
			}
			set
			{
				Set(ref _activityList, value);
			}
		}

		public ClientNewActivityCount NewActicityCount
		{
			get
			{
				return _newActicityCount;
			}
			set
			{
				Set(ref _newActicityCount, value);
			}
		}

		public int ReadSlotID
		{
			get
			{
				return _read_slot_id;
			}
			set
			{
				Set(ref _read_slot_id, value);
			}
		}

		public int NextReadSlotID
		{
			get
			{
				return _read_slot_id + 1;
			}
		}

		public static ActivityViewModel create()
		{
			ActivityViewModel viewModel = new ActivityViewModel();
			viewModel.init();
			return viewModel;
		}

		protected override void init()
		{
			base.init();

			_activityList = new List<ClientActivity>();
			_activityMap = new Dictionary<int, ClientActivity>();
			_newActicityCount = ClientNewActivityCount.createEmpty();
			_read_slot_id = 0;
		}

		public void ClearActivityList()
		{
			_activityList.Clear();
			_activityMap.Clear();
		}

		public void appendActivity(List<ClientActivity> appendList)
		{
			foreach(ClientActivity activity in appendList)
			{
				if( _activityMap.ContainsKey( activity.slot_id))
				{
					continue;
				}

				_activityList.Add(activity);
				_activityMap.Add(activity.slot_id, activity);
			}

			_activityList.Sort((a, b) => { 
				if( a.slot_id > b.slot_id)
				{
					return -1;
				}
				
				if( a.slot_id < b.slot_id)
				{
					return 1;
				}

				return 0;			
			});

			notifyPropetyChanged("ActivityList");
		}

		public override void updateFromAck(MapPacket ack)
		{
			if( ack.contains(MapPacketKey.ClientAck.activity_read_slot_id))
			{
				_read_slot_id = (int)ack.get(MapPacketKey.ClientAck.activity_read_slot_id);
			}
		}
	}
}
