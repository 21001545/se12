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
using Firebase.Auth;
using Festa.Client.Module.Net;
using System;
using System.Text.RegularExpressions;

namespace Festa.Client
{
	public class UISettings : UISingletonPanel<UISettings>
	{
		[SerializeField]
		private Sprite[] checkMarks = new Sprite[3];        // 0 : check inactive, 1 : check active, 2 : x

		// 내 정보 설정을 위한 변수들
		[Header("Name")]
		[SerializeField]
		private TMP_Text txt_name;

		[Header("Phone number")]
		[SerializeField]
		private TMP_Text txt_phoneNumber;

		[Header("Email address")]
		[SerializeField]
		private TMP_Text tmp_emailTitle;
		[SerializeField]
		private TMP_Text txt_emailAddress;
		[SerializeField]
		private GameObject go_setEmail;

		[Header("----verify email")]
		[SerializeField]
		private GameObject[] emailSteps = new GameObject[3];
		[SerializeField]
		private TMP_InputField input_email;
		[SerializeField]
		private Image img_emailInputCheck;
		[SerializeField]
		private Image img_emailInputBg;
		[SerializeField]
		private TMP_Text txt_emailInputErrorMsg;
		[SerializeField]
		private Button btn_emailInput;
		[SerializeField]
		private TMP_InputField input_emailCode;
		[SerializeField]
		private TMP_Text txt_emailVerify_errorMsg;
		[SerializeField]
		private VerificationTimer verifTimer;
		[SerializeField]
		private ResendCodeTimer resendCodeTimer;
		[SerializeField]
		private TMP_Text txt_verifiedEmail;
		[SerializeField]
		private GameObject go_emailBack;
		[SerializeField]
		private GameObject go_emailTitle;

		[Header("Language")]
		[SerializeField]
		private TMP_Text txt_language;
		[SerializeField]
		private GameObject go_setLanguage;
		[SerializeField]
		private GameObject languageItemPrefab;
		[SerializeField]
		private Transform languageItemParent;

		private List<RefLanguage> _refLanguageList;
		private scrollItem _tempSelectedLanguage;

		[Header("Gender")]
		[SerializeField]
		private TMP_Text txt_gender;
		[SerializeField]
		private GameObject go_setGender;
		[SerializeField]
		private scrollItem[] genderItems = new scrollItem[3];

		// 2022.4.15 이강희
		private int _tempSelectedGender;

		[Header("Weight")]
		[SerializeField]
		private TMP_Text txt_weight;
		[SerializeField]
		private GameObject go_setWeight;
		[SerializeField]
		private snapPicker_weight snapPickerWeight;

		[Header("Height")]
		[SerializeField]
		private TMP_Text txt_height;
		[SerializeField]
		private GameObject go_setHeight;
		[SerializeField]
		private snapPicker_height snapPickerHeight;

		[Header("Toggles")]
		// 2022.4.15 이강희
		[SerializeField]
		private UIToggle tgl_distance_unit;
		[SerializeField]
		private UIToggle tgl_temperature_unit;

		private setting_userData _settingData;          // db 에 저장할 정보

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefDataContainer RefDataContainer => GlobalRefDataContainer.getInstance();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		// 2022.4.15 이강희 UISingletonPanel 최초 초기화시 한번말 호출되는 함수
		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			snapPickerHeight.init();
			snapPickerWeight.init();
		}

		private void Start()
        {
			loadLanguageList();
			inputLanguageItems();

		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			// 2022.4.15 이강희 - ViewModel과 Binding
			resetBindings();

			base.open(param, transitionType, closeType);

			// 설정하는 패널들 기본 세팅
			go_setEmail.SetActive(true);
			for (int i = 0; i < emailSteps.Length; ++i)
				emailSteps[i].SetActive(false);

			go_emailBack.SetActive(false);
			go_emailTitle.SetActive(false);

			go_setLanguage.SetActive(true);
			go_setGender.SetActive(true);
			go_setWeight.SetActive(true);
			go_setHeight.SetActive(true);

			// 언어, 성별 정보 설정
			setupLanguage();
			setupGenderItem();
			//setupGender();
		}

