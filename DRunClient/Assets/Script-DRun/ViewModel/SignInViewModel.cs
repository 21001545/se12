using Festa.Client;
using Festa.Client.Module;
using System;
using System.Text;
using UnityEngine;

namespace DRun.Client.ViewModel
{
	public class SignInViewModel : AbstractViewModel
	{
		private string _email;
		private long _loginTime;

		public string EMail
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

		public long LoginTime
		{
			get
			{
				return _loginTime;
			}
			set
			{
				Set(ref _loginTime, value);
			}
		}

		public static SignInViewModel create()
		{
			SignInViewModel vm = new SignInViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
		}

		private byte[] getEncryptKey()
		{
			return Encoding.UTF8.GetBytes("l" + "i" + "f" + "e" + "f" + "e" + "s" + "t" + "a" + "d" + "r" + "u" + "n");
		}

		private string getCacheKey()
		{
			return EncryptUtil.makeHashCodePositive(GlobalConfig.gameserver_url).ToString();
		}

		public void loadCache()
		{
			_email = "";

			try
			{
				string data = PlayerPrefs.GetString(getCacheKey(), "");
				if( string.IsNullOrEmpty(data) == false)
				{
					byte[] decrypted = EncryptUtil.decrypt(Convert.FromBase64String(data), getEncryptKey());
					string decrypted_str = Encoding.UTF8.GetString(decrypted);

					JsonObject json = new JsonObject(decrypted_str);

					_email = json.getString("email");
					if( json.contains("loginTime"))
					{
						_loginTime = json.getLong("loginTime");
					}
					else
					{
						_loginTime = TimeUtil.unixTimestampUtcNow() - TimeUtil.msDay * 30;
					}
				}
			}
			catch(System.Exception e)
			{
				Debug.LogWarning(e);
			}
		}

		public void saveCache()
		{
			try
			{
				JsonObject json = new JsonObject();
				json.put("email", _email);
				json.put("loginTime", _loginTime);

				byte[] encrypted = EncryptUtil.encrypt(Encoding.UTF8.GetBytes(json.encode()), getEncryptKey());
				string encrypted_str = Convert.ToBase64String(encrypted);

				PlayerPrefs.SetString(getCacheKey(), encrypted_str);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}


		}
	}
}
