using Festa.Client.Module.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UIPanelNavigationStackItem
	{
		public class SnapData
		{
			public UIAbstractPanel panel;
			public UIPanelOpenParam param;

			public static SnapData create(UIAbstractPanel panel)
			{
				SnapData data = new SnapData();
				data.panel = panel;
				data.param = panel.getOpenParam();
				return data;
			}
		}

		private List<SnapData> _prevList;
		private List<SnapData> _nextList;

		public static UIPanelNavigationStackItem create(UIAbstractPanel prev,UIAbstractPanel next)
		{
			UIPanelNavigationStackItem item = new UIPanelNavigationStackItem();
			item.init(prev, next);
			return item;
		}

		private void init(UIAbstractPanel prev,UIAbstractPanel next)
		{
			_prevList = new List<SnapData>();
			_nextList = new List<SnapData>();

			addPrev(prev);
			addNext(next);
		}

		public void addPrev(UIAbstractPanel prev)
		{
			_prevList.Add(SnapData.create(prev));
		}

		public void addNext(UIAbstractPanel next)
		{
			_nextList.Add(SnapData.create(next));
		}

		public void runBackNavigation()
		{
			//if (isLast)
				openPrevList(TransitionEventType.openImmediately);
/*			else
				openPrevList(TransitionEventType.openImmediately);*/

			//closeNextList();
		}

/*		private void closeNextList()
		{
			foreach(SnapData next in _nextList)
			{
                bool hasSameLayer = false;
                foreach (SnapData prev in _prevList)
                {
                    if (prev.panel.getLayer() == next.panel.getLayer())
                    {
                        hasSameLayer = true;
                        break;
                    }
                }

                if (hasSameLayer == false)
                {
                    next.panel.close();
				}
			}
		}*/

		private void openPrevList(int transitType)
		{
			foreach (SnapData prev in _prevList)
			{
				prev.panel.open(prev.param, transitType);
			}
		}
	}
}