		//2022.4.15 이강희
		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			ProfileViewModel profile_vm = ViewModel.Profile;
			HealthViewModel health_vm = ViewModel.Health;

			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.Profile), updateName);
			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.Email), updateEMail);
			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.SettingMap), updateOtherSettings);
			_bindingManager.makeBinding(health_vm, nameof(health_vm.Body), updateBody);
		}

        // 이름 설정
        private void updateName(object obj)
		{
			txt_name.text = ViewModel.Profile.Profile.name;

			// 전화번호 설정도 여기서함
			if( FirebaseAuth.DefaultInstance.CurrentUser != null)
			{
				txt_phoneNumber.text = FirebaseAuth.DefaultInstance.CurrentUser.PhoneNumber;
			}
			else
			{
				txt_phoneNumber.text = "N/A";
			}
		}

#region email

		public class EmailStep
		{
			public static int goBack = -1;
			public static int input = 0;
			public static int verify = 1;
			public static int confirmed = 2;
		}

		private int _currentEmailStep = -1;
		private UIInnerPanelTransition _innerTransition;

		private void openEmailStep()
        {
			if(_innerTransition == null)
				_innerTransition = this.GetComponent<UIInnerPanelTransition>();

			if(_currentEmailStep == EmailStep.input)
            {
				_innerTransition.slideLeftRight(emailSteps[EmailStep.input].GetComponent<RectTransform>(), 375f, 0f, false);
            }
			else if(_currentEmailStep == EmailStep.verify)
            {
				_innerTransition.slideLeftRight(emailSteps[EmailStep.verify].GetComponent<RectTransform>(), 375f, 0f, false);
			}
			else if(_currentEmailStep == EmailStep.confirmed)
            {
				_innerTransition.slideLeftRight(emailSteps[EmailStep.confirmed].GetComponent<RectTransform>(), 375f, 0f, false);
			}
        }

		private void closeEmailStep()
        {
			if (_innerTransition == null)
				_innerTransition = this.GetComponent<UIInnerPanelTransition>();

			if (_currentEmailStep == EmailStep.goBack)
			{
				_innerTransition.reset();
				_innerTransition.slideLeftRight(emailSteps[EmailStep.input].GetComponent<RectTransform>(), 0f, 375f, true);
			}
			else if (_currentEmailStep == EmailStep.input)
			{
				_innerTransition.slideLeftRight(emailSteps[EmailStep.input].GetComponent<RectTransform>(), 0f, 375f, true);
			}
			else if (_currentEmailStep == EmailStep.verify)
			{
				_innerTransition.slideLeftRight(emailSteps[EmailStep.confirmed].GetComponent<RectTransform>(), 0f, 375f, true);
			}
			else if(_currentEmailStep == EmailStep.confirmed)
            {
				_innerTransition.slideLeftRight(null, 0f, 375f, true);
			}
		}

		private void setStep(int step, bool open = true)
        {
			_currentEmailStep = step;

			if(open)
				openEmailStep();
			else
            {
				closeEmailStep();

/*                if (step == EmailStep.goBack)
                {
                    go_setEmail.SetActive(false);
                    return;
                }*/
            }

			if(step == EmailStep.goBack)
            {
				go_emailBack.SetActive(false);
				go_emailTitle.SetActive(false);
			}
			else if (step == EmailStep.input)
			{
				go_emailBack.SetActive(false);
				go_emailTitle.SetActive(false);
				txt_emailInputErrorMsg.gameObject.SetActive(false);
				input_email.shouldHideMobileInput = true;
				input_email.ActivateInputField();
				resetEmailInput();
			}
			else if (step == EmailStep.verify)
			{
				go_emailBack.SetActive(true);
				go_emailTitle.SetActive(true);
				input_emailCode.text = String.Empty;
				input_emailCode.ActivateInputField();
				input_emailCode.shouldHideMobileInput = true;

				txt_emailVerify_errorMsg.gameObject.SetActive(false);
				int validSecs = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Settings.verifyEmail_timeout, 1);
				verifTimer.setCountTime(validSecs);
				verifTimer.turnOnTimer();

				int codeSecs = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Settings.verifyEmail_resendCode, 1);
				resendCodeTimer.setCountTime(codeSecs);
				resendCodeTimer.turnOnTimer();
            }
            else if(step == EmailStep.confirmed)
            {
				go_emailBack.SetActive(false);
				go_emailTitle.SetActive(false);
				txt_verifiedEmail.text = ViewModel.Profile.Email.email;
            }
        }

        private bool checkEmailFormat(string email)
		{
			Regex emailRegex = new Regex(@"^([0-9a-zA-Z-_.]+)@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$");
			return emailRegex.IsMatch(email);
		}

        private void Update()
        {
			if(_currentEmailStep == EmailStep.input)
            {
				if(img_emailInputCheck.sprite == checkMarks[0])
                {
					if (input_email.isFocused)
						img_emailInputBg.color = ColorChart.gray_500;
					else
						img_emailInputBg.color = ColorChart.gray_150;
				}

				btn_emailInput.interactable = input_email.text.Length > 0;
			}
			else if(_currentEmailStep == EmailStep.verify)
            {
				if (verifTimer.isCounting())
				{
					if (input_emailCode.text.Length == 6)
                    {
						confirmEMail();
					}
					else if(input_emailCode.text.Length > 0)
						txt_emailVerify_errorMsg.gameObject.SetActive(false);
				}
				else
				{
					txt_emailVerify_errorMsg.gameObject.SetActive(true);
					setConfirmed_verify(false, 1);

					input_emailCode.text = String.Empty;
				}
			}
        }

		private void setstepVerifyEmail()
        {
			setStep(EmailStep.verify);
		}

        public void registerEMail()
        {
			// 이메일 일단 등록, 인증메일 발송
            MapPacket req = Network.createReq(CSMessageID.Account.RegisterEMailReq);
            req.put("email", input_email.text);

			UIBlockingInput.getInstance().open();
			Network.call(req, ack =>
            {
				UIBlockingInput.getInstance().close();
				if (ack.getResult() != ResultCode.ok)
                {
                    // 이미 다른 유저에게 등록된 메일이다
                    if (ack.getResult() == ResultCode.error_email_used_by_other)
                    {
                        setEmailInput(false, 0);
                    }
                }
                else
                {
					if (_currentEmailStep == EmailStep.verify)
                    {
						// 코드 재전송을 누른 경우
						txt_emailVerify_errorMsg.gameObject.SetActive(false);
						verifTimer.turnOnTimer();

						UIToastNotification.spawn(StringCollection.get("signIn.mobileverification.codeResent", 0));
					}
					else
                    {
						setEmailInput(true);
						Invoke("setstepVerifyEmail", 0.7f);		// 이렇게 안하면 바로 넘어가버려서,, 초록색이 된 모습이 안 보임!!
					}
				}
            });
        }

        private void confirmEMail()
        {
			// 인증번호 인증
			txt_emailVerify_errorMsg.gameObject.SetActive(false);

            MapPacket req = Network.createReq(CSMessageID.Account.ConfirmEMailReq);
            req.put("data", input_emailCode.text);

			UIBlockingInput.getInstance().open();
			Network.call(req, ack =>
            {
				UIBlockingInput.getInstance().close();

				if(ack.getResult() == ResultCode.error_invalid_token)
                {
					setConfirmed_verify(false, 0);
                }
				else if (ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
					setConfirmed_verify(true);
				}
				else
                {
					// 아니면 그냥 다 time out 으로 처리,, 이거맞냐
					setConfirmed_verify(false, 1);
				}
            });

			input_emailCode.text = String.Empty;
		}

        private void updateEMail(object obj)
		{
			string email = ViewModel.Profile.Email.email;

			// 일단 크기부터 조정해볼게
			txt_emailAddress.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(311f - tmp_emailTitle.preferredWidth, 32f);

			// 인증까지 완료된 경우에만 띄워요~!
			if (email != null && ViewModel.Profile.Email.status == 2)
				setAddButton(txt_emailAddress, true, email);
			else
            {
				string msg = StringCollection.get("setting.account.add", 0);
				setAddButton(txt_emailAddress, false, msg);
			}
		}

		private void setEmailInput(bool ok, int errorCode = 0)
        {
			img_emailInputBg.color = ColorChart.isConfirmed_input(ok);
			txt_emailInputErrorMsg.gameObject.SetActive(true);

			if (ok)
            {
				img_emailInputCheck.sprite = checkMarks[1];
				string value = StringCollection.get("setting.account.confirmed_emailInput", 0);
				txt_emailInputErrorMsg.color = ColorChart.success_300;
				txt_emailInputErrorMsg.text = value;
			}
			else
            {
				img_emailInputCheck.sprite = checkMarks[2];
                string value = StringCollection.get("setting.account.error_emailInput", errorCode);
				txt_emailInputErrorMsg.color = ColorChart.error_300;
				txt_emailInputErrorMsg.text = value;
            }
		}

        private void resetEmailInput()
        {
            img_emailInputBg.color = ColorChart.gray_150;
			txt_emailInputErrorMsg.gameObject.SetActive(false);
			img_emailInputCheck.sprite = checkMarks[0];
        }

        private void setConfirmed_verify(bool verified, int errorCode = 0)
        {
			txt_emailVerify_errorMsg.gameObject.SetActive(!verified);

            if (verified)
            {
				// 다음으로 넘어가자구~!
				setStep(EmailStep.confirmed);
			}
            else
            {
                txt_emailVerify_errorMsg.text = StringCollection.get("setting.account.error_emailVerify", errorCode);
            }
        }

        public void onClickSetEmail(bool open)
        {
			for(int i = 0; i < emailSteps.Length; ++i)
				emailSteps[i].SetActive(false);

			input_email.text = String.Empty;	// 최초로 열 때는 이메일 정보를 리셋한당 (festa-304)
			setStep(EmailStep.input);
		}

		public void onClickEmailInput()
        {
			if(checkEmailFormat(input_email.text))
				registerEMail();
			else
				setEmailInput(checkEmailFormat(input_email.text), 1);
        }

		public void onClickEmailInputClear()
        {
			input_email.text = String.Empty;
			input_email.ActivateInputField();
        }

		public void onClickEmailConfirmed()
        {
			setStep(EmailStep.confirmed, false);
        }

		public void onClickEmailBack()
        {
			// 어차피 confirmed 단계에서는 백버튼 사라지니까 그냥 이렇게 해도 괜찮당
			setStep(_currentEmailStep - 1, false);
        }

        #endregion

        #region my info

        public void setupGenderItem()
        {
            for (int i = 0; i < genderItems.Count(); ++i)
            {
                int index = i;

                genderItems[i].gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
					if (_tempSelectedGender == index + 1)
						return;

                    if (_tempSelectedGender != 0)
                        genderItems[_tempSelectedGender - 1].setPicked(false);

                    genderItems[index].setPicked(true);
					_tempSelectedGender = index + 1;
                });
            }
        }

        public void onClickCommitGender()
		{
			ViewModel.Health.Body.gender = _tempSelectedGender;
			sendChangeBody(() => {

				// 탭 닫기~~
				onClickSetGender(false);

			});
		}

		public void onClickGenderItem(int gender)
        {
			_tempSelectedGender = gender;
			updateGenderItems(gender);
        }

		public void onClickSetGender(bool isOpen)
		{
			if(isOpen)
			{
				_tempSelectedGender = ViewModel.Health.Body.gender;
				updateGenderItems(_tempSelectedGender);
			}

			go_setGender.GetComponent<SwipeDownPanel>().swipePanel(isOpen);
		}

