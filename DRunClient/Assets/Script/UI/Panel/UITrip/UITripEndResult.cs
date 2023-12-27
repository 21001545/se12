using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UITripEndResult : UISingletonPanel<UITripEndResult>
	{
		[Header("=== pages ===")]
		[SerializeField]
		private GameObject go_coinPanel;
		[SerializeField]
		private GameObject go_tripPanel;

		[Header("-- title --")]
		[SerializeField]
		private TMP_Text txt_dateTime;
		[SerializeField]
		private TMP_InputField input_tripTitle;
		[SerializeField]
		private Image img_tripIcon;
		[SerializeField]
		private TMP_Text txt_tripType;
		[SerializeField]
		public Sprite[] tripIcons;

		[Header("-- time --")]
		[SerializeField]
		private TMP_Text txt_hourValue;
		[SerializeField]
		private TMP_Text txt_minValue;
		[SerializeField]
		private TMP_Text txt_secValue;

		[Header("-- other data --")]
		[SerializeField]
		private TMP_Text txt_stepValue;
		[SerializeField]
		private TMP_Text txt_disValue;
		[SerializeField]
		private TMP_Text txt_disLabel;
		[SerializeField]
		private TMP_Text txt_speedValue;
		[SerializeField]
		private TMP_Text txt_speedLabel;
		[SerializeField]
		private TMP_Text txt_altValue;
		[SerializeField]
		private TMP_Text txt_altLabel;
		[SerializeField]
		private TMP_Text txt_calValue;

		public GameObject go_loading;

		// 계속 써야 하는 데이터들
		public Double data_distance;
		public Double data_speed;
		public DateTime data_tripDate;
		public int data_totalSteps;
		public Double data_altitude;
		public Double data_calories;
		public string data_location;


        public UnityMapBox mapBox;
		public RectTransform rect_mapBox;
		private bool _quitNow = false;

		[SerializeField]
		private UIToggle toggle_save_camera;
		[SerializeField]
		private UITripEndResult_scrollDelegate _scrollerDelegate;

        private ClientTripLog _log;
		private bool _initMapBox;
		private bool _isValidPathBound;
		private MBLongLatCoordinate _pathCenter;
		private MBLongLatCoordinate _pathMin;
		private MBLongLatCoordinate _pathMax;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public ClientTripLog getTripLog()
		{
			return _log;
		}

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			_initMapBox = true;
			go_loading.SetActive(false);
			mapBox.init(false, Camera.main, ClientMain.instance.getMBStyleCache(), "ckvta2ylt1mq614s8vrp37ktq");
			mapBox.getInputFSM().OnClickActor = onClickActor;
			rect_mapBox.anchoredPosition = new Vector2(rect_mapBox.anchoredPosition.x, rect_mapBox.anchoredPosition.y + 552f);
		}

		public void setup(ClientTripLog log)
		{
			_quitNow = false;
			go_coinPanel.SetActive(true);
			go_tripPanel.SetActive(false);

			input_tripTitle.shouldHideMobileInput = true;

			// 코인 페이지
			_scrollerDelegate.setCoins(log.coin_steps, log.coin_bonus);

			// 활동 페이지
			setTripResult(log);
		}

		private void setTripResult(ClientTripLog log)
        {
			RefStringCollection stringCollection = GlobalRefDataContainer.getStringCollection();

			data_tripDate = log.end_time.ToLocalTime();
			string year = data_tripDate.Year.ToString("N0").Substring(2, 2);
			string time = data_tripDate.ToString("hh:mm tt");
			txt_dateTime.text = $"{year}.{data_tripDate.Month.ToString("D2")}.{data_tripDate.Day.ToString("D2")} - {time}";
			input_tripTitle.text = log.name;
			int tripType = UIMap.getInstance().getCurrentTripType();
			img_tripIcon.sprite = tripIcons[tripType];
			txt_tripType.text = stringCollection.get("triproute.category", tripType);   // 클라에서 가져오기,, 맞겟지,,??

			// 2022.6.20 이강희
			//TimeSpan diff_time = log.end_time - log.begin_time;
			TimeSpan diff_time = TimeSpan.FromMilliseconds(log.period_time);
			txt_hourValue.text = diff_time.Hours.ToString("D2");
			txt_minValue.text = diff_time.Minutes.ToString("D2");
			txt_secValue.text = diff_time.Seconds.ToString("D2");

			// 2022.7.19 단위 옵션 반영
			int unit = ClientMain.instance.getViewModel().Profile.Setting_DistanceUnit;
			if (unit == UnitDefine.DistanceType.km)
			{
				data_distance = log.distance_total;
				txt_disValue.text = data_distance.ToString("F2");
				txt_disLabel.text = stringCollection.getFormat("triproute.menu.distance", 0, "km");

				data_speed = (log.distance_total / diff_time.TotalHours);
				txt_speedValue.text = data_speed.ToString("F2");
				txt_speedLabel.text = stringCollection.getFormat("triproute.menu.velocity", 0, "km/h");
			}
			else
			{
				data_distance = UnitDefine.km_2_mil(log.distance_total);
				txt_disValue.text = data_distance.ToString("F2");
				txt_disLabel.text = stringCollection.getFormat("triproute.menu.distance", 0, "mile");

				data_speed = (data_distance / diff_time.TotalHours);
				txt_speedValue.text = data_speed.ToString("F2");
				txt_speedLabel.text = stringCollection.getFormat("triproute.menu.velocity", 0, "mile/h");
			}

			// 기타 데이터 값
			data_totalSteps = log.step_total;
			txt_stepValue.text = data_totalSteps.ToString("N0");

			data_altitude = ClientMain.instance.getViewModel().Location.CurrentAltitude;	// 로그에 없어서 우선은 현재고도
			txt_altValue.text = data_altitude.ToString("F2");

			data_calories = log.calorie_total;
			txt_calValue.text = data_calories.ToString("N0");

			// 2022.08.17 소현 : 고도 단위도 바뀌나?? 일단 미터로 통일
			txt_altLabel.text = stringCollection.getFormat("triproute.menu.altitude", 0, "m");

			data_location = ClientMain.instance.getViewModel().Location.CurrentAddress;		// 로그에 없어서 일단 현재 위치

			_log = log;
			_isValidPathBound = _log.calcPathBound(out _pathCenter, out _pathMin, out _pathMax);

			//go_loading.SetActive(true);

			//UMBOfflineRenderer.getInstance().buildForTripPath(_log.path_data, result => {

			//	go_loading.SetActive(false);

			//	if( image_map.texture != null)
			//	{
			//		UnityEngine.Object.DestroyImmediate(image_map.texture);
			//	}
			//	image_map.texture = result;
			//	image_map.gameObject.SetActive(true);

			//	Debug.Log("offline render end");
			//});
		}

		public override void update()
		{
			base.update();
			if( gameObject.activeSelf == true && _initMapBox == false)
			{
				mapBox.update();
			}
		}

		public override void onTransitionEvent(int type)
		{
			base.onTransitionEvent(type);
			if( type == TransitionEventType.start_close)
			{
				mapBox.getLabelManager().setEnable(false);
			}
			else if( type == TransitionEventType.end_open)
			{
				mapBox.getLabelManager().setEnable(true);
				_scrollerDelegate.setup(_log);

			}
			else if(type == TransitionEventType.start_open)
			{
				if (_initMapBox)
				{
					_initMapBox = false;
				}

				mapBox.gameObject.SetActive(true);
				setupMapBoxPosition();
				createTripPaths();
				createTripPhotos();
				createTripFlags();
				zoomPathBound();
			}
			else if(type == TransitionEventType.end_close)
            {
				if(_quitNow)
				{
                    // 여기 스트링테이블~~
                    UIToastNotification.spawn("탐험하기 결과는 [통계] 에서\n더 자세하게 볼 수 있어요.");
					_quitNow = false;
                }
			}
		}

		private void setupMapBoxPosition()
		{
			if( _isValidPathBound)
			{
				mapBox.getControl().moveTo(_pathCenter, 16);
			}
		}

		private void createTripPaths()
		{
			mapBox.removeAllActors();

			foreach(ClientTripPathData path in _log.path_data)
			{
				UMBActor path_source = mapBox.styleData.actorSourceContainer.getSource("path").actor_source;
				UMBActor_TripPath actor_trippath = (UMBActor_TripPath)mapBox.spawnActor(path_source, MBLongLatCoordinate.zero);

				actor_trippath.updatePath(path);
			}
		}

		private void createTripPhotos()
		{
			mapBox.getTripPhotoManager().clear();
			if( _log._photoList != null)
			{
				mapBox.getTripPhotoManager().add(_log._photoList);
			}
			mapBox.getTripPhotoManager().onUpdateZoom();
		}

		private void createTripFlags()
		{
			UMBActor flag_source = mapBox.styleData.actorSourceContainer.getSource("flag").actor_source;

			MBLongLatCoordinate startLocation = _log.getStartLocation();
			MBLongLatCoordinate endLocation = _log.getEndLocation();

			if( startLocation.isZero() || endLocation.isZero())
			{
				return;
			}

			UMBActor_TripFlag start_flag = (UMBActor_TripFlag)mapBox.spawnActor(flag_source, startLocation);
			start_flag.setup(0);

			UMBActor_TripFlag end_flag = (UMBActor_TripFlag)mapBox.spawnActor(flag_source, endLocation);
			end_flag.setup(1);
		}

		private void zoomPathBound()
		{
			if( _isValidPathBound == false)
			{
				return;
			}

			UMBControl control = mapBox.getControl();

			Vector2 extent = control.calcScreenExtent(_pathMin, _pathMax);
			Vector2 mbExtent = mapBox.calcScreenExtent();

			float bestZoom = control.calcFitZoom(extent, mbExtent, new Vector2(8, 16), 0.75f);
			control.zoom(bestZoom, true);
		}

		public void onClickShare()
		{
			ViewModel.MakeMoment.reset( MakeMomentViewModel.EditMode.make,null, ViewModel.Location.createCurrentPlaceData());
			ViewModel.MakeMoment.TripLog = _log;

			//UMBOfflineRenderer.getInstance().buildForTripPath(_log.path_data, result => {
			//	ViewModel.MakeMoment.TripLogImage = result;
			//});

			//UIBackNavigation.getInstance().setup(this, UIPhotoTake.getInstance());
			//UIBackNavigation.getInstance().open();
			UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.moment);
			UIPhotoTake.getInstance().open();

			ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
		}

