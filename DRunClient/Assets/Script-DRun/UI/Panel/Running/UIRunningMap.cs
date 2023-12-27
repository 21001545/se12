using DRun.Client.Logic.MapBox;
using DRun.Client.Module;
using DRun.Client.Running;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIRunningMap : UISingletonPanel<UIRunningMap>
	{
		public UnityMapBox mapBox;

		public TMP_Text text_address;
		public Image image_compass;
		public UIColorToggleButton btn_locationMode;
		public Animator animatorTooltip;

		[SerializeField] 
		private Sprite _meLogo;

		[SerializeField] 
		private Sprite _meBg;

		[Header("========= Running Status =========")]
		public TMP_Text text_drn_total;
		public TMP_Text text_distance;
		public TMP_Text text_total_time;
		public TMP_Text text_velocity;
		public TMP_Text text_step_count;
		public TMP_Text text_calories;
		public GameObject drn_root;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RunningViewModel runningVM => ViewModel.Running;
		private UMBViewModel mapBoxVM => mapBox.getViewModel();
		private RunningRecorder runningRecorder => ClientMain.instance.getRunningRecorder();

		private UMBActor_CurrentPoint _actorCurrentPoint;
		private UMBActor_StartPoint _actorStartPoint;
		private UMBActor_RunningPath _actorRunningPath;
		private bool _mapBoxInitPosition;
		private IntervalTimer _addressTimer;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			mapBox.init(false, Camera.main, ClientMain.instance.getMBStyleCache(), MBAccess.defaultStyle);
			_mapBoxInitPosition = false;

			_addressTimer = IntervalTimer.create(3.0f, false, false);

			MBLongLatCoordinate initPos = new MBLongLatCoordinate(127.05065589703139, 37.50850136014864);

			_actorStartPoint = (UMBActor_StartPoint)mapBox.spawnActor("drun.running.startPoint", initPos);
			_actorCurrentPoint = (UMBActor_CurrentPoint)mapBox.spawnActor("drun.running.currentPoint", initPos);
			_actorRunningPath = (UMBActor_RunningPath)mapBox.spawnActor("drun.path", initPos);
			_actorRunningPath.thickness = 5.0f;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);

			drn_root.SetActive(runningVM.isProMode());
		}

		public override void update()
		{
			base.update();

			if(gameObject.activeSelf)
			{
				mapBox.update();
				updateAddress();
			}
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_close)
			{
				mapBox.getLabelManager().setEnable(false);
			}
			else if( type == TransitionEventType.end_open)
			{
				mapBox.getLabelManager().setEnable(true);
			}
			else if( type == TransitionEventType.start_open)
			{
				_actorRunningPath.initColorSpeed();

				MBLongLatCoordinate lastLocation = runningVM.CurrentLocation;
				if (lastLocation.isZero() == false)
				{
					mapBox.getControl().moveTo(lastLocation, 16);
					mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);
					_mapBoxInitPosition = true;
				}

				updateAddressNow();
				text_address.text = "";
			}
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			_bindingManager.makeBinding(runningVM, nameof(runningVM.Status), updateStatus);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.PathEventList), updatePathEvent);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.DRNTotal), updateDRNTotal);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Distance), updateDistance);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Velocity), updateVelocity);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.TotalTime), updateTotalTime);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.StepCount), updateStepCount);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Calories), updateCalories);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.StartLocation), updateStartLocation);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.CurrentLocation), updateCurrentLocation);

			_bindingManager.makeBinding(mapBoxVM, nameof(mapBoxVM.CurrentLocationMode), updateLocationMode);
			_bindingManager.makeBinding(mapBoxVM, nameof(mapBoxVM.ZAngle), updateZAngle);
		}

		private void updateStatus(object obj)
		{
			// NOTE 윤상 2023-02-07
			// 지도 상 현재위치 나타내는 점 (NFT 이미지) -> Drun 로고로 변경했음.

			//if( runningVM.isMarathonMode())
			//{
			//	_actorCurrentPoint.profileIcon.setEmpty();
			//}
			//else
			//{
			//	ClientMain.instance.getNFTMetadataCache().getMetadata(runningVM.NFTItem.token_id, cache =>
			//	{
			//		if( cache != null)
			//		{
			//			_actorCurrentPoint.profileIcon.setImageFromCDN(cache.imageUrl);
			//		}
			//	});
			//}
		}

		private void updateAddress()
		{
			if (gameObject.activeSelf && _addressTimer.update())
			{
				updateAddressNow();
			}
		}

		private void updateAddressNow()
		{
			MBLongLatCoordinate location = runningVM.CurrentLocation;
			if (location.isZero())
			{
				_addressTimer.setNext();
			}
			else
			{
				QueryAddress step = QueryAddress.create(location);
				step.run(result => { 
					if( result.succeeded())
					{
						text_address.text = step.getAddressString();
					}

					_addressTimer.setNext();
				});
			}
		}

		#region bindingVM
		//private void updateStatus(object obj)
		//{
		//	// 최초 생성
		//	if( runningVM.Status == StateType.tracking && runningVM.PrevStatus == StateType.wait_first_move)
		//	{
		//		_actorRunningPath.clear();
		//		_actorRunningPath.setup(runningVM.PathList);
		//	}

		//	if ( runningVM.Status == StateType.none)
		//	{
		//		_actorRunningPath.clear();
		//	}
		//}

		private void updatePathEvent(object obj)
		{
			List<RunningPathEvent> eventList = new List<RunningPathEvent>();
			runningVM.popPathEvent(eventList);
			foreach(RunningPathEvent e in eventList)
			{
				if( e.eventType == RunningPathEvent.EventType.reset)
				{
					_actorRunningPath.clear();
				}
				else if( e.eventType == RunningPathEvent.EventType.create_path)
				{
					_actorRunningPath.updatePath(e.pathData);
				}
				else if( e.eventType == RunningPathEvent.EventType.append_log)
				{
					_actorRunningPath.updatePath(e.pathData);
				}
			}
		}

		private void updateDRNTotal(object obj)
		{
			text_drn_total.text = "+" + StringUtil.toDRNStringDefault(runningVM.DRNTotal);
		}

		private void updateDistance(object obj)
		{
			text_distance.text = StringUtil.toDistanceString(runningVM.Distance);
		}

		private void updateVelocity(object obj)
		{
			text_velocity.text = runningVM.Velocity.ToString("N1");
		}

		private void updateTotalTime(object obj)
		{
			TimeSpan totalTime = runningVM.TotalTime;
			text_total_time.text = $"{totalTime.Hours.ToString("D2")}:{totalTime.Minutes.ToString("D2")}:{totalTime.Seconds.ToString("D2")}";
		}

		private void updateStepCount(object obj)
		{
			text_step_count.text = runningVM.StepCount.ToString("N0");
		}

		private void updateCalories(object obj)
		{
			text_calories.text = runningVM.Calories.ToString("N");
		}

		private void updateStartLocation(object obj)
		{
			_actorStartPoint.changePosition(runningVM.StartLocation);
		}

		private void updateCurrentLocation(object obj)
		{
			MBLongLatCoordinate location = runningVM.CurrentLocation;
			
			if( location.isZero())
			{
				return;
			}

			_actorCurrentPoint.changePosition(location);
			if (_mapBoxInitPosition == false)
			{
				_mapBoxInitPosition = true;
				mapBox.getControl().moveTo(location, 16);
				mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);

				updateAddressNow();
			}

			if( mapBoxVM.CurrentLocationMode == UMBDefine.CurrentLocationMode.follow)
			{
				mapBox.getControl().scrollTo(location);
			}
		}

		#endregion

		#region bindingMapBoxVM
		private void updateLocationMode(object obj)
		{
			if( mapBoxVM.CurrentLocationMode == UMBDefine.CurrentLocationMode.none)
			{
				btn_locationMode.setStatus(false);
			}
			else
			{
				btn_locationMode.setStatus(true);
			}
		}

		private void updateZAngle(object obj)
		{
			float angle = mapBoxVM.ZAngle;
			image_compass.transform.localRotation = Quaternion.Euler(0, 0, angle);

			angle %= 360.0f;
			if( angle < 0)
			{
				angle += 360.0f;
			}

			if( (int)angle == 0 || (int)angle == 360)
			{
				image_compass.transform.parent.gameObject.SetActive(false);
			}
			else
			{
				image_compass.transform.parent.gameObject.SetActive(true);
			}
		}

		#endregion

		#region control

		public void onClick_Back()
		{
			UIRunningStatus.getInstance().open(
				new UIPanelOpenParam_CloseFromMapToRunningStatus()
			);
		}

		public void onClick_Location()
		{
			int cur_mode = mapBoxVM.CurrentLocationMode;
			if( cur_mode == UMBDefine.CurrentLocationMode.follow)
			{
				mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.none);
			}
			else
			{
				mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);

				MBLongLatCoordinate location = runningVM.CurrentLocation;
				if(location.isZero() == false)
				{
					mapBox.getControl().scrollTo(location);
				}
			}
		}

		public void onClick_Tooltip()
		{
			animatorTooltip.SetTrigger("show");
		}

		public void onClick_Compass()
		{
			mapBox.getControl().setRotateZVelocity(0);
			mapBox.getControl().rotateZ(0, false);
		}

		#endregion
	}
}
