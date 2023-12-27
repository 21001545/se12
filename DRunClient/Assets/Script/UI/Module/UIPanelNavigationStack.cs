using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class UIPanelNavigationStack
	{
		private Stack<UIPanelNavigationStackItem> _navStack;

		public static UIPanelNavigationStack create()
		{
			UIPanelNavigationStack stack = new UIPanelNavigationStack();
			stack.init();
			return stack;
		}

		private void init()
		{
			_navStack = new Stack<UIPanelNavigationStackItem>();
		}

		public void clear()
		{
			_navStack.Clear();
		}

		public int size()
		{
			return _navStack.Count;
		}

		public UIPanelNavigationStackItem push(UIAbstractPanel prev,UIAbstractPanel next)
		{
			UIPanelNavigationStackItem item = UIPanelNavigationStackItem.create(prev, next);
			_navStack.Push(item);
			return item;
		}

		public void pop()
		{
			if( _navStack.Count == 0)
			{
				return;
			}

			UIPanelNavigationStackItem item = _navStack.Pop();

/*			if (_navStack.Count == 0)
				item.runBackNavigation(true);
			else*/
				item.runBackNavigation();
		}
	}
}