/*		// 하단 See Result 버튼 클릭시 호출
		public void onClickSeeResult()
        {
			Animator animator = GetComponent<Animator>();
			if (animator != null )
            {
				animator.SetBool("detail", true);
            }
        }*/

		private void onClickActor(UMBActor pick_actor)
		{
			if( pick_actor is UMBActor_Photo)
			{
				UMBActor_Photo photoActor = pick_actor as UMBActor_Photo;
				List<ClientTripPhoto> photoList = photoActor.getPhotoDataList();

				UIMakeTripPhotoPopup.getInstance().setup(mapBox, photoList);
				UIMakeTripPhotoPopup.getInstance().open();
			}
		}

		public void onClickSeeResult()
        {
			go_coinPanel.SetActive(false);
			go_tripPanel.SetActive(true);
        }


		public void onClickEditTitle()
        {
			input_tripTitle.ActivateInputField();
        }

		public void onClickQuit()
        {
			_quitNow = true;
			mapBox.gameObject.SetActive(false);

			if (input_tripTitle.text != _log.name)
			{
				// 이름을 변경한 경우 수정
				changeTripName(input_tripTitle.text);
			}

			UIMainTab.getInstance().open();
			UIMap.getInstance().open(null, TransitionEventType.openImmediately);
			close();
            ClientMain.instance.getPanelNavigationStack().clear();
        }

        public void onClickCertify()
        {
			if(input_tripTitle.text != _log.name)
            {
				// 이름을 변경한 경우 수정
				changeTripName(input_tripTitle.text);
            }

			UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.trip_certify);
			UIPhotoTake.getInstance().open();
            ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
        }

		// 서버에 탐험기록 이름 바꾸는 코드 (inputField값이 변경되면 호출, 너무 자주하면 않되고 편집이 완료되는 시점에 호출)
		// 2022.08.26 소현 : 일단은 종료 혹은 인증하기 버튼을 누를 때만 호출해 볼게요!!
		private void changeTripName(string name)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.ChangeTripNameReq);
			req.put("id", _log.trip_id);
			req.put("name", name);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					_log.name = name;
				}
			});
		}
	}
}
