using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Festa.Client.RefData;
using System;

namespace Festa.Client
{
    public class UIStatistics_myTrip : EnhancedScrollerCellView
    {
        [SerializeField]
        private UIStatistics_myTripInnerScroll _innerScroll;

        [SerializeField]
        private Image img_list;
        [SerializeField]
        private Image img_gallery;

        public void setup(string[] descList)
        {
            _innerScroll.setupData(descList, GlobalRefDataContainer.getStringCollection().get("calendar.month.unit", DateTime.UtcNow.Month - 1));
        }

        public void onClickListType(int type)
        {
            setListTypeImage(type);
            UIStatistics.getInstance().exploreScrollDelegate.loadList(type);
        }

        public void setListTypeImage(int type)
        {
            if (type == UIStatistics_exploreScroller.ListType.list)
            {
                img_list.color = ColorChart.gray_700;
                img_gallery.color = ColorChart.gray_400;
            }
            else if (type == UIStatistics_exploreScroller.ListType.gallery)
            {
                img_list.color = ColorChart.gray_400;
                img_gallery.color = ColorChart.gray_700;
            }
        }
    }
}