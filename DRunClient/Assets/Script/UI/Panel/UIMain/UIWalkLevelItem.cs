//using Festa.Client.Module;
//using Festa.Client.Module.Net;
//using Festa.Client.NetData;
//using Festa.Client.RefData;
//using System;
//using TMPro;
//using UnityEngine.UI;

//namespace Festa.Client
//{
//	public class UIWalkLevelItem : ReusableMonoBehaviour
//	{
//		public Image img_icon;
//		public Image img_selected;
//		public Image img_lock;
//		public TMP_Text txt_name;
//		public TMP_Text txt_desc;
//		public TMP_Text txt_subscribe;
//		public TMP_Text txt_button_tip;
//		public TMP_Text txt_button;
//		public UnityEngine.UI.Button btn;

//		//
//		private RefWalkLevel _ref_data;
//		private ClientWalkLevel _data;

//		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
//		private ClientNetwork Network => ClientMain.instance.getNetwork();
//		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

//		public override void onCreated(ReusableMonoBehaviour source)
//		{
//			_data = null;
//		}

//		public override void onReused()
//		{
//			_data = null;
//		}

//		public void delete()
//		{
//			GameObjectCache.getInstance().delete(this);
//		}

//		public void setup(RefWalkLevel ref_walk_level,ClientWalkLevel data)
//		{
//			_ref_data = ref_walk_level;
//			_data = data;

//			RefStringCollection stringCollection = GlobalRefDataContainer.getStringCollection();

//			txt_name.text = stringCollection.get("RefWalkLevel", _ref_data.level);
//			txt_desc.text = stringCollection.getFormat("steps.walklevel.desc", 0, _ref_data.daily_max_coins);

//			updateUI();
//		}

//		public void updateData(ClientWalkLevel data)
//		{
//			_data = data;

//			updateUI();
//		}

//		private int checkStatusType()
//		{
//			if( _data == null)
//			{
//				// stat비교
//				int str = ClientMain.instance.getViewModel().Stature.STR.level;

//				if (str >= _ref_data.unlock_stat_value)
//				{
//					return ClientWalkLevel.StatusType.unlocked;
//				}
//				else
//				{
//					return ClientWalkLevel.StatusType.locked;
//				}
//			}

//			return _data.status;
//		}

//		public void updateUI()
//		{
//			RefStringCollection stringCollection = GlobalRefDataContainer.getStringCollection();

//			int status = checkStatusType();

//			img_selected.gameObject.SetActive(status == ClientWalkLevel.StatusType.active_not_subscribe ||
//												status == ClientWalkLevel.StatusType.active_subscribe);
//			img_lock.gameObject.SetActive(status == ClientWalkLevel.StatusType.locked);

//			if( status == ClientWalkLevel.StatusType.active_subscribe)
//			{
//				txt_button.text = stringCollection.get("steps.walklevel.btn.active_subscribe", 0);

//				txt_button_tip.gameObject.SetActive(_ref_data.level != 1);

//				TimeSpan remain_time = _data.expire_date - DateTime.UtcNow;
//				txt_button_tip.text = stringCollection.getFormat("steps.walklevel.tip.remain_days", 0, (int)remain_time.TotalDays);

//				txt_subscribe.gameObject.SetActive(_ref_data.level != 1);
//				txt_subscribe.text = stringCollection.getFormat("steps.walklevel.tip.subscribing", 0);

//				btn.interactable = true;
//			}
//			else if( status == ClientWalkLevel.StatusType.active_not_subscribe)
//			{
//				txt_button.text = stringCollection.get("steps.walklevel.btn.active_not_subscribe", 0);

//				txt_button_tip.gameObject.SetActive(_ref_data.level != 1);

//				TimeSpan remain_time = _data.expire_date - DateTime.UtcNow;
//				txt_button_tip.text = stringCollection.getFormat("steps.walklevel.tip.remain_days_will_expire", 0, (int)remain_time.TotalDays);

