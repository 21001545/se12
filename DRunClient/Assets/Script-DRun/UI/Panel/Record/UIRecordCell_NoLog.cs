using EnhancedUI.EnhancedScroller;
using Festa.Client;
using TMPro;

namespace DRun.Client
{
	public class UIRecordCell_NoLog : EnhancedScrollerCellView
	{
		public float height;
		public TMP_Text text_desc;

		public void setup(int pageType)
		{
			if( pageType == UIRecord.PageType.promode)
			{
				text_desc.text = GlobalRefDataContainer.getStringCollection().get("pro.record.log.empty", 0);
			}
			else
			{
				text_desc.text = GlobalRefDataContainer.getStringCollection().get("pro.record.log.empty", 1);
			}
		}
	}
}