#endregion

#region language setting

        private void onClickLangItem(scrollItem _item)
        {
			if (_tempSelectedLanguage == _item)
				return;

            if (_tempSelectedLanguage != null)
                _tempSelectedLanguage.setPicked(false);

            _tempSelectedLanguage = _item;
            _tempSelectedLanguage.setPicked(true);
        }

        private void loadLanguageList()
		{
			List<RefLanguage> refLanguageList = new List<RefLanguage>();
			Dictionary<int, RefData.RefData> dic = RefDataContainer.getMap<RefLanguage>();

			foreach (KeyValuePair<int, RefData.RefData> item in dic)
			{
				RefLanguage refLanguage = item.Value as RefLanguage;
				if (refLanguage.enable == false)
				{
					continue;
				}

				refLanguageList.Add(refLanguage);
			}

			refLanguageList.Sort((a, b) =>
			{

				if (a.order_in_option < b.order_in_option)
				{
					return -1;
				}
				else if (a.order_in_option > b.order_in_option)
				{
					return 1;
				}

				return 0;
			});

			_refLanguageList = refLanguageList;
		}

		private void inputLanguageItems()
        {
			// 이미 아이템이 있다면 텍스트만 바꿔준당
			if(languageItemParent.childCount > 0)
            {
				for(int i = 0; i < languageItemParent.childCount; ++i)
                {
					scrollItem item = languageItemParent.GetChild(i).GetComponent<scrollItem>();
					item.setText(StringCollection.get(LanguageType.getNameKey(_refLanguageList[i].lang_type), 0));
                }
            }
            else
            {
				for (int i = 0; i < _refLanguageList.Count; ++i)
				{
					GameObject item = Instantiate(languageItemPrefab, languageItemParent);

					scrollItem lang_item = item.GetComponent<scrollItem>();
					lang_item.setText(StringCollection.get(LanguageType.getNameKey(_refLanguageList[i].lang_type), 0));
					lang_item.setItemIndex(_refLanguageList[i].lang_type);
					// 2022.4.18 이강희 국기 url설정
					//lang_item.setFlag(LanguageType.getCountryFlagURLFromLanguageType(_refLanguageList[i].lang_type));

					item.GetComponent<Button>().onClick.AddListener(() =>
					{
						onClickLangItem(lang_item);
					});
				}
			}

		}			

		public void onClickCommitLanguage()
		{
			// 선택된 게 없다면,, 리턴!
			if (_tempSelectedLanguage == null)
				return;

			else
			{
				// 바뀐 언어로 데이터 넣어주고
				getSettingData().language = _tempSelectedLanguage.getItemIndex();

				// 메인 설정 뷰 언어 설정 바꾸고
				updateUILanguage(_tempSelectedLanguage.getItemIndex());

				// 언어 아이템도 리로드해주고
				inputLanguageItems();

				// 창 닫아준다~
				onClickSetLanguage(false);
			}
		}

		public void setupLanguage()
        {
			int currentType = StringCollection.getCurrentLangType();
			setAddButton(txt_language, true, StringCollection.get(LanguageType.getNameKey(currentType), 0));
		}

        public void onClickSetLanguage(bool isOpen)
		{
			go_setLanguage.GetComponent<SwipeDownPanel>().swipePanel(isOpen);

			int currentType = StringCollection.getCurrentLangType();
			foreach(Transform item in languageItemParent.transform)
            {
				scrollItem lang_item = item.GetComponent<scrollItem>();
				if (lang_item.getItemIndex() == currentType)
                {
					_tempSelectedLanguage = lang_item;
					lang_item.setPicked(true);
					return;
				}
            }
		}

		public void updateUILanguage(int _langType)
		{
			StringCollection.setCurrentLangType(_tempSelectedLanguage.getItemIndex());

			// UI를 update시켜준다 (보이는 것만 해주면 된다)
			List<UIRefString> ui_string_list = new List<UIRefString>();
			ClientMain.instance.root.GetComponentsInChildren<UIRefString>(false, ui_string_list);

			foreach (UIRefString ui_string in ui_string_list)
			{
				ui_string.applyText();
			}

			// 현재 창의 binding모두 업데이트
			_bindingManager.updateAllBindings();

			// 언어 변경에 따라 지도 주소 업데이트
			//ClientMain.instance.getLocation().forceUpdateAdress();

			// 메인 화면 국기도 바꿔줘야 해
			setupLanguage();
		}

		private void changeSetting(int config_id,int value)
		{
			ViewModel.Profile.setSetting(config_id, value);

			MapPacket req = Network.createReq(CSMessageID.Account.ChangeAccountSettingReq);
			req.put("id", config_id);
			req.put("data", value);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			});
		}

