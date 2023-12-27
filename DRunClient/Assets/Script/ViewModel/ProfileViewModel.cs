using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class ProfileViewModel : AbstractViewModel
	{
		private ClientProfile _profile;
		private ClientSignup _signup;
		private ClientRegisterEMail _email;
		private ClientDoNotDisturb _doNotDisturb;
		private Dictionary<int, ClientPushNotificationConfig> _pushConfigMap;
		private Dictionary<int, ClientAccountSetting> _settingMap;

		public ClientProfile Profile
		{
			get
			{
				return _profile;
			}
			set
			{
				Set(ref _profile, value);
			}
		}

		public ClientSignup Signup
		{
			get
			{
				return _signup;
			}
			set
			{
				Set(ref _signup, value);
			}
		}

		public ClientRegisterEMail Email
		{
			get
			{
				return _email;
			}
			set
			{
				Set(ref _email, value);
			}
		}

		public ClientDoNotDisturb DoNotDisturb
		{
			get
			{
				return _doNotDisturb;
			}
			set
			{
				Set(ref _doNotDisturb, value);
			}
		}

		public Dictionary<int, ClientPushNotificationConfig> PushConfigMap
		{
			get
			{
				return _pushConfigMap;
			}
		}

		public Dictionary<int, ClientAccountSetting> SettingMap
		{
			get
			{
				return _settingMap;
			}
		}

		public int Setting_DistanceUnit => getSettingWithDefault(ClientAccountSetting.ConfigID.distance_unit, UnitDefine.DistanceType.km);
		public int Setting_TemperatureUnit => getSettingWithDefault(ClientAccountSetting.ConfigID.temperature_unit, UnitDefine.TemperatureType.c);

		public static ProfileViewModel create()
		{
			ProfileViewModel vm = new ProfileViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_pushConfigMap = new Dictionary<int, ClientPushNotificationConfig>();
			_settingMap = new Dictionary<int, ClientAccountSetting>();
		}

		public int getSettingWithDefault(int config_id,int def)
		{
			ClientAccountSetting setting;
			if( _settingMap.TryGetValue(config_id, out setting) == false)
			{
				return def;
			}

			return setting.value;
		}

		public void setSetting(int config_id,int value)
		{
			ClientAccountSetting setting;
			if( _settingMap.TryGetValue(config_id, out setting) == false)
			{
				setting = new ClientAccountSetting();
				setting.config_id = config_id;
				setting.value = value;

				_settingMap.Add(config_id, setting);
			}
			else
			{
				setting.value = value;
			}
		}

		public int getPushConfig(int config_id,int def = ClientPushNotificationConfig.Status.enable)
		{
			ClientPushNotificationConfig config;
			if( _pushConfigMap.TryGetValue(config_id, out config) == false)
			{
				return def;
			}

			return config.status;
		}

		public override void updateFromAck(MapPacket ack)
		{
			if (ack.contains(MapPacketKey.ClientAck.profile))
			{
				Profile = (ClientProfile)ack.get(MapPacketKey.ClientAck.profile);
			}
			if (ack.contains(MapPacketKey.ClientAck.signup))
			{
				Signup = (ClientSignup)ack.get(MapPacketKey.ClientAck.signup);
			}
			if (ack.contains(MapPacketKey.ClientAck.register_email))
			{
				Email = (ClientRegisterEMail)ack.get(MapPacketKey.ClientAck.register_email);
			}
			if (ack.contains(MapPacketKey.ClientAck.push_donot_disturb))
			{
				DoNotDisturb = (ClientDoNotDisturb)ack.get(MapPacketKey.ClientAck.push_donot_disturb);
			}
			if (ack.contains(MapPacketKey.ClientAck.push_config))
			{
				List<ClientPushNotificationConfig> config_list = ack.getList<ClientPushNotificationConfig>(MapPacketKey.ClientAck.push_config);
				foreach(ClientPushNotificationConfig config in config_list)
				{
					if( _pushConfigMap.ContainsKey( config.config_id))
					{
						_pushConfigMap.Remove(config.config_id);
					}

					_pushConfigMap.Add(config.config_id, config);
				}

				notifyPropetyChanged("PushConfigMap");
			}
			if( ack.contains(MapPacketKey.ClientAck.account_setting))
			{
				List<ClientAccountSetting> setting_list = ack.getList<ClientAccountSetting>(MapPacketKey.ClientAck.account_setting);
				foreach(ClientAccountSetting setting in setting_list)
				{
					if( _settingMap.ContainsKey( setting.config_id))
					{
						_settingMap.Remove(setting.config_id);
					}

					_settingMap.Add(setting.config_id, setting);
				}

				notifyPropetyChanged("SettingMap");
			}

		}
	}
}
