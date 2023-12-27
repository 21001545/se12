
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
	public class UITripStatus : UISingletonPanel<UITripStatus>
	{
		[SerializeField]
		private RectTransform rect_mainContainer;

		public GameObject baseClickObject;

		public FadePanelTransition transStatus;

		public Image tripTypeIcon;
		public Sprite[] tripTypeSprites;

		public Transform typeItemRoot;

		public Button tripPlay;

		public Sprite[] tripStatusIcons;

		[Header("Trip Value..")]
        [SerializeField]
        private TMP_Text txt_time_value;
        [SerializeField]
        private TMP_Text txt_step_value;
        [SerializeField]
        private TMP_Text txt_distance_value;
        [SerializeField]
        private TMP_Text txt_speed_value;
		[SerializeField]
		private TMP_Text txt_altitude_value;
		[SerializeField]
		private TMP_Text txt_calories_value;

		[SerializeField]
		private TMP_Text txt_distance_label;
		[SerializeField]
		private TMP_Text txt_speed_label;
		[SerializeField]
		private TMP_Text txt_altitude_label;

		[SerializeField]
		private RectTransform rect_hideArrow;
		[SerializeField]
        private Button btn_pause;
        [SerializeField]
        private RectTransform rect_playButton;
        [SerializeField]
        private RectTransform rect_stopButton;
		[SerializeField]
		private CanvasGroup can_playButton;
		[SerializeField]
		private CanvasGroup can_stopButton;
		[SerializeField]
        private GameObject info_pannel;
		[SerializeField]
		private SwipeDownPanel _GPSStopPanel;
		[SerializeField]
		private TMP_Text txt_GPSStop;

		[SerializeField]
		private ParticleSystem _fireworkParticles;

		private bool _showMainTab = false;
		private Vector2SmoothDamper _containerDamper;

        #region Scroll...
        [SerializeField]
		private DragListener info_scroll;
		private int current_info_scroll_index = 0;
		private Vector2 info_scroll_content_size;
        private Vector2 info_scroll_drag_start_position;
        private Vector2 info_scroll_drag_origin_position;

        public Image[] info_scroll_step_dots;

        Coroutine info_scroll_coroutine = null;
		#endregion

		private List<UITripStatus_TypeItem> _typeItemList;
		//private Animator _animator;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private IntervalTimer _timer;


		private int Setting_Distance_Unit => ViewModel.Profile.getSettingWithDefault(ClientAccountSetting.ConfigID.distance_unit, UnitDefine.DistanceType.km);

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			transStatus.init(null);
			_timer = IntervalTimer.create(1.0f, true, false);
			//_animator = GetComponent<Animator>();
		}

		public override void update()
		{
			base.update();

			transStatus.update();

			if( gameObject.activeSelf && _timer.update())
			{
				updateTime();
			}

			if(_containerDamper != null && _containerDamper.update())
            {
				rect_mainContainer.anchoredPosition = _containerDamper.getCurrent();
            }
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);
			float duration = UIMainTab.getInstance().getTransition().getDuration();
			_containerDamper = Vector2SmoothDamper.create(Vector2.zero, duration);

			if (ViewModel.Trip.Data.status == ClientTripConfig.StatusType.paused)
            {
				togglePause(false);
			}
			else
            {
				togglePause(true);
            }

			transStatus.startOpen();


			int tripType = UIMap.getInstance().getCurrentTripType();
			tripTypeIcon.sprite = tripTypeSprites[tripType];

            // 이벤트 바인딩..
            info_scroll_content_size = info_scroll.GetComponent<RectTransform>().rect.size;
			
            info_scroll.OnDragEvent.RemoveAllListeners();
			info_scroll.OnDragStartEvent.RemoveAllListeners();
			info_scroll.OnDragEndEvent.RemoveAllListeners();

			info_scroll.OnDragEvent.AddListener((UnityEngine.EventSystems.PointerEventData eventData) =>
            {
                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localCursor))
                    return;
                var pointerDelta = localCursor - info_scroll_drag_start_position;
                pointerDelta.y = 0.0f;
                Vector2 position = info_scroll_drag_origin_position + pointerDelta;
				info_scroll.rectTransform.anchoredPosition = position;

            });

			info_scroll.OnDragStartEvent.AddListener((UnityEngine.EventSystems.PointerEventData eventData) =>
            {
                // Animation 중지.
                info_scroll_drag_origin_position = info_scroll.rectTransform.anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out info_scroll_drag_start_position);
                if (info_scroll_coroutine != null)
                {
                    StopCoroutine(info_scroll_coroutine);
					info_scroll_coroutine = null;
                }
            });

			info_scroll.OnDragEndEvent.AddListener((UnityEngine.EventSystems.PointerEventData eventData) =>
            {
                // 현재 위치에 따라, 어디로 scroll 할래?

                float currentPosition = info_scroll.rectTransform.anchoredPosition.x;
                Debug.Log(currentPosition);
				Debug.Log(info_scroll_content_size);

				if (current_info_scroll_index == 0)
                {
                    if (currentPosition <= -info_scroll_content_size.x * 0.15f)
                    {
                        current_info_scroll_index++;
                    }
                }
                else if (current_info_scroll_index == 1)
                {
                    if (currentPosition >= -info_scroll_content_size.x * 0.85f)
                    {
                        current_info_scroll_index--;
                    }
                }

                for (int i = 0; i < info_scroll_step_dots.Length; ++i)
                {
					info_scroll_step_dots[i].color = i == current_info_scroll_index ? ColorChart.gray_600 : ColorChart.gray_300;
					info_scroll_step_dots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(i == current_info_scroll_index ? 10f : 5f, 5f);
				}
                float scrollToPosition = -current_info_scroll_index * info_scroll_content_size.x;
				info_scroll_coroutine = StartCoroutine(infoDragScroll(scrollToPosition));
            });
        }

        IEnumerator infoDragScroll(float to)
        {
			var cachedWaitForEndOfFrame = new WaitForEndOfFrame();
			while (Math.Abs(info_scroll.rectTransform.anchoredPosition.x - to) > 1.0f)
            {
                float diff = to - info_scroll.rectTransform.anchoredPosition.x;
				info_scroll.rectTransform.anchoredPosition = new Vector2(info_scroll.rectTransform.anchoredPosition.x + diff * Time.deltaTime * 5, info_scroll.rectTransform.anchoredPosition.y);
				yield return cachedWaitForEndOfFrame;
			}

			info_scroll.rectTransform.anchoredPosition = new Vector2(to, info_scroll.rectTransform.anchoredPosition.y);
			info_scroll_coroutine = null;
        }

		public void onClickBase()
		{
			UIMainTab.getInstance().open();
		}

		// 테스트용 임시 함수
		public void playParticles()
		{
			_fireworkParticles.Play();
		}

		public void onSelectTripType(int trip_type)
		{
			UIMap.getInstance().reqChangeTripType(trip_type);
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			TripViewModel trip_vm = ViewModel.Trip;

			_bindingManager.makeBinding(trip_vm, nameof(trip_vm.Data), onUpdateTripConfig);
		}

		private void onUpdateTripConfig(object value)
		{
			ClientTripConfig config = (ClientTripConfig)value;
			int unit = ClientMain.instance.getViewModel().Profile.Setting_DistanceUnit;

            tripTypeIcon.sprite = tripTypeSprites[config.trip_type];

			// 2022.08.17 소현 : 고도 단위도 바뀌나?? 일단 미터로 통일
			txt_altitude_label.text = StringCollection.getFormat("triproute.menu.altitude", 0, "m");


			if (unit == UnitDefine.DistanceType.km)
			{
				txt_distance_label.text = StringCollection.getFormat("triproute.menu.distance", 0, "km");
				txt_speed_label.text = StringCollection.getFormat("triproute.menu.velocity", 0, "km/h");
			}
			else
			{
				txt_distance_label.text = StringCollection.getFormat("triproute.menu.distance", 0, "mile");
				txt_speed_label.text = StringCollection.getFormat("triproute.menu.velocity", 0, "mile/h");
			}

			if ( config.status == ClientTripConfig.StatusType.none)
			{
				txt_step_value.text = "0";
				txt_distance_value.text = "0.00";
				txt_speed_value.text = "0.00";
				txt_calories_value.text = "0";
				txt_altitude_value.text = "0.00";

				baseClickObject.SetActive(true);

			}
			else if( config.status == ClientTripConfig.StatusType.trip)
			{
				txt_step_value.text = config.step_amount.ToString("N0");

				// 2022.08.17 소현 : 속도계산 이거 맞나??
				txt_speed_value.text = ViewModel.Trip.calcCurrentPace(unit).ToString("F2");
				txt_calories_value.text = (config.calorie_total / 1000.0).ToString("N0");
				txt_altitude_value.text = ViewModel.Location.CurrentAltitude.ToString("F2");		// 일단 미터로

				if (unit == UnitDefine.DistanceType.km)
				{
					txt_distance_value.text = config.distance_total.ToString("F2");
				}
				else
				{
					txt_distance_value.text = UnitDefine.km_2_mil(config.distance_total).ToString("F2");
				}

				baseClickObject.SetActive(false);
			}
			else if( config.status == ClientTripConfig.StatusType.paused) 
			{
				// 2022.06.22 이강희 멈추는 순간에도 이전값과 비교해서 값이 증가할 수 있기 때문에 UI를 갱신해줘야 함
				txt_step_value.text = config.step_amount.ToString("N0");

				// 2022.08.17 소현 : 속도계산 이거 맞나??
				txt_speed_value.text = ViewModel.Trip.calcCurrentPace(unit).ToString("F2");
				txt_calories_value.text = (config.calorie_total / 1000.0).ToString("N0"); // TODO Kcal로 표시?? (서버에서는 cal단위로 보냄)
				txt_altitude_value.text = ViewModel.Location.CurrentAltitude.ToString("F2");

				if (unit == UnitDefine.DistanceType.km)
				{
					txt_distance_value.text = config.distance_total.ToString("F2");
				}
				else
				{
					txt_distance_value.text = UnitDefine.km_2_mil(config.distance_total).ToString("F2");
				}
			}

			//Debug.Log($"update trip status: status[{config.status}] step_amount[{config.step_amount}] distance_total[{config.distance_total}] calorie_total[{config.calorie_total}]");

			updateTime();
		}

		private void updateTime()
        {
            ClientTripConfig config = ViewModel.Trip.Data;
            if ( config == null)
			{
				return;
			}

			if( config.status == ClientTripConfig.StatusType.none)
			{
				txt_time_value.text = "00:00:00";
			}
			else
			{

                TimeSpan diff_time = ViewModel.Trip.calcTripPeriodTime();
                txt_time_value.text = string.Format("{0}:{1}:{2}", diff_time.Hours.ToString("D2"), diff_time.Minutes.ToString("D2"), diff_time.Seconds.ToString("D2"));
			}
		}

		public override void onTransitionEvent(int type)
        {
			if( type == TransitionEventType.start_open)
            {
				//
				if( ViewModel.Trip.Data.status == ClientTripConfig.StatusType.paused)
                {
					//_animator.SetTrigger("pause");
					baseClickObject.SetActive(false);
				}
				else if( ViewModel.Trip.Data.status == ClientTripConfig.StatusType.trip)
                {
					//_animator.SetTrigger("play");
					baseClickObject.SetActive(false);
				}
			}
        }

		public void onClickPlay()
        {
			togglePause(true);
            //GetComponent<Animator>()?.SetTrigger("play");
            if (ViewModel.Trip.Data.status == ClientTripConfig.StatusType.paused)
            {
                UIMap.getInstance().resumeTrip();
			}
        }

		private void togglePause(bool activatePause)
        {
			float duration = 0.3f;

			var trans = Color.white;
			trans.a = 0f;

			if (activatePause)
			{
				btn_pause.image.color = trans;
				btn_pause.gameObject.SetActive(true);

				DOTween.To(() => rect_stopButton.anchoredPosition, x => rect_stopButton.anchoredPosition = x, new Vector2(0f, 40f), duration);
				DOTween.To(() => rect_playButton.anchoredPosition, x => rect_playButton.anchoredPosition = x, new Vector2(0f, 40f), duration);
				DOTween.To(() => btn_pause.image.color, x => btn_pause.image.color = x, Color.white, duration);

				Invoke("activatePause", duration);
			}
			else
			{
				can_playButton.alpha = 0f;
				can_stopButton.alpha = 0f;
				rect_stopButton.gameObject.SetActive(true);
				rect_playButton.gameObject.SetActive(true);
				btn_pause.gameObject.SetActive(false);

				DOTween.To(() => rect_stopButton.anchoredPosition, x => rect_stopButton.anchoredPosition = x, new Vector2(38.5f, 40f), duration);
				DOTween.To(() => rect_playButton.anchoredPosition, x => rect_playButton.anchoredPosition = x, new Vector2(-38.5f, 40f), duration);
				DOTween.To(() => can_playButton.alpha, x => can_playButton.alpha = x, 1f, duration);
				DOTween.To(() => can_stopButton.alpha, x => can_stopButton.alpha = x, 1f, duration);
			}

			txt_GPSStop.text = StringCollection.get("triproute.pausingMsg", 0);
			_GPSStopPanel.swipePanel(!activatePause);
		}

		private void activatePause()
        {
			rect_stopButton.gameObject.SetActive(false);
			rect_playButton.gameObject.SetActive(false);
		}

		public void showGPSWait()
		{
			txt_GPSStop.text = StringCollection.get("triproute.pausingMsg", 1);
			_GPSStopPanel.swipePanel(true);
		}

		public void hideGPSWait()
		{
			_GPSStopPanel.swipePanel(false);
		}

        public void onClickPause()
        {
			if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.trip)
			{
				return;
			}
			UIMap.getInstance().pauseTrip();
			togglePause(false);
		}

        public void onClickStop()
        {
			UIPopup.spawnDeleteCancel(StringCollection.get("triproute.stopTrip.title", 0), StringCollection.get("triproute.stopTrip.desc", 0), onClickStopOK, null, StringCollection.get("triproute.stopTrip.button", 0));
        }

		public void onClickStopOK()
		{
            close(TransitionEventType.openImmediately);
            toggleMainTab();
            UIMap.getInstance().endTrip();
        }

        public void onClickInfoHide()
        {
			_showMainTab = !_showMainTab;
			toggleMainTab();
            //info_pannel.SetActive(false);
        }

		public void resetInfo()
        {
			_containerDamper.reset(rect_mainContainer.anchoredPosition);
			_showMainTab = false;
			toggleMainTab();
        }

		public void toggleMainTab()
		{
			if (_containerDamper.update())
				return;

			Quaternion upRot = Quaternion.AngleAxis(0f, Vector3.forward);
			Quaternion downRot = Quaternion.AngleAxis(180f, Vector3.forward);

            if (_showMainTab)
			{
				_containerDamper.setTarget(new Vector2(rect_mainContainer.anchoredPosition.x, 52f));
				//DOTween.To(() => rect_mainContainer.anchoredPosition, x => rect_mainContainer.anchoredPosition = x, new Vector2(rect_mainContainer.anchoredPosition.x, posOffset), duration).SetEase(Ease.OutCubic);
				rect_hideArrow.rotation = downRot;
                UIMainTab.getInstance().open();
			}
			else
			{
				_containerDamper.setTarget(Vector2.zero);
				//DOTween.To(() => rect_mainContainer.anchoredPosition, x => rect_mainContainer.anchoredPosition = x, new Vector2(rect_mainContainer.anchoredPosition.x, 0f), duration);
				rect_hideArrow.rotation = upRot;
                UIMainTab.getInstance().close();
			}
		}

		public void onClickCamera()
        {
			if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.none)
			{
				UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.trip_photo);
			}
			else
			{
				ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.make, null, ViewModel.Location.createCurrentPlaceData());
				UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.moment);
			}

			// 2022.06.22 소현 : 일단은 모먼트랑 똑같이 구현, 나중에 기획 정해지면 바꾸기~~
            UIPhotoTake.getInstance().open();

            ///*UIPanelNavigationStackItem stack = */ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
            ////stack.addPrev(UIMainTab.getInstance());
        }

		public void testOnClickThumbnail()
        {
			UIMakeTripPhotoPopup.getInstance().open();
		}
    }
}
