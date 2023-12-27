using EnhancedUI.EnhancedScroller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class UIGalleryPickerRowItem : EnhancedScrollerCellView
	{
		public UIGalleryPickerItem[] items;

		public void setup(int index, List<NativeGallery.NativePhotoContext> list, bool isSinglePick, UIGalleryPickerScrollDelegate scrollDelegate)
		{
			for(int i = 0; i < items.Length; ++i)
			{
				UIGalleryPickerItem item = items[i];

				int id = index * items.Length + i;

				if( id < list.Count)
				{
					item.enable(true);
					item.setup(id, list[id], isSinglePick, scrollDelegate);
				}
				else
				{
					item.enable(false);
				}
			}
		}

		public override void RefreshCellView() { 
			foreach(UIGalleryPickerItem item in items)
			{
				item.resetSelectIndex();
			}
		}
	}
}