#endregion

#region height, weight, gender

		public void onClickSetWeight(bool isOpen)
		{
			go_setWeight.GetComponent<SwipeDownPanel>().swipePanel(isOpen);
			
			// 2022.4.15 이강희 snapPicker을 열때 현재 body의 몸무게 값으로 picker를 초기화 하도록 구현
			if (isOpen)
			{
				snapPickerWeight.updateFromData();
			}
		}

		public void onClickSetHeight(bool isOpen)
		{
			go_setHeight.GetComponent<SwipeDownPanel>().swipePanel(isOpen);

			// 2022.4.15 이강희 snapPicker을 열때 현재 body의 몸무게 값으로 picker를 초기화 하도록 구현
			if (isOpen)
			{
				snapPickerHeight.updateFromData();
			}
		}

		public void committedWeight(string _value)
        {
			if (_value == "")
            {
				string value = StringCollection.get("setting.account.add", 0);
				setAddButton(txt_weight, false, value);
			}
			else
				setAddButton(txt_weight, true, _value);
		}

		public void committedHeight(string _value)
		{
			if (_value == "")
            {
				string value = StringCollection.get("setting.account.add", 0);
				setAddButton(txt_height, false, value);
			}
			else
				setAddButton(txt_height, true, _value);
		}

        // 2022.4.15 이강희
        public void sendChangeBody(Action callback)
        {
            // 현재 ClientBody값을 서버로 보냄
            ClientBody body = ViewModel.Health.Body;
            MapPacket req = Network.createReq(CSMessageID.Account.ChangeBodyReq);
            req.put("gender", body.gender);
            req.put("weight", body.weight);
            req.put("height", body.height);
            req.put("stride", body.stride);
            req.put("weight_unit_type", body.weight_unit_type);
            req.put("height_unit_type", body.height_unit_type);
            req.put("stride_unit_type", body.stride_unit_type);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);
                }

                callback();
            });
        }

        // 바디 설정 (성별,몸무게,키)
        private void updateBody(object obj)
        {
            ClientBody body = ViewModel.Health.Body;

            updateGenderItems(body.gender);

            if (body.gender > 0)
                setAddButton(txt_gender, true, genderItems[body.gender - 1].getText());
			else
            {
				string value = StringCollection.get("setting.account.add", 0);
				setAddButton(txt_gender, false, value);
			}

            committedHeight(body.getHeightDisplayString());
            committedWeight(body.getWeightDisplayString());
        }

        private void updateGenderItems(int gender)
        {
            // 성별 설정
            for (int i = 0; i < genderItems.Count(); ++i)
            {
                genderItems[i].setPicked((i + 1) == gender);
            }
        }