//				txt_subscribe.gameObject.SetActive(_ref_data.level != 1);
//				txt_subscribe.text = stringCollection.getFormat("steps.walklevel.tip.reserve_unsubscribe", 0);

//				btn.interactable = true;
//			}
//			else if( status == ClientWalkLevel.StatusType.suspended)
//			{
//				if( _ref_data.level > ClientMain.instance.getViewModel().StepReward.CurrentWalkLevel.level)
//				{
//					txt_button.text = stringCollection.get("steps.walklevel.btn.suspended.upgrade", 0);
//				}
//				else
//				{
//					txt_button.text = stringCollection.get("steps.walklevel.btn.suspended.downgrade", 0);
//				}

//				txt_subscribe.gameObject.SetActive(false);
				
//				txt_button_tip.gameObject.SetActive( _ref_data.level != 1);
//				txt_button_tip.text = stringCollection.getFormat("steps.walklevel.tip.remain_days", 0, _data.remain_time / TimeUtil.msDay);

//				int diff = _ref_data.level - ViewModel.StepReward.CurrentWalkLevel.level;

//				if( diff < -1)
//				{
//					btn.interactable = false;
//				}
//				else
//				{
//					btn.interactable = true;
//				}
//			}
//			else if( status == ClientWalkLevel.StatusType.unlocked ||
//				status == ClientWalkLevel.StatusType.expired)
//			{
//				txt_button.text = stringCollection.getFormat("steps.walklevel.btn.unlock_price", 0,_ref_data.unlock_cost_value.ToString("N0"));

//				txt_subscribe.gameObject.SetActive(false);
//				txt_button_tip.gameObject.SetActive(true);
//				txt_button_tip.text = stringCollection.getFormat("steps.walklevel.tip.unlock_duration", 0, 30);

//				btn.interactable = true;
//			}
//			else if( status == ClientWalkLevel.StatusType.locked)
//			{
//				txt_button_tip.text = stringCollection.get("steps.walklevel.tip.unlock_condition", 0);

//				txt_subscribe.gameObject.SetActive(false);
//				txt_button_tip.gameObject.SetActive(true);
//				txt_button.text = string.Format("{0} {1}", stringCollection.get("entity", _ref_data.unlock_stat_code), _ref_data.unlock_stat_value);

//				btn.interactable = true;
//			}
//		}

//		public void onClickButton()
//		{
//			int status = checkStatusType();

//			if (status == ClientWalkLevel.StatusType.active_subscribe ||
//				status == ClientWalkLevel.StatusType.locked)
//			{
//				return;
//			}

//			if( status == ClientWalkLevel.StatusType.active_not_subscribe)
//			{
//				cancelDowngrade();
//			}
//			else if( status == ClientWalkLevel.StatusType.suspended)
//			{
//				ClientWalkLevel currentWalkLevel = ViewModel.StepReward.CurrentWalkLevel;

//				// 다운인지 업인지
//				if( _data.level > currentWalkLevel.level)
//				{
//					// 즉시 업그레이드
//					upgradeWalkLevel();
//				}
//				else if( _data.level < currentWalkLevel.level)
//				{
//					if( currentWalkLevel.status == ClientWalkLevel.StatusType.active_subscribe)
//					{
//						// 일단 구독을 끊어준다
//						downgradeWalkLevel();
//					}
//					else if( currentWalkLevel.status == ClientWalkLevel.StatusType.active_not_subscribe)
//					{
//						// 만료가 되지 않으면 다운그레이드 불가
//						if( DateTime.UtcNow >= currentWalkLevel.expire_date)
//						{
//							downgradeWalkLevel();
//						}
//					}
//				}
//			}
//			else if( status == ClientWalkLevel.StatusType.unlocked ||
//					status == ClientWalkLevel.StatusType.expired)
//			{
//				purchaseWalkLevel();
//			}
//		}

