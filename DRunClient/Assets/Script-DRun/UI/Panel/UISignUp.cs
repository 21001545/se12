using DRun.Client.Logic.Account;
using DRun.Client.Logic.SignIn;
using DRun.Client.Module;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DRun.Client
{
    public class UISignUp : UISingletonPanel<UISignUp>
    {
        public static class Page
        {
            public const int terms = 0;
            public const int input_email = 1;
            public const int input_email_verifycode = 2;
            public const int input_password = 3;
            public const int input_password_re = 4;
            public const int input_name = 5;
            public const int input_body = 6;
        }

        [Header("============== Main ============== ")]
        public GameObject[] pages;
        public RectTransform step_gauge;
        public TMP_Text text_page;

        [Header("============== Step0_Terms ====================")]
        public UISpriteToggleButton toggleAgreeAll;
        public UISpriteToggleButton togglePrivacy;
        public UISpriteToggleButton toggleService;
        public UISpriteToggleButton toggleLocation;
        public Button btn_agree;

        [Header("============== Step1_InputEmail ============== ")]
        public UIInputField input_email;
        public UILoadingButton btn_send_verify_code;
        public TMP_Text error_check_signup;

        [Header("============== Step2_InputEmailVerifyCode ==============")]
        public TMP_InputField input_email_verify_code;
        public TMP_Text input_verify_code_sub_message;
        public TMP_Text input_verify_code_timeout;
        public TMP_Text input_verify_code_button_text;
        public TMP_Text input_verify_code_error_message;
        public TMP_Text[] input_codes;
        public Image[] input_verify_code_background;
        public Image[] input_verify_code_underbar;
        public Image[] input_verify_code_caret;
        public UILoadingButton btn_verify_code;

        [Header("============== Step3_InputPassword ==============")]
        public TMP_InputField input_password;
        public TMP_Text input_password_limit_1;
        public TMP_Text input_password_limit_2;
        public UILoadingButton btn_submit_password;

        [Header("============== Step4_InputPassword(Re) ==============")]
        public TMP_InputField input_password_re;
        public TMP_Text input_password_re_mismatch;
        public UILoadingButton btn_submit_password_re;

        [Header("============== Step5_InputName ==============")]
        public TMP_InputField input_name;
        public TMP_Text input_name_condition1;
        public TMP_Text input_name_condition2;
        public TMP_Text input_name_check;
        public UILoadingButton btn_submit_name;

        [Header("============== Step6_InputBody ==============")]
        public UIColorToggleButton toggle_male;
        public UIColorToggleButton toggle_female;
        public TMP_Text body_height;
        public TMP_Text body_weight;
        public UILoadingButton btn_submit_body;

        //-----------------------------------------------------------------------------------------

        private UnityAction[] setupPageHandlers;
        private int _currentPage;

        private ClientFSM FSM => ClientMain.instance.getFSM();
        private SignUpViewModel ViewModel => ClientMain.instance.getViewModel().SignUp;
        private ClientNetwork Network => ClientMain.instance.getNetwork();
        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        private bool _input_verify_code_selected = false;
        private IntervalTimer _timerCheckName;
        private IntervalTimer _timerVerifyCode;
        private bool _resending_verify_code;
        private bool _checking_verify_code;

        private bool _checkingName;
        private string _checkedName;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            setupPageHandlers = new UnityAction[pages.Length];
            setupPageHandlers[0] = setupPage_Terms;
            setupPageHandlers[1] = setupPage_InputEmail;
            setupPageHandlers[2] = setupPage_InputEmailVerifyCode;
            setupPageHandlers[3] = setupPage_InputPassword;
            setupPageHandlers[4] = setupPage_InputPasswordRe;
            setupPageHandlers[5] = setupPage_InputName;
            setupPageHandlers[6] = setupPage_InputBody;
            _currentPage = -1;

            //
            input_email_verify_code.onSelect.AddListener(onSelect_VerifyCode);
            input_email_verify_code.onDeselect.AddListener(onDeselect_VerifyCode);

            //
            _timerVerifyCode = IntervalTimer.create(1.0f, true, false);

            _timerCheckName = IntervalTimer.create(1.0f, false, false);
            _checkedName = string.Empty;
        }

        public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);

            if( type == TransitionEventType.end_open)
            {
                if( getOpenParam() != null)
                {
                    int step = (int)getOpenParam().get("signUpStep");
                    if( step == SignUpStep.created)
                    {
                        showPage(Page.input_name);
                    }
                    else if( step == SignUpStep.complete_name)
                    {
                        showPage(Page.input_body);
                    }
                    else
                    {
                        Debug.Log($"invalid signup step:{step}");
                    }
                }
                else
                {
                    showPage(Page.terms);
                }
            }
        }

        public override void update()
        {
            base.update();

            checkName();
            timerVerifyCode();
        }

        public void setupPage_Terms()
        {
            toggleAgreeAll.setStatus(false);
            togglePrivacy.setStatus(false);
            toggleService.setStatus(false);
            toggleLocation.setStatus(false);

            btn_agree.interactable = false;
        }

        public void setupPage_InputEmail()
        {
            error_check_signup.gameObject.SetActive(false);

            input_email.text = "";
            input_email.ActivateInputField();

            btn_send_verify_code.interactable = false;
        }

        public void setupPage_InputEmailVerifyCode()
        {
            input_email_verify_code.text = "";
            input_email_verify_code.ActivateInputField();

            input_verify_code_underbar[0].gameObject.SetActive(true);

            input_verify_code_sub_message.text = StringCollection.getFormat("signIn.emailverification.desc", 0, ViewModel.EMail);
            updateVerifyCodeTime();
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

        public void setupPage_InputName()
        {
            input_name.text = "";
            input_name.ActivateInputField();

            input_name_condition1.gameObject.SetActive(true);
            input_name_condition2.gameObject.SetActive(true);
            input_name_check.gameObject.SetActive(false);

            btn_submit_name.interactable = false;

            _checkedName = "";
            _timerCheckName.setNext();
        }

        public void setupPage_InputBody()
        {
            ViewModel.Gender = ClientBody.Gender.male;
            ViewModel.Height = 0;
            ViewModel.Weight = 0;

            updateGender();
            updateHeight();
            updateWeight();
            updateSubmit();
        }

        private void showPage(int page)
        {
            setStepGauge((float)(page +1)/ pages.Length);
            text_page.text = $"{page}/{pages.Length}";

            for (int i = 0; i < pages.Length; ++i)
            {
                pages[i].SetActive(i == page);
            }

            setupPageHandlers[page]();
            _currentPage = page;
        }

        private void setStepGauge(float ratio)
        {
            RectTransform rtParent = step_gauge.parent as RectTransform;

            Vector2 offsetMax = step_gauge.offsetMax;

            offsetMax.x = -(1.0f - ratio) * rtParent.rect.width + 3;

            step_gauge.offsetMax = offsetMax;
        }

        public void updateSignupStep(int step)
        {
            MapPacket req = Network.createReq(CSMessageID.Account.UpdateSignupStepReq);
            req.put("step", step);

            Network.call(req, ack => { 
                if( ack.getResult() == ResultCode.ok)
                {
                    ClientMain.instance.getViewModel().updateFromPacket(ack);
                }
            });

        }

        #region Step0_Terms
        public void onClick_AgreeAll()
        {
            bool newStatus = !toggleAgreeAll.status;
            if( newStatus)
            {
                toggleAgreeAll.setStatus(true);
                togglePrivacy.setStatus(true);
                toggleService.setStatus(true);
                toggleLocation.setStatus(true);
            }
            else
            {
                toggleAgreeAll.setStatus(false);
                togglePrivacy.setStatus(false);
                toggleService.setStatus(false);
                toggleLocation.setStatus(false);
            }
            btn_agree.interactable = toggleAgreeAll.status;
        }

        public void onClick_Privacy()
        {
            bool newStatus = !togglePrivacy.status;
            togglePrivacy.setStatus(newStatus);

            toggleAgreeAll.setStatus(togglePrivacy.status && toggleService.status && toggleLocation.status);
            btn_agree.interactable = toggleAgreeAll.status;
        }

        public void onClick_Service()
        {
            bool newStatus = !toggleService.status;
            toggleService.setStatus(newStatus);

            toggleAgreeAll.setStatus(togglePrivacy.status && toggleService.status && toggleLocation.status);
            btn_agree.interactable = toggleAgreeAll.status;
        }

        public void onClick_Location()
        {
            bool newStatus = !toggleLocation.status;
            toggleLocation.setStatus(newStatus);

            toggleAgreeAll.setStatus(togglePrivacy.status && toggleService.status && toggleLocation.status);
            btn_agree.interactable = toggleAgreeAll.status;
        }

        public void onClick_CancelTerms()
        {
            FSM.changeState(ClientStateType.signin);
        }

        public void onClick_AgreeTerms()
        {
            showPage(Page.input_email);
        }

        public void onClick_ShowTerms_Privacy()
        {
            string path = StringCollection.get("signIn.terms.url.privacy", 0);
            string url = GlobalConfig.fileserver_url + "/" + path;

            UIFullscreenWebView.spawnURL(url);
        }

        public void onClick_ShowTerms_Service()
        {
            string path = StringCollection.get("signIn.terms.url.service", 0);
            string url = GlobalConfig.fileserver_url + "/" + path;

            UIFullscreenWebView.spawnURL(url);
        }

        public void onClick_ShowTerms_Location()
        {
            string path = StringCollection.get("signIn.terms.url.location", 0);
            string url = GlobalConfig.fileserver_url + "/" + path;

            UIFullscreenWebView.spawnURL(url);
        }

        #endregion


        #region Step1_InputEMail
        public void onValueChanged_InputEMail(string value)
        {
            btn_send_verify_code.interactable = StringUtil.checkEMail(value);
        }

        public void onClick_SendEmailVerifyCode()
        {
            btn_send_verify_code.beginLoading();

            string email = input_email.text;
            SendEMailVerifyCodeProcessor step = SendEMailVerifyCodeProcessor.create(email, true);
            step.run(result => {
                btn_send_verify_code.endLoading();

                // 성공했으니 다음단계로 넘어가보자
                if (result.succeeded())
                {
                    ViewModel.EMail = email;
                    ViewModel.ResendVerifyCodeTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_resend, 60));
                    ViewModel.VerifyEmailExpireTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_timeout, 600));
                    ViewModel.VerifyEmailResult = -1;

                    showPage(Page.input_email_verifycode);
                }
                else
                {
                    if( step.checkSignUpResult() == false)
                    {
                        error_check_signup.gameObject.SetActive(true);
                    }
                }
            });
        }

        public void onClick_ReturnToLogin()
        {
            FSM.changeState(ClientStateType.signin);
        }

        #endregion

        #region Step2_InputEmailVerifyCode
        public void onValueChanged_InputEMailVerifyCode(string value)
        {
            for(int i = 0; i < input_codes.Length; ++i)
            {
                if( i < value.Length)
                {
                    input_codes[i].text = value[i].ToString();
                    input_verify_code_background[i].gameObject.SetActive(false);
                }
                else
                {
                    input_codes[i].text = "";

                    if( _input_verify_code_selected && i == value.Length)
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

            if( value.Length == 6 && _checking_verify_code == false)
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
            for(int i = 0; i < input_codes.Length; ++i)
            {
                input_verify_code_underbar[i].gameObject.SetActive(_input_verify_code_selected && i == value.Length);
                input_verify_code_caret[i].gameObject.SetActive(_input_verify_code_selected && i == value.Length);

                if( i < value.Length)
                {
                    input_verify_code_background[i].gameObject.SetActive(false);
                }
                else
                {
                    if( _input_verify_code_selected && i == value.Length)
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
            if( _currentPage != Page.input_email_verifycode)
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

            TimeSpan timeout = ViewModel.VerifyEmailExpireTime - DateTime.UtcNow;
            TimeSpan resend = ViewModel.ResendVerifyCodeTime - DateTime.UtcNow;

            int timeout_seconds = (int)timeout.TotalSeconds;

            if (timeout_seconds > 0)
            {
                if (timeout_seconds > 60)
                {
                    input_verify_code_timeout.text = $"{(timeout_seconds / 60).ToString("D2")}:{(timeout_seconds % 60).ToString("D2")}";
                }
                else
                {
                    input_verify_code_timeout.text = $"{timeout_seconds.ToString("D2")}";
                }

                input_verify_code_timeout.color = UIStyleDefine.ColorStyle.gray500;
			
                if(ViewModel.VerifyEmailResult != -1 && ViewModel.VerifyEmailResult != Festa.Client.ResultCode.ok)
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
                input_verify_code_timeout.color = UIStyleDefine.ColorStyle.error;
                input_verify_code_error_message.text = StringCollection.get("signIn.error.timeover", 0);
            }

            if ( resend.TotalSeconds > 0)
            {
                input_verify_code_button_text.text = StringCollection.getFormat("signIn.button.retry", 0, (int)resend.TotalSeconds);
                btn_verify_code.interactable = false;
            }
            else
            {
                input_verify_code_button_text.text = StringCollection.get("signIn.button.emailverification.retry", 0);
                btn_verify_code.interactable = true;
            }
        }

        public void onClick_ResendVerifyCode()
        {
            btn_verify_code.beginLoading();
            _resending_verify_code = true;

            string email = ViewModel.EMail;
            SendEMailVerifyCodeProcessor step = SendEMailVerifyCodeProcessor.create(email, false);
            step.run(result => {
                btn_verify_code.endLoading();
                _resending_verify_code = false;

                // 성공했으니 다음단계로 넘어가보자
                if (result.succeeded())
                {
                    ViewModel.EMail = email;
                    ViewModel.ResendVerifyCodeTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_resend, 60));
                    ViewModel.VerifyEmailExpireTime = TimeUtil.futureFromNow(TimeUtil.msSecond * GlobalRefDataContainer.getConfigInteger(RefConfig.Key.SignIn.email_verification_timeout, 600));
                    ViewModel.VerifyEmailResult = -1;

                    setupPage_InputEmailVerifyCode();
                }

            });
        }

        public void onClick_OtherEMail()
        {
            showPage(Page.input_email);
        }

        private void verifyEmailCode(string value)
        {
            _checking_verify_code = true;

            btn_verify_code.beginLoading();
			
            VerifyEMailProcessor step = VerifyEMailProcessor.create(ViewModel.EMail, value);
            step.run(result => {
                _checking_verify_code = false;
                btn_verify_code.endLoading();

                if( result.succeeded() && step.getCheckResult() == true)
                {
                    input_email_verify_code.DeactivateInputField();
                    ViewModel.VerifyEmailResult = Festa.Client.ResultCode.ok;

                    // 버그 수정
                    TouchScreenKeyboardUtil.getInstance().waitForKeyboardHidingComplete(() => {
                        showPage(Page.input_password);
                    });
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

        #region Step3_InputPassword
        public void onValueChanged_InputPassword(string value)
        {
            if( string.IsNullOrEmpty(value))
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

            btn_submit_password.beginLoading();

            TouchScreenKeyboardUtil.getInstance().waitForKeyboardHidingComplete(() => {
				btn_submit_password.endLoading();

				showPage(Page.input_password_re);
			});
        }

        #endregion

        #region Step4_InputPasswordRe
        public void onValueChanged_InputPasswordRe(string value)
        {
            if( string.IsNullOrEmpty(value))
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

        public void onClick_InputPasswordReSubmit()
        {
            btn_submit_password_re.beginLoading();

            string password = EncryptUtil.password(ViewModel.Password);

            SignUpProcessor step = SignUpProcessor.create(ViewModel.EMail, password);
            step.run(result => { 
			
                if( result.failed())
                {
                    UIPopup.spawnOK("##회원가입 실패", () => {
                        showPage(Page.input_email);
                    });
                }
                else
                {
                    // 로그인 정보 저장
                    ClientMain.instance.getViewModel().SignIn.EMail = ViewModel.EMail;
                    ClientMain.instance.getViewModel().SignIn.LoginTime = TimeUtil.unixTimestampUtcNow(); // 2023.03.21 이강희
					ClientMain.instance.getViewModel().SignIn.saveCache();

                    TouchScreenKeyboardUtil.getInstance().waitForKeyboardHidingComplete(() => {
						showPage(Page.input_name);
					});
                }
			
            });
        }

        private bool _movingToReback = false;

        public void onClick_InputPasswordReBack()
        {
            if(_movingToReback)
            {
                return;
            }

            _movingToReback = true;

            TouchScreenKeyboardUtil.getInstance().waitForKeyboardHidingComplete(() => {
                _movingToReback = false;
                showPage(Page.input_password);
			});
        }

        #endregion

        #region Step5_InputName
        public void onValueChanged_InputName(string value)
        {
            if(_checkingName)
            {
                return;
            }

            if( string.IsNullOrEmpty(value))
            {
                input_name_condition1.gameObject.SetActive(true);
                input_name_condition2.gameObject.SetActive(true);
                input_name_check.gameObject.SetActive(false);
                btn_submit_name.interactable = false;
            }
            else
            {
                bool check = StringUtil.checkForNickname(value);
				
                if( check == false)
                {
                    input_name_condition1.gameObject.SetActive(true);
                    input_name_condition2.gameObject.SetActive(true);
                    input_name_check.gameObject.SetActive(false);
                    btn_submit_name.interactable = false;
                }
                else
                {
                    // async로 서버에 물어본다
                    input_name_condition1.gameObject.SetActive(false);
                    input_name_condition2.gameObject.SetActive(false);
                    input_name_check.gameObject.SetActive(false);
                    btn_submit_name.interactable = _checkedName == value;
                }
            }
        }

        private void checkName()
        {
            if( _currentPage != Page.input_name)
            {
                return;
            }

            if( _timerCheckName.update() == false)
            {
                return;
            }

            string currentName = input_name.text;
            if( string.IsNullOrEmpty(currentName) || currentName == _checkedName)
            {
                _timerCheckName.setNext();
                return;
            }

            if( StringUtil.checkForNickname(currentName) == false)
            {
                _timerCheckName.setNext();
                return;
            }

            checkNameToServer(currentName);
        }

        private void checkNameToServer(string name)
        {
            _checkingName = true;
            _checkedName = name;
            //input_name_limit.gameObject.SetActive(true);
            input_name_check.gameObject.SetActive(false);

            btn_submit_name.beginLoading();

            MapPacket req = Network.createReq(CSMessageID.Account.CheckNameReq);
            req.put("name", name);

            Debug.Log($"check name:{name}");

            Network.call(req, ack => {
                _checkingName = false;
                btn_submit_name.endLoading();
                _timerCheckName.setNext();
				
                if( ack.getResult() == Festa.Client.ResultCode.ok)
                {
                    bool check_result = (bool)ack.get("check_result");

                    input_name_check.gameObject.SetActive(true);
                    //input_name_limit.gameObject.SetActive(false);
                    if ( check_result)
                    {
                        input_name_check.text = StringCollection.get("signIn.username.canuse", 0);
                        input_name_check.color = UIStyleDefine.ColorStyle.success;

                        btn_submit_name.interactable = true;
                    }
                    else
                    {
                        input_name_check.text = StringCollection.get("signIn.username.alreadyused",0);
                        input_name_check.color = UIStyleDefine.ColorStyle.error;

                        btn_submit_name.interactable = false;
                    }
                }
            });
        }
		
        public void onClick_SubmitName()
        {
            ChangeNameProcessor step = ChangeNameProcessor.create(input_name.text);
            step.run(result => { 
                if( result.succeeded())
                {
                    updateSignupStep(SignUpStep.complete_name);

                    showPage(Page.input_body);
                }
                else
                {
                    UIPopup.spawnOK("##이름 설정 실패", () => {
                        showPage(Page.input_name);
                    });
                }
            });
        }

        #endregion

        #region Step6_Body

        private void updateGender()
        {
            toggle_male.setStatus(ViewModel.Gender == ClientBody.Gender.male);
            toggle_female.setStatus(ViewModel.Gender == ClientBody.Gender.female);
        }
        private void updateHeight()
        {
            // 일단 cm
            if( ViewModel.Height == 0)
            {
                body_height.text = StringCollection.get("signIn.common.add", 0);
                body_height.color = UIStyleDefine.ColorStyle.gray700;
            }
            else
            {
                body_height.text = $"{(int)ViewModel.Height}cm";
                body_height.color = UIStyleDefine.ColorStyle.gray200;
            }
        }

        private void updateWeight()
        {
            // 일단 kg
            if( ViewModel.Weight == 0)
            {
                body_weight.text = StringCollection.get("signIn.common.add", 0);
                body_weight.color = UIStyleDefine.ColorStyle.gray700;
            }
            else
            {
                body_weight.text = $"{ViewModel.Weight.ToString("N1")}kg";
                body_weight.color = UIStyleDefine.ColorStyle.gray200;
            }
        }

        private bool checkBodyFormComplete()
        {
            if( ViewModel.Height == 0)
            {
                return false;
            }

            if( ViewModel.Weight == 0)
            {
                return false;
            }

            return true;
        }

        private void updateSubmit()
        {
            btn_submit_body.interactable = checkBodyFormComplete();
        }

        public void onClick_Height()
        {
            UISelectHeight.getInstance().open(ViewModel.Height == 0 ? 170 : ViewModel.Height, (value) => {
                ViewModel.Height = value;
                updateHeight();
                updateSubmit();
            });
        }

        public void onClick_Weight()
        {
            UISelectWeight.getInstance().open(ViewModel.Weight == 0 ? 50 : ViewModel.Weight, (value) => {
                ViewModel.Weight = value;
                updateWeight();
                updateSubmit();
            });
        }

        public void onClick_Gender_Male()
        {
            ViewModel.Gender = ClientBody.Gender.male;
            updateGender();
            updateSubmit();
        }

        public void onClick_Gender_Female()
        {
            ViewModel.Gender = ClientBody.Gender.female;
            updateGender();
            updateSubmit();
        }

        public void onClick_Submit_Body()
        {
            ClientBody body = ClientMain.instance.getViewModel().Health.Body;
            body.gender = ViewModel.Gender;
            body.weight = ViewModel.Weight;
            body.height = ViewModel.Height;

            ChangeBodyProcessor step = ChangeBodyProcessor.create(body);
            step.run(result => {

                if( result.succeeded())
                {
                    updateSignupStep(SignUpStep.complete_body);

                    // 2023.02.24 회원가입시에는 걸음 수 초기값 설정이 않되는 이슈 수정
                    ClientMain.instance.getHealth().getDevice().setLastRecordTime(TimeUtil.unixTimestampUtcNow());

                    FSM.changeState(ClientStateType.end_loading);
                }
                else
                {
                    UIPopup.spawnOK("##신체정보 설정 실패", () => {
                    });
                }
            });
        }

        #endregion
    }
}