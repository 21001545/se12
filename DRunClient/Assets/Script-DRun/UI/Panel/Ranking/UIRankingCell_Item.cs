using DRun.Client.Module;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.RefData;
using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIRankingCell_Item : UIRankingCell_BaseItem
	{
		[Header("======== rank =======")]
		public Image image_icon;
		public Sprite[] top_icon;
		public TMP_Text text_ranking;

		[Header("======== name =======")]
		public TMP_Text text_name;

		[Header("======== updown =======")]
		public TMP_Text text_new;
		public GameObject root_updown;
		public Image updown_icon;
		public TMP_Text updown_value;
		public Sprite[] updown_sprite;

		[Header("======== score =======")]
		public TMP_Text text_normal_value;
		public TMP_Text text_drn_value;

		protected ClientRankingData _data;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void setup(ClientRankingData data)
		{
			_data = data;

			setupRank();
			// name
			setupName();
			setupUpDown();
			setupScore();
		}

		protected virtual void setupRank()
		{
			if( _data.rank < 0)
			{
				image_icon.gameObject.SetActive(false);
				text_ranking.gameObject.SetActive(true);
				text_ranking.text = "-";
				return;
			}

			// icon or rank
			if (_data.rank < 3)
			{
				image_icon.gameObject.SetActive(true);
				image_icon.sprite = top_icon[_data.rank];
				text_ranking.gameObject.SetActive(false);
			}
			else
			{
				image_icon.gameObject.SetActive(false);
				text_ranking.gameObject.SetActive(true);
				text_ranking.text = (_data.rank + 1).ToString("N0");
			}
		}

		protected virtual void setupUpDown()
		{
			if( _data.rank == -1 || _data.time_type == ClientRunningLogCumulation.TimeType.total)
			{
				text_new.gameObject.SetActive(false);
				root_updown.gameObject.SetActive(false);

				return;
			}

			if (_data.prev_rank == -1)
			{
				text_new.gameObject.SetActive(true);
				root_updown.gameObject.SetActive(false);
			}
			else if (_data.prev_rank != _data.rank)
			{
				int delta = _data.rank - _data.prev_rank;

				text_new.gameObject.SetActive(false);
				root_updown.gameObject.SetActive(true);

				if (delta < 0)
				{
					updown_icon.sprite = updown_sprite[0];
					updown_value.text = Mathf.Abs(delta).ToString();
					updown_value.color = UIStyleDefine.ColorStyle.sub_02;
				}
				else if (delta > 0)
				{
					updown_icon.sprite = updown_sprite[1];
					updown_value.text = delta.ToString();
					updown_value.color = UIStyleDefine.ColorStyle.sub_03;
				}
			}
			else
			{
				text_new.gameObject.SetActive(false);
				root_updown.gameObject.SetActive(false);
			}
		}

		protected virtual void setupScore()
		{
			if( _data.mode_type == ClientRankingData.ModeType.step)
			{
				text_normal_value.gameObject.SetActive(true);
				text_drn_value.gameObject.SetActive(false);

				//text_normal_value.text = _data.score.ToString("N0") + "걸음";
				text_normal_value.text = StringCollection.getFormat("ranking.item.stepcount", 0, _data.score.ToString("N0"));
			}
			else if (_data.mode_type == ClientRankingData.ModeType.promode)
			{
				text_normal_value.gameObject.SetActive(false);
				text_drn_value.gameObject.SetActive(true);

				text_drn_value.text = StringUtil.toDRNStringDefault(_data.score);
			}
			else if( _data.mode_type == ClientRankingData.ModeType.marathon)
			{
				text_normal_value.gameObject.SetActive(true);
				text_drn_value.gameObject.SetActive(false);

				long total_seconds = _data.score;

				TimeSpan timeSpan = TimeSpan.FromSeconds(total_seconds);

				int hour = timeSpan.Hours;
				int minute = timeSpan.Minutes;
				int second = timeSpan.Seconds;

				text_normal_value.text = $"{hour.ToString("D2")}:{minute.ToString("D2")}:{second.ToString("D2")}";
			}
		}

		protected virtual void setupName()
		{
			if( _data._profileCache != null)
			{
				text_name.text = _data._profileCache.Profile.name;
			}
			else
			{
				text_name.text = "...";
			}



			//ClientMain.instance.getProfileCache().getProfileCache(_data.account_id, result => { 
			//	if( result.succeeded())
			//	{
			//		// 그럴 수 있다
			//		ClientProfileCache profileCache = result.result();
			//		if( profileCache._accountID == _data.account_id)
			//		{
			//			text_name.text = profileCache.Profile.name;
			//		}
			//	}
			//});
		}

	}
}
