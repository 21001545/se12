using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module.UI;
using TMPro;
using Firebase.Auth;
using UnityEngine.Events;
using Festa.Client.Module;
//using PhoneNumbers;
using UnityEngine.UI;
using Festa.Client.Module.Net;
using System;
using Festa.Client.RefData;
using Firebase.Extensions;

namespace Festa.Client
{
	public class UIFirebaseLogin : UISingletonPanel<UIFirebaseLogin>
	{
		public class StepType 
        {
			public static int Prologue = 0;
			public static int PhoneNumber = 1;
			public static int VerifyCode = 2;
            public static int Name = 3;
            public static int Picture = 4;
            public static int Notification = 5;
            public static int PicturePerfect = 6;
			public static int AlreadyHasAccount = 7;
            public static int Done = 8;
        };

		//public TMP_Text	txt_countryPhoneNumber;
		public UIPhotoThumbnail imageCountryFlag;

		private FirebaseAuth _fbAuth;
		private string _verifyingID;
        private UnityAction<FirebaseUser> _loginSuccessCallback;
        private UnityAction _initAccountDoneCallback;
        private UnityAction _restartLoginStateCallback;

		private CountryPhoneNumber _currentCountryPhoneNumber;
		//private AsYouTypeFormatter _formatter;
        
        private Stack _stepStack = new Stack();

		private int _currentStepType = StepType.Prologue;
		public int CurrentStepType
        {
            get
            {
                return _currentStepType;
            }
            set
            {
                Set(ref _currentStepType, value);
            }
        }

        [Header("Common")]
		[SerializeField]
		private Image img_stepGauge;
        [SerializeField]
        private Button btn_stepBack;
        private UIInnerPanelTransition _innerTransition;

        [Header("Step_Prologue")]
        [SerializeField]
        private GameObject step_prologue;

        [Header("Step_PhoneNumber")]
        #region phone number
        [SerializeField]
        private Image img_phoneInputBg;
		[SerializeField]
		private GameObject step_phoneNumber;
        [SerializeField]
        private Button btn_step_phone_next;
        [SerializeField]
        private TMP_InputField input_phoneNumber;
        [SerializeField]
        private GameObject go_clearPhoneNumber;

        // 현재 코드를 확인중인가?? (fb continue with 때문에!)
        private bool _isCheckingPhoneCode = false;
        #endregion

        [Header("Step_Verify")]
        #region verify
        [SerializeField]
        private GameObject step_verify;
        [SerializeField]
        private TMP_InputField input_verifyCode;
        [SerializeField]
        private TMP_Text txt_verify_errorMsg;
        [SerializeField]
        private VerificationTimer verifTimer;
        [SerializeField]
        private ResendCodeTimer resendCodeTimer;

        private FirebaseUser _newFirebaseUser = null;
        #endregion

        [Header("Step_Name")]
        #region name
        [SerializeField]
        private Image img_nameInputBg;
        [SerializeField]
        private Image img_checkMark;
        [SerializeField]
        private GameObject step_name;
        [SerializeField]
        private TMP_InputField input_name;
		[SerializeField]
		private TMP_Text txt_name_valid_status;
        [SerializeField]
        private TMP_Text txt_name_wordCount;
        [SerializeField]
        private Button btn_step_name_next;
        [SerializeField]
        private GameObject go_clearName;
        [SerializeField]
        private Sprite[] checkMarks = new Sprite[3];    // 0 : check inactive, 1 : check active, 2 : x

        private Coroutine CachedValidateNameCoroutine;

		// 입력된 이름이 유효한가?
		private bool _isValidName = false;
        #endregion

        [Header("Step_Picture")]
        [SerializeField]
        private GameObject step_picture;
		[SerializeField]
        private GameObject step_picturePerfect;

        [SerializeField]
        private UIPhotoThumbnail step_picture_photoThumbnail;
        private string selectedPhotoThumbnailPath;

        [SerializeField]
        private UIPhotoThumbnail step_picturePerfect_photoThumbnail;

        [SerializeField]
		private Button btn_step_picture_next;

        [SerializeField]
        private TMP_Text txt_picturePerfect_UserName;

        [Header("Step_Notification")]
        [SerializeField]
        private GameObject step_notification;

