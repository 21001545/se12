using EnhancedUI.EnhancedScroller;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UISelectCountryPhoneNumber_Item : EnhancedScrollerCellView
	{
		[SerializeField]
		private GameObject go_checkMark;

		public Image imageBase;
		public TMP_Text textNumber;
		public UIPhotoThumbnail countryFlag;

		private CountryPhoneNumber _data;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public UISelectCountryPhoneNumber_Item setup(CountryPhoneNumber data,bool isSelected)
		{
			_data = data;

			textNumber.text = $"({data.countryName}) +{data.countryCode}";
			countryFlag.setImageFromCDN(data.getFlagURL());

			imageBase.color = isSelected ? ColorChart.primary_100 : ColorChart.white;
			go_checkMark.SetActive(isSelected);

			if (isSelected)
				return this;
			else
				return null;
		}

		public CountryPhoneNumber getCountryPhoneNumber()
        {
			return _data;
        }

		public string getRegionCode()
        {
			return _data.regionCode;
        }

		public void setNotPicked()
        {
			imageBase.color = Color.clear;
			go_checkMark.SetActive(false);
		}

		public void onClick()
		{
			imageBase.color = ColorChart.primary_100;
			go_checkMark.SetActive(true);
			UISelectCountryPhoneNumber.getInstance().onClickItem(this);
		}
		
	}
}
