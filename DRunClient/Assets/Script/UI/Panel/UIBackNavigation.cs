using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;

namespace Festa.Client
{
	public class UIBackNavigation : UISingletonPanel<UIBackNavigation>
	{
		private struct BackNavigationItem
		{
			public UIAbstractPanel backPanel;
			public UIAbstractPanel backPanel2;
			public UIAbstractPanel currentPanel;

			public BackNavigationItem(UIAbstractPanel back, UIAbstractPanel cur, UIAbstractPanel back2 = null)
			{
				backPanel = back;
				backPanel2 = back2;
				currentPanel = cur;
			}
		}

		private Stack<BackNavigationItem> _navStack;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_navStack = new Stack<BackNavigationItem>();
		}

		public void setup(UIAbstractPanel backPanel,UIAbstractPanel currentPanel,UIAbstractPanel backPanel2 = null)
		{
			_navStack.Push(new BackNavigationItem(backPanel, currentPanel, backPanel2));
		}

		public void clearStack()
		{
			_navStack.Clear();
		}

		public void onClickBackButton()
		{
			if( _navStack.Count > 0)
			{
				BackNavigationItem item = _navStack.Pop();
				doBackNavigation(item);
			}

			if( _navStack.Count == 0)
			{
				close();
			}
		}

		public void backTo(UIAbstractPanel targetPanel,UIAbstractPanel from)
		{
			// 스택을 하나씩 까면서 뒤로 가야 되는데, 일단 간단하게 구현해보자

			for(int i = 0; i < _navStack.Count; ++i)
			{
				BackNavigationItem item = _navStack.Pop();
				if( item.backPanel == targetPanel)
				{
					item.currentPanel = from;

					doBackNavigation(item);
					break;
				}
			}
		}

		private void doBackNavigation(BackNavigationItem item)
		{
			UIAbstractPanel back = item.backPanel;
			UIAbstractPanel back2 = item.backPanel2;
			UIAbstractPanel current = item.currentPanel;

			// back은 열고 current는 닫아야한다
			if( back.getLayer() != current.getLayer())
			{
				current.close();
			}

			back.open();
			if( back2 != null)
			{
				back2.open();
			}
			
		}
	}
}
