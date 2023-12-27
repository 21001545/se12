using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIShopItem_Tree : EnhancedScrollerCellView
	{
		[SerializeField]
		private Animator _animator;

		public TMP_Text txt_name;
        //public TMP_Text txt_desc;

        [SerializeField]
        private TMP_Text txt_get_desc;

        [SerializeField]
        private TMP_Text txt_coin;

        [SerializeField]
        private TMP_Text txt_coin_detail;

        [SerializeField]
        private TMP_Text txt_step;

        [SerializeField]
        private TMP_Text txt_step_detail;

        [SerializeField]
        private TMP_Text txt_time;

        [SerializeField]
        private TMP_Text txt_time_detail;

        [SerializeField]
		private GameObject go_remain_time;

        [SerializeField]
        private TMP_Text txt_remain_time;

        [SerializeField]
        private TMP_Text txt_button; //cost

		public Button button;

		[SerializeField]
		private Slider slide_time;

        [SerializeField]
        private Image img_bg_select;

        [SerializeField]
        private Image img_bg_unselect;

        [SerializeField]
        private Image img_thumbnail;

        [SerializeField]
        private Image img_detail_thumbnail;

        private RefShopItem _refShopItem;
		private RefTree _refTree;

		private ClientTree _treeData;
		private int _statusType;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private TreeViewModel TreeVM => ClientMain.instance.getViewModel().Tree;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		private class StatusType
		{
			// 현재 나무가 아니고, 소유한 적이 없거나, 만료된 경우
			public const int buy = 2;

			// 현재 나무가 아니고, 소유하고 있고 만료되지 않고 기간이 남아 있는 경우
			public const int back_to_use = 3;

			// 현재 나무이고 기본 나무가 아닌경우
			public const int extent_active = 4;

			// 현재 나무이고 기본 나무가 아닌경우, 만료되었으나 수학할게 남아 있는 경우
			public const int extent_active_will_expire = 5;

			// 현재 나무, 그러나 만료된 상태
			public const int extent_expired = 6;

			// 현재 나무가 아니고, 기본나무 인 경우
			public const int back_to_use_init_tree = 7;

			// 현재 나무이고 기본 나무인경우
			public const int active_init_tree = 8;

			public const int unknown = -1;
		}

		public void setup(RefShopItem refShopItem)
		{
			_refShopItem = refShopItem;
			_refTree = GlobalRefDataContainer.getInstance().get<RefTree>(_refShopItem.item_id);

			_treeData = TreeVM.TreeMap.get(_refTree.id);

			updateUI();
		}

		public override void RefreshCellView()
		{
			_treeData = TreeVM.TreeMap.get(_refTree.id);

			updateUI();
		}

		private void updateUI()
		{

			txt_name.text = StringCollection.get("Tree.Name", _refTree.id);
			//txt_desc.text = $"step[{_refTree.production_stepcount}]\ncoin[{_refTree.production_coin}]\ndays[{_refTree.available_duration}]";
			txt_coin.text = StringCollection.getFormat("tree.desc.coins.front", 0, _refTree.production_coin); 
			txt_step.text = StringCollection.getFormat("tree.desc.step.front", 0, _refTree.production_stepcount); 

			if (_refTree.available_duration == 0 )
            {
                txt_time.text = StringCollection.get("tree.free.duration", 0);
            }
			else
            {
                txt_time.text = StringCollection.getFormat("tree.desc.day.front", 0, _refTree.available_duration);
            }
			
			txt_get_desc.text = StringCollection.getFormat("tree.desc.minavailablestepcount.back", 0, _refTree.available_stepcount_min);

			txt_coin_detail.text = StringCollection.getFormat("tree.desc.steps.productioncoin.back", 0, _refTree.production_coin, _refTree.production_stepcount);
            txt_step_detail.text = StringCollection.getFormat("tree.desc.steps.maxavailablestepcount.back", 0, _refTree.available_stepcount_max);

			if (_refTree.available_duration == 0)
			{
				txt_time_detail.text = StringCollection.get("tree.free.cost.back", 0);
			}
			else
			{
				txt_time_detail.text = StringCollection.getFormat("tree.desc.availabledays.back", 0, _refTree.available_duration);
			}

			if (string.IsNullOrEmpty(_refTree.shop_thumbnail) )
			{
				Debug.LogError($"empty thumbnail path of treeshop");
			}
			else
            {
				var sprite = Resources.Load<Sprite>(_refTree.shop_thumbnail);
				img_thumbnail.sprite = sprite;
            }

            if (string.IsNullOrEmpty(_refTree.shop_detail_thumbnail))
            {
                Debug.LogError($"empty detail thumbnail path of treeshop");
            }
            else
            {
                var sprite = Resources.Load<Sprite>(_refTree.shop_detail_thumbnail);
                img_detail_thumbnail.sprite = sprite;
            }

            _statusType = checkStatus();

			updateUI_remainTime();
			updateUI_button();
			updateUI_active();
		}

		private void updateUI_remainTime()
		{
			int configDurationUnitTime = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.duration_unit_time, 1440);

			if( _statusType == StatusType.back_to_use)
			{
				go_remain_time.gameObject.SetActive(true);

				int remain_unit_times = UnityEngine.Mathf.CeilToInt(_treeData.remain_time / configDurationUnitTime);

				// 남은시간을 hour단위로 보여줌
				if (configDurationUnitTime < 1440)
				{
					int remain_hours = (int)((remain_unit_times * configDurationUnitTime) / TimeUtil.msMinute);
					txt_remain_time.text = $"{remain_hours} minutes left";
				}
				else
				{
					int remain_day = UnityEngine.Mathf.CeilToInt(_treeData.remain_time / TimeUtil.msDay);
					txt_remain_time.text = $"{remain_day} days left";
				}
				
				slide_time.value = _treeData.remain_time / _refTree.available_duration;

			}
			else if( _statusType == StatusType.extent_active)
			{
				go_remain_time.gameObject.SetActive(true);

				TimeSpan diff = _treeData.expire_time - DateTime.UtcNow;
				
				if( configDurationUnitTime < 1440)
				{
					txt_remain_time.text = $"{(int)diff.TotalMinutes} minutes left";
				}
				else
				{
					txt_remain_time.text = $"{(int)diff.TotalDays} days left";
                }
                slide_time.value = _treeData.remain_time / _refTree.available_duration;
            }
			else if( _statusType == StatusType.extent_active_will_expire)
			{
				go_remain_time.gameObject.SetActive(true);
				//txt_remain_time.text = "will expire after harvest";
			}
			else if( _statusType == StatusType.extent_expired)
			{
				go_remain_time.gameObject.SetActive(true);
				//txt_remain_time.text = "expired";
			}
			else
			{
				go_remain_time.gameObject.SetActive(false);
			}
		}

		private void updateUI_button()
        {
			if (_refShopItem.cost == 0 )
            {
                txt_button.text = $"<sprite name=\"coin\"> {StringCollection.get("tree.free.cost",0)}";
            }
			else
            {
                txt_button.text = $"<sprite name=\"coin\"> {_refShopItem.cost.ToString("N0")}";
            }

            if (_statusType == StatusType.buy)
			{
				button.interactable = true;
			}
			else if (_statusType == StatusType.back_to_use || _statusType == StatusType.back_to_use_init_tree)
			{
				button.interactable = true;
				txt_button.text = StringCollection.get("tree.desc.backtouse.front", 0); 
			}
			else if (_statusType == StatusType.extent_active || _statusType == StatusType.extent_expired || _statusType == StatusType.extent_active_will_expire)
			{
				button.interactable = true;
				//txt_button.text = $"EXTEND <sprite name=\"coin\">{_refShopItem.cost}";
			}
			else if (_statusType == StatusType.active_init_tree)
			{
				button.interactable = false;
				txt_button.text = StringCollection.get("tree.desc.inuse.front", 0);
			}
			else
			{
				txt_button.text = "invalid status";
				//button.interactable = false;
			}

		}

		private void updateUI_active()
		{
			if(_statusType == StatusType.extent_active ||
				_statusType == StatusType.extent_active_will_expire ||
				_statusType == StatusType.active_init_tree ||
				_statusType == StatusType.extent_expired)
			{
                img_bg_select.gameObject.SetActive(true);
                img_bg_unselect.gameObject.SetActive(false);
            }
			else
            {
                img_bg_select.gameObject.SetActive(false);
                img_bg_unselect.gameObject.SetActive(true);
            }
		}

		private bool isCurrentTree()
		{
			return _refTree.id == TreeVM.TreeConfig.tree_id;
		}

		private int checkStatus()
		{
			if (isCurrentTree() == false)
			{
				// 소유한적이 한번도 없다
				if (_treeData == null || _treeData.status == ClientTree.Status.expired)
				{
					return StatusType.buy;
				}

				if( _treeData.status == ClientTree.Status.reserved)
				{
					if( _refTree.available_duration == 0)
					{
						return StatusType.back_to_use_init_tree;
					}
					else
					{
						return StatusType.back_to_use;
					}
				}
			}
			else
			{
				if( _treeData.status == ClientTree.Status.expired)
				{
					return StatusType.extent_expired;
				}
				else if( _treeData.status == ClientTree.Status.active)
				{
					if( _refTree.available_duration == 0)
					{
						return StatusType.active_init_tree;
					}
					else
					{
						TimeSpan diff = _treeData.expire_time - DateTime.UtcNow;
						// 만료되었다 (아마 보상이 남아 있어서 그런듯
						if( diff.TotalMilliseconds < 0)
						{
							return StatusType.extent_active_will_expire;
						}
						else
						{
							return StatusType.extent_active;
						}
					}
				}
			}

			Debug.LogError($"invalid status : isCurrentTree[{isCurrentTree()}] status[{(_treeData != null ? _treeData.status : -1)}]");
			return StatusType.unknown;
		}

		public void onClickButton()
		{
		//	// 현재 나무가 아니고, 소유한 적이 없거나, 만료된 경우
		//public const int buy = 2;

		//// 현재 나무가 아니고, 소유하고 있고 만료되지 않고 기간이 남아 있는 경우
		//public const int back_to_use = 3;

		//// 현재 나무 
		//public const int extent_active = 4;

		//// 현재 나무, 그러나 만료된 상태
		//public const int extent_expired = 5;

			if( _statusType == StatusType.back_to_use || _statusType == StatusType.back_to_use_init_tree)
			{
				backToUse();
			}
			else if (_statusType == StatusType.buy || _statusType == StatusType.extent_active || _statusType == StatusType.extent_expired || _statusType == StatusType.extent_active_will_expire)
			{
				purchase();
			}
			else
			{
				Debug.LogError($"invalid status type : {_statusType}");
			}
		}

		private bool checkNeedConfirmRemainTime()
		{
			ClientTree currentTree = TreeVM.getCurrentTree();
			RefTree refCurrentTree = GlobalRefDataContainer.getInstance().get<RefTree>(currentTree.tree_id);

			// 유효기간이 있다
			if (refCurrentTree.available_duration > 0)
			{
				// 이미 만료된 나무는 경고를 띄울 필요 없다
				if( currentTree.status == ClientTree.Status.expired)
				{
					return false;
				}

				TimeSpan diff = currentTree.expire_time - DateTime.UtcNow;

				long configDurationUnitTime = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.duration_unit_time, 1440) * TimeUtil.msMinute;
				long remain_time = (long)diff.TotalMilliseconds;

				//if (diff.TotalDays < 1.0f)
				if (remain_time < configDurationUnitTime)
				{
					return true;
				}
			}

			return false;
		}

		private void backToUse()
		{
			bool needConfirmRemainTime = checkNeedConfirmRemainTime();

			if( needConfirmRemainTime)
            {
                UITreeReplaceConfirm.spawn(() =>
                {
					reqChangeTree();
                });
			}
			else
			{
				reqChangeTree();
			}
		}

		private void purchase()
		{
			// 코인이 있는지 검사
			if(ViewModel.Wallet.FestaCoin < _refShopItem.cost)
			{
				UIPopup.spawnOK(StringCollection.get("tree.purchase.popup.notenough.coin", 0));
				return;
			}

			UnityAction confirmBuy = () => {
				// 구매 확인
				int max_coin_product = _refTree.available_stepcount_max * _refTree.production_coin / _refTree.production_stepcount;
				string desc = StringCollection.getFormat("tree.purchase.popup.desc", 0, max_coin_product);

				if (_statusType == StatusType.extent_active || _statusType == StatusType.extent_expired || _statusType == StatusType.extent_active_will_expire)
                {
                    UITreeExtendConfirm.spawn(_refTree, _refShopItem, _treeData, () =>
                     {
                         reqPurchase();
                     });
                }	
				else
                {
                    UITreePurchaseConfirm.spawn(_refTree, _refShopItem, () =>
                    {
                        reqPurchase();
                    });
                }
			};

			if(checkNeedConfirmRemainTime())
            {
                UITreeReplaceConfirm.spawn( () =>
                {
                    reqPurchase();
                });
			}
			else
			{
				confirmBuy();
			}
		}

		private void reqChangeTree()
		{
			button.interactable = false;
			UIBlockingInput.getInstance().open();

			MapPacket req = Network.createReq(CSMessageID.Tree.ChangeTreeReq);
			req.put("id", _refShopItem.item_id);

			Network.call(req, ack => {
				button.interactable = true;
				UIBlockingInput.getInstance().close();

				if (ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			});
		}

		private void reqPurchase()
		{
			button.interactable = false;
			UIBlockingInput.getInstance().open();

			MapPacket req = Network.createReq(CSMessageID.Shop.PurchaseShopItemReq);
			req.put("id", _refShopItem.id);

			Network.call(req, ack => {
				button.interactable = true;
				UIBlockingInput.getInstance().close();

				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			});
		}

		// 앞면의 more 버튼 클릭.
		public void onClickMore()
        {
            _animator.SetTrigger("more");
        }

		// 뒷면의 less 버튼 클릭.
		public void onClickLess()
        {
            _animator.SetTrigger("less");
        }
	}
}
