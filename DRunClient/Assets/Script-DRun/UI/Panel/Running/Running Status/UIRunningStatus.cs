using System.Collections;
using Drun.Client;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.Running;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIRunningStatus : UISingletonPanel<UIRunningStatus>
	{
		// public Animator animator_alertStop;
		public GameObject gpsAlert;
		public Button btnGPSDebugger;

		[Header("===== Control (Play/Pause/Stop) =====")]
		[Header("Container")]
		public GameObject controlPaused;
		public GameObject controlTracking_ProMode;
		public GameObject controlTracking_Marathon;

		[Space(10)]
		public TMP_Text gpsAlert_Text;

		[SerializeField]
		private GameObject[] _blur_backgrounds;

		public GameObject[] mode_root;

		public UIRunningStatusTransitionAnimation _statusTransitionAnim;
		public GameObject saving_root;

		[Header("====== Running Status ========")]
		public TMP_Text text_drn_total;
		public TMP_Text text_distance;
		public TMP_Text text_total_time;
		public TMP_Text text_velocity;
		public TMP_Text text_step_count;
		public TMP_Text text_calories;
		public TMP_Text text_mine_distance;

		public Button btn_pause_running;
		public Button btn_resume_running;

		private ButtonDoubleClickBlocker.Many _click_blockers;

		[Header("====== PFP Stats ===========")]
		public UICircleLineFillAnimator heart_gauge;
		public UICircleLineFillAnimator distance_gauge;
		public UICircleLineFillAnimator stamina_gauge;
		public TMP_Text text_heart_value;
		public TMP_Text text_distance_value;
		public TMP_Text text_stamina_value;

		[Header("====== Marathon ============")]
		public UIDotGauge goal_gauge;
		public TMP_Text goal_value;
		public GameObject btn_marathon_stop;
		public GameObject btn_marathon_complete;

		[SerializeField]
		private Sprite _finishedOnActionSprite;

		[Header("====== Profile ============")]
		public UIPhotoThumbnail image_profile;

		[Header("====== GPS Signal Status ============")]
		public Image image_gps_signal_status;
		public Sprite[] sprite_gps_signal_status;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getInstance().getStringCollection();
		private RunningViewModel runningVM => ViewModel.Running;
		private RunningRecorder runningRecorder => ClientMain.instance.getRunningRecorder();

		private readonly WaitForSeconds _delayForTransitionAnim =
			new(UIRunningStatusTransitionAnimation.DefaultTransitionDurationInSeconds);

		private OpeningState _openingState;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			_click_blockers = new ButtonDoubleClickBlocker.Many(
				buttons: new[]
				{
					btn_pause_running,
					btn_resume_running
				}
			);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);

			setupProfileImage();

			showGPSDebugger();
			heart_gauge.init();
			distance_gauge.init();
			stamina_gauge.init();
			saving_root.SetActive(false);

			switch (param)
			{
				case UIPanelOpenParam_RunFromAppStart _0:
					_openingState = OpeningState.FromAppStart;
					break;

				case UIPanelOpenParam_CloseFromMapToRunningStatus _1:
					_openingState = OpeningState.FromMap;
					break;

				default:
					_openingState = OpeningState.FromStart;
					break;
			}
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				bool isProMode = runningVM.RunningType == ClientRunningLogCumulation.RunningType.promode;
				bool isMarathon = runningVM.RunningType == ClientRunningLogCumulation.RunningType.marathon;

				// 상단 배경 모드에 따라 고체
				mode_root[0].gameObject.SetActive(isProMode);
				mode_root[1].gameObject.SetActive(isMarathon);

				// 맨 뒤 블러 배경 교체
				_blur_backgrounds[0].gameObject.SetActive(isProMode);
				_blur_backgrounds[1].gameObject.SetActive(isMarathon);
			}

			//if (type == TransitionEventType.end_open)
			//{

			//}
		}

		private void showGPSDebugger()
		{
			btnGPSDebugger.gameObject.SetActive(GlobalConfig.isInhouseBranch());
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
				return;

			RunningViewModel runningVM = ViewModel.Running;

			_bindingManager.makeBinding(runningVM, nameof(runningVM.Status), updateStatus);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Goal), updateGoal);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.GoalRatio), updateGoalRatio);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.DRNTotal), updateDRNTotal);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Distance), updateDistance);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.MineDistance), updateMineDistance);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Velocity), updateVelocity);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.TotalTime), updateTotalTime);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.StepCount), updateStepCount);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.Calories), updateCalories);
			_bindingManager.makeBinding(runningVM, nameof(runningVM.UpdateStat), updateStat);	// 계산 순서가 제일 마지막인걸 설정
			_bindingManager.makeBinding(runningVM, nameof(runningVM.GPSSignalStatus), updateGPSSignalStatus);
		}

		public void onClick_Stop(bool fullClick)
		{
			if (fullClick == false)
            {
				// 좀 더 길게 눌러야 정지 됩니다: 팝업 띄우기.
                UIToast.spawn(
                        StringCollection.get("pro.pause",0),
                        new(20, -606))
                    .setType(UIToastType.error)
                    .withTransition<FadePanelTransition>()
                    .autoClose(3.0f);

				return;
			}

			saving_root.SetActive(true);
			runningRecorder.stop();
		}

		public void onClick_MarathonComplete()
		{
			saving_root.SetActive(true);
			runningRecorder.stop();
		}

		public void onClick_Resume()
		{
			if (_click_blockers.isBlocked)
				return;

			_click_blockers.block(2);

			_statusTransitionAnim.stopAndResumeToPause();
			runningRecorder.resume();
		}

		public void onClick_Pause()
		{
			if (_click_blockers.isBlocked) 
				return;

			_click_blockers.block(2);

			_statusTransitionAnim.pauseToStopAndResume();
			runningRecorder.pause();
		}

		public void onClick_Map()
		{
			UIRunningMap.getInstance().open();
		}

		private void setupProfileImage()
		{
			// 기획에 물어보기 전에 일단
			ClientNFTItem nftItem = ViewModel.ProMode.EquipedNFTItem;
			if( nftItem != null)
			{
				ClientMain.instance.getNFTMetadataCache().getMetadata(nftItem.token_id, cache => { 
					if( cache != null)
					{
						image_profile.setImageFromCDN(cache.imageUrl);
					}
				});
			}
			else
			{
				image_profile.setImage( (Texture2D)ClientMain.instance.basicModeLevelImage.getImage(ViewModel.BasicMode.LevelData.level), true);
			}
		}

		private void checkNCompleteMarathonMode()
		{
			if(runningVM.isMarathonMode() == false)
			{
				return;
			}

			if( runningVM.Status == StateType.paused && runningVM.GoalRatio >= 1.0f)
			{
				string refStr = StringCollection.get("marathon.complete.goal_reached", 0);
				UIToast.spawn(refStr, new Vector2(20, -704))
					.withSprite(_finishedOnActionSprite, _finishedOnActionSprite.rect)
					.setType(UIToastType.check)
					.setPaddingY(new(12, 12))
					.toggleTextWrap(false)
					.autoClose(2.5f);

				runningRecorder.stop();
			}

			//goal_gauge.setGauge(runningVM.GoalRatio);
		}

		public void onClick_GPSDebugger()
		{
			UIGPSDebugger.getInstance().open();
		}

