using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
//using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UISelectCountryPhoneNumber : UISingletonPanel<UISelectCountryPhoneNumber>, IEnhancedScrollerDelegate
	{
		[SerializeField]
		private float cellViewSize = 44f;

		public EnhancedScroller scroller;
		public UISelectCountryPhoneNumber_Item itemSource;

		private string _currentCountryCode;
		private UISelectCountryPhoneNumber_Item _currentItem;
		private List<CountryPhoneNumber> _itemList;
		private Action<CountryPhoneNumber> _callback;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);

			prepareItemList();
		}

		public void setup(string cc, Action<CountryPhoneNumber> callback)
		{
			_currentCountryCode = cc;
			_callback = callback;
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				scroller.ReloadData();
			}
		}

		private void prepareItemList()
		{
			if( _itemList != null)
			{
				sortItemList();
				return;
			}

			_itemList = new List<CountryPhoneNumber>();

			//var util = PhoneNumberUtil.GetInstance();
			//foreach (string regionCode in util.GetSupportedRegions())
			//{
			//	CountryPhoneNumber item = new CountryPhoneNumber();
			//	item.regionCode = regionCode;
			//	item.countryCode = util.GetCountryCodeForRegion(regionCode);
			//	item.countryName = StringCollection.get($"CountryName.{regionCode}", 0);

			//	_itemList.Add(item);
			//}

			sortItemList();
		}

		private void sortItemList()
		{
			_itemList.Sort((a, b) => {
				if( a.regionCode == _currentCountryCode)
				{
					return -1;
				}
				else if( b.regionCode == _currentCountryCode)
				{
					return 1;
				}
				else
				{
					return string.CompareOrdinal(a.countryName, b.countryName);
				}
			});
		}

		public void onClickBackground()
		{
			close();
		}

		public void onClickItem(UISelectCountryPhoneNumber_Item item)
        {
            if (_currentItem == item)
                return;

            if (_currentItem != null)
                _currentItem.setNotPicked();

            _currentItem = item;
            _currentCountryCode = item.getRegionCode();
        }

        public void onClickCommit()
        {
			close();
            _callback(_currentItem.getCountryPhoneNumber());
        }

        #region scroller
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UISelectCountryPhoneNumber_Item cellView = scroller.GetCellView(itemSource) as UISelectCountryPhoneNumber_Item ;

			CountryPhoneNumber data = _itemList[dataIndex];
			UISelectCountryPhoneNumber_Item item = cellView.setup(data, data.regionCode == _currentCountryCode);

			if(item != null)
				_currentItem = item;

			return cellView;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return cellViewSize;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _itemList.Count;
		}
		#endregion
	}
}
