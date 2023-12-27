using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class UIProfile_MomentRowItem : EnhancedScrollerCellView
	{
		public UIProfile_MomentItem[] items;
		
		public void setup(List<ClientMoment> momentList)
		{
			for(int i = 0; i < items.Length; ++i)
			{
				if( i >= momentList.Count)
				{
					items[i].setupEmpty();
				}
				else
				{
					items[i].setup(momentList[i]);
				}
			}
		}
	}
}