//		private void cancelDowngrade()
//		{
//			string message = StringCollection.get("steps.walklevel.popup.cancel_downgrade", 0);
//			UIPopup.spawnYesNo(message, () => {

//				MapPacket req = Network.createReq(CSMessageID.HealthData.CancelDowngradeWalkLevelReq);
//				req.put("id", _ref_data.level);

//				Network.call(req, ack => { 
//					if( ack.getResult() == ResultCode.ok)
//					{
//						ViewModel.updateFromPacket(ack);
//					}
//				});
//			});
//		}

//		private void purchaseWalkLevel()
//		{
//			RefStringCollection sc = GlobalRefDataContainer.getStringCollection();

//			string message = sc.getFormat("steps.walklevel.popup.purchase", 0, sc.get("RefWalkLevel", _ref_data.level), 30);
//			string btn_buy = sc.getFormat("steps.walklevel.popup.purchase.btn_buy", 0, _ref_data.unlock_cost_value.ToString("N0"));

//			UIPopup popup = UIPopup.spawnYesNo( message, () => {

//				if (_ref_data.unlock_cost_value > ClientMain.instance.getViewModel().Wallet.FestaCoin)
//				{
//					string not_enough_coin_message = sc.getFormat("popup.common.not_enough_coin", 0);
//					UIPopup.spawnYesNo(not_enough_coin_message, () => {
//							// 상점으로 이동
//						});
//				}
//				else
//				{
//					callPurchaseWalkLevel();
//				}

//			});

//			//popup.txt_btn_yes.text = btn_buy;
//		}

//		private void callPurchaseWalkLevel()
//		{
//			btn.interactable = false;
//			UIBlockingInput.getInstance().open();

//			MapPacket req = Network.createReq(CSMessageID.HealthData.PurchaseWalkLevelReq);
//			req.put("prev_id", ViewModel.StepReward.CurrentWalkLevel);
//			req.put("id", _ref_data.level);

//			Network.call(req, ack => {
//				btn.interactable = true;
//				UIBlockingInput.getInstance().close();

//				if ( ack.getResult() == ResultCode.ok)
//				{
//					ViewModel.updateFromPacket(ack);
//				}
//			});
//		}

//		private void upgradeWalkLevel()
//		{
//			string message = StringCollection.getFormat("steps.walklevel.popup.upgrade", 0, StringCollection.get("RefWalkLevel", _ref_data.level));

//			UIPopup.spawnYesNo(message, () => {
//				callChangeWalkLevel();
//			});
//		}

//		private void downgradeWalkLevel()
//		{
//			ClientWalkLevel currentWalkLevel = ViewModel.StepReward.CurrentWalkLevel;
//			TimeSpan remain_time = currentWalkLevel.expire_date - DateTime.UtcNow;

//			string message = StringCollection.getFormat("steps.walklevel.popup.downgrade", 0, StringCollection.get("RefWalkLevel", _ref_data.level), (int)remain_time.TotalDays);

//			UIPopup.spawnYesNo(message, () => {
//				callChangeWalkLevel();
//			});
//		}

//		private void callChangeWalkLevel()
//		{
//			int current_walk_level = ViewModel.StepReward.CurrentWalkLevel.level;
//			int this_walk_level = _ref_data.level;

//			btn.interactable = false;
//			UIBlockingInput.getInstance().open();

//			MapPacket req = ClientMain.instance.getNetwork().createReq(CSMessageID.HealthData.ChangeWalkLevelReq);
//			req.put("prev_id", current_walk_level);
//			req.put("id", this_walk_level);

//			Network.call(req, ack => {
//				btn.interactable = true;
//				UIBlockingInput.getInstance().close();

//				if (ack.getResult() == ResultCode.ok)
//				{
//					ViewModel.updateFromPacket(ack);
//				}
//			});
//		}
//	}
//}
