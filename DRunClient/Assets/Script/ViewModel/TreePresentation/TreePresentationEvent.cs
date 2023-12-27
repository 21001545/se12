using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class TreePresentationEvent
	{
		// EventType
		private int _type;
		// RefTree.id
		private int _tree_id;
		// 걸음 수
		private int _step_count;
		// 획득시 얻게된 코인 수
		private int _harvest_amount;

		// 나무 RefData
		private RefTree _refTree;
		// 나무 성장도 ( 0 : RefTree.available_stepcount_min 1 : RefTree.available_stepcount_max)
		private float _grow_ratio;

		private bool _isWithered;
        private bool _isSeed;

        public static class EventType
		{
			public const int init_tree = 1;		// 클라 최초 실행 (로그인)
			public const int change_tree = 2;	// 나무 변경
			public const int grow_up = 3;       // 나무 성장
			public const int harvest = 4;		// 코인 수확
			public const int wither = 5;		// 시듬
		}

		public int getType()
		{
			return _type;
		}

		public int getTreeID()
		{
			return _tree_id;
		}

		public int getStepCount()
		{
			return _step_count;
		}

		public float getGrowRatio()
		{
			return _grow_ratio;
		}

		public RefTree getRefTree()
		{
			return _refTree;
		}

		public bool isWithered()
		{
			return _isWithered;
		}

        public bool isSeed()
        {
            return _isSeed;
        }

        public void setWithered()
		{
			_isWithered = true;
		}

		//
		public static TreePresentationEvent createInitTree(ClientTreeConfig treeConfig,bool isWithered)
		{
			TreePresentationEvent e = new TreePresentationEvent();
			e.init(EventType.init_tree, treeConfig.tree_id, treeConfig.step_count, 0, isWithered);
			return e;
		}

		public static TreePresentationEvent createChangeTree(ClientTreeConfig treeConfig)
		{
			TreePresentationEvent e = new TreePresentationEvent();
			e.init(EventType.change_tree, treeConfig.tree_id, treeConfig.step_count, 0, false, true);
			return e;
		}

		public static TreePresentationEvent createGrowUp(ClientTreeConfig treeConfig)
		{
			TreePresentationEvent e = new TreePresentationEvent();
			e.init(EventType.grow_up, treeConfig.tree_id, treeConfig.step_count, 0, false);
			return e;
		}

		public static TreePresentationEvent createHarvest(ClientTreeConfig treeConfig,int harvest_amount,bool isWithered)
		{
			TreePresentationEvent e = new TreePresentationEvent();
			e.init(EventType.harvest, treeConfig.tree_id, treeConfig.step_count, harvest_amount, isWithered);
			return e;
		}

		public static TreePresentationEvent createWither(ClientTreeConfig treeConfig)
		{
			TreePresentationEvent e = new TreePresentationEvent();
			e.init(EventType.wither, treeConfig.tree_id, treeConfig.step_count, 0, true);
			return e;
		}

		private void init(int type,int tree_id,int step_count,int harvest_amount,bool isWithered, bool isSeed =false)
		{
			_type = type;
			_tree_id = tree_id;
			_step_count = step_count;
			_harvest_amount = harvest_amount;
			_isWithered = isWithered;

			_refTree = GlobalRefDataContainer.getInstance().get<RefTree>(_tree_id);
			_isSeed = isSeed;

			// 이걸 -1 ~ +1로 출력해보자
			if (_step_count < _refTree.available_stepcount_min)
				_grow_ratio = (float)(_step_count - _refTree.available_stepcount_min ) / _refTree.available_stepcount_min;
			else
				_grow_ratio = UnityEngine.Mathf.Clamp( (float)(_step_count - _refTree.available_stepcount_min) / (float)(_refTree.available_stepcount_max - _refTree.available_stepcount_min), 0, 1);
		}

		public override string ToString()
		{
			return $"type[{_type}] tree_id[{_tree_id}] step_count[{_step_count}] harvest_amount[{_harvest_amount}] grow_ratio[{_grow_ratio}]";
		}
	}
}
