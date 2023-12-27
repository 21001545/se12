using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStatistics_galleryGroup : EnhancedScrollerCellView
{
    //public void insertItem(GameObject obj)
    //{
    //    obj.transform.SetParent(this.transform);
    //}

    private List<UIStatistics_galleryItem> _itemList = new List<UIStatistics_galleryItem>();

    public void setup(UIStatistics_exploreScroller.GalleryGroup group, UIStatistics_galleryItem itemSource)
    {
        for(int i = 0; i < group.items.Count; ++i)
        {
            UIStatistics_exploreScroller.ListTicket listTicket = group.items[i];

			UIStatistics_galleryItem item;
			if ( i >= _itemList.Count)
            {
                item = itemSource.make<UIStatistics_galleryItem>(transform, GameObjectCacheType.ui);
                _itemList.Add(item);
            }
            else
            {
                item = _itemList[i];
            }


            item.setup(group.items[i], () => {
                UIStatisticsTripDetail.getInstance().open();
                UIStatisticsTripDetail.getInstance().setup(listTicket._log);
				UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIStatistics.getInstance(), UIStatisticsTripDetail.getInstance());
			});
        }

        // 남는건 지운다
        if( _itemList.Count > group.items.Count)
        {
            List<UIStatistics_galleryItem> delList = new List<UIStatistics_galleryItem>();
            for(int i = group.items.Count; i < _itemList.Count; ++i)
            {
                delList.Add(_itemList[i]);
            }

            foreach(UIStatistics_galleryItem item in delList)
            {
                _itemList.Remove(item);
            }

            GameObjectCache.getInstance().delete(delList);
        }
    }

    public void onWillRecycle()
    {
        foreach(UIStatistics_galleryItem item in _itemList)
        {
            item.clear();
        }
    }
}
