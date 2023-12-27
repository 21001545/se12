using AwesomeCharts;
using Festa.Client.Logic;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
    public class UIStatisticsTripDetail : UISingletonPanel<UIStatisticsTripDetail>
    {
        [Header("result texts")]
        [SerializeField]
        private TMP_Text txt_dateTime;
        [SerializeField]
        private TMP_InputField input_title;
        [SerializeField]
        private Image img_tripIcon;
        [SerializeField]
        private TMP_Text txt_tripType;
        [SerializeField]
        private TMP_Text txt_hourValue;
        [SerializeField]
        private TMP_Text txt_minValue;
        [SerializeField]
        private TMP_Text txt_secValue;

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

        [Header("chart texts")]
        [SerializeField]
        private TMP_Text txt_speedUnit;
        [SerializeField]
        private TMP_Text txt_altUnit;

        [Header("charts")]
        [SerializeField]
        private LineChart _speedChart;
        [SerializeField]
        private TMP_Text txt_speedLabel_0;
        [SerializeField]
        private TMP_Text txt_speedLabel_1;
        [SerializeField]
        private TMP_Text txt_speedLabel_2;

        [SerializeField]
        private LineChart _altChart;
        [SerializeField]
        private TMP_Text txt_altLabel_0;
        [SerializeField]
        private TMP_Text txt_altLabel_1;
        [SerializeField]
        private TMP_Text txt_altLabel_2;

        [Header("top area cover")]
        [SerializeField]
        private CanvasGroup can_circleBg;
        [SerializeField]
        private CanvasGroup can_fullCoverBg;
        [SerializeField]
        private ScrollRect _scrollRect;

        public UnityMapBox mapBox;

        private bool _initMapBox;
        private ClientTripLog _log;
		private bool _isValidPathBound;
		private MBLongLatCoordinate _pathCenter;
		private MBLongLatCoordinate _pathMin;
		private MBLongLatCoordinate _pathMax;

		private ClientNetwork Network => ClientMain.instance.getNetwork();

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            mapBox.init(false, Camera.main, ClientMain.instance.getMBStyleCache(), "ckvta2ylt1mq614s8vrp37ktq");
            mapBox.getInputFSM().OnClickActor = onClickActor;
            _initMapBox = true;

		}

        //public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        //{
        //    base.open(param, transitionType, closeType);

        //}

        public void setup(ClientTripLog log)
        {
            _log = log;
			_isValidPathBound = _log.calcPathBound(out _pathCenter, out _pathMin, out _pathMax);

            resetResultPanel();

			int configDistanceType = ClientMain.instance.getViewModel().Profile.Setting_DistanceUnit;
            int speedUnitType = configDistanceType;
            int altitudeUnitType = configDistanceType == UnitDefine.DistanceType.km ? UnitDefine.DistanceType.m : UnitDefine.DistanceType.ft;

            TripChartDataBuilder chartDataBuilder = TripChartDataBuilder.create(_log, speedUnitType, altitudeUnitType);
            chartDataBuilder.run(result => { 
                
                if( result.succeeded())
                {
					resetSpeedChart( chartDataBuilder);
					resetAltChart(chartDataBuilder);
				}

			});
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
            }
            else if( type == TransitionEventType.start_open)
            {
                _initMapBox = false;

				mapBox.gameObject.SetActive(true);
                setupMapBoxPosition();
                createTripPaths();
                createTripPhotos();
                createTripFlags();
                zoomPathBound();
            }
        }

        public void resetResultPanel()
        {
            // UITripEndResult 에서처럼 설정하도록 우선 해 두었습니다..!!
            RefStringCollection stringCollection = GlobalRefDataContainer.getStringCollection();
            ClientTripLog log = _log;

            var data_tripData = log.end_time.ToLocalTime();
            string year = data_tripData.Year.ToString("N0").Substring(2, 2);
            string time = data_tripData.ToString("hh:mm tt");
            txt_dateTime.text = $"{year}.{data_tripData.Month.ToString("D2")}.{data_tripData.Day.ToString("D2")} - {time}";

            input_title.text = log.name;
            int tripType = UIMap.getInstance().getCurrentTripType();
            img_tripIcon.sprite = UITripEndResult.getInstance().tripIcons[tripType];
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
                txt_disValue.text = log.distance_total.ToString("F2");
                txt_disLabel.text = stringCollection.getFormat("triproute.menu.distance", 0, "km");

                txt_speedValue.text = (log.distance_total / diff_time.TotalHours).ToString("F2");
                txt_speedLabel.text = stringCollection.getFormat("triproute.menu.velocity", 0, "km/h");
            }
            else
            {
                txt_disValue.text = UnitDefine.km_2_mil(log.distance_total).ToString("F2");
                txt_disLabel.text = stringCollection.getFormat("triproute.menu.distance", 0, "mile");

                txt_speedValue.text = (UnitDefine.km_2_mil(log.distance_total) / diff_time.TotalHours).ToString("F2");
                txt_speedLabel.text = stringCollection.getFormat("triproute.menu.velocity", 0, "mile/h");
            }

            // 기타 데이터 값
            txt_stepValue.text = log.step_total.ToString("N0");

            // 로그에 없어서 우선은 현재고도
            txt_altValue.text = ClientMain.instance.getViewModel().Location.CurrentAltitude.ToString("F2");

            txt_calValue.text = log.calorie_total.ToString("N0");

            // 2022.08.17 소현 : 고도 단위도 바뀌나?? 일단 미터로 통일
            txt_altLabel.text = stringCollection.getFormat("triproute.menu.altitude", 0, "m");
        }

        public void resetSpeedChart(TripChartDataBuilder chartData)
        {
            //List<float> dataList = new List<float>(){ 29, 10, 100, 29, 199, 50, 40, 20};
            float totalDis = (float)_log.distance_total;
            int unit = ClientMain.instance.getViewModel().Profile.Setting_DistanceUnit;
            txt_speedUnit.text = "(km)";
            if (unit == UnitDefine.DistanceType.mil)
            {
                totalDis = (float)UnitDefine.km_2_mil(totalDis);
                txt_speedUnit.text = "(mile)";
            }

            txt_speedLabel_0.text = (totalDis / 4f).ToString("F2");
            txt_speedLabel_1.text = (totalDis / 2f).ToString("F2");
            txt_speedLabel_2.text = (3f * totalDis / 4f).ToString("F2");

            List<float> dataList = chartData.getSpeedChart();
            var speedData = _speedChart.GetChartData();

            var speedDataSet = speedData.DataSets[0];
            speedDataSet.Entries.Clear();

            for (int i = 0; i < dataList.Count; ++i)
            {
                LineEntry entry = new LineEntry(i, dataList[i]);
                speedDataSet.Entries.Add(entry);
            }

            _speedChart.SetDirty();
        }

        public void resetAltChart(TripChartDataBuilder chartData)
        {
            var totalDis = _log.distance_total;
            int unit = ClientMain.instance.getViewModel().Profile.Setting_DistanceUnit;
            txt_altUnit.text = "(km)";
            if (unit == UnitDefine.DistanceType.mil)
            {
                totalDis = UnitDefine.km_2_mil(totalDis);
                txt_altUnit.text = "(mile)";
            }

            txt_altLabel_0.text = (totalDis / 4f).ToString("F2");
            txt_altLabel_1.text = (totalDis / 2f).ToString("F2");
            txt_altLabel_2.text = (3f * totalDis / 4f).ToString("F2");

            List<float> dataList = chartData.getAltitudeChart();
            var altData = _altChart.GetChartData();

            var altDataSet = altData.DataSets[0];
            altDataSet.Entries.Clear();

            for (int i = 0; i < dataList.Count; ++i)
            {
                LineEntry entry = new LineEntry(i, dataList[i]);
                altDataSet.Entries.Add(entry);
            }

            _altChart.SetDirty();
        }

        public void onScrollerValueChanged()
        {
            float threshold = 0.4f;

            if (_scrollRect.normalizedPosition.y <= threshold)
            {
                can_circleBg.alpha = (_scrollRect.normalizedPosition.y - (threshold - 0.15f)) / 0.15f;
                can_fullCoverBg.alpha = (threshold - _scrollRect.normalizedPosition.y) / 0.15f;
            }
            else
            {
                can_circleBg.alpha = 1;
                can_fullCoverBg.alpha = 0;
            }
        }

        public void onClickBackNavigation()
        {
            mapBox.gameObject.SetActive(false);

            if (input_title.text != _log.name)
            {
                // 이름을 변경한 경우 수정
                changeTripName(input_title.text);
            }

            UIStatistics.getInstance().TabType = UIStatistics.TabTypes.History;
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        private void changeTripName(string name)
        {
            MapPacket req = Network.createReq(CSMessageID.Trip.ChangeTripNameReq);
            req.put("id", _log.trip_id);
            req.put("name", name);

            Network.call(req, ack => {
                if (ack.getResult() == ResultCode.ok)
                {
                    _log.name = name;
                }
            });
        }

        #region map box

        public override void update()
        {
            base.update();
            if (gameObject.activeSelf == true && _initMapBox == false)
            {
                mapBox.update();
            }
        }

        private void onClickActor(UMBActor pick_actor)
		{
			//if (pick_actor is UMBActor_Photo)
			//{
			//	UMBActor_Photo photoActor = pick_actor as UMBActor_Photo;
			//	List<ClientTripPhoto> photoList = photoActor.getPhotoDataList();

			//	UIMakeTripPhotoPopup.getInstance().setup(mapBox, photoList);
			//	UIMakeTripPhotoPopup.getInstance().open();
			//}
		}

		private void setupMapBoxPosition()
		{
			if (_isValidPathBound)
			{
				mapBox.getControl().moveTo(_pathCenter, 16);
			}
		}

		private void createTripPaths()
		{
			mapBox.removeAllActors();

			foreach (ClientTripPathData path in _log.path_data)
			{
				UMBActor path_source = mapBox.styleData.actorSourceContainer.getSource("path").actor_source;
				UMBActor_TripPath actor_trippath = (UMBActor_TripPath)mapBox.spawnActor(path_source, MBLongLatCoordinate.zero);

				actor_trippath.updatePath(path);
			}
		}

		private void createTripPhotos()
		{
			mapBox.getTripPhotoManager().clear();
			if (_log._photoList != null)
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

			if (startLocation.isZero() || endLocation.isZero())
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
			if (_isValidPathBound == false)
			{
				return;
			}

			UMBControl control = mapBox.getControl();

			Vector2 extent = control.calcScreenExtent(_pathMin, _pathMax);
			Vector2 mbExtent = mapBox.calcScreenExtent();

			float bestZoom = control.calcFitZoom(extent, mbExtent, new Vector2(8, 16), 0.75f);
			control.zoom(bestZoom, true);
		}

        #endregion
    }
}