using Assets.Script.MapBox.Unity.Actor;
using Festa.Client.Logic;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMap : UISingletonPanel<UIMap>
	{
		[Header("-- Map Box --")]
		public UnityMapBox mapbox;
		public Image currentLocationModeImage;
		public Image projectionModeImage;

		private bool map_started;
		private ClientViewModel _viewModel;
		private UMBActor_DesertFox _myActor;
		private UMBActor _point_notFiltered;
		private UMBActor _point_filtered;
		private List<UMBActor> _testActors;

		[Header("-- UI Overlay --")]
		[SerializeField]
		private SwipeDownPanel _normalStatePanel;
		[SerializeField]
		private SwipeDownPanel _tripStatePanel;
		[SerializeField]
		private TMP_Text txt_weatherIcon;
		[SerializeField]
		private TMP_Text txt_weather;
		[SerializeField]
		private GameObject go_loading;

		public TMP_Text txt_address;
		public Image compassImage;

		[SerializeField]
		private Animator _animator;
		[SerializeField]
		private Image[] _tripTypeIcon;
		[SerializeField]
		private TMP_Text[] _tripTypeLabel;
		[SerializeField]
		private Image _loadingTripType;
        [SerializeField]
        private UIMap_tripTypeScroller _scroller;
		public UICheers cheerPanel;
		public UIToggleButton toggleDeco;

		private List<UMBActor_TripPath> _tripPathList;
		private List<UMBActor_TripFlag> _tripFlagList;
		private UMBViewModel mbViewModel => mapbox.getViewModel();

		private FloatSmoothDamper _addressAlphaDamper;
		private List<UMBActor_TripPath> _testTripPath;
		private IntervalTimer _timerQueryTripCheerable;
		private IntervalTimer _timerQueryLatestTripCheering;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		//private ClientLocationManager LocationManager => ClientMain.instance.getLocation();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			mapbox.init(false, Camera.main, ClientMain.instance.getMBStyleCache(), "ckvta2ylt1mq614s8vrp37ktq");
			_tripPathList = new List<UMBActor_TripPath>();
			_tripFlagList = new List<UMBActor_TripFlag>();
			_addressAlphaDamper = FloatSmoothDamper.create(1.0f, 0.1f);
			_testActors = new List<UMBActor>();
			_timerQueryTripCheerable = IntervalTimer.create(10.0f, false, true);
			_timerQueryLatestTripCheering = IntervalTimer.create(1.0f, false, true);

			mapbox.getInputFSM().OnClickActor = onClickActor;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			_viewModel = ClientMain.instance.getViewModel();

			resetBindings();

			base.open(param, transitionType, closeType);

			_tripStatePanel.gameObject.SetActive(true);
			_normalStatePanel.gameObject.SetActive(true);

			_tripStatePanel.showHideImmediately(false);
			_normalStatePanel.swipePanel(true);
			_scroller.init();

			toggleDeco.IsOn = mapbox.getViewModel().ShowMapDeco;
		}

		public override void close(int transitionType = 0)
		{
			base.close(transitionType);
		}

		public override void update()
		{
			base.update();

			if (mapbox.gameObject.activeSelf)
			{
				mapbox.update();
				updateMyActorDirection();
			}

			if( gameObject.activeSelf)
			{
				queryTripCheerable();
				queryTripLatestCheering();
			}
		}

		public override void updateFixed()
		{
			base.updateFixed();

			if (mapbox.gameObject.activeSelf)
			{
				mapbox.updateFixed();
			}
		}

		public override void onTransitionEvent(int type)
		{
			base.onTransitionEvent(type);
			if (type == TransitionEventType.start_close)
			{
				//mapbox.gameObject.SetActive(false);
				mapbox.getLabelManager().setEnable(false);
			}
			else if (type == TransitionEventType.end_open)
			{
				//mapbox.gameObject.SetActive(true);
				mapbox.getLabelManager().setEnable(true);

//#if UNITY_EDITOR
//				createTestPath();
//#endif
			}
			else if (type == TransitionEventType.start_open)
			{
				MBLongLatCoordinate pos = ClientMain.instance.getViewModel().Location.CurrentLocation;
				bool isInit = false;

				if (_myActor == null)
				{
					_myActor = (UMBActor_DesertFox)mapbox.spawnActor("avatar.desertfox", pos);

					mapbox.spawnActor("party.camp", new MBLongLatCoordinate(127.05065589703139, 37.50850136014864));
					mapbox.spawnActor("kpt_room", new MBLongLatCoordinate(127.054899, 37.507457));
					//mapbox.spawnActor("party.cafe", new MBLongLatCoordinate(127.05975961624351, 37.5084772879995));
					isInit = true;
				}

				if (_point_notFiltered == null)
				{
					_point_notFiltered = mapbox.spawnActor("point", pos);
					_point_notFiltered.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.5f);
				}
				if (_point_filtered == null)
				{
					_point_filtered = mapbox.spawnActor("point", pos);
					_point_filtered.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
				}

				// 2021.11.29 이강희
				// Tab이동시 zoom 및 scroll 초기화
				int zoom = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Map.Zoom.init, 16);
				if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.none)
				{
					mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow_fitzoom);
				}

				setupLastTripPath();
				setupLastPhoto();

				if (isInit)
				{
					mapbox.getControl().moveTo(pos, zoom);
				}
				else
				{
					mapbox.getControl().scrollTo(pos);
					mapbox.getControl().zoom(zoom, false);
				}

				setZoomByTripPathExtent();
			}
		}

		public int getCurrentTripType()
        {
			return _scroller.getCurrentTripType();
        }

		private void initZoomScrollForStartTrip()
		{
			MBLongLatCoordinate pos = ClientMain.instance.getViewModel().Location.CurrentLocation;
			int zoom = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Map.Zoom.trip_start, 17);
			mapbox.getControl().scrollTo(pos);
			mapbox.getControl().zoom(zoom, false);

			mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			LocationViewModel vm = ClientMain.instance.getViewModel().Location;
			TripViewModel trip_vm = ClientMain.instance.getViewModel().Trip;

			UMBViewModel umb_vm = mapbox.getViewModel();

			// 2022.01.24
			umb_vm.MapReveal = ClientMain.instance.getViewModel().MapReveal;

			_bindingManager.makeBinding(vm, nameof(vm.CurrentLocation), onUpdateCurrentLocation);

