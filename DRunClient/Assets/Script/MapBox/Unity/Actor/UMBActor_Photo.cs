using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBActor_Photo : UMBActor
	{
		public RectTransform pivot;
		public UIPhotoThumbnail photo;
		public GameObject go_mergeObj;
		public TMP_Text txt_mergeCount;

		private ClientTripPhoto _data;
		private List<UMBActor_Photo> _mergedList;
		
		public ClientTripPhoto getData()
		{
			return _data;
		}

		public void clearMergedList()
		{
			_mergedList.Clear();
		}

		public List<UMBActor_Photo> getMergedList()
		{
			return _mergedList;
		}

		public void addMerged(UMBActor_Photo item)
		{
			_mergedList.Add(item);
		}

		public List<ClientTripPhoto> getPhotoDataList()
		{
			List<ClientTripPhoto> list = new List<ClientTripPhoto>();
			list.Add(_data);
			foreach(UMBActor_Photo item in _mergedList)
			{
				list.Add(item.getData());
			}

			return list;
		}

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);
			pivot.gameObject.SetActive(true);
			go_mergeObj.SetActive(false);
			_mergedList = new List<UMBActor_Photo>();
		}

		public override void onReused()
		{
			base.onReused();
			pivot.gameObject.SetActive(true);
			go_mergeObj.SetActive(false);
			_mergedList.Clear();
		}

		public void setup(ClientTripPhoto data)
		{
			_data = data;
			string photo_url = _data.getPhotoURL(GlobalConfig.fileserver_url);

			if (photo_url == null)
			{
				photo.setEmpty();
			}
			else
			{
				photo.setImageFromCDN(photo_url);
			}

			pivot.gameObject.SetActive(true);
			go_mergeObj.SetActive(false);
		}
		
		public void showtxt_mergeCount()
		{
			go_mergeObj.SetActive(_mergedList.Count > 0);
			txt_mergeCount.text = (_mergedList.Count + 1).ToString();
		}

		public void show(bool b)
		{
			pivot.gameObject.SetActive(b);
			showtxt_mergeCount();

			_canPick = b;
		}

		public void onClickPhoto()
        {
			// 2022.08.18 이강희
			//UIMakeTripPhotoPopup.getInstance().open();
        }
	}
}
