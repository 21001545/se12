using DRun.Client.Logic.ProMode;
using DRun.Client.Module;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Script.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIInventoryCell_PFPItem : EnhancedScrollerCellView
	{
		public UIPhotoThumbnail image_pfp;
		public TMP_Text text_tokenID;
		public TMP_Text text_level;
		public TMP_Text text_distance;
		public TMP_Text text_time;
		public GameObject mode_normal;
		public GameObject mode_select;

		[SerializeField]
		private UISwitcher _normal_select_switcher;

		[SerializeField]
		private UIAlphaBlender _normal_select_alphaBlender;

		public GameObject selected_frame;
		public Image image_grade;
		public UISpriteToggleButton toggle_selected;

		public Sprite[] spriteGrade;

		private ClientNFTItem _nftItem;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private bool _isTweenAlreadyInit = false;
		private IEnumerable<TweenerCore<Color, Color, ColorOptions>> _enable_normal_tweeners;
		private IEnumerable<TweenerCore<Color, Color, ColorOptions>> _disable_normal_tweeners;

		private const float BlendDurationInSeconds = 0.6f;

		public void setup(ClientNFTItem item)
		{
			_nftItem = item;

			if (!_isTweenAlreadyInit)
			{
				initTweens(_normal_select_alphaBlender);
				_isTweenAlreadyInit = true;
			}

			setupUI();
		}

		private void initTweens(UIAlphaBlender alphaBlender)
		{
			_enable_normal_tweeners ??= alphaBlender.blendToEnableOnlyOneTweener(
							enableOnlyIndex: 1,
							range: (from: 0, to: 1),
							BlendDurationInSeconds
							).Select(t => t.Pause().SetAutoKill(false));

			_disable_normal_tweeners = alphaBlender.blendToEnableOnlyOneTweener(
				enableOnlyIndex: 0,
				range: (from: 0, to: 1),
				BlendDurationInSeconds
			).Select(t => t.Pause().SetAutoKill(false));
		}

		private void setupUI()
		{
			text_tokenID.text = $"#{_nftItem.token_id}";
			text_level.text = $"Lv.{_nftItem.level}";
			text_distance.text = StringUtil.toDistanceString(_nftItem.total_distance) + "km";

			TimeSpan time = TimeSpan.FromSeconds(_nftItem.total_time);

			text_time.text = $"{((int)time.TotalHours).ToString("D2")}:{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}";

			bool selected = ViewModel.ProMode.Data.nft_token_id == _nftItem.token_id;

			selected_frame.SetActive(selected);
			toggle_selected.setStatus(selected);
			image_grade.sprite = spriteGrade[_nftItem.grade - 201];

			ClientMain.instance.getNFTMetadataCache().getMetadata(_nftItem.token_id, cache =>
			{
if (cache == null)
				{
					image_pfp.setEmpty();
				}
				else
				{
					image_pfp.setImageFromCDN(cache.imageUrl);
				}
			});

			bool selectPFP = UIInventory.getInstance().SelectPFP;
			if (selectPFP)
			{
				_normal_select_switcher.@switch(1);
				foreach (var t in _enable_normal_tweeners)
					t.Restart();
			}
			else
			{
				_normal_select_switcher.@switch(0);
				foreach (var t in _disable_normal_tweeners)
					t.Restart();
			}

			//mode_normal.SetActive(selectPFP == false);
			//mode_select.SetActive(selectPFP == true);

			toggle_selected.setStatus(selected);
		}

		public override void RefreshCellView()
		{
			setupUI();
		}

		public void onClick_Body()
		{
			if (UIInventory.getInstance().SelectPFP)
			{
				onClick_Select();
			}
			else
			{
				onClick_Profile();
			}
		}

		public void onClick_Select()
		{
			if (UIInventory.getInstance().SelectPFP == false)
			{
				return;
			}

			if (_nftItem.token_id == ViewModel.ProMode.Data.nft_token_id)
			{
				return;
			}

			UIInventory.getInstance().loading.SetActive(true);

			EquipNFTProcessor step = EquipNFTProcessor.create(_nftItem.token_id);
			step.run(result =>
			{

				UIInventory.getInstance().loading.SetActive(false);

				if (result.succeeded())
				{
					UIInventory.getInstance().endSelectPFP();
				}

			});
		}

		public void onClick_Profile()
		{
			UIPFPDetail.getInstance().open(UIPanelOpenParam_PFPDetail.create(_nftItem, ViewModel.ProMode.NFTBonus, UIInventory.getInstance()));
		}
	}
}
