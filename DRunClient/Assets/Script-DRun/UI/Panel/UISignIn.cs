
using DRun.Client.Logic.SignIn;
using DRun.Client.Module;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace DRun.Client
{
	public class UISignIn : UISingletonPanel<UISignIn>
	{
		public UIInputField inputEMail;
		public UIInputField inputPassword;

		public UILoadingButton btn_login;

		public TMP_Text error_email;
		public TMP_Text error_password;

		public ProceduralImage img_borderEmail;
		public ProceduralImage img_borderPassword;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private ClientFSM FSM => ClientMain.instance.getFSM();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			inputEMail.hideClearButton();
			inputPassword.hideClearButton();
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				inputEMail.text = "";
				inputPassword.text = "";
				error_email.text = "";
				error_password.text = "";
			}
		}

		public bool checkInput()
		{
			bool checkId = true;
			bool checkPw = true;

			string email = inputEMail.text;
			string password = inputPassword.text;

			img_borderEmail.color = Color.white;
			img_borderPassword.color = Color.white;

			if ( string.IsNullOrEmpty( email))
			{
				error_email.text = StringCollection.get("signIn.error.emailEmpty", 0);
				checkId = false;
			}
			else if( StringUtil.checkEMail(email) == false)
			{
				error_email.text = StringCollection.get("signIn.error.emailWrong", 0);
				checkId = false;
			}
			else
			{
				error_email.text = "";
			}

			// input field 아래 border 색상 갱신
			// Color Palette 의 Error Color
			if (!checkId)
				img_borderEmail.color = UIStyleDefine.ColorStyle.error;

			if ( string.IsNullOrEmpty( password))
			{
				error_password.text = StringCollection.get("signIn.error.passwordEmpty", 0);
				checkPw = false;
			}
			else
			{
				error_password.text = "";
			}

			if (!checkPw)
				img_borderPassword.color = UIStyleDefine.ColorStyle.error;

			return checkId && checkPw;
		}

		public void onClickSignIn()
		{
			if( checkInput() == false)
			{
				return;
			}

			string email = inputEMail.text;
			string password = EncryptUtil.password(inputPassword.text);

			btn_login.beginLoading();
			SignInProcessor step = SignInProcessor.create(email, password);
			step.run(result => {
				btn_login.endLoading();

				if( result.failed())
				{
					// 회원가입이 않되어 있다
					if( step.getErrorCode() == ResultCode.error_not_exists)
					{
						error_email.text = StringCollection.get("signIn.error.emailNotSignUp", 0);
						inputEMail.text = "";
						inputPassword.text = "";
						error_password.text = "";
						img_borderEmail.color = UIStyleDefine.ColorStyle.error;
					}
					// 비밀번호가 틀렸다
					else if( step.getErrorCode() == ResultCode.error_wrong_password)
					{
						error_password.text = StringCollection.get("signIn.error.passwordWrong", 0);
						inputPassword.text = "";
						img_borderPassword.color = UIStyleDefine.ColorStyle.error;
					}
				}
				else
				{
					//close();
					//UILoading.getInstance().open();

					// 로그인 정보 저장
					ClientMain.instance.getViewModel().SignIn.EMail = email;
					ClientMain.instance.getViewModel().SignIn.LoginTime = TimeUtil.unixTimestampUtcNow(); // 2023.03.21 이강희
					ClientMain.instance.getViewModel().SignIn.saveCache();

					FSM.changeState(ClientStateType.login);
				}
			});
		}

		public void onClickForgotPassword()
		{
			FSM.changeState(ClientStateType.resetpassword);
		}

		public void onClickSignUp()
		{
			FSM.changeState(ClientStateType.signup);
		}
	}

}