/*			_bindingManager.makeBinding(this, "CurrentTripType", (value) =>
			{
				var selectColor = new Color(0, 0, 0);
				var unselectColor = new Color(198 / 255.0f, 198 / 255.0f, 206 / 255.0f);

				for (int i = 0; i < _tripTypeIcon.Length; ++i)
				{
					_tripTypeIcon[i].color = (i == CurrentTripType ? selectColor : unselectColor);
					_tripTypeLabel[i].color = (i == CurrentTripType ? selectColor : unselectColor);
				}
			});*/
			//_bindingManager.makeBinding(trip_vm, nameof(trip_vm.Data), onUpdateTripConfig);
			//_bindingManager.makeBinding(trip_vm.PathList, onUpdateTripPath);
			_bindingManager.makeBinding(trip_vm, nameof(trip_vm.CurrentTripPathData), onUpdateTripPath);
			_bindingManager.makeBinding(vm, nameof(vm.CurrentAddress), txt_address, nameof(txt_address.text), null);
			_bindingManager.makeBinding(umb_vm, nameof(umb_vm.CurrentLocationMode), onUpdateCurrentLocationMode);
			_bindingManager.makeBinding(umb_vm, nameof(umb_vm.ProjectionMode), onUpdateProjectionMode);
			_bindingManager.makeBinding(umb_vm, nameof(umb_vm.ZAngle), onUpdateZAngle);
			_bindingManager.makeBinding(umb_vm, nameof(umb_vm.Zoom), onUpdateZoom);

			// 날씨 정보 갱신.
			WeatherViewModel weather_vm = ClientMain.instance.getViewModel().Weather;
			_bindingManager.makeBinding(weather_vm, nameof(weather_vm.Data), onUpdateWeather);

			// 응원받은 목록 갱신
			_bindingManager.makeBinding(trip_vm, nameof(trip_vm.CurrentUnreadCheeringList), onUpdateCurrentUnreadCheeringList);
		}

		private void onUpdateWeather(object value)
		{
			WeatherViewModel vm = ViewModel.Weather;
			if (vm.Data != null)
			{
				string weather_text = vm.Data.getDisplayName(ViewModel.Profile.Setting_TemperatureUnit);
				//weather_text += $"         {Mathf.FloorToInt((float)ViewModel.Location.CurrentAltitude)}m";

				txt_weatherIcon.text = vm.Data.getWeatherIcon();
				txt_weather.text = vm.Data.getTemperature();
			}
		}

		private void onUpdateCurrentLocation(object value)
		{
			MBLongLatCoordinate pos = (MBLongLatCoordinate)value;

			mapbox.updateCurrentLocation(pos);

			applyLocationModeToControl(pos);

			if (_myActor != null)
			{
				_myActor.changePosition(pos);
			}
			//if (_point_notFiltered != null)
			//{
			//	_point_notFiltered.changePosition(ClientMain.instance.getLocation().getDevice().getLastLocationNotFiltered());
			//}
			//if (_point_filtered != null)
			//{
			//	_point_filtered.changePosition(ClientMain.instance.getLocation().getDevice().getLastLocation());
			//}

			// 2022.08.10 이강희 고도값 표시 갱신을 위해 (임시코드)
			onUpdateWeather(null);
		}

		private void onUpdateCurrentLocationMode(object value)
		{
			if (mapbox.getViewModel().CurrentLocationMode == UMBDefine.CurrentLocationMode.none)
			{
				currentLocationModeImage.color = ColorChart.gray_500;
			}
			else if (mapbox.getViewModel().CurrentLocationMode == UMBDefine.CurrentLocationMode.follow)
			{
				currentLocationModeImage.color = ColorChart.primary_300;
			}
			else if( mapbox.getViewModel().CurrentLocationMode == UMBDefine.CurrentLocationMode.follow_fitzoom)
			{
				currentLocationModeImage.color = ColorChart.primary_300;
			}
		}

		private void onUpdateProjectionMode(object value)
		{
			if (mapbox.getViewModel().ProjectionMode == UMBDefine.ProjectionMode.two_d)
			{
				projectionModeImage.color = new Color( 0.8f, 0.8f, 0.8f, 1.0f);
			}
			else if (mapbox.getViewModel().ProjectionMode == UMBDefine.ProjectionMode.three_d)
			{
				projectionModeImage.color = Color.blue;
			}
		}

		public void onClickCurrentLocation()
		{
			int cur_mode = mapbox.getViewModel().CurrentLocationMode;
			if (cur_mode == UMBDefine.CurrentLocationMode.none)
			{
				mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);
			}
			else if(cur_mode == UMBDefine.CurrentLocationMode.follow)
			{
				if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.none)
				{
					mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow_fitzoom);
				}
				else
				{
					mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.none);
				}
			}
			else if(cur_mode == UMBDefine.CurrentLocationMode.follow_fitzoom)
			{
				mapbox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);
			}

			applyLocationModeToControl(ViewModel.Location.CurrentLocation);

			//#if UNITY_EDITOR
			//			testSearchByDistance();
			//#endif
		}

		public void onClickProjectionMode()
		{
			int cur_mode = mapbox.getViewModel().ProjectionMode;
			if (cur_mode == UMBDefine.ProjectionMode.two_d)
			{
				mapbox.getViewModel().ProjectionMode = UMBDefine.ProjectionMode.three_d;
			}
			else
			{
				mapbox.getViewModel().ProjectionMode = UMBDefine.ProjectionMode.two_d;
			}
		}

		//private int _styleIndex = 0;
		//private string[] _styleIDs =
		//{
		//	"ckvta2ylt1mq614s8vrp37ktq",
		//	"ckotstrsb4hf317o45vwur7i7",
		//	"cl06go948004715qoc2nfhwob",
		//	"cl14mro8h000o16k835qsj4rz"
		//};

		public void onClickStyle()
		{
			//_styleIndex++;
			//if (_styleIndex >= _styleIDs.Length)
			//{
			//	_styleIndex = 0;
			//}

			//mapbox.setStyle(_styleIDs[_styleIndex]);
		}

		public void onClickDeco()
		{
			toggleDeco.IsOn = !toggleDeco.IsOn;
			mapbox.getViewModel().ShowMapDeco = toggleDeco.IsOn;
		}

		public void onClickCheers()
        {
			cheerPanel = UICheers.spawn();
        }

		public void onClickHistory()
		{
		//	UITripHistory.getInstance().open();
		}

		public void onClickTrip()
		{
			UIMainTab.getInstance().close();

			_normalStatePanel.swipePanel(false);
			_tripStatePanel.swipePanel(true);
		}

		public void onClickStartTripCancel()
		{
			UIMainTab.getInstance().open();
			_normalStatePanel.swipePanel(true);
			_tripStatePanel.swipePanel(false);
		}


		public void onClickSettings()
		{
			UIMapSettings.getInstance().open();
		}

		private void onUpdateTripPath(object obj)
		{
			ClientTripPathData data = ViewModel.Trip.CurrentTripPathData;
			if (data != null)
			{
				UMBActor_TripPath actor_trippath = _tripPathList[_tripPathList.Count - 1];
				actor_trippath.updatePath(data);
			}
		}

		public void onClickLoadingSkip()
		{
			//_animator.SetTrigger("Skip");
			onLoadingFinish();
		}

		[SerializeField]
		private Animation _anim;

		public void onClickStartTrip()
		{
			_tripStatePanel.swipePanel(false);
			go_loading.SetActive(true);
			//_loadingTripType.sprite = _tripTypeIcon[_scroller.getCurrentTripType()].sprite;
			_animator.enabled = true;
			_animator.Play("NewLoading", -1);

			// gps를 미리 활성화 해보자
			ViewModel.Trip.TryStartTripTime = TimeUtil.unixTimestampUtcNow();
			//ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.trip);

			// 2022.08.12 소현 : 시안에 로딩이 없는데,, 일단 빼보자
			//onLoadingFinish();
			//_animator.SetTrigger("Loading");
		}

		// 3,2,1 로딩 끝나면 호출 되는 이벤트!
		public void onLoadingFinish()
		{
			go_loading.SetActive(false);
			_animator.enabled = false;
			UITripStatus.getInstance().open();
			UITripStatus.getInstance().resetInfo();
			startTrip();
		}

