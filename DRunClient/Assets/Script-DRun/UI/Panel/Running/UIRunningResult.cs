using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.Running;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace DRun.Client
{
	public class UIRunningResult : UISingletonPanel<UIRunningResult>
	{
		public UnityMapBox mapBox;
		public RectTransform mapBoxRoot;

		[Space(10)] public TMP_Text text_distance;
		public TMP_Text text_date;
		public TMP_Text text_time;
		public TMP_Text text_step;
		public TMP_Text text_calorie;
		public TMP_Text text_velocity;

		[Space(10)] public GameObject[] mode_pages;

		[Header("========= Pro Mode ========")]
		public TMP_Text text_drn_running;

		public TMP_Text text_drn_bonus;
		public UIPhotoThumbnail image_profile;

		[Header("========= Marathon Mode ======")]
		public TMP_Text text_goal_ratio;
		public TMP_Text text_current;
		public TMP_Text text_goal;

		/// <summary>
		/// 진행도 ~ 100%
		/// </summary>
		public UICircleLine gauge_goal;
		/// <summary>
		/// 진행도 100% 도달하면 이걸로 바뀜.
		/// </summary>
		public GameObject complete_goal;

		[Header("========== Share ==========")]
		[SerializeField]
		Texture2D _screenShotPreview;

		[SerializeField]
		private ScreenShotCapturer _capturer;

		[SerializeField] private Button _btn_share_pro;
		[SerializeField] private Button _btn_share_marathon;

		private UMBActor_StartPoint _actorStartPoint;
		private UMBActor_StartPoint _actorEndPoint;
		private UMBActor_RunningPath _actorRunningPath;

		private MBTileCoordinateDouble _centerTilePos;
		private MBTileCoordinateDouble _minTilePos;
		private MBTileCoordinateDouble _maxTilePos;

		private static readonly MBLongLatCoordinate initLongLat = new(126.05065589703139, 37.50850136014864);


		float mapBoxRootOriginalHeight;

		private ClientRunningLog _log;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getInstance().getStringCollection();


		public const string DefaultScreenShotExtension = ".jpg";
		public static string DefaultScreenShotName
		{
			get
			{
				var now = DateTime.Now;
				string hms = string.Join("", now
					.ToShortTimeString().Where(c => c != ' ')).Replace(':', '_');
				return $"screenshot_{now.ToShortDateString()}_{hms}" + DefaultScreenShotExtension;
			}
		}

		public const string DefaultAlbumName = "Drun";
		public const int DefaultCompressQuality = 100;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			mapBox.init(false, Camera.main, ClientMain.instance.getMBStyleCache(), MBAccess.defaultStyle);

			_actorStartPoint = (UMBActor_StartPoint)mapBox.spawnActor("drun.result.startPoint", initLongLat);
			_actorEndPoint = (UMBActor_StartPoint)mapBox.spawnActor("drun.result.endPoint", initLongLat);
			_actorRunningPath = (UMBActor_RunningPath)mapBox.spawnActor("drun.path", initLongLat);
			_actorRunningPath.thickness = 5.0f;

			mapBoxRootOriginalHeight = mapBoxRoot.sizeDelta.y;

			_capturer.initCopiedMap();

		}

		public void open(ClientRunningLog log)
		{
			base.open();

			_log = log;

			// 2023.2.13 스토어 버전 공유 버튼 hide
			bool isOpenShareFeature = GlobalConfig.isOpenShareFeature();
			_btn_share_pro.gameObject.SetActive(isOpenShareFeature);
			_btn_share_marathon.gameObject.SetActive(isOpenShareFeature);
		}

		public override void onTransitionEvent(int type)
		{
			if (type == TransitionEventType.start_open)
			{
				mapBox.gameObject.SetActive(true);
				setup();
			}
			else if (type == TransitionEventType.end_open)
			{
				mapBox.getLabelManager().setEnable(true);
			}
			else if (type == TransitionEventType.start_close)
			{
				mapBox.gameObject.SetActive(false);

				UIMainTab.getInstance().openCurrentTabPage();
				mapBox.getLabelManager().setEnable(false);
			}
		}

		public override void update()
		{
			base.update();

			if (gameObject.activeSelf)
			{
				mapBox.update();
			}
		}

		private void setup()
		{
			setupMapBox(this.mapBox, calcBestZoom, _actorStartPoint, _actorEndPoint, _actorRunningPath);
			setupUI();

			var c = _capturer;
			setupMapBox(
				c.CopiedMapBox,
				() => calcZoomForCaptureCopied(c.CopiedMapBox, c.CopiedMapBoxRoot, c.RenderCamera),
				c.CopiedActorStartPoint,
				c.CopiedActorEndPoint,
				c.CopiedActorRunningPath
			);
		}

		private void calcCenterTilePos()
		{
			MBLongLatCoordinate minPos = MBLongLatCoordinate.zero;
			MBLongLatCoordinate maxPos = MBLongLatCoordinate.zero;

			var _startLocation = MBLongLatCoordinate.zero;
			var _endLocation = MBLongLatCoordinate.zero;

			for (int i = 0; i < _log.pathList.Count; ++i)
			{
				ClientTripPathData data = _log.pathList[i];

				// 2022.08.03 log가 하나도 없을 경우 문제가 생긴다, 서버를 수정하였으나, 나중에도 혹시 몰라서
				if (data.path_list.Count == 0)
				{
					continue;
				}

				if( data.size_lon == 0 && data.size_lat == 0)
				{
					data.recalcBound();
				}

				if (i == 0)
				{
					minPos = new MBLongLatCoordinate(data.min_lon, data.min_lat);
					maxPos = new MBLongLatCoordinate(data.min_lon + data.size_lon, data.min_lat + data.size_lat);
					_startLocation = data.getFirstLocation();
					_endLocation = data.getLastLocation();
				}
				else
				{
					minPos.pos.x = System.Math.Min(data.min_lon, minPos.pos.x);
					minPos.pos.y = System.Math.Min(data.min_lat, minPos.pos.y);
					maxPos.pos.x = System.Math.Max(data.min_lon + data.size_lon, maxPos.pos.x);
					maxPos.pos.y = System.Math.Max(data.min_lat + data.size_lat, maxPos.pos.y);

					_endLocation = data.getLastLocation();
				}
			}

			MBLongLatCoordinate centerPos = new MBLongLatCoordinate();
			centerPos.pos = (minPos.pos + maxPos.pos) / 2;

			int zoom = 15;

			_centerTilePos = MBTileCoordinateDouble.fromLonLat(centerPos, zoom);
			_minTilePos = MBTileCoordinateDouble.fromLonLat(minPos, zoom);
			_maxTilePos = MBTileCoordinateDouble.fromLonLat(maxPos, zoom);
		}

		private Vector2 calcScreenExtent(UnityMapBox mapBox)
		{
			Vector3 minWorldPos = mapBox.getControl().tilePosToWorldPosition(_minTilePos);
			Vector3 maxWorldPos = mapBox.getControl().tilePosToWorldPosition(_maxTilePos);

			Vector3 minSPos = mapBox.getTargetCamera().WorldToScreenPoint(minWorldPos);
			Vector3 maxSPos = mapBox.getTargetCamera().WorldToScreenPoint(maxWorldPos);

			Vector3 scExtent = maxSPos - minSPos;

			Debug.Log($"Camera WH: [{mapBox.getTargetCamera().pixelWidth}, {mapBox.getTargetCamera().pixelHeight}]");
			Debug.Log($"Screen WH: [{Screen.width}, {Screen.height}]");

			scExtent.x = Mathf.Abs(scExtent.x);
			scExtent.y = Mathf.Abs(scExtent.y);

			return scExtent;
		}

		private static Rect GetScreenPositionFromRect(RectTransform rt, Camera camera)
		{
			// getting the world corners
			var corners = new Vector3[4];
			rt.GetWorldCorners(corners);

			// getting the screen corners
			for (var i = 0; i < corners.Length; i++)
				corners[i] = camera.WorldToScreenPoint(corners[i]);

			// getting the top left position of the transform
			var position = (Vector2)corners[1];
			// inverting the y axis values, making the top left corner = 0.
			position.y = Screen.height - position.y;
			// calculate the siz, width and height, in pixle format
			var size = corners[2] - corners[0];

			return new Rect(position, size);
		}

		private float calcBestZoom()
		{
			Vector2 extent = calcScreenExtent(this.mapBox);

			mapBoxRoot.sizeDelta = new(mapBoxRoot.sizeDelta.x, mapBoxRootOriginalHeight);

			Vector2 mapboxExtent =
				GetScreenPositionFromRect(mapBox.transform as RectTransform, mapBox.getTargetCamera()).size;

			//if (_size.x > _size.y)
			//{
			//	mapboxExtent.x = mapBox.getTargetCamera().pixelWidth;
			//	mapboxExtent.y = mapBox.getTargetCamera().pixelHeight * _size.y / _size.x;
			//}
			//else
			//{
			//	mapboxExtent.y = mapBox.getTargetCamera().pixelHeight;
			//	mapboxExtent.x = mapBox.getTargetCamera().pixelWidth * _size.x / _size.y;
			//}

			return UMBControl.calcFitZoom(_centerTilePos.zoom, extent, mapboxExtent, new Vector2(8, 18), 0.65f);
		}

		private float calcBestZoomForCopied(UnityMapBox mapbox, RectTransform mapboxRoot, Camera renderCam)
		{
			Vector2 extent = calcScreenExtent(mapbox);
			mapboxRoot.sizeDelta = new(mapboxRoot.sizeDelta.x, mapBoxRootOriginalHeight);

			Vector2 mapboxExtent = GetScreenPositionFromRect(mapboxRoot, renderCam).size;

			return UMBControl.calcFitZoom(_centerTilePos.zoom, extent, mapboxExtent, new Vector2(8, 18), 0.65f);
		}

		private float calcZoomForCaptureCopied(UnityMapBox mapbox, RectTransform mapboxRoot, Camera renderCam)
		{
			Vector2 extent = calcScreenExtent(mapbox);
			float screenShotBottomHeight = Mathf.Abs(_capturer.CopiedMapShareBottomExtent.rect.height);
			Debug.Log($"screen shot bottom box height: {screenShotBottomHeight}");

			float mapboxNewheight = mapboxRoot.rect.height - screenShotBottomHeight;
			Debug.Log($"Mapbox new height: {mapboxNewheight}");

			mapboxRoot.sizeDelta = new(mapboxRoot.sizeDelta.x, mapboxNewheight);

			Vector2 mapboxExtent = GetScreenPositionFromRect(mapboxRoot.transform as RectTransform,  renderCam).size;

			return UMBControl.calcFitZoom(_centerTilePos.zoom, extent, mapboxExtent, new Vector2(4, 18), 0.65f);
		}

		public delegate void SetupMapBox(UnityMapBox mapbox, Func<float> zoomCalculator, UMBActor_StartPoint startPoint, UMBActor_StartPoint endPoint, UMBActor_RunningPath runningPath);

		private void setupMapBox(UnityMapBox mapbox, Func<float> zoomCalculator, UMBActor_StartPoint startPoint, UMBActor_StartPoint endPoint, UMBActor_RunningPath runningPath)
		{
			MBLongLatCoordinate startLocation = _log.getStartLocation();
			MBLongLatCoordinate endLocation = _log.getEndLocation();
			if (startLocation.isZero() || endLocation.isZero())
			{
				return;
			}

			calcCenterTilePos();

			mapbox.getControl().initCurrentTilePos(_centerTilePos);

			float bestZoom = zoomCalculator();
			int targetTileZoom = MapBoxDefine.clampControlZoom((int)bestZoom);

			double tileScale = mapbox.getControl().calcTileScale(targetTileZoom, _centerTilePos.zoom);
			_centerTilePos.tile_pos.x *= tileScale;
			_centerTilePos.tile_pos.y *= tileScale;
			_centerTilePos.zoom = targetTileZoom;

			mapbox.getControl().moveTo(_centerTilePos, bestZoom);

			//
			startPoint.changePosition(startLocation);
			endPoint.changePosition(endLocation);

			runningPath.clear();
			foreach (ClientTripPathData path in _log.pathList)
			{
				runningPath.updatePath(_log.running_type, path);
			}
		}

		private void setupUI()
		{
			_capturer.Share_text_distance.text = text_distance.text =
					StringUtil.toDistanceString(_log.distance);
			
			_capturer.Share_text_velocity.text = text_velocity.text =
					_log.velocity.ToString("N1");
			
			_capturer.Share_text_calorie.text = text_calorie.text = 
					StringUtil.toCaloriesString(_log.calories);
			
			_capturer.Share_text_step.text = text_step.text = 
					_log.step_count.ToString("N0");

			TimeSpan totalTime = TimeSpan.FromSeconds(_log.running_time);
			_capturer.Share_text_time.text = text_time.text =
				$"{totalTime.Hours:D2}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";

			DateTime recordTime = _log.end_time.ToLocalTime();

			int year = recordTime.Year;
			int month = recordTime.Month;
			int day = recordTime.Day;
			int hour = recordTime.Hour;
			int minute = recordTime.Minute;

			_capturer.Share_text_date.text = text_date.text =
				$"{year:D4}.{month:D2}.{day:D2}  {hour:D2}:{minute:D2}";

			mode_pages[0].SetActive(_log.isProMode());
			mode_pages[1].SetActive(_log.isProMode() == false);

			if (_log.isProMode())
			{
				text_drn_running.text = "+" + StringUtil.toDRNStringDefault(_log.drn_running);
				text_drn_bonus.text = "+" + StringUtil.toDRNStringDefault(_log.drn_bonus);
				setupProfileImage();
			}
			else
			{
				float goalRatio = _log.getGoalRatio();
				int asPercentage = (int)(goalRatio * 100);

				// 마라톤 달성도 100% 도달 여부확인.
				if (asPercentage >= 100)
				{
					gauge_goal.gameObject.SetActive(false);
					text_goal_ratio.gameObject.SetActive(false);
					complete_goal.gameObject.SetActive(true);
				}
				else
				{
					gauge_goal.gameObject.SetActive(true);
					text_goal_ratio.gameObject.SetActive(true);
					complete_goal.gameObject.SetActive(false);

					gauge_goal.setFillAmount(goalRatio);
					text_goal_ratio.text = $"{asPercentage}%";
				}

				// 우측 km 목표거리 km 표시 갱신.
				if (_log.running_sub_type == ClientRunningLogCumulation.MarathonType._free_time)
				{
					int goal_minute = _log.goal;
					int result_minute = _log.running_time / 60;


					int curHour = result_minute % 60;
					string minutes = StringCollection.getFormat("marathon.result.goal.time.minutes", 0, curHour);
					text_current.text = minutes;

					// 목표 시간 00시간 (00분 은 생략)
					string timeStr = StringCollection.getFormat(
						"marathon.result.goal.time", 0,
						goal_minute / 60,
						goal_minute % 60);
					text_goal.text = timeStr;
				}
				else
				{
					double result = _log.distance;
					double goal_km = _log.goal / 1000.0;

					text_current.text = $"{result.ToString("N2")}km";
					text_goal.text = StringCollection.getFormat(
						"marathon.result.goal.option",
						0, goal_km.ToString("N2")
					);
				}

				//complete_goal.SetActive(goalRatio >= 1.0f);
			}
		}

		private void setupProfileImage()
		{
			int token_id;
			if (int.TryParse(_log.pfp, out token_id) == false)
			{
				image_profile.setEmpty();
				return;
			}

			ClientMain.instance.getNFTMetadataCache().getMetadata(token_id, cache =>
			{
				if (cache != null)
				{
					image_profile.setImageFromCDN(cache.imageUrl);
				}
				else
				{
					image_profile.setEmpty();
				}
			});
		}

		public void onClick_Close()
		{
			var c = _capturer;
			setupMapBox(
				c.CopiedMapBox,
				() => calcBestZoomForCopied(c.CopiedMapBox, c.CopiedMapBoxRoot, c.RenderCamera),
				c.CopiedActorStartPoint,
				c.CopiedActorEndPoint,
				c.CopiedActorRunningPath
			);

			UIMainTab.getInstance().open();
		}


		public void onClick_shareButton()
		{
			var c = _capturer;		
			StartCoroutine(c.capture(before, after));

			void before()
			{
			}

			void after(Texture2D screenShot)
			{
			

#if UNITY_EDITOR
				saveScreenShot(screenShot);
#else
				new NativeShare().AddFile(screenShot, DefaultScreenShotName).Share();
#endif

			}
		}

		/// <summary>
		/// 
		/// </summary>gg
		/// <param name="screenShot"></param>
		/// <param name="onSuccessSaving">param runningPath 는 Android 전용. (iOS -> string.Empty)</param>
		private void saveScreenShot(Texture2D screenShot, Action<string> onSuccessSaving = null)
		{

#if UNITY_EDITOR
			string fileDir = Path.Combine(Application.temporaryCachePath, "drun");

			if (!Directory.Exists(fileDir))
				Directory.CreateDirectory(fileDir);

			string fileFullPath = Path.Combine(fileDir, DefaultScreenShotName);

			try
			{
				// 타겟 directory 없으면 만들기!
				var encodedOrig = screenShot.EncodeToJPG(DefaultCompressQuality);
				File.WriteAllBytes(fileFullPath, encodedOrig);

				onSuccessSaving?.Invoke(fileFullPath);
			}
			catch (DirectoryNotFoundException ex) { Debug.LogException(ex); }
			finally
			{
				// open file saved directory.
				Process.Start(@$"{fileDir}");
			}

#elif UNITY_ANDROID || UNITY_IOS

			try
			{
				NativeGallery.SaveImageToGallery(
					screenShot,
					DefaultAlbumName,
					DefaultScreenShotName,
					(success, path) =>
					{
						Debug.Log($"Save Image To Gallery {(success ? "success!" : "failed!")}");

						if (success)
							onSuccessSaving?.Invoke(path);
					}
				);
			}
			catch (Excetion ex) { Debug.LogException(ex); }
#endif
		}
	}
}
