using DRun.Client.Logic.Wallet;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UICheckWalletPinHash : UISingletonPanel<UICheckWalletPinHash>
	{
		public UIInputPinCode input_password;
		public GameObject wrong_password;

		private UnityAction<bool> _callback;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
		}

		public void open(UnityAction<bool> callback)
		{
			base.open();
			_callback = callback;
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				input_password.reset();
				wrong_password.SetActive(false);
			}
		}

		public void onValueChanged_InputPassword()
		{
			if (input_password.CodeList.Count == 6)
			{
				checkPassword();
			}
			else if( input_password.CodeList.Count > 0)
			{
				wrong_password.SetActive(false);
			}
		}

		public void onClick_Back()
		{
			_callback(false);
		}

		private void checkPassword()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 6; ++i)
			{
				sb.Append(input_password.CodeList[i].ToString());
			}

			string pinHash = EncryptUtil.password(sb.ToString());

			CheckPinHashProcessor step = CheckPinHashProcessor.create(pinHash);
			
			step.run(result => { 

				if( result.failed())
				{
					_callback(false);
				}
				else
				{
					if( step.getCheckResult() == false)
					{
						wrong_password.SetActive(true);
						input_password.reset();
					}
					else
					{
						_callback(true);
					}
				}
			});
		}
	}
}