/*		public void onChangeTripType(int type)
		{
			CurrentTripType = type;
		}*/

		private void removeAllTripPaths()
		{
			mapbox.removeActors(_tripPathList);
			_tripPathList.Clear();

			mapbox.removeActors(_tripFlagList);
			_tripFlagList.Clear();
		}

		//private UMBActor_TripPath appendNewTripPath(MBLongLatCoordinate pos)
		//{
		//	UMBActor path_source = mapbox.styleData.actorSourceContainer.getSource("path").actor_source;
		//	UMBActor_TripPath actor_trippath = (UMBActor_TripPath)mapbox.spawnActor(path_source, pos);

		//	_tripPathList.Add(actor_trippath);

		//	return actor_trippath;
		//}

		private void createTripPathStartFlag()
		{
			MBLongLatCoordinate startLocation = ViewModel.Trip.CurrentTripPathDataList[0].getFirstLocation();

			UMBActor flag_source = mapbox.styleData.actorSourceContainer.getSource("flag").actor_source;
			UMBActor_TripFlag actor_flag = (UMBActor_TripFlag)mapbox.spawnActor(flag_source, startLocation);

			actor_flag.setup(0);

			_tripFlagList.Add(actor_flag);

			//
			//ViewModel.Trip.CurrentTripPathDataList
		}

		private UMBActor_TripPath appendNewTripPath(ClientTripPathData data)
		{
			UMBActor path_source = mapbox.styleData.actorSourceContainer.getSource("path").actor_source;
			UMBActor_TripPath actor_trippath = (UMBActor_TripPath)mapbox.spawnActor(path_source, MBLongLatCoordinate.zero);
			actor_trippath.updatePath(data);

			_tripPathList.Add(actor_trippath);
			return actor_trippath;
		}

		private void setupLastTripPath()
		{
			if (ViewModel.Trip.CurrentTripPathDataList.Count > 0)
			{
				removeAllTripPaths();
				foreach (ClientTripPathData data in ViewModel.Trip.CurrentTripPathDataList)
				{
					appendNewTripPath(data);
				}

				createTripPathStartFlag();
			}
		}

		private void setupLastPhoto()
		{
			if( ViewModel.Trip.CurrentTripPhotoList.Count > 0)
			{
				mapbox.getTripPhotoManager().clear();
				mapbox.getTripPhotoManager().add(ViewModel.Trip.CurrentTripPhotoList);
				mapbox.getTripPhotoManager().onUpdateZoom();
			}
		}

		public void startTrip()
		{
			TripStartProcessor step = TripStartProcessor.create(_scroller.getCurrentTripType());
			step.run(result => { 
				if( result.succeeded())
				{
					setupLastTripPath();
					setupLastPhoto();
					initZoomScrollForStartTrip();
				}
			});
		}

		public void pauseTrip()
		{
			if (_viewModel.Trip.Data.status != ClientTripConfig.StatusType.trip)
			{
				Debug.LogWarning("invalid trip status");
				return;
			}

			TripPauseProcessor step = TripPauseProcessor.create();
			step.run(result => { 
				if( result.succeeded())
				{
					appendNewTripPath(step.getNewPathData());
				}
			});
		}
		public void reqChangeTripType(int trip_type)
		{
			// 소현 : 이게 기획 제외된 그건가??
/*			CurrentTripType = trip_type;
			MapPacket req = Network.createReq(CSMessageID.Trip.ChangeTripTypeReq);
			req.put("type", CurrentTripType);

			Network.call(req, ack =>
			{
				if (ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			});*/
		}

		public void resumeTrip()
		{
			if (_viewModel.Trip.Data.status != ClientTripConfig.StatusType.paused)
			{
				Debug.LogWarning("invalid trip status");
				return;
			}

			//TripResumeProcessor step = TripResumeProcessor.create(_scroller.getCurrentTripType());
			//step.run(result => { 
			//	if( result.succeeded())
			//	{
			//		appendNewTripPath( step.getNewPathData());

			//		initZoomScrollForStartTrip();
			//	}
			//});
		}

		public void endTrip()
		{
			//TripEndProcessor step = TripEndProcessor.create(ViewModel.Trip.Data);
			//step.run(result => { 
			//	if( result.succeeded())
			//	{
			//		removeAllTripPaths();
			//		mapbox.getTripPhotoManager().clear();

			//		ClientTripLog log = step.getTripLog();

			//		UITripEndResult.getInstance().setup(log);
			//		UITripEndResult.getInstance().open();
			//		close();
			//	}
			//});
		}

		public void addTripPhoto(List<NativeGallery.NativePhotoContext> photo_list)
		{
			int trip_id = ViewModel.Trip.Data.next_trip_id;
			MBLongLatCoordinate location = ViewModel.Location.CurrentLocation;

			TripAddPhotoProcessor step = TripAddPhotoProcessor.create(trip_id, location, photo_list);
			step.run(result => { 
				if (result.succeeded())
				{
					mapbox.getTripPhotoManager().add(step.getTripPhoto());
					mapbox.getTripPhotoManager().onUpdateZoom();
				}
			});
		}

		public void onClickCamera()
		{

			//// 테스트 코드
			//MapPacket reqHistory = Network.createReq(CSMessageID.Trip.QueryTripListReq);
			//reqHistory.put("target", 9);
			//reqHistory.put("begin", 3);
			//reqHistory.put("count", 10);

			//Network.call(reqHistory, ackHistory =>
			//{
			//	if (ackHistory.getResult() == ResultCode.ok)
			//	{
			//		ClientTripLog log = ackHistory.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log)[0];

			//		UITripEndResult.getInstance().setup(log);
			//		UITripEndResult.getInstance().open();
			//		close();
			//	}
			//});

			// 난 무거우니까 잠시 내려가 있자
			close();

			ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.make, null, ViewModel.Location.createCurrentPlaceData());
			UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.moment);
			UIPhotoTake.getInstance().open();

			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}

		public void updateMyActorDirection()
		{
			if (_myActor == null)
			{
				return;
			}

			Quaternion orientation = mapbox.getInputModule().getAttitudeOrientation();
			_myActor.setDirection(orientation.eulerAngles.z);
		}

		public void onUpdateZAngle(object o)
		{
			float angle = mapbox.getViewModel().ZAngle;
			compassImage.transform.localRotation = Quaternion.Euler(0, 0, angle);

            // 2022.08.25 소현 : 소숫점때문에 안 들어오는 애들이 있네!!
            if ((int)angle == 0 || (int)angle == 360f || (int)angle == -360f)
                compassImage.gameObject.SetActive(false);
            else
                compassImage.gameObject.SetActive(true);
        }

		public void onClickCompass()
		{
			mapbox.getControl().setRotateZVelocity(0);
			mapbox.getControl().rotateZ(0,false);
		}

		public void onUpdateZoom(object o)
		{
			float zoom = mapbox.getViewModel().Zoom;

			int config_hide = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Map.Zoom.hide_address, 5);
			if( zoom <= (float)config_hide)
			{
				_addressAlphaDamper.setTarget(0);
			}
			else
			{
				_addressAlphaDamper.setTarget(1);
			}
		}

		private void createTestPath()
		{
			if( _testTripPath != null)
			{
				return;
			}

			_testTripPath = new List<UMBActor_TripPath>();

			// 테스트 코드
			MapPacket reqHistory = Network.createReq(CSMessageID.Trip.QueryTripListReq);
			reqHistory.put("target", 9);
			reqHistory.put("begin", 3);
			reqHistory.put("count", 10);

			Network.call(reqHistory, ackHistory =>
			{
				if (ackHistory.getResult() == ResultCode.ok)
				{
					List<ClientTripLog> logList = ackHistory.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log);
					foreach(ClientTripLog log in logList)
					{
						if (log.trip_id == 196)
						{
							foreach (ClientTripPathData path in log.path_data)
							{
								UMBActor path_source = mapbox.styleData.actorSourceContainer.getSource("path").actor_source;
								UMBActor_TripPath actor_trippath = (UMBActor_TripPath)mapbox.spawnActor(path_source, new MBLongLatCoordinate(0, 0));
								actor_trippath.updatePath(path);

								_testTripPath.Add(actor_trippath);
							}
						}
					}

					//UITripEndResult.getInstance().setup(log);
					//UITripEndResult.getInstance().open();
					//close();
				}
			});
		}

		//private void testSearchByDistance()
		//{
		//	MBLongLatCoordinate currentLocation = ViewModel.Location.CurrentLocation;

		//	MapPacket req = Network.createReq(CSMessageID.Map.SearchByDistanceReq);
		//	req.put("longitude", currentLocation.lon);
		//	req.put("latitude", currentLocation.lat);
		//	req.put("count", 100);
		//	req.put("radius", 5);   // 반경 10km

		//	Network.call(req, ack => { 
		//		if( ack.getResult() == ResultCode.ok)
		//		{
		//			foreach(UMBActor actor in _testActors)
		//			{
		//				mapbox.removeActor(actor);
		//			}
		//			_testActors.Clear();

		//			List<ClientSearchByDistance> searchList = ack.getList<ClientSearchByDistance>("data");
		//			foreach(ClientSearchByDistance member in searchList)
		//			{
		//				Debug.Log($"account_id[{member.account_id}] location[{member.longitude},{member.latitude}] distance[{member.distance}]");

		//				ClientMain.instance.getProfileCache().getProfileCache(member.account_id, result => { 
		//					if( result.succeeded())
		//					{
		//						ClientProfileCache profile = result.result();
		//						UMBActor_Photo actor = (UMBActor_Photo)mapbox.spawnActor("photo", new MBLongLatCoordinate(member.longitude, member.latitude));

		//						string url = profile.Profile.getPicktureURL(GlobalConfig.fileserver_url);
		//						actor.setup(url);
		//						_testActors.Add(actor);
		//					}
		//				});
		//			}
		//		}
		//	});
		//}

		void OnDrawGizmos()
		{
			if( UIMap.getInstance() == null)
			{
				return;
			}
			
			if( ViewModel.Trip.Data == null)
			{
				return;
			}

			if( ViewModel.Trip.Data.status == ClientTripConfig.StatusType.none)
			{
				return;
			}


			MBLongLatCoordinate min;
			MBLongLatCoordinate max;
			if( ViewModel.Trip.calcCurrentTripPathBoundWithCenterPosition(ViewModel.Location.CurrentLocation, out min, out max) == false)
			{
				return;
			}

			UMBControl control = mapbox.getControl();

			MBTileCoordinateDouble tilePosMin = MBTileCoordinateDouble.fromLonLat( min, control.getCurrentTilePos().zoom);
			MBTileCoordinateDouble tilePosMax = MBTileCoordinateDouble.fromLonLat( max, control.getCurrentTilePos().zoom);

			Vector3 worldMin = control.tilePosToWorldPosition(tilePosMin);
			Vector3 worldMax = control.tilePosToWorldPosition(tilePosMax);

			Vector3 center = (worldMin + worldMax) / 2.0f;
			Vector3 size = (worldMin - worldMax) / 2.0f;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			size.z = 0;
			
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(center, size * 2);
		}

		void setZoomByTripPathExtent()
		{
			if( ViewModel.Trip.Data.status == ClientTripConfig.StatusType.none)
			{
				return;
			}

			MBLongLatCoordinate min;
			MBLongLatCoordinate max;
			if (ViewModel.Trip.calcCurrentTripPathBoundWithCenterPosition(ViewModel.Location.CurrentLocation, out min, out max) == false)
			{
				return;
			}

			RectTransform rt = transform as RectTransform;
			Vector2 mapBoxExtent = new Vector2(mapbox.getTargetCamera().pixelWidth, mapbox.getTargetCamera().pixelHeight);

			UMBControl control = mapbox.getControl();
			Vector2 extent = control.calcScreenExtent(min, max);
			float bestZoom = control.calcFitZoom(extent, mapBoxExtent, new Vector2(8, 16), 0.75f);

			control.zoom(bestZoom, false);

			//Debug.Log($"fit zoom: {bestZoom}");
		}

		private void applyLocationModeToControl(MBLongLatCoordinate pos)
		{
			if (mbViewModel.CurrentLocationMode == UMBDefine.CurrentLocationMode.follow)
			{
				mapbox.getControl().scrollTo(pos);
			}
			else if (mbViewModel.CurrentLocationMode == UMBDefine.CurrentLocationMode.follow_fitzoom)
			{
				mapbox.getControl().scrollTo(pos);
				setZoomByTripPathExtent();
			}
		}

		private void onClickActor(UMBActor pick_actor)
		{
			if (pick_actor is UMBActor_Party ||
				pick_actor is UMBActor_Avatar ||
				pick_actor is UMBActor_DesertFox)
			{
				onClickActor_PartyDemo(pick_actor);
			}
			else if (pick_actor is UMBActor_Photo)
			{
				onClickActor_Photo(pick_actor as UMBActor_Photo);
			}
			else if (pick_actor is UMBActor_KPTRoom)
			{
				string url = StringCollection.get("url.kpt", 0);
				if (url.StartsWith("https") == false)
				{
					url = "https://kptdev1.lifefestaserver.com/";
				}
				UIFullscreenWebView.spawnURL(url);
			}
		}

		private void onClickActor_PartyDemo(UMBActor pick_actor)
		{
			Texture2D party_image = null;
			string level_name = null;

			if (pick_actor is UMBActor_Party)
			{
				UMBActor_Party party_actor = pick_actor as UMBActor_Party;
				party_image = party_actor.party_image;
				level_name = party_actor.scene_name;
			}
			else if (pick_actor is UMBActor_Avatar)
			{
				UMBActor_Avatar avatar_actor = pick_actor as UMBActor_Avatar;
				party_image = avatar_actor.party_image;
				level_name = avatar_actor.scene_name;
			}
			else if (pick_actor is UMBActor_DesertFox)
			{
				UMBActor_DesertFox avatar_actor = pick_actor as UMBActor_DesertFox;
				party_image = avatar_actor.party_image;
				level_name = avatar_actor.scene_name;
			}

			if (party_image != null)
			{
				mapbox.getInputFSM().changeState(UMBInputStateType.sleep);
				UIJoinParty.spawn(party_image, () =>
				{

					GameObject go = Resources.Load<GameObject>("SceneTransition");
					GameObject new_go = GameObject.Instantiate(go);
					FadeSceneTransition transition = new_go.GetComponent<FadeSceneTransition>();
					transition.startTransition(level_name);

					mapbox.getInputFSM().changeState(UMBInputStateType.wait);
				},
				() =>
				{
					mapbox.getInputFSM().changeState(UMBInputStateType.wait);
				}
				);
			}
			else
			{
				mapbox.getInputFSM().changeState(UMBInputStateType.wait);
			}
		}

		private void onClickActor_Photo(UMBActor_Photo photoActor)
		{
			List<ClientTripPhoto> photoList = photoActor.getPhotoDataList();

			UIMakeTripPhotoPopup.getInstance().setup(mapbox, photoList);
			UIMakeTripPhotoPopup.getInstance().open();

			// 테스트 사진 삭제하기
			//TripRemovePhotoProcessor step = TripRemovePhotoProcessor.create(photoList[0]);
			//step.run(result => { 
			//	if( result.succeeded())
			//	{
			//		mapbox.getTripPhotoManager().remove(photoList[0]);
			//		mapbox.getTripPhotoManager().onUpdateZoom();
			//	}
			//});
		}

		private void queryTripCheerable()
		{
			if( _timerQueryTripCheerable.update())
			{
				QueryTripCheerableListProcessor step = QueryTripCheerableListProcessor.create();
				step.run(result => {
					_timerQueryTripCheerable.setNext();
				});
			}
		}

		public void queryTripLatestCheering()
		{
			if( _timerQueryLatestTripCheering.update())
			{
				if( ViewModel.Trip.Data.status == ClientTripConfig.StatusType.none)
				{
					_timerQueryLatestTripCheering.setNext();
					return;
				}

				if( ViewModel.Trip.CheeringConfig.next_slot_id <= (ViewModel.Trip.LatestCheeringID + 1))
				{
					_timerQueryLatestTripCheering.setNext();
					return;
				}

				TripGetLatestCheeringListProcessor step = TripGetLatestCheeringListProcessor.create();
				step.run(result => {
					_timerQueryLatestTripCheering.setNext();
				});
			}
		}

		private void onUpdateCurrentUnreadCheeringList(object obj)
		{
			List<ClientTripCheering> newCheeringList = new List<ClientTripCheering>();
			ViewModel.Trip.popCurrentUnreadlTripCheeringList(newCheeringList);

			if( newCheeringList.Count > 0)
			{
				// 탐험도중 다른 사람으로 부터 받은 응원 목록
				// 하나씩 연출을 보여주면 됨
				foreach(ClientTripCheering cheering in newCheeringList)
				{
					// 일단 종류를 선택하는 유아이가 없어서,, 한 종류로 가보자!
					Debug.Log($"응원효과 연출: account_id[{cheering.cheerer_id}] cheer_type[{cheering.cheer_type}] cheer_id[{cheering.cheer_id}]");
					UITripStatus.getInstance().playParticles();

				}
			}
		}

	}
}