#region bindingVM
		private void updateStatus(object obj)
		{
			if (!isActiveAndEnabled)
				return;

			int status = runningVM.Status;
			gpsAlert.SetActive(status == StateType.wait_gps ||
								status == StateType.wait_first_move);

			if( status == StateType.wait_gps)
			{
				gpsAlert_Text.text = StringCollection.get("pro.record.wait_gps", 0);
			}
			else if( status == StateType.wait_first_move)
			{
				gpsAlert_Text.text = StringCollection.get("pro.record.wait_first_move", 0);
			}

			// pro mode 처리
			if( runningVM.isProMode())
			{
				if (status == StateType.paused)
				{
					_statusTransitionAnim.isPausing = true;
					controlPaused.SetActive(true);

					// Transition Animation 때문에 지연 설정.
					if (isActiveAndEnabled)
						StartCoroutine(delayedOnPausedProMode());
				}
				else
				{
					_statusTransitionAnim.isPausing = false;
					controlTracking_ProMode.SetActive(true);

					if (isActiveAndEnabled)
						StartCoroutine(delayedOnElseProMode());
				}

				_statusTransitionAnim.update(_openingState);
				controlTracking_Marathon.SetActive(false);
			}
			else
			{
				controlPaused.SetActive(false);
				controlTracking_ProMode.SetActive(false);
				controlTracking_Marathon.SetActive(true);
			}

			// 2023.2.9 런닝 기록 서버 저장하기 관련 처리 추가
			if(status == StateType.end ||
				status == StateType.write_running_log ||
				status == StateType.fail_write_running_log)
			{
				saving_root.SetActive(true);
			}
			else
			{
				saving_root.SetActive(false);
			}

			// 마라톤 모드 자동으로 완료 넘기기 구현
			checkNCompleteMarathonMode();
		}


		private IEnumerator delayedOnPausedProMode()
		{
			yield return _delayForTransitionAnim;

			controlTracking_ProMode.SetActive(false);
		}

		private IEnumerator delayedOnElseProMode()
		{
			yield return _delayForTransitionAnim;

			controlPaused.SetActive(false);
		}

		private void updateGoal(object obj)
		{
			if( runningVM.RunningType != ClientRunningLogCumulation.RunningType.marathon)
			{
				return;
			}

			if (runningVM.RunningSubType == ClientRunningLogCumulation.MarathonType._free_distance)
			{
				double goal_km = (double)runningVM.Goal / 1000.0;
				goal_value.text = StringCollection.getFormat("marathon.status.goal.freemode.distance", 0, goal_km);
			}
			else if ( runningVM.RunningSubType == ClientRunningLogCumulation.MarathonType._free_time)
			{
				int goal_minute = runningVM.Goal;

				int hour = goal_minute / 60;
				int minute = goal_minute % 60;

				goal_value.text = StringCollection.getFormat("marathon.status.goal.freemode.time", 0, hour, minute);
			}
			else
			{
				double goal_km = (double)runningVM.Goal / 1000.0;
				goal_value.text = $"{goal_km}km";
			}
		}

		private void updateGoalRatio(object obj)
		{
			goal_gauge.setGauge(runningVM.GoalRatio);

			bool isComplete = runningVM.GoalRatio >= 1.0f;
			btn_marathon_stop.SetActive(isComplete == false);
			btn_marathon_complete.SetActive(isComplete == true);
		}

		private void updateDRNTotal(object obj)
		{
			text_drn_total.text = StringUtil.toDRNStringDefault(runningVM.DRNTotal);
		}

		private void updateDistance(object obj)
		{
			text_distance.text = StringUtil.toDistanceString(runningVM.Distance);
		}

		private void updateMineDistance(object obj)
		{
			text_mine_distance.text = StringUtil.toDistanceString(runningVM.MineDistance);
		}
		
		private void updateVelocity(object obj)
		{
			text_velocity.text = runningVM.Velocity.ToString("N1");
		}

		private void updateTotalTime(object obj)
		{
			//string timeString = StringUtil.toRunningTimeString(runningVM.TotalTime);

			//Debug.Log($"time[{runningVM.TotalTime}] to [{timeString}]");

			text_total_time.text = StringUtil.toRunningTimeString(runningVM.TotalTime);
		}

		private void updateStepCount(object obj)
		{
			text_step_count.text = runningVM.StepCount.ToString("N0");
		}

		private void updateCalories(object obj)
		{
			text_calories.text = runningVM.Calories.ToString("N");
		}

		private void updateStat(object obj)
		{
			if( runningVM.isProMode() == false)
			{
				return;
			}

			heart_gauge.transtion(runningVM.NFTHeartRatio);
			distance_gauge.transtion(runningVM.NFTDistanceRatio);
			stamina_gauge.transtion(runningVM.NFTStaminaRatio);

			ClientNFTItem nftItem = runningVM.NFTItem;
			RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(nftItem.grade);

			if (nftItem.max_heart == 0)
			{
				text_heart_value.text = "∞";
			}
			else
			{
				if( runningVM.NFTBonusHeart > 0)
				{
					text_heart_value.text = $"{runningVM.NFTHeart}<color=#E33ABC>+{runningVM.NFTBonusHeart}</color>";
				}
				else
				{
					text_heart_value.text = $"{runningVM.NFTHeart}";
				}

			}
			text_stamina_value.text = $"{StringUtil.toStaminaString(runningVM.NFTStamina)}";

			string curDistance = StringUtil.toStatDistanceString(runningVM.NFTDistance / 1000.0);
			//string maxDistance = StringUtil.toStatDistanceString(nftItem.max_distance / 1000.0);

			if( runningVM.NFTBonusDistance > 0)
			{
				string bonusDistance = StringUtil.toStatDistanceString(runningVM.NFTBonusDistance / 1000.0);
				text_distance_value.text = $"{curDistance}<color=#4F92E7>+{bonusDistance}</color>";
			}
			else
			{
				text_distance_value.text = $"{curDistance}";
			}
		}

		private void updateGPSSignalStatus(object obj)
		{
			image_gps_signal_status.sprite = sprite_gps_signal_status[runningVM.GPSSignalStatus];
		}

#endregion
	}
}