        [Header("Step_Duplicate Account")]
        [SerializeField]
        private GameObject step_alreadyAccount;

        [SerializeField]
        private UIPhotoThumbnail step_alreadyAccount_photoThumbnail;

        [SerializeField]
        private TMP_Text txt_alreadyAccount_phoneNumber;

        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();

        public void setLoginSuccessCallback(UnityAction<FirebaseUser> callback)
        {
            _loginSuccessCallback = callback;
		}

		public void setInitAccountDoneCallback(UnityAction callback)
        {
			_initAccountDoneCallback = callback;
		}

        public void setRestartLoginCallback(UnityAction callback)
        {
            _restartLoginStateCallback = callback;
        }

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);

#if !UNITY_EDITOR // 2021.11.03 에디터에서 crash발생
			_fbAuth = FirebaseAuth.DefaultInstance;
#endif
            _innerTransition = this.GetComponent<UIInnerPanelTransition>();

			initCurrentCountryPhoneNumber();
			initUI();
		}

        public void setStep(int type, bool open = true)
        {
            _stepStack.Push(CurrentStepType);
            CurrentStepType = type;
            if(open)
            {
                openStep();
            }

            if (type == StepType.PhoneNumber)
            {
                // 필드 비우고, 필드 포커싱, 버튼 비활
                input_phoneNumber.text = String.Empty;
                input_phoneNumber.shouldHideMobileInput = true;
                input_phoneNumber.ActivateInputField();
                btn_step_phone_next.interactable = false;
            }
            else if (type == StepType.VerifyCode)
            {
                // 필드 비우고, 입력 가능, 필드 포커싱, 에러 날리고, 타이머 리셋, 인풋필드 비우기
                input_verifyCode.text = String.Empty;
                input_verifyCode.ActivateInputField();
                input_verifyCode.shouldHideMobileInput = true;
                input_verifyCode.caretColor = ColorChart.primary_500;
                _isCheckingPhoneCode = false;

                txt_verify_errorMsg.gameObject.SetActive(false);

                int validSecs = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Login.verifyPhone_timeout, 1);
                verifTimer.setCountTime(validSecs);
                verifTimer.turnOnTimer();

                int codeSecs = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Login.verifyPhone_resendCode, 1);
                resendCodeTimer.setCountTime(codeSecs);
                resendCodeTimer.turnOnTimer();
            }
            else if (type == StepType.Name)
            {
                // 필드 비우고, 필드 포커싱, 상태메시지 초기화, 유아이 색 돌리고, 버튼 비활
                input_name.text = String.Empty;
                input_name.shouldHideMobileInput = true;
                input_name.ActivateInputField();

                txt_name_wordCount.text = $"{input_name.text.Length}/{input_name.characterLimit}";
                txt_name_valid_status.gameObject.SetActive(false);
                setUIColor_inputName(0);
                btn_step_name_next.interactable = false;

                updateSignupStep(type);
            }
            else if (type == StepType.Picture)
            {
                // 사진 비우고, 버튼 비활
                step_picture_photoThumbnail.setEmpty();
                btn_step_picture_next.interactable = false;

                updateSignupStep(type);
            }
            else if (type == StepType.Notification)
            {
                btn_stepBack.gameObject.SetActive(false);
                updateSignupStep(type);
            }
            else if (type == StepType.AlreadyHasAccount)
            {
                // Thumbnail 셋팅을 해보자.
                if (ViewModel.Profile != null && ViewModel.Profile.Profile != null)
                {
                    string path = ViewModel.Profile.Profile.getPicktureURL(GlobalConfig.fileserver_url);
                    if (string.IsNullOrEmpty(path) == false)
                        step_alreadyAccount_photoThumbnail.setImageFromCDN(path);
                }
            }
            else if (type == StepType.PicturePerfect
                || type == StepType.AlreadyHasAccount || type == StepType.Done)
            {
                btn_stepBack.gameObject.SetActive(false);
            }
        }

        private void initCurrentCountryPhoneNumber()
		{
			LocaleManager locale = ClientMain.instance.getLocale();
			string cc = locale.getCountryCode();
			//int country_code = PhoneNumberUtil.GetInstance().GetCountryCodeForRegion(cc);

			//_currentCountryPhoneNumber = new CountryPhoneNumber();
			//_currentCountryPhoneNumber.regionCode = cc;
			//_currentCountryPhoneNumber.countryCode = country_code;

			//_formatter = PhoneNumberUtil.GetInstance().GetAsYouTypeFormatter(_currentCountryPhoneNumber.regionCode);
		}

		private void initUI()
		{
            // open 할 때 한 번만 호출되는 아이
            // step specific 하지 않은 애들만 초기화 하기로 해!

			setupCountryPhoneNumber();

            if(CurrentStepType == StepType.Prologue)
            {
                step_prologue.SetActive(false);
                step_phoneNumber.SetActive(false);
                step_verify.SetActive(false);
                step_name.SetActive(false);
                step_picture.SetActive(false);
                step_notification.SetActive(false);
                step_alreadyAccount.SetActive(false);
                step_picturePerfect.SetActive(false);
                openStep();
            }

            if ( _bindingManager.getBindingList().Count == 0 )
            {
				_bindingManager.makeBinding(this, "CurrentStepType", (value) => {

                    /*					step_phoneNumber.SetActive(CurrentStepType== StepType.PhoneNumber);
                                        step_verify.SetActive(CurrentStepType== StepType.VerifyCode);
                                        step_name.SetActive(CurrentStepType== StepType.Name);
                                        step_picture.SetActive(CurrentStepType== StepType.Picture);
                                        step_picturePerfect.SetActive(CurrentStepType == StepType.PicturePerfect);
                                        step_notification.SetActive(CurrentStepType == StepType.Notification);
                                        step_alreadyAccount.SetActive(CurrentStepType == StepType.AlreadyHasAccount);*/

                    setStepGauge(CurrentStepType / 5.0f);
					btn_stepBack.gameObject.SetActive(CurrentStepType > 0);
                });
            }
		}

        private void openStep()
        {
            if (CurrentStepType == StepType.Prologue)
            {
                _innerTransition.slideLeftRight(step_prologue.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            else if (CurrentStepType == StepType.PhoneNumber)
            {
                _innerTransition.slideLeftRight(step_phoneNumber.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            else if (CurrentStepType == StepType.VerifyCode)
            {
                _innerTransition.slideLeftRight(step_verify.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            else if (CurrentStepType == StepType.Name)
            {
                _innerTransition.slideLeftRight(step_name.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            else if (CurrentStepType == StepType.Picture)
            {
                _innerTransition.slideLeftRight(step_picture.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            // 여긴 좀 특별하네!
            else if (CurrentStepType == StepType.PicturePerfect)
            {
                _innerTransition.openImmediately(step_picture.GetComponent<RectTransform>());
            }
            else if (CurrentStepType == StepType.Notification)
            {
                _innerTransition.slideLeftRight(step_notification.GetComponent<RectTransform>(), 375f, 0f, false);
            }
            else if (CurrentStepType == StepType.AlreadyHasAccount)
            {
                _innerTransition.slideLeftRight(step_alreadyAccount.GetComponent<RectTransform>(), 375f, 0f, false);
            }
        }

        private void closeStep()
        {
            if (CurrentStepType == StepType.Prologue)
            {
                _innerTransition.slideLeftRight(step_prologue.GetComponent<RectTransform>(), 0f, 375f, true);
            }
            else if (CurrentStepType == StepType.PhoneNumber)
            {
                _innerTransition.slideLeftRight(step_phoneNumber.GetComponent<RectTransform>(), 0f, 375f, true);
            }
            else if (CurrentStepType == StepType.VerifyCode)
            {
                _innerTransition.slideLeftRight(step_verify.GetComponent<RectTransform>(), 0f, 375f, true);
            }
            else if (CurrentStepType == StepType.Name)
            {
                _innerTransition.slideLeftRight(step_name.GetComponent<RectTransform>(), 0f, 375f, true);
            }
            else if (CurrentStepType == StepType.Picture)
            {
                _innerTransition.slideLeftRight(step_picture.GetComponent<RectTransform>(), 0f, 375f, true);
            }
/*            // 여긴 좀 특별하네!
            else if (CurrentStepType == StepType.PicturePerfect)
            {
                step_picturePerfect.SetActive(true);
            }*/
            else if (CurrentStepType == StepType.Notification)
            {
                _innerTransition.slideLeftRight(step_notification.GetComponent<RectTransform>(), 0f, 375f, true);
            }
            else if (CurrentStepType == StepType.AlreadyHasAccount)
            {
                _innerTransition.slideLeftRight(step_alreadyAccount.GetComponent<RectTransform>(), 0f, 375f, true);
            }
        }

        private void setConfirmed_verify(bool verified)
        {
            Debug.Log($"setConfirmed_verify[{verified}]");

            txt_verify_errorMsg.gameObject.SetActive(!verified);

            if (verified)
            {
                // 다음으로 넘어가자구~!
                // 혹시 몰라 한번만 쓰고 버림
                if( _loginSuccessCallback == null)
                {
                    Debug.LogError("loginSuccessCallback is null");
                }

                _loginSuccessCallback?.Invoke(_newFirebaseUser);
                _loginSuccessCallback = null;
            }
            else
            {
                _isCheckingPhoneCode = false;
                input_verifyCode.text = String.Empty;
                input_verifyCode.caretColor = ColorChart.primary_500;
            }
        }

        public void RequestCode()
		{
//			var util = PhoneNumberUtil.GetInstance();
			
//			PhoneNumber number = util.Parse(input_phoneNumber.text, _currentCountryPhoneNumber.regionCode);
//			string e164_number = util.Format(number, PhoneNumberFormat.E164);

//            txt_alreadyAccount_phoneNumber.text = e164_number;
//#if UNITY_EDITOR
//            Debug.Log($"e164{e164_number}:{util.IsValidNumber(number)}");
//            if (CurrentStepType == StepType.VerifyCode)
//            {
//                // 코드 재전송인 경우
//                verifTimer.turnOnTimer();
//                txt_verify_errorMsg.gameObject.SetActive(false);

//                UIToastNotification.spawn(StringCollection.get("signIn.mobileverification.codeResent", 0));
//            }
//            else
//                onClickStepNext(StepType.VerifyCode);
//            return;
//#endif

//            Debug.Log($"RequestCode for:{e164_number}");

//			UIBlockingInput.getInstance().open();

//			PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(_fbAuth);
//			provider.VerifyPhoneNumber(e164_number, PhoneAuthProvider.MaxTimeoutMs, null,
//				verificationCompleted: (credential) => {
//					Debug.Log("vertificationCompleted");
//				},
//				verificationFailed: (error) => {
//                    MainThreadDispatcher.dispatch(() =>
//                    {
//                        UIBlockingInput.getInstance().close();
//                        Debug.Log(string.Format("vertificationFailed:{0}", error));

//                        UIPopup.spawnOK(error, () =>
//                        {
//                            verifTimer.turnOnTimer();
//                        });
//                    });
//				},
//				codeSent: (id, token) => {
//                    MainThreadDispatcher.dispatch(() =>
//                    {
//                        UIBlockingInput.getInstance().close();
//                        _verifyingID = id;

//                        if(CurrentStepType == StepType.VerifyCode)
//                        {
//                            // 코드 재전송인 경우
//                            verifTimer.turnOnTimer();
//                            txt_verify_errorMsg.gameObject.SetActive(false);

//                            UIToastNotification.spawn(StringCollection.get("signIn.mobileverification.codeResent", 0));
//                        }
//                        else
//                            onClickStepNext(StepType.VerifyCode);
//                    });
//				},
//				codeAutoRetrievalTimeOut: (id) => {
//					Debug.LogWarning(string.Format("codeAutoRetrievalTimeOut:id[{0}]", id));
//				}
//			);
		}

        public void SubmitCode()
		{
#if UNITY_EDITOR
            // 에디터는 그냥 임의로,, 123456 이 아니면 false
            if (input_verifyCode.text == "123456")
                setConfirmed_verify(true);
            else
                setConfirmed_verify(false);

#else
			UIBlockingInput.getInstance().open();

            try
            {
                string code = input_verifyCode.text;
                PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(_fbAuth);

                Debug.Log($"verify_code is : {code}");

                Credential credential = null;

                try
                {
                    credential = provider.GetCredential(_verifyingID, code);
                }
                catch (System.Exception e)
                {
                    UIBlockingInput.getInstance().close();
                    Debug.LogException(e);

                    txt_verify_errorMsg.text = StringCollection.get("singIn.verify.error_notValid", 0); // 이렇게 다 유효하지 않다고 처리해 버려도 되는 거야!??~~~~~~~~~~~~
                    setConfirmed_verify(false);
                    throw e;
                }

                if (credential == null)
                {
                    txt_verify_errorMsg.text = StringCollection.get("singIn.verify.error_notValid", 0);
                    setConfirmed_verify(false);
                    return;
                }

                Debug.Log("start SignInWithCredential");

                _fbAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                {
                    UIBlockingInput.getInstance().close();
                    if (task.IsFaulted)
                    {
                        Debug.LogException(task.Exception);

                        txt_verify_errorMsg.text = StringCollection.get("singIn.verify.error_notValid", 0);
                        setConfirmed_verify(false);
                    }
                    else
                    {
                        // 초록불!~
                        _newFirebaseUser = task.Result;
                        setConfirmed_verify(true);
                        return;
                    }
                });
            }
            catch(System.Exception e)
            {
                txt_verify_errorMsg.text = StringCollection.get("singIn.verify.error_notValid", 0);
                setConfirmed_verify(false);
                Debug.Log(e);
            }
#endif
        }

        #region 공통 기능 처리

        private void setStepGauge(float value)
        {
			value = Mathf.Max(0.0f, Mathf.Min(value, 1.0f));
            float width = -img_stepGauge.rectTransform.offsetMax.x + img_stepGauge.rectTransform.rect.width;
			img_stepGauge.rectTransform.offsetMax = new Vector2(width * (value - 1.0f), img_stepGauge.rectTransform.offsetMax.y);
        }

        public void onClickBack()
        {
            if (CurrentStepType > 0)
            {
                int prevStep = (int)_stepStack.Pop();

                if (prevStep == StepType.VerifyCode)
                {
                    // 아예 state 에 다시 진입해야만
                    updateSignupStep(StepType.PhoneNumber);
                    _restartLoginStateCallback?.Invoke();
                    setStep(StepType.PhoneNumber, false);
                }
                else
                {
                    CurrentStepType = prevStep;
                }

                closeStep();
            }
        }

        #endregion

        #region Step별 처리
        public void onClickStepNext(int stepType)
        {
			setStep(stepType);
        }

		public void onClickPhoneNumberNext()
        {
#if !UNITY_EDITOR
			RequestCode();
#else 
            onClickStepNext(StepType.VerifyCode);
#endif
        }

        public void onClickVerifyNext()
        {
            // 혹시 몰라 한번만 쓰고 버림
            _loginSuccessCallback?.Invoke(_newFirebaseUser);
            _loginSuccessCallback = null;
        }

        // 전화 번호 입력 햇을 때..
        public void onInputPhoneNumberChanged()
        {
            //_formatter.Clear();
            
            //string value = input_phoneNumber.text;
            //string result = "";

            //for (int i = 0; i < value.Length; ++i)
            //{
            //    char ch = value[i];
            //    if (char.IsNumber(ch) == false)
            //    {
            //        continue;
            //    }

            //    result = _formatter.InputDigit(ch);
            //}

            //input_phoneNumber.text = result;
            //input_phoneNumber.stringPosition = result.Length;

            //// 유효한지 확인
            //bool isValid = false;
            //var e164Number = $"+{_currentCountryPhoneNumber.countryCode.ToString()}{result}";

            //try
            //{
            //    var phoneNumber = PhoneNumberUtil.GetInstance().Parse(e164Number, null);
            //    isValid = PhoneNumberUtil.GetInstance().IsValidNumber(phoneNumber);
            //}

            //catch (NumberParseException ex)
            //{
            //}

            //// 유효하면 버튼 켜짐
            //btn_step_phone_next.interactable = isValid;
            ////CommitButtonInteractable(btn_step_phone_next, isValid);
        }

/*        public void onClickVerifyCode()
        {
            if(verifTimer.isCounting())
            {
                Debug.Log("onClickVerifyCode");
                if (_verify_keyboard != null)
                {
                    Debug.Log($"onClickVerifyCode active {_verify_keyboard.active}");
                    if (_verify_keyboard.active == false)
                    {
#if UNITY_IOS
                    _verify_keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.OneTimeCode, false, false, true, false, "", 6);
#else
                        _verify_keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false, false, true, false, "", 6);
#endif
                        Debug.Log($"onClickVerifyCode active {_verify_keyboard.active}");
                    }
                }
            }
        }*/

        public void Update()
        {
            if(CurrentStepType == StepType.PhoneNumber)
            {
                if(input_phoneNumber.isFocused)
                    img_phoneInputBg.color = ColorChart.gray_500;
                else
                    img_phoneInputBg.color = ColorChart.gray_150;

                if (input_phoneNumber.text.Length > 0)
                    go_clearPhoneNumber.SetActive(true);
                else
                    go_clearPhoneNumber.SetActive(false);
            }
            else if(CurrentStepType == StepType.Name)
            {
                if(!txt_name_valid_status.gameObject.activeSelf)
                {
                    if (input_name.isFocused)
                        img_nameInputBg.color = ColorChart.gray_500;
                    else
                        img_nameInputBg.color = ColorChart.gray_150;
                }

                if (input_name.text.Length > 0)
                    go_clearName.SetActive(true);
                else
                    go_clearName.SetActive(false);
            }
            else if(CurrentStepType == StepType.VerifyCode)
            {
                if (verifTimer.isCounting())
                {
                    if (input_verifyCode.text.Length == 6 && !_isCheckingPhoneCode)
                    {
                        input_verifyCode.caretColor = Color.clear;
                        _isCheckingPhoneCode = true;
                        SubmitCode();
                    }
                    else if(input_verifyCode.text.Length > 0)
                        txt_verify_errorMsg.gameObject.SetActive(false);
                }
                else
                {
                    txt_verify_errorMsg.text = StringCollection.get("signIn.verify.error_timeout", 0);
                    txt_verify_errorMsg.gameObject.SetActive(true);

                    input_verifyCode.text = String.Empty;
                }
            }
        }

        public void onClickClearInputField(TMP_InputField input)
        {
            input.text = String.Empty;
            input.ActivateInputField();
        }

        private void setUIColor_inputName(int i, string tempName = "")
        {
            // 색이랑 이미지만 바꿔줄 거야~~

            if (i == -1)
                img_checkMark.sprite = checkMarks[0];
            else
                img_checkMark.sprite = checkMarks[i];           // 체크마크 바꿔 넣고
            txt_name_valid_status.gameObject.SetActive(true);
            var sc = GlobalRefDataContainer.getInstance().getStringCollection();

            switch (i)
            {
                case -1: // 확인 중
                    img_checkMark.sprite = checkMarks[0];
                    txt_name_valid_status.color = ColorChart.gray_500;   // 에러메시지 부분 색 바꾼당
                    txt_name_wordCount.color = ColorChart.gray_500;
                    txt_name_valid_status.text = sc.get("signIn.username.check", 0);
                    break;

                case 0: // 회색체크
                    // 포커싱 여부는 업뎃에서 확인하므로! 여기서는 에러부분만 신경 써 본당
                    img_checkMark.sprite = checkMarks[0];
                    txt_name_wordCount.color = ColorChart.gray_500;
                    txt_name_valid_status.gameObject.SetActive(false);
                    break;

                case 1: // 초록체크
                    img_checkMark.sprite = checkMarks[1];
                    img_nameInputBg.color = ColorChart.success_100;
                    txt_name_valid_status.color = ColorChart.success_300;
                    txt_name_wordCount.color = ColorChart.success_300;
                    txt_name_valid_status.text = sc.getFormat("signIn.username.canuse", 0, tempName);
                    break;

                case 2: // 엑스자
                    img_checkMark.sprite = checkMarks[2];
                    img_nameInputBg.color = ColorChart.error_100;
                    txt_name_valid_status.color = ColorChart.error_300;
                    txt_name_wordCount.color = ColorChart.error_300;
                    txt_name_valid_status.text = sc.getFormat("signIn.username.alreadyused", 0, tempName);
                    break;
            }
        }

        // 이름 입력 했을 때, 
        // 이벤트 호출할때마다 네트워크에 요청하면 부하가 올테니, 최소 0.5초 입력을 안하면.. 네트워크에 보내도록 해보자.
        public void onInputNameChanged()
        {
            txt_name_wordCount.text = $"{input_name.text.Length}/{input_name.characterLimit}";

            if (CachedValidateNameCoroutine != null)
            {
				StopCoroutine(CachedValidateNameCoroutine);
				CachedValidateNameCoroutine = null;
			}

			_isValidName = false;
            btn_step_name_next.interactable = _isValidName;

            if(input_name.text.Length > 0)
                CachedValidateNameCoroutine = StartCoroutine(ValidateNameCoroutine());
		}

		private IEnumerator ValidateNameCoroutine()
        {
            setUIColor_inputName(-1);
            yield return new WaitForSeconds(0.5f);

            // 아직 중복 체크 API가 없다.
            // 무조건 되는 걸로..!

            string tempName = input_name.text;
            MapPacket req = Network.createReq(CSMessageID.Account.CheckNameReq);
            req.put("name", tempName);
            
            bool wait = true;
            Network.call(req, ack =>
            {
				// 에러가 나면 일단 사용은 못하게 하자. 중복이든.. 서버 에러든..
				// 서버에서 통과가 안되었을 테니..
				// ResultCode.error_username_already_exists
				_isValidName = ack.getResult() == ResultCode.ok;
				wait = false;
			});

            yield return new WaitWhile(() => { return wait; });

			if (_isValidName)
                setUIColor_inputName(1, tempName);
			else
                setUIColor_inputName(2, tempName);

            btn_step_name_next.interactable = _isValidName;
        }

		public void onClickNameNext()
        {
            // 이름 업데이트를 합시다.

            MapPacket req = Network.createReq(CSMessageID.Account.ModifyProfileReq);
            req.put("agent_id", 0);
            req.put("agent_upload_id", 0);
            req.put("name", input_name.text);
            req.put("message", "");

            UIBlockingInput.getInstance().open();
            Network.call(req, ack =>
            {
                UIBlockingInput.getInstance().close();
                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);
                    txt_picturePerfect_UserName.text = input_name.text;

                    // 다음 스탭으로 넘겨보자.
                    onClickStepNext(StepType.Picture);
                }
				else
                {
                    // 사실 여기서는 이제 중복될 일이 없는 거 아닌가,,~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
					// 이름이 중복 되었다. 다시.. 변경하도록 해보자.
                    _isValidName = false;
                    setUIColor_inputName(2, input_name.text);
                    btn_step_name_next.interactable = _isValidName;
                }
            });
        }


		public void onClickPicture()
        {
            // 음 사진 선택하는게 없으니 임의로 할까?
            UIGalleryPicker.getInstance().setFinishCallback(onSelectPhoto);
            UIGalleryPicker.getInstance().open();
            UIGalleryPicker.getInstance().setMaxCount(1);
        }

		// UIPhotoTake로 부터 전달되는... 사진 리스트.
        public void onSelectPhoto(List<NativeGallery.NativePhotoContext> photoList)
        {
            if (photoList.Count > 0)
            {
                step_picture_photoThumbnail.setImageFromFile(photoList[0]);
                step_picturePerfect_photoThumbnail.setImageFromFile(photoList[0]);
                selectedPhotoThumbnailPath = photoList[0].path;

                btn_step_picture_next.interactable = true;
            }
        }
        IEnumerator WaitForParticle()
		{
			yield return new WaitForSeconds(2.0f);

            onClickStepNext(StepType.Notification);
        }

        public void onClickPictureSkip()
        {
            onClickStepNext(StepType.Notification);
        }

        public void onClickAlreadySign()
        {
            onClickStepNext(StepType.Notification);
        }

        public void onClickPictureNext()
        {
            // 서버에 보내는게 없어서,
            // 바로 재생
            List<string> photo_list = new List<string>();
            photo_list.Add(selectedPhotoThumbnailPath);

            HttpFileUploader uploader = Network.createFileUploader(photo_list);
            UIBlockingInput.getInstance().open();
            uploader.run(ack =>
            {
                UIBlockingInput.getInstance().close();
                // 실패 해도 그냥 넘기자.
                // 유저가 다시 ..등록 하겠지.
                if (ack.getInteger("result") != ResultCode.ok)
                {
                    UIPopup.spawnOK("upload fail..", () =>
                    {
                        onClickStepNext(StepType.Notification);
                    });
                }
                else
                {
                    // Profile.picture_url이 위 업로드에서 받아오질 않네,
                    // 다시한번 그냥 보내서 받아보자. 위에 api가 수정되기 전까진.
                    MapPacket req = Network.createReq(CSMessageID.Account.ModifyProfileReq);
                    req.put("agent_id", ack.getInteger("agent_id"));
                    req.put("agent_upload_id", ack.getInteger("id"));
                    req.put("name", input_name.text);
                    req.put("message", "");

                    UIBlockingInput.getInstance().open();
                    Network.call(req, ack =>
                    {
                        UIBlockingInput.getInstance().close();
                        if (ack.getResult() == ResultCode.ok)
                        {
                            ViewModel.updateFromPacket(ack);
                            // 다음 스탭으로 넘겨보자.
                            onClickStepNext(StepType.PicturePerfect);
                            StartCoroutine(WaitForParticle());
                        }
                        else
                        {
                            onClickStepNext(StepType.Notification);
                        }
                    });
                }
            });
        }

		public void onClickNotification(bool skip)
        {
            if(skip)
            {
                // 노티설정안함
                PlayerPrefs.SetInt("isNotiSkipped", 1);
            }
            else
            {
                // 노티받을게요!
                PlayerPrefs.SetInt("isNotiSkipped", 0);
            }

            updateSignupStep(StepType.Done);

            // 2022.8.24 로딩창을 다시 띄워주자
            UILoading.getInstance().open();

            _initAccountDoneCallback?.Invoke();
            _initAccountDoneCallback = null;

        }

		// 해당 계정 계속 사용...
		public void onClickContinueAccount()
        {
            onClickStepNext(StepType.Notification);
        }

		public void onClickResetAccount()
        {
            var sc = GlobalRefDataContainer.getInstance().getStringCollection();

            UIPopup.spawnYesNo(sc.get("singIn.reset.title.popup", 0), sc.get("singIn.reset.desc.popup", 0), () =>
            {
                string firebase_id = ClientMain.instance.getData().getStartupContext().firebase_id;

                // 리셋이므로 콜백은 그냥 꺼버림.
                _initAccountDoneCallback = null;

                UIBlockingInput.getInstance().open();

                MapPacket req = Network.createReq(CSMessageID.Account.WithdrawReq);
                req.put("firebase_id", firebase_id);

                Network.call(req, ack =>
                {
                    UIBlockingInput.getInstance().close();
					
					//서버에 다시 로그인~
                    ClientMain.instance.getFSM().changeState(ClientStateType.server_login);
                });
            });
        }
#endregion


        public void onClickSelectCountry()
		{
			UISelectCountryPhoneNumber.getInstance().setup(_currentCountryPhoneNumber.regionCode, onSelectCountryPhoneNumber);
			UISelectCountryPhoneNumber.getInstance().open();
		}

        private void onSelectCountryPhoneNumber(CountryPhoneNumber data)
        {
            //_currentCountryPhoneNumber = data;
            //_formatter = PhoneNumberUtil.GetInstance().GetAsYouTypeFormatter(_currentCountryPhoneNumber.regionCode);
            //setupCountryPhoneNumber();
        }

        private void setupCountryPhoneNumber()
		{
			//txt_countryPhoneNumber.text = $"+{_currentCountryPhoneNumber.countryCode.ToString()}";
			imageCountryFlag.setImageFromCDN(_currentCountryPhoneNumber.getFlagURL());
		}

        //2021.11.19 이강희 progress 저장
        public void updateSignupStep(int step)
        {
            MapPacket req = Network.createReq(CSMessageID.Account.UpdateSignupStepReq);
            req.put("step", step);

            Network.call(req, ack => { 
                if( ack.getResult() == ResultCode.ok)
				{
                    ViewModel.updateFromPacket(ack);
				}
            });
		}
	}
}


