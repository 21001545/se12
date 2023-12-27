using EnhancedUI.EnhancedScroller;
using UnityEngine;
using TMPro;
using Festa.Client;

namespace DRun.Client
{
	public class UISnapPickerCell : EnhancedScrollerCellView
	{
		public TMP_Text text;
		public float height;

		private UISnapPicker _owner;

		public void setup(UISnapPickerData data,UISnapPicker owner)
		{
			text.text = data.getText();
			text.horizontalAlignment = data.getTextHorizontalAlign();

			_owner = owner;
	
		}

		public void onClick()
		{
			// 
			if( string.IsNullOrEmpty(text.text))
			{
				return;
			}

			if( _owner.scroller.IsTweening)
			{
				return;
			}

			int targetDataIndex = dataIndex - _owner.selectOffset;

			_owner.jumpTo(targetDataIndex, false);
		}
	}
}
