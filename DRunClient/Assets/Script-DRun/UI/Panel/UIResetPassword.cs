using DRun.Client.Logic.SignIn;
using DRun.Client.Module;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace DRun.Client
{
	public class UIResetPassword : UISingletonPanel<UIResetPassword>
	{
		#region fields
		public static class Page
		{
			public const int verify_email = 0;
			public const int input_password = 1;
			public const int input_password_re = 2;
		}

		[Header("========== main =========")]
		public GameObject[] pages;

		[Header("========== verify email =========")]
		public UIInputField input_email;
		public UILoadingButton btn_send_verify_code;
		public TMP_Text error_email;

		public TMP_InputField input_email_verify_code;
		public TMP_Text input_verify_code_timeout;
		public TMP_Text input_verify_code_button_text;
		public TMP_Text input_verify_code_error_message;
		public TMP_Text[] input_codes;
		public Image[] input_verify_code_background;
		public Image[] input_verify_code_underbar;
		public Image[] input_verify_code_caret;
		public ProceduralImage img_email_border;

		public GameObject auth_code_container;

		[Header("========== input password =========")]
		public TMP_InputField input_password;
		public TMP_Text input_password_limit_1;
		public TMP_Text input_password_limit_2;
		public UILoadingButton btn_submit_password;

		[Header("========== input password =========")]
		public TMP_InputField input_password_re;
		public TMP_Text input_password_re_mismatch;
		public UILoadingButton btn_submit_password_re;

		private UnityAction[] setupPageHandlers;
		private int _currentPage;

		private ClientFSM FSM => ClientMain.instance.getFSM();
		private SignUpViewModel ViewModel => ClientMain.instance.getViewModel().SignUp;
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		private bool _input_verify_code_selected = false;
		private IntervalTimer _timerVerifyCode;
		private bool _resending_verify_code;
		private bool _checking_verify_code;
		private int _timeoutSeconds;
		public bool IsWaitingResend => _timeoutSeconds > 0;

		#endregion fields

		#region override

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			setupPageHandlers = new UnityAction[pages.Length];
			setupPageHandlers[0] = setupPage_VerifyEMail;
			setupPageHandlers[1] = setupPage_InputPassword;
			setupPageHandlers[2] = setupPage_InputPasswordRe;
			_currentPage = -1;

			input_email_verify_code.onSelect.AddListener(onSelect_VerifyCode);
			input_email_verify_code.onDeselect.AddListener(onDeselect_VerifyCode);

			_timerVerifyCode = IntervalTimer.create(1.0f, true, false);

			this.reset();
		}

		public override void onTransitionEvent(int type)
		{
			base.onTransitionEvent(type);

			if (type == TransitionEventType.start_open)
			{
				showPage(Page.verify_email);
			}
		}

		public override void update()
		{
			base.update();

			timerVerifyCode();
		}


		private void reset()
		{
			btn_send_verify_code.interactable = true;
			_timeoutSeconds = 0;

			// 이메일 인풋 초기화!
			ViewModel.EMail = string.Empty;
			auth_code_container.SetActive(false);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);

			this.reset();
		}

		#endregion override

		#region page

		public void onClick_Back()
		{
			reset();
			if( _currentPage == Page.verify_email)
			{
				FSM.changeState(ClientStateType.signin);
			}
			else if( _currentPage == Page.input_password)
			{
				showPage(Page.verify_email);
			}
			else if( _currentPage == Page.input_password_re)
			{
				showPage(Page.input_password);
			}
		}

		private void showPage(int page)
		{
			auth_code_container.SetActive(false);

			for(int i = 0; i < pages.Length; ++i)
			{
				pages[i].SetActive(i == page);
			}

			setupPageHandlers[page]();
			_currentPage = page;
		}

		public void setupPage_VerifyEMail()
		{
			input_email.text = "";
			input_email_verify_code.text = "";
			input_email.ActivateInputField();

			ViewModel.EMail = "";
			updateVerifyCodeTime();

			error_email.gameObject.SetActive(false);
			btn_send_verify_code.interactable = false;
		}

		public void setupPage_InputPassword()
		{
			input_password.text = "";
			input_password.ActivateInputField();

			btn_submit_password.interactable = false;
		}

		public void setupPage_InputPasswordRe()
		{
			input_password_re.text = "";
			input_password_re.ActivateInputField();

			input_password_re_mismatch.gameObject.SetActive(false);
			btn_submit_password_re.interactable = false;
		}

		#endregion page

		#region Setup0_VerifyEMail

		public void onValueChanged_InputEMail(string value)
		{
			// 재전송 대기 면 안됨.
			btn_send_verify_code.interactable = !IsWaitingResend && StringUtil.checkEMail(value);

			img_email_border.color = Color.white;
		}

		public void onValueChanged_InputEMailVerifyCode(string value)
		{
			for (int i = 0; i < input_codes.Length; ++i)
			{
				if (i < value.Length)
				{
					input_codes[i].text = value[i].ToString();
					input_verify_code_background[i].gameObject.SetActive(false);
				}
				else
				{
					input_codes[i].text = "";

					if (_input_verify_code_selected && i == value.Length)
					{
						input_verify_code_background[i].gameObject.SetActive(false);
					}
					else
					{
						input_verify_code_background[i].gameObject.SetActive(true);
					}
				}

			}

			updateVerifyCodeUnderbar(value);

			if (value.Length == 6 && _checking_verify_code == false)
			{
				verifyEmailCode(value);
			}
		}

		public void onSelect_VerifyCode(string value)
		{
			_input_verify_code_selected = true;
			updateVerifyCodeUnderbar(value);
		}

		public void onDeselect_VerifyCode(string value)
		{
			_input_verify_code_selected = false;
			updateVerifyCodeUnderbar(value);
		}

		private void updateVerifyCodeUnderbar(string value)
		{
			for (int i = 0; i < input_codes.Length; ++i)
			{
				bool isUnderbarFocused = _input_verify_code_selected && i == value.Length;
			
				//input_verify_code_underbar[i].gameObject.SetActive(isUnderbarShow);

				// focus 된 언더바 -> 하얀색 / 나머지 -> gray700
				input_verify_code_underbar[i].color = isUnderbarFocused ? 
					Color.white :
					UIStyleDefine.ColorStyle.gray700;
				input_verify_code_caret[i].gameObject.SetActive(isUnderbarFocused);

				if (i < value.Length)
				{
					input_verify_code_background[i].gameObject.SetActive(false);
				}
				else
				{
					if (isUnderbarFocused)
					{
						input_verify_code_background[i].gameObject.SetActive(false);
					}
					else
					{
						input_verify_code_background[i].gameObject.SetActive(true);
					}
				}
			}
		}

		private void timerVerifyCode()
		{
			if( _currentPage != Page.verify_email)
			{
				return;
			}

			if( _timerVerifyCode.update())
			{
				updateVerifyCodeTime();
			}
		}

		private void updateVerifyCodeTime()
		{
			if(_resending_verify_code || _checking_verify_code)
			{
				return;
			}
			if( string.IsNullOrEmpty(ViewModel.EMail))
			{
				input_verify_code_error_message.text = "";
				input_verify_code_button_text.text = StringCollection.get("forgotPassword.emailverification.send", 0);
				input_verify_code_timeout.text = "10:00";
				return;
			}

			TimeSpan timeout = ViewModel.VerifyEmailExpireTime - DateTime.UtcNow;
			TimeSpan resend = ViewModel.ResendVerifyCodeTime - DateTime.UtcNow;

			_timeoutSeconds = (int)timeout.TotalSeconds;

			if (_timeoutSeconds > 0)
			{
				if (_timeoutSeconds > 60)
				{
					input_verify_code_timeout.text = $"{(_timeoutSeconds / 60).ToString("D2")}:{(_timeoutSeconds % 60).ToString("D2")}";
				}
				else
				{
					input_verify_code_timeout.text = $"{_timeoutSeconds.ToString("D2")}";
				}

				for (var i = 0; i < input_verify_code_background.Length; ++i)
				{
					input_verify_code_underbar[i].color = UIStyleDefine.ColorStyle.gray700;
					input_verify_code_background[i].color = UIStyleDefine.ColorStyle.gray700;
				}

				if (ViewModel.VerifyEmailResult != -1 && ViewModel.VerifyEmailResult != ResultCode.ok)
				{
					input_verify_code_error_message.text = StringCollection.get("signIn.error.emailverification", 0);
				}
				else
				{
					input_verify_code_error_message.text = "";
				}
			}
			else
			{
				// time is expired.
				input_verify_code_error_message.text = StringCollection.get("signIn.error.timeover", 0);
				input_verify_code_timeout.text = "0:00";

				for (var i = 0; i < input_verify_code_background.Length; ++i)
				{
					input_verify_code_underbar[i].color = UIStyleDefine.ColorStyle.error;
					input_verify_code_background[i].color = UIStyleDefine.ColorStyle.error;
				}
			}

			if (resend.TotalSeconds > 0)
			{
				input_verify_code_button_text.text = StringCollection.getFormat("signIn.button.retry", 0, (int)resend.TotalSeconds);
				btn_send_verify_code.interactable = false;
			}
			else
			{
				input_verify_code_button_text.text = StringCollection.get("forgotPassword.emailverification.send", 0);
				btn_send_verify_code.interactable = true;
			}
		}

		private void showAuthCodeAndToast()
		{
			// 인증 코드 첫 보내기 처리.
			// 이미 인증 코드 보내기가 활성화 되어있었다면 ? (재 인증 하기)
			if (auth_code_container.activeInHierarchy)
			{
				string refStrResendingVerification = StringCollection.get("", 0) ?? "인증 메일을 다시 전송 하였습니다.";

				UIToast.spawn(refStrResendingVerification, new(20, -459))
					.autoClose(3)
					.toggleIcon(false)
					.toggleTextWrap(false)
					.useBackdrop(false)
					.withTransition<SlideUpDownTransition>(c =>
					{
						c.slideDirection = SlideUpDownTransition.SlideDirection.DownToUp;
					});
			}

			auth_code_container.SetActive(true);
		}

		public void onClick_SendVerifyCode()
		{
			btn_send_verify_code.beginLoading();
			_resending_verify_code = true;

			string email = input_email.text;

			// 가입여부 체크 먼저
			checkEMail(email, check_email_result => { 
				if( check_email_result.failed())
				{
					btn_send_verify_code.endLoading();
					_resending_verify_code = false;

					img_email_border.color = UIStyleDefine.ColorStyle.error;

					return;
				}

				// 가입되지 않음
				if (check_email_result.result() == true)
				{
					btn_send_verify_code.endLoading();
					_resending_verify_code = false;

					error_email.gameObject.SetActive(true);
					input_email.text = "";
					btn_send_verify_code.interactable = false;

					img_email_border.color = UIStyleDefine.ColorStyle.error;
					return;
				}
				else // email
				{
					error_email.gameObject.SetActive(false);

					img_email_border.color = Color.white;
				}

				// 아이디 검증 완료.
				SendEMailVerifyCodeProcessor step = SendEMailVerifyCodeProcessor.create(email, false);
				step.run(result => {
					btn_send_verify_code.endLoading();
					_resending_verify_code = false;

					if (result.succeeded())
					{
						btn_send_verify_code.interactable = false;

						ViewModel.EMail = email;
						ViewModel.ResendVerifyCodeTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_resend, 60));
						ViewModel.VerifyEmailExpireTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_timeout, 600));
						ViewModel.VerifyEmailResult = -1;

						//
						input_email_verify_code.text = "";
						input_email_verify_code.ActivateInputField();

						updateVerifyCodeTime();

						showAuthCodeAndToast();
					}
					else
					{
						UIPopup.spawnOK("#인증코드 발송에 실패하였습니다.\n잠시 후 다시 시도해주세요.");
					}
				});

			});
		}

		private void checkEMail(string email,Handler<AsyncResult<bool>> handler)
		{
			MapPacket req = Network.createReqWithoutSession(CSMessageID.Auth.CheckSignUpReq);
			req.put("email", email);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture<bool>(ack.makeErrorException()));
				}
				else
				{
					handler(Future.succeededFuture<bool>((bool)ack.get("check_result")));
				}
			});
		}

		private void verifyEmailCode(string value)
		{
			_checking_verify_code = true;

			// btn_verify_code.beginLoading();

			VerifyEMailProcessor step = VerifyEMailProcessor.create(ViewModel.EMail, value);
			step.run(result =>
			{
				_checking_verify_code = false;
				//btn_verify_code.endLoading();

				if (result.succeeded() && step.getCheckResult() == true)
				{
					ViewModel.VerifyEmailResult = Festa.Client.ResultCode.ok;
					ViewModel.Code = value;

					showPage(Page.input_password);
				}
				else
				{
					// 다시 입력
					ViewModel.VerifyEmailResult = Festa.Client.ResultCode.error;

					input_email_verify_code.text = "";
					updateVerifyCodeTime();
				}
			});
		}

		#endregion

		#region Setup1_InputPassword
		public void onValueChanged_InputPassword(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				input_password_limit_1.color = UIStyleDefine.ColorStyle.gray500;
				input_password_limit_2.color = UIStyleDefine.ColorStyle.gray500;
				btn_submit_password.interactable = false;
			}
			else
			{
				bool check1 = StringUtil.checkPassword_1(value);
				bool check2 = StringUtil.checkPassword_2(value);

				input_password_limit_1.color = check1 ? UIStyleDefine.ColorStyle.success : UIStyleDefine.ColorStyle.error;
				input_password_limit_2.color = check2 ? UIStyleDefine.ColorStyle.success : UIStyleDefine.ColorStyle.error;

				btn_submit_password.interactable = check1 && check2;
			}
		}

		public void onClick_InputPasswordSubmit()
		{
			ViewModel.Password = input_password.text;

			showPage(Page.input_password_re);
		}

		#endregion

		#region Setup2_InputPasswordRe
		public void onValueChanged_InputPasswordRe(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				input_password_re_mismatch.gameObject.SetActive(false);
				btn_submit_password_re.interactable = false;
			}
			else
			{
				bool match = value.Equals(ViewModel.Password);
				input_password_re_mismatch.gameObject.SetActive(match == false);
				btn_submit_password_re.interactable = match;
			}
		}
		
		public void onClick_InputPasswordReBack()
		{
			showPage(Page.input_password);
		}

		public void onClick_InputPasswordResubmit()
		{
			btn_submit_password_re.beginLoading();

			string email = ViewModel.EMail;
			string code = ViewModel.Code;
			string password = EncryptUtil.password(ViewModel.Password);

			ResetPasswordProcessor step = ResetPasswordProcessor.create(email, code, password);
			step.run(result => { 
				if( result.failed())
				{
					UIPopup.spawnOK("#비밀번호 재설정 실패", ()=>{
						showPage(Page.verify_email);
					});
				}
				else
				{
					UIPopup.spawnOK(StringCollection.get("forgotPassword.success", 0), () =>
					{
						FSM.changeState(ClientStateType.signin);
					});
				}
			});
		}
		#endregion
	}
}
