using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class TreeViewModel : AbstractViewModel
	{
		private ClientTreeConfig			_config;
		private ObservableDictionary<int, ClientTree>	_treeMap;
		private List<TreePresentationEvent> _presentationEvents;

		public ClientTreeConfig TreeConfig
		{
			get { return _config; }
			set
			{
				Set(ref _config, value);
			}
		}

		public List<TreePresentationEvent> PresentationEvents
		{
			get { return _presentationEvents; }
			set
			{
				Set(ref _presentationEvents, value);
			}
		}

		public ObservableDictionary<int, ClientTree> TreeMap => _treeMap;

		public static TreeViewModel create()
		{
			TreeViewModel vm = new TreeViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_treeMap = new ObservableDictionary<int, ClientTree>();
			_presentationEvents = new List<TreePresentationEvent>();
		}

		public override void updateFromAck(MapPacket ack)
		{
			bool updated = false;
			if( ack.contains(MapPacketKey.ClientAck.tree_list))
			{
				updateTree(ack.getList<ClientTree>(MapPacketKey.ClientAck.tree_list));
				updated = true;
			}
			if (ack.contains(MapPacketKey.ClientAck.tree_config))
			{
				updateTreeConfig((ClientTreeConfig)ack.get(MapPacketKey.ClientAck.tree_config));
				updated = true;
			}

			if ( updated)
			{
				// 수확 이벤트 (구대하거나, 변경 할때도 발생)
				if ( ack.contains(MapPacketKey.ClientAck.tree_harvest_coinamount))
				{
					int harvest_amount = (int)ack.get(MapPacketKey.ClientAck.tree_harvest_coinamount);
					TreePresentationEvent e = TreePresentationEvent.createHarvest(_config, harvest_amount, isCurrentTreeWithered());
					appendPresentationEvent(e);
				}

				notifyPropetyChanged("TreeMap");
			}
		}

		public void updateTree(List<ClientTree> treeList)
		{
			foreach(ClientTree tree in treeList)
			{
				// 나무가 시들어 버림 (최초 로딩때는 발생 않함)
				if( _config != null && tree.tree_id == _config.tree_id)
				{
					ClientTree prevTree = _treeMap.get(tree.tree_id);
					
					if( prevTree != null && prevTree.status != tree.status && tree.status == ClientTree.Status.expired)
					{
						TreePresentationEvent e = TreePresentationEvent.createWither(_config);
						appendPresentationEvent(e);
					}
				}

				_treeMap.put(tree.tree_id, tree);
			}
		}

		public void updateTreeConfig(ClientTreeConfig config)
		{
			if( _config != null)
			{
				recordPresentationEvent(config);
			}
			else
			{
				ClientTree currentTree = _treeMap.get(config.tree_id);

				appendPresentationEvent(TreePresentationEvent.createInitTree(config, currentTree.status == ClientTree.Status.expired));
			}

			TreeConfig = config;
		}

		private void recordPresentationEvent(ClientTreeConfig config)
		{
			TreePresentationEvent e = null;

			// 나무 교환
			if (_config.tree_id != config.tree_id)
			{
				e = TreePresentationEvent.createChangeTree(config);
			}
			// 나무 성장
			else if( _config.step_count < config.step_count)
			{
				e = TreePresentationEvent.createGrowUp(config);
			}
			
			if( e == null)
			{
				return;
			}

			appendPresentationEvent(e);
		}

		public void appendPresentationEvent(TreePresentationEvent e)
		{
			if( _presentationEvents.Count == 0)
			{
				Debug.Log(e);
				_presentationEvents.Add(e);
				return;
			}

			// 같은 성장 이벤트는 합칠 수 있다
			if( _presentationEvents[ _presentationEvents.Count - 1].getType() == TreePresentationEvent.EventType.grow_up &&
				e.getType() == TreePresentationEvent.EventType.grow_up)
			{
				_presentationEvents.RemoveAt( _presentationEvents.Count - 1 );
				_presentationEvents.Add(e);
				Debug.Log(e);
			}
			else
			{
				_presentationEvents.Add(e);
				Debug.Log(e);
			}

			notifyPropetyChanged("PresentationEvents");
		}

		public bool popPresentationEvents(List<TreePresentationEvent> event_list)
		{
			event_list.AddRange(_presentationEvents);
			_presentationEvents.Clear();

			return event_list.Count > 0;
		}

		public int calcHarvestableCoinAmount(int step_count,RefTree refTree)
		{
			int amount = (step_count / refTree.production_stepcount) * refTree.production_coin;
			if(step_count < refTree.available_stepcount_min)
			{
				return 0;
			}

			return amount;
		}

		public int calcHarvestableCoinAmount_Current()
		{
			RefTree refTree = GlobalRefDataContainer.getInstance().get<RefTree>(_config.tree_id);
			return calcHarvestableCoinAmount(_config.step_count, refTree);
		}

		public ClientTree getCurrentTree()
		{
			return _treeMap.get(_config.tree_id);
		}

		public bool isCurrentTreeWithered()
		{
			return getCurrentTree().status == ClientTree.Status.expired;
		}

		
	}
}
