using DRun.Client.Module;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UISelectWithdrawNFTItem_Cell : EnhancedScrollerCellView
	{
		public UIPhotoThumbnail image_pfp;
		public TMP_Text text_tokenID;
		public TMP_Text text_level;
		public TMP_Text text_distance;
		public TMP_Text text_time;
		public Image image_grade;

		public Sprite[] spriteGrade;

		public GameObject selectedFrame;

		private ClientNFTItem _nftItem;
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public ClientNFTItem getNFTItem()
		{
			return _nftItem;
		}

		public void setup(ClientNFTItem nftItem)
		{
			_nftItem = nftItem;

			setupUI();
		}

		private void setupUI()
		{
			text_tokenID.text = $"#{_nftItem.token_id}";
			text_level.text = $"Lv.{_nftItem.level}";
			text_distance.text = StringUtil.toDistanceString(_nftItem.total_distance) + "km";

			TimeSpan time = TimeSpan.FromSeconds(_nftItem.total_time);

			text_time.text = $"{((int)time.TotalHours).ToString("D2")}:{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}";

			bool selected = ViewModel.ProMode.Data.nft_token_id == _nftItem.token_id;
			selectedFrame.SetActive(selected);
			image_grade.sprite = spriteGrade[_nftItem.grade - 201];

			ClientMain.instance.getNFTMetadataCache().getMetadata(_nftItem.token_id, cache => { 
				if( cache == null)
				{
					image_pfp.setEmpty();
				}
				else
				{
					image_pfp.setImageFromCDN(cache.imageUrl);
				}
			});
		}

		public override void RefreshCellView()
		{
			setupUI();
		}

		public void onClick()
		{
			UISelectWithdrawNFTItem.getInstance().onClickItem(this);
		}
	}
}