#endregion

#region measure

        // 2022.4.15 이강희 - 거리 단위 Toggle handler
        public void onChangeDistanceUnit()
		{
			int new_distance_unit = UnitDefine.DistanceType.unknown;
			if (tgl_distance_unit.isOn())
			{
				new_distance_unit = UnitDefine.DistanceType.mil;
			}
			else
			{
				new_distance_unit = UnitDefine.DistanceType.km;
			}

			changeSetting(ClientAccountSetting.ConfigID.distance_unit, new_distance_unit);
		}

		// 2022.4.15 이강희 - 온도 단위 Toggle handler
		public void onChangeTemperatureUnit()
		{
			int new_temperature_unit = UnitDefine.TemperatureType.unknown;
			if (tgl_temperature_unit.isOn())
			{
				new_temperature_unit = UnitDefine.TemperatureType.f;
			}
			else
			{
				new_temperature_unit = UnitDefine.TemperatureType.c;
			}

			changeSetting(ClientAccountSetting.ConfigID.temperature_unit, new_temperature_unit);
		}

        // 기타 서버 저장 설정들 (거리단위, 온도단위)
        private void updateOtherSettings(object obj)
        {
            int distance_unit = ViewModel.Profile.getSettingWithDefault(ClientAccountSetting.ConfigID.distance_unit, UnitDefine.DistanceType.km);
            int temperature_unit = ViewModel.Profile.getSettingWithDefault(ClientAccountSetting.ConfigID.temperature_unit, UnitDefine.TemperatureType.c);

            tgl_distance_unit.set(distance_unit == UnitDefine.DistanceType.km ? false : true, true, false);
            tgl_temperature_unit.set(temperature_unit == UnitDefine.TemperatureType.c ? false : true, true, false);
        }

#endregion

        public setting_userData getSettingData()
        {
            if (_settingData == null)
                _settingData = new setting_userData();

            return _settingData;
        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }


		private void setAddButton(TMP_Text text, bool set, string value)
		{
			text.text = value;

			if (set)
				text.color = ColorChart.gray_500;
			else
				text.color = ColorChart.primary_300_highlighted;
		}
	}
}
