using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Festa.Client.ViewModel;
using Festa.Client.NetData;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using System;

namespace Festa.Client
{
	public class UIPushSettings : UISingletonPanel<UIPushSettings>
	{
		[SerializeField]
		private GameObject go_donotdisturbTime;
		[SerializeField]
		private GameObject go_setTime;
		//[SerializeField]
		//private SnapPicker_time snapPickerTime;

		[SerializeField]
		private TMP_Text txt_beginTime;
		[SerializeField]
		private TMP_Text txt_endTime;

		[SerializeField]
		private UIToggle tgl_social;
		[SerializeField]
		private UIToggle tgl_moment;
		[SerializeField]
		private UIToggle tgl_message;
		[SerializeField]
		private UIToggle tgl_place;
		[SerializeField]
		private UIToggle tgl_activity;
		[SerializeField]
		private UIToggle tgl_donotdisturb;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();


		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			//snapPickerTime.init();
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			ProfileViewModel profile_vm = ViewModel.Profile;
			

			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.DoNotDisturb), updateTime);
			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.PushConfigMap), updatePushNotification);
		}

		private void updatePushNotification(object obj)
        {
			int social_push = ViewModel.Profile.getPushConfig(ClientPushNotificationConfig.ConfigID.social);
			int moment_push = ViewModel.Profile.getPushConfig(ClientPushNotificationConfig.ConfigID.moment);
			int message_push = ViewModel.Profile.getPushConfig(ClientPushNotificationConfig.ConfigID.message);
			int place_push = ViewModel.Profile.getPushConfig(ClientPushNotificationConfig.ConfigID.place);
			int activity_push = ViewModel.Profile.getPushConfig(ClientPushNotificationConfig.ConfigID.activity);

			tgl_social.set(social_push == ClientPushNotificationConfig.Status.enable ? true : false, true, false);
			tgl_moment.set(moment_push == ClientPushNotificationConfig.Status.enable ? true : false, true, false);
			tgl_message.set(message_push == ClientPushNotificationConfig.Status.enable ? true : false, true, false);
			tgl_place.set(place_push == ClientPushNotificationConfig.Status.enable ? true : false, true, false);
			tgl_activity.set(activity_push == ClientPushNotificationConfig.Status.enable ? true : false, true, false);
		}

        private void updateTime(object obj)
        {
            // begin, end 둘 다 한꺼번에 바꿔줘~~
            ClientDoNotDisturb donotDisturb = ViewModel.Profile.DoNotDisturb;

			txt_beginTime.text = ClientDoNotDisturb.getTimeString_12hr(donotDisturb.LocalBeginTime);
			txt_endTime.text = ClientDoNotDisturb.getTimeString_12hr(donotDisturb.LocalEndTime);

            tgl_donotdisturb.set(donotDisturb.status == ClientDoNotDisturb.Status.enable, true, false);

			// 콜백 안 쓰니까 여기서 끄고 켜주기!
			go_donotdisturbTime.SetActive(tgl_donotdisturb.isOn());
		}

        #region change push notification - toggle

        public void onChangeSocialPush()
        {
			int pushEnabled = ClientPushNotificationConfig.Status.enable;
			if(!tgl_social.isOn())
            {
				pushEnabled = ClientPushNotificationConfig.Status.disable;
            }

			changeSetting(ClientPushNotificationConfig.ConfigID.social, pushEnabled);
        }
		public void onChangeMomentPush()
		{
			int pushEnabled = ClientPushNotificationConfig.Status.enable;
			if (!tgl_moment.isOn())
			{
				pushEnabled = ClientPushNotificationConfig.Status.disable;
			}

			changeSetting(ClientPushNotificationConfig.ConfigID.moment, pushEnabled);
		}

		public void onChangeMessagePush()
		{
			int pushEnabled = ClientPushNotificationConfig.Status.enable;
			if (!tgl_message.isOn())
			{
				pushEnabled = ClientPushNotificationConfig.Status.disable;
			}

			changeSetting(ClientPushNotificationConfig.ConfigID.message, pushEnabled);
		}

		public void onChangePlacePush()
		{
			int pushEnabled = ClientPushNotificationConfig.Status.enable;
			if (!tgl_place.isOn())
			{
				pushEnabled = ClientPushNotificationConfig.Status.disable;
			}

			changeSetting(ClientPushNotificationConfig.ConfigID.place, pushEnabled);
		}

		public void onChangeActivityPush()
		{
			int pushEnabled = ClientPushNotificationConfig.Status.enable;
			if (!tgl_activity.isOn())
			{
				pushEnabled = ClientPushNotificationConfig.Status.disable;
			}

			changeSetting(ClientPushNotificationConfig.ConfigID.activity, pushEnabled);
		}

		private void changeSetting(int config_id, int status)
		{

			MapPacket req = Network.createReq(CSMessageID.Account.ChangePushConfigReq);
			req.put("id", config_id);
			req.put("data", status);

			Network.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			});
		}

		#endregion


        public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		public void displayDonotdisturbTime(bool _show)
        {
			go_donotdisturbTime.SetActive(_show);
        }

		public void onClickDoNotDisturbToggle()
        {
            go_donotdisturbTime.SetActive(tgl_donotdisturb.isOn());

            ClientDoNotDisturb donotDisturb = ViewModel.Profile.DoNotDisturb;

            int pushEnabled = ClientDoNotDisturb.Status.enable;
            if (!tgl_donotdisturb.isOn())
            {
                pushEnabled = ClientDoNotDisturb.Status.disable;
            }

            MapPacket req = Network.createReq(CSMessageID.Account.ChangeDoNotDisturbReq);
            req.put("data", pushEnabled);
            req.put("begin", donotDisturb.begin_time);
            req.put("end", donotDisturb.end_time);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);
                }
            });
        }

        public void onClickSetTimeOpen(bool beginTime)
        {
			//snapPickerTime.isBeginTime(beginTime);
            go_setTime.GetComponent<SwipeDownPanel>().swipePanel(true);

           // snapPickerTime.updateFromData();
        }

        public void onClickSetTimeClose()
        {
            go_setTime.GetComponent<SwipeDownPanel>().swipePanel(false);
        }

        public void sendChangeTime(Action callBack)
        {
            ClientDoNotDisturb donotDisturb = ViewModel.Profile.DoNotDisturb;

            int pushEnabled = ClientDoNotDisturb.Status.enable;
            if (!tgl_donotdisturb.isOn())
            {
                pushEnabled = ClientDoNotDisturb.Status.disable;
            }

            MapPacket req = Network.createReq(CSMessageID.Account.ChangeDoNotDisturbReq);
            req.put("data", pushEnabled);
            req.put("begin", donotDisturb.begin_time);
            req.put("end", donotDisturb.end_time);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);
                }

				callBack();
            });
        }
    }
}
