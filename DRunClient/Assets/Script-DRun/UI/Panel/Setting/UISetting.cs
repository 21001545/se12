using System;

using DRun.Client.Logic.Account;
using DRun.Client.Logic.SignIn;
using DRun.Client.Module;

using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;

using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace DRun.Client
{
	[RequireComponent(typeof(SlideUpDownTransition))]
	public class UISetting : UISingletonPanel<UISetting>
	{
		public class PageType
		{
			public const int menu = 0;
			public const int privacy = 1;
			public const int name = 2;
			public const int gender = 3;
			public const int height_weight = 4;
			public const int terms = 5;
			public const int help = 6;
		}

		public GameObject[] pageList;

		[Header("================ menu ==============")]
		public TMP_Text txt_version;

		public GameObject top_base;

		[Header("================ privacy ===========")]
		public TMP_Text txt_email;
		public TMP_Text txt_name;
		public TMP_Text txt_gender;
		public TMP_Text txt_height;
		public TMP_Text txt_weight;

		[Header("================ gender ===========")]
		public UIMultiColorToggleButton toggleMale;
		public UIMultiColorToggleButton toggleFemale;

		[Header("================ name ===========")]
		public UIInputField inputName;
		public TMP_Text input_name_limit;
		public TMP_Text input_name_check;
		public UILoadingButton btn_submit_name;

		[Header("================ height / weight ===========")]
		public TMP_Text txt_title;

		/// <summary>
		/// 0 - height
		/// 1 - weight
		/// </summary>
		[Space(10)]
		public GameObject[] height_weight_containers;

		public UILoadingButton btn_height_weight_apply;

		/// <summary>
		/// true - picking height.
		/// false - picking weight.
		/// </summary>
		private bool _isPickerOpen = false;

		public enum Measurement
		{
			weight,
			height,
		}

		//private Measurement _lastMeasurement;
		/// <summary>
		/// UnitDefine.HeightType 으로 체크.
		/// </summary>
		private int _heightUnit = UnitDefine.DistanceType.cm;

		public TMP_Text txt_cm;

		public TMP_Text txt_ft;
		public TMP_Text txt_inches;

		public TMP_Text txt_kg;
		public TMP_Text txt_kg_float;

		public const int StartingHeight = 170;
		public const int StartingWeight = 60;


		//-----------------------------------------
		private int _currentPage = -1;

		private IntervalTimer _timerCheckName;
		private bool _checkingName;
		private string _checkedName;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientBody BodyVM => ViewModel.Health.Body;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_timerCheckName = IntervalTimer.create(1.0f, false, false);
			_checkedName = string.Empty;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);

			setupUI();
			showPage(PageType.menu, useTransition: false);

			// 2022-12-27 윤상
			// close 시 transition animation 이후 원래 위치로 이동 안해서, 강제로 이동.
			// 아마 UIPanel 설정 시에 포지션이 다른 곳으로 이동되서??
			var currentPage = currentPageRectTransform(PageType.menu);
			currentPage.anchoredPosition = Vector2.zero;

			btn_height_weight_apply.interactable = true;

			UISelectHeight.getInstance()
				.pickerUnit
				.onSnapped
				.RemoveListener(updateHeightUnit);

			UISelectHeight.getInstance()
				.pickerUnit
				.onSnapped
				.AddListener(updateHeightUnit);

			UISelectWeight.getInstance()
				.pickerUnit
				.onSnapped
				.RemoveListener(updateWeightUnit);

			UISelectWeight.getInstance()
				.pickerUnit
				.onSnapped
				.AddListener(updateWeightUnit);
		}

		public override void update()
		{
			base.update();

			checkName();
		}

		public override void onLanguageChanged(int newLanguage)
		{
			setupUI();
		}

		private void setupUI()
		{
			txt_version.text = Application.version;
			txt_email.text = ViewModel.SignIn.EMail;
			txt_name.text = ViewModel.Profile.Profile.name;
			//txt_gender.text = ViewModel.Body.gender == ClientBody.Gender/.
			txt_height.text = ViewModel.Health.Body.height.ToString("N0") + "cm";
			string weightStr = ViewModel.Health.Body.weight.ToString("N1");

			txt_weight.text = (weightStr.EndsWith('0') ? 
				weightStr.Substring(0, weightStr.Length - 2) : // 100.0 에서 마지막 .0 자름.
				weightStr) + "kg";


			if (ViewModel.Health.Body.gender == ClientBody.Gender.male)
			{
				txt_gender.text = StringCollection.get("signIn.physical.desc.male", 0);
			}
			else
			{
				txt_gender.text = StringCollection.get("signIn.physical.desc.female", 0);
			}

			setupGenderUI();
		}

		private void setupGenderUI()
		{
			toggleMale.setStatus(ViewModel.Health.Body.gender == ClientBody.Gender.male);
			toggleFemale.setStatus(ViewModel.Health.Body.gender == ClientBody.Gender.female);
		}

		private void setupHeightUI()
		{
			if (_heightUnit == UnitDefine.DistanceType.cm)
			{
				txt_cm.text = BodyVM.height.ToString("N0");
				return;
			}

			if (_heightUnit == UnitDefine.DistanceType.ft)
			{
				UISelectHeight.cm_to_ft(
					Mathf.CeilToInt((float)BodyVM.height),
					out int newHigh,
					out int newLow
				);

				txt_ft.text = newHigh + "\'";
				txt_inches.text = newLow + "\"";
			}
		}

		private void setupWeightUI()
		{
			var newWeights = BodyVM.weight
				.ToString("N1")
				.Split('.');

			txt_kg.text = newWeights[0];
			txt_kg_float.text = newWeights[1];
		}

		private void showPage(int type, bool useTransition = true, bool isRightToLeft = true)
		{
			// 상단 Background 켜고 끄기
			top_base.gameObject.SetActive(
				type switch
				{
					PageType.menu => true,
					PageType.privacy => true,
					PageType.terms => true,

					_ => false,
				}
			);

			_currentPage = type;
			for (int i = 0; i < pageList.Length; i++)
			{
				pageList[i].SetActive(i == type);
			}

			if (useTransition)
			{
				currentPageRectTransform(type).moveInDirection(
				    delta: (
					    from: (isRightToLeft ? 1 : -1) * (float)Screen.width * 0.5f,
					    to: 0
				    )
			    );
			}
		}

		private RectTransform currentPageRectTransform(int type)
		{
			return pageList[type].GetComponent<RectTransform>();
		}

		public void onClick_Back()
		{
			switch (_currentPage)
			{
				case PageType.menu:
					{
						UIMainTab.getInstance().open();
						currentPageRectTransform(PageType.menu).moveInDirection(delta: (from: 0, to: Screen.width * 0.5f));
						break;
					}

				// privacy page 다음 depth 인 애들 (depth: 2) 
				case PageType.name:
				case PageType.gender:
					{
						showPage(PageType.privacy, isRightToLeft: false);
						break;
					}

				// 바로 위로 올라감.
				case PageType.height_weight when _isPickerOpen:
					{
						closeHeightWeightPicker();
						goto case PageType.name;
					}

				default:
					{
						showPage(PageType.menu, isRightToLeft: false);
						break;
					}
			}

			void closeHeightWeightPicker()
			{
				UISelectHeight.getInstance().toggleBackdrop(true);
				UISelectWeight.getInstance().toggleBackdrop(true);

				// Picker 열려있으면 닫기.
				if (UISelectHeight.getInstance().gameObject.activeInHierarchy)
					UISelectHeight.getInstance().close();

				if (UISelectWeight.getInstance().gameObject.activeInHierarchy)
					UISelectWeight.getInstance().close();

				_isPickerOpen = false;
			}
		}

		#region Menu
		public void onClick_Menu_Privacy() => showPage(PageType.privacy);
		public void onClick_Menu_Language() => UISelectLanguage.getInstance().open();
		public void onClick_Menu_Terms() => showPage(PageType.terms);
		public void onClick_Menu_Help() => showPage(PageType.help);

		#endregion

		#region Privacy
		public void onClick_Privacy_Name()
		{
			showPage(PageType.name);

			inputName.text = "";
			inputName.ActivateInputField();

			input_name_check.gameObject.SetActive(false);

			btn_submit_name.interactable = false;
			_checkedName = "";
			_timerCheckName.setNext();
		}

		public void onClick_Privacy_Gender()
		{
			setupGenderUI();
			showPage(PageType.gender);
		}

		public void onClick_Privacy_Height()
		{
			showPage(PageType.height_weight);
			selectHeightOrWeight(true);

			// TODO: Preference -> Metrics / Imperial 선택.
			selectMetricOrImperial(true);

			UISelectHeight.getInstance().toggleBackdrop(false);
			openHeightPicker();
		}

		public void onClick_Privacy_Weight()
		{
			showPage(PageType.height_weight);
			selectHeightOrWeight(false);

			UISelectWeight.getInstance().toggleBackdrop(false);
			openWeightPicker();
		}

		#endregion

		#region Name
		public void onValueChange_InputName(string value)
		{
			if (_checkingName)
			{
				return;
			}

			if (string.IsNullOrEmpty(value))
			{
				input_name_limit.gameObject.SetActive(true);
				input_name_check.gameObject.SetActive(false);
				btn_submit_name.interactable = false;
			}
			else
			{
				bool check = StringUtil.checkForNickname(value);

				if (check == false)
				{
					input_name_limit.gameObject.SetActive(true);
					input_name_check.gameObject.SetActive(false);
					btn_submit_name.interactable = false;
				}
				else
				{
					input_name_limit.gameObject.SetActive(false);
					input_name_check.gameObject.SetActive(false);
					btn_submit_name.interactable = _checkedName == value;
				}
			}

		}

		private void checkName()
		{
			if (_currentPage != PageType.name)
			{
				return;
			}

			if (_timerCheckName.update() == false)
			{
				return;
			}

			string currentName = inputName.text;
			if (string.IsNullOrEmpty(currentName) || currentName == _checkedName)
			{
				_timerCheckName.setNext();
				return;
			}

			if (StringUtil.checkForNickname(currentName) == false)
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

			input_name_check.gameObject.SetActive(false);

			btn_submit_name.beginLoading();

			MapPacket req = Network.createReq(CSMessageID.Account.CheckNameReq);
			req.put("name", name);

			Debug.Log($"check name:{name}");
			Network.call(req, ack =>
			{
				_checkingName = false;
				btn_submit_name.endLoading();
				_timerCheckName.setNext();

				if (ack.getResult() == Festa.Client.ResultCode.ok)
				{
					bool check_result = (bool)ack.get("check_result");

					input_name_check.gameObject.SetActive(true);
					if (check_result)
					{
						input_name_check.text = StringCollection.get("signIn.username.canuse", 0);
						input_name_check.color = UIStyleDefine.ColorStyle.success;

						btn_submit_name.interactable = true;
					}
					else
					{
						input_name_check.text = StringCollection.get("signIn.username.alreadyused", 0);
						input_name_check.color = UIStyleDefine.ColorStyle.error;

						btn_submit_name.interactable = false;
					}
				}
			});
		}

		public void onClick_Name_SubmitName()
		{
			ChangeNameProcessor step = ChangeNameProcessor.create(inputName.text);
			step.run(result =>
			{
				if (result.succeeded())
				{
					setupUI();
					showPage(PageType.privacy, isRightToLeft: false);
				}
				else
				{
					UIPopup.spawnOK("##이름 설정 실패", () =>
					{
						showPage(PageType.privacy, isRightToLeft: false);
					});
				}
			});
		}

		#endregion

		#region Gender

		public void onClick_Gender_Male()
		{
			toggleMale.setStatus(true);
			toggleFemale.setStatus(false);
		}

		public void onClick_Gender_Female()
		{
			toggleMale.setStatus(false);
			toggleFemale.setStatus(true);
		}

		public void onClick_Gender_Confirm()
		{
			int newGender = toggleMale.status ? ClientBody.Gender.male : ClientBody.Gender.female;

			ClientBody body = ViewModel.Health.Body;
			body.gender = newGender;

			ChangeBodyProcessor step = ChangeBodyProcessor.create(body);
			step.run(result =>
			{
				if (result.succeeded())
				{
					setupUI();
				}
				showPage(PageType.privacy, isRightToLeft: false);
			});
		}

		#endregion

		#region height / weight

		private void applyNewHeightAndWeight()
		{
			ChangeBodyProcessor.create(BodyVM)
				.run(result =>
			{
				if (result.succeeded())
				{
					setupUI();
					showPage(PageType.privacy);
				}
				else
				{
					UIPopup.spawnOK("##신체정보 설정 실패", () =>
					{
					});
				}
			});
		}

		private void selectHeightOrWeight(bool isHeight)
		{
			if (isHeight)
			{
				txt_title.text = StringCollection.get("signIn.physical.desc.height", 0).NullIfEmpty() ?? "키";
				height_weight_containers[0].SetActive(true);
				height_weight_containers[1].SetActive(false);
			}
			else
			{
				txt_title.text = StringCollection.get("signIn.physical.desc.weight", 0).NullIfEmpty() ?? "몸무게";
				height_weight_containers[0].SetActive(false);
				height_weight_containers[1].SetActive(true);
			}
		}

		/// <summary>
		/// metric - 미터계
		/// imperial - 피트/인치
		/// </summary>
		/// <param name="isMetric"></param>
		public void selectMetricOrImperial(bool isMetric)
		{
			var heightContainer = height_weight_containers[0];
			var metricContainer = heightContainer.transform.GetChild(0);
			var imperialContainer = heightContainer.transform.GetChild(1);

			if (isMetric)
			{
				metricContainer.gameObject.SetActive(true);
				imperialContainer.gameObject.SetActive(false);
			}
			else
			{
				metricContainer.gameObject.SetActive(false);
				imperialContainer.gameObject.SetActive(true);
			}
		}


		public void openHeightPicker()
		{
			_isPickerOpen = true;
			//_lastMeasurement = Measurement.height;

			UISelectHeight.getInstance().open(
				BodyVM.height == 0 ? StartingHeight : BodyVM.height,
				newHeightInCm =>
				{
					if (Math.Abs(BodyVM.height - newHeightInCm) <= Mathf.Epsilon)
						return;

					BodyVM.height = newHeightInCm;
					setupHeightUI();
					applyNewHeightAndWeight();
				}
			);
		}

		public void openWeightPicker()
		{
			_isPickerOpen = true;
			//_lastMeasurement = Measurement.weight;

			UISelectWeight.getInstance().open(
				BodyVM.weight == 0 ? StartingWeight : BodyVM.weight,
				newWeightInKg =>
				{
					if (Math.Abs(BodyVM.weight - newWeightInKg) <= Mathf.Epsilon)
						return;

					BodyVM.weight = newWeightInKg;
					setupWeightUI();
					applyNewHeightAndWeight();
				}
			);
		}

		private void updateHeightUnit(int param)
		{
			if (param == 0) // UnitDefine.DistanceType.cm
			{
				selectMetricOrImperial(true);
				setupHeightUI();
				_heightUnit = UnitDefine.DistanceType.cm;

				return;
			}

			if (param == 1) // UnitDefine.DistanceType.ft
			{
				selectMetricOrImperial(false);
				setupHeightUI();
				_heightUnit = UnitDefine.DistanceType.ft;
			}
		}

		private void updateWeightUnit(int param)
		{
			setupWeightUI();

			if (param == 0)
			{
				
			}

		}

		//private void updateWeightHeightButton()
		//{
		//	switch (_lastMeasurement)
		//	{
		//		case Measurement.weight:
		//			btn_height_weight_apply.interactable = BodyVM.weight > 0;
		//			break;

		//		case Measurement.height:
		//			btn_height_weight_apply.interactable = BodyVM.height > 0;
		//			break;
		//	}
		//}

		#endregion height / weight

		#region Terms
		public void onClick_Terms_Service()
		{
			string path = StringCollection.get("signIn.terms.url.service", 0);
			string url = Festa.Client.GlobalConfig.fileserver_url + "/" + path;

			UIFullscreenWebView.spawnURL(url);
		}

		public void onClick_Terms_Privacy()
		{
			string path = StringCollection.get("signIn.terms.url.privacy", 0);
			string url = Festa.Client.GlobalConfig.fileserver_url + "/" + path;

			UIFullscreenWebView.spawnURL(url);
		}

		public void onClick_Terms_Location()
		{
			string path = StringCollection.get("signIn.terms.url.location", 0);
			string url = Festa.Client.GlobalConfig.fileserver_url + "/" + path;

			UIFullscreenWebView.spawnURL(url);
		}



		#endregion

		#region help
		public void onClick_Help_HomePage()
		{
			Application.OpenURL(StringCollection.get("setting.url.homepage", 0));
		}

		public void onClick_Help_Discord()
		{
			Application.OpenURL(StringCollection.get("setting.url.discord", 0));
		}

		public void onClick_Help_Instagram()
		{
			Application.OpenURL(StringCollection.get("setting.url.instagram", 0));
		}

		public void onClick_Help_Logout()
		{
			UIPopup.spawnYesNo(StringCollection.get("setting.page6.confirm_logout", 0), () =>
			{

				ClientMain.instance.getViewModel().SignIn.EMail = "";
				ClientMain.instance.getViewModel().SignIn.saveCache();

				close();
				UIMainTab.getInstance().close();
				UILoading.getInstance().open();

				ClientMain.instance.getFSM().changeState(ClientStateType.select_server);
			});
		}

		public void onClick_Help_Withdraw()
		{
			string title = StringCollection.get("setting.page6.confirm_withdraw.title", 0);
			string message = StringCollection.get("setting.page6.confirm_withdraw.desc", 0);
			UIPopup.spawnYesNo(title, message, () =>
			{

				MapPacket req = Network.createReq(CSMessageID.Account.WithdrawReq);
				req.put("firebase_id", ViewModel.SignIn.EMail);

				Network.call(req, ack =>
				{

					ClientMain.instance.getViewModel().SignIn.EMail = "";
					ClientMain.instance.getViewModel().SignIn.saveCache();

					close();
					UIMainTab.getInstance().close();
					UILoading.getInstance().open();

					ClientMain.instance.getFSM().changeState(ClientStateType.select_server);

				});
			});
		}

		public void onClick_Help_Contact()
		{
			string mailto = StringCollection.get("setting.page6.contact_email", 0);
			string subject = StringCollection.get("setting.help_email.subject", 0);
			string body = StringCollection.getFormat("setting.help_email.body", 0, SystemInfo.deviceModel, SystemInfo.operatingSystem, Application.version);

			subject = escapeURL(subject);
			body = escapeURL(body);

			Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body);
		}

		private string escapeURL(string data)
		{
			return UnityWebRequest.EscapeURL(data).Replace("+", "%20");
		}

		#endregion
	}
}