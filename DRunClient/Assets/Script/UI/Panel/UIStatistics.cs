using AwesomeCharts;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
    public class UIStatistics : UISingletonPanel<UIStatistics>
    {
        public TMP_FontAsset font_regular;
        public TMP_FontAsset font_bold;

        private int _graphType = GraphTypes.Daily;

        public int GraphType
        {
            get { return _graphType; }
            set { Set<int>(ref _graphType, value); }
        }
        public class GraphTypes
        {
            public const int Daily = 0;
            public const int Weekly = 1;
            public const int Monthly = 2;
        }

        private int _tabType = TabTypes.Insight;

        public int TabType
        {
            get { return _tabType; }
            set { Set<int>(ref _tabType, value); }
        }

        public class TabTypes
        {
            public const int Insight = 0;
            public const int History = 1;
            public const int LifeStat = 2;
        }

        [Header("main tabs")]
        public TMP_Text txt_btn_insight;
        public TMP_Text txt_btn_hisotry;
        public TMP_Text txt_btn_lifestat;
        public RectTransform rect_selectedLine;
        [SerializeField]
        private GameObject[] mainTabs;

        #region insight objects

        public class RealtimeGroupID
        {
            public const int Description = 0;
            public const int TodayTotalCal = 1;
            public const int MeVsAll = 2;
            public const int MeVsFriends = 3;
            public const int TodayTopTime = 4;
            public const int TodayVsYesterday = 5;
        }

        [Header("[-- Insights --]")]

        [SerializeField]
        private Sprite[] subTabImages = new Sprite[2];
        [SerializeField]
        private Sprite[] upDownArrows = new Sprite[2];

        [Header("Insight sub tabs")]
        #region insight sub tabs
        [SerializeField]
        private GameObject go_insightDaily;
        [SerializeField]
        private GameObject go_insightWeekly;
        [SerializeField]
        private GameObject go_insightMonthly;
        [SerializeField]
        private Image img_subTabDaily;
        [SerializeField]
        private Image img_subTabWeekly;
        [SerializeField]
        private Image img_subTabMonthly;
        [SerializeField]
        private TMP_Text txt_subTabDaily;
        [SerializeField]
        private TMP_Text txt_subTabWeekly;
        [SerializeField]
        private TMP_Text txt_subTabMonthly;
        #endregion

        [Header("daily")]
        [SerializeField]
        private TMP_Text txt_goalSteps;
        [SerializeField]
        private TMP_Text txt_dailySteps_upper;
        [SerializeField]
        private TMP_Text txt_dailySteps_big;
        [SerializeField]
        private TMP_Text txt_dailyUpDownSteps;
        [SerializeField]
        private Image img_dailyUpDownArrow;
        [SerializeField]
        private Image img_dailyStepGauge;
        [SerializeField]
        private GameObject go_dailyStepGaugeFull;
        [SerializeField]
        private Animator dailyFoxAnimator;
        [SerializeField]
        private float dailyChartAnimationSpeed;
        private bool _updateDailyChartAnimation = false;
        private float _fillRatio = 0f;

        private int _goal_count = 9000;
        public int GoalCount
        {
            get { return _goal_count; }
            set { Set<int>(ref _goal_count, value); }
        }

        private bool _quering = false;

        [Header("weekly")]
        [SerializeField]
        private TMP_Text txt_weeklySteps_upper;
        [SerializeField]
        private WeeklyBarChart week_average_chart;
        [SerializeField]
        private TMP_Text txt_weeklyUpDownSteps;
        [SerializeField]
        private Image img_weeklyUpDownArrow;
        [SerializeField]
        private Button btn_week_average_next;
        [SerializeField]
        private TMP_Text txt_weekSwitch;
        [SerializeField]
        private RectTransform rect_weeklyAverageLine;
        [SerializeField]
        private TMP_Text txt_weeklyAverageValue;
        [SerializeField]
        private RectTransform rect_weeklyAverageLabel;
        [SerializeField]
        private UIStatisticsInnerScroll weeklyInnerScroll;


        // 현재 표시되고 있는 주간 평균 걸음의 begin time
        private long _currentWeek_beginTime;
        public long CurrentWeekBeginTime
        {
            get { return _currentWeek_beginTime; }
            set { Set(ref _currentWeek_beginTime, value); }
        }

        [Header("monthly")]
        [SerializeField]
        private TMP_Text txt_monthlySteps_upper;
        [SerializeField]
        private RectTransform calendar_cell_parent = null;
        //[SerializeField]
        //private GameObject calendar_cell_prefab = null;
        [SerializeField]
        private Image img_foxFill;
        [SerializeField]
        private GameObject go_foxColor;
        [SerializeField]
        private Button btn_month_next;
        [SerializeField]
        private TMP_Text txt_monthSwitch;
        [SerializeField]
        private UIStatisticsInnerScroll monthlyInnerScroll;

        // 현재 표시되고 있는 월간 달력의 begin time
        private DateTime _currentMonth;
        private int _currentMonth_int;
        public int CurrentMonthInt
        {
            get { return _currentMonth_int; }
            set
            {
                Set(ref _currentMonth_int, value);
            }
        }

        [Header("realtime data")]
        [SerializeField]
        private GameObject[] go_realtimePanels = new GameObject[6];
        [SerializeField]
        private TMP_Text txt_caloriesFood_upper;
        [SerializeField]
        private TMP_Text txt_caloriesFood_cal;

        [SerializeField]
        private TMP_Text txt_meVsAll_upper;
        [SerializeField]
        private WeeklyBarChart me_vs_all_chart;
        [SerializeField]
        private TMP_Text[] meVsAllValues = new TMP_Text[2];
        [SerializeField]
        private TMP_Text[] meVsAllName = new TMP_Text[2];

        [SerializeField]
        private TMP_Text txt_meVsFriends_upper;
        [SerializeField]
        private WeeklyBarChart me_vs_friends_chart;
        [SerializeField]
        private GameObject go_meVsFriends_dotdotdot;
        [SerializeField]
        private GameObject go_meVsFriendsChart;
        [SerializeField]
        private GameObject go_noFriends;
        [SerializeField]
        private RectTransform rect_meVsFriendsChart;
        [SerializeField]
        private RectTransform rect_meVsFriendsLabel;

        [SerializeField]
        private TMP_Text[] meVsFriendsValues = new TMP_Text[4];
        [SerializeField]
        private TMP_Text[] meVsFriendsName = new TMP_Text[4];

        [SerializeField]
        private TMP_Text txt_dailyByTime_upper;
        [SerializeField]
        private WeeklyBarChart daily_byTime_chart;

        [SerializeField]
        private TMP_Text txt_todayVsYest_upper;
        [SerializeField]
        private LineChart today_vs_yest_chart;

        // 이번주의 begin_time
        private long _thisWeek_beginTime;

        [Header("accumulated data sets")]
        [SerializeField]
        private TMP_Text txt_accumCal;
        [SerializeField]
        private TMP_Text txt_accumDis;
        [SerializeField]
        private TMP_Text txt_accumSteps;
        [SerializeField]
        private TMP_Text txt_accumDisDesc;

        #endregion

        [Header("[-- explore --]")]
        [SerializeField]
        public UIStatistics_exploreScroller exploreScrollDelegate;
        private int _listType = UIStatistics_exploreScroller.ListType.list;

        #region life stat objects

        [Header("[-- Life Stat --]")]
        [SerializeField]
        private GameObject[] go_lifeStatPanels;
        [SerializeField]
        private LifeStatGraphRenderer lifeStatGraphRenderer;

        #endregion

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private ClientNetwork Network => ClientMain.instance.getNetwork();
        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            // 일단 열고 시작해요!
            for (int i = 0; i < go_realtimePanels.Length; i++)
            {
                go_realtimePanels[i].SetActive(true);
            }
            for(int i = 0; i < go_lifeStatPanels.Length; ++i)
            {
                go_lifeStatPanels[i].SetActive(true);
            }

            // 열면 일단 무조건,, 인사이트 데일리
            GraphType = GraphTypes.Daily;
            setChartType();
            resetTodayTopTodayYestChart();

            weeklyInnerScroll.setGraphType(GraphTypes.Weekly);
            monthlyInnerScroll.setGraphType(GraphTypes.Monthly);

            var now = DateTime.Now;
            DateTime begin_date = new DateTime(now.Year, now.Month, now.Day);
            begin_date = begin_date.AddDays(-(int)now.DayOfWeek);       // 한 주의 시작은 일요일!

            _thisWeek_beginTime = _currentWeek_beginTime = TimeUtil.unixTimestampFromDateTime(begin_date.ToUniversalTime());
            _currentMonth = DateTime.UtcNow;
            _currentMonth_int = DateTime.UtcNow.Month;

            // 탐험기록 스크롤 초기화
            exploreScrollDelegate.init();

            resetBindings();
            base.open(param, transitionType, closeType);

            // 2022.05.26 이강희 오늘의 걸음 수 목표값 설정
            GoalCount = ViewModel.Health.TodayStepGoalCount;

            // 서버로 부터 top rank, friend rank 얻어오기
            loadTopRankFromServer();
            loadFriendRankFromServer();
            loadTripLogFromServer();
        }

        public void onBack()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        private void resetBindings()
        {
            if (_bindingManager.getBindingList().Count > 0)
            {
                return;
            }

            _bindingManager.makeBinding(this, "TabType", value =>
            {
                txt_btn_insight.color = _tabType == TabTypes.Insight ? ColorChart.gray_900 : ColorChart.gray_400;
                txt_btn_hisotry.color = _tabType == TabTypes.History ? ColorChart.gray_900 : ColorChart.gray_400;
                txt_btn_lifestat.color = _tabType == TabTypes.LifeStat ? ColorChart.gray_900 : ColorChart.gray_400;

                mainTabs[0].SetActive(_tabType == TabTypes.Insight);
                mainTabs[1].SetActive(_tabType == TabTypes.History);
                mainTabs[2].SetActive(_tabType == TabTypes.LifeStat);

                // 넓이가 바뀌는 경우에 대응해 볼게용~
                if (_tabType == TabTypes.Insight)
                {
                    rect_selectedLine.pivot = new Vector2(0f, 1f);
                    rect_selectedLine.anchorMin = new Vector2(0f, 1f);
                    rect_selectedLine.anchorMax = new Vector2(0f, 1f);
                    DOTween.To(() => rect_selectedLine.anchoredPosition, x => rect_selectedLine.anchoredPosition = x, new Vector2(16.5f, -51f), 0.1f);
                }
                else if (_tabType == TabTypes.History)
                {
                    rect_selectedLine.pivot = new Vector2(0.5f, 1f);
                    rect_selectedLine.anchorMin = new Vector2(0.5f, 1f);
                    rect_selectedLine.anchorMax = new Vector2(0.5f, 1f);
                    DOTween.To(() => rect_selectedLine.anchoredPosition, x => rect_selectedLine.anchoredPosition = x, new Vector2(0f, -51f), 0.1f);

                    resetExploreScrollData();
                }
                else if (_tabType == TabTypes.LifeStat)
                {
                    rect_selectedLine.pivot = new Vector2(1f, 1f);
                    rect_selectedLine.anchorMin = new Vector2(1f, 1f);
                    rect_selectedLine.anchorMax = new Vector2(1f, 1f);
                    DOTween.To(() => rect_selectedLine.anchoredPosition, x => rect_selectedLine.anchoredPosition = x, new Vector2(-16.5f, -51f), 0.1f);

                    drawStatGraph();
                }
            });

            _bindingManager.makeBinding(this, "CurrentWeekBeginTime", value =>
            {
                btn_week_average_next.interactable = _currentWeek_beginTime < _thisWeek_beginTime;
                resetWeekAverageChart();
            });

            _bindingManager.makeBinding(this, "CurrentMonthInt", value =>
            {
                btn_month_next.interactable = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1) > new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                resetMonthChart();
            });

            HealthViewModel health_vm = ViewModel.Health;
            ProfileViewModel profile_vm = ViewModel.Profile;

            _bindingManager.makeBinding(this, "GraphType", value => {

                // 버튼이미지랑 글씨색 바꾸고
                img_subTabDaily.sprite = subTabImages[GraphType == GraphTypes.Daily ? 0 : 1];
                img_subTabWeekly.sprite = subTabImages[GraphType == GraphTypes.Weekly ? 0 : 1];
                img_subTabMonthly.sprite = subTabImages[GraphType == GraphTypes.Monthly ? 0 : 1];
                txt_subTabDaily.color = GraphType == GraphTypes.Daily ? ColorChart.white : ColorChart.gray_700;
                txt_subTabWeekly.color = GraphType == GraphTypes.Weekly ? ColorChart.white : ColorChart.gray_700;
                txt_subTabMonthly.color = GraphType == GraphTypes.Monthly ? ColorChart.white : ColorChart.gray_700;

                // 판넬 바꿔 열고
                go_insightDaily?.SetActive(_graphType == GraphTypes.Daily);
                go_insightWeekly?.SetActive(_graphType == GraphTypes.Weekly);
                go_insightMonthly?.SetActive(_graphType == GraphTypes.Monthly);

                // 할 일 하기
                if (_graphType == GraphTypes.Daily)
                {
                    resetDailyStepsChart(health_vm.TodayStepCount, true);
                }
                else if (_graphType == GraphTypes.Weekly)
                {
                    resetWeekAverageChart();
                }
                else
                {
                    resetMonthChart();
                }
            });

            _bindingManager.makeBinding(this, "GoalCount", value =>
            {
                resetDailyStepsChart(health_vm.TodayStepCount);
            });

            _bindingManager.makeBinding(health_vm, nameof(health_vm.TodayStepCount), (obj) =>
            {
                txt_dailySteps_upper.text = StringCollection.getFormat("statistics.daily.steps.desc", 0, health_vm.TodayStepCount.ToString("N0"));
                resetDailyStepsChart(health_vm.TodayStepCount);
            });

            _bindingManager.makeBinding(health_vm, nameof(health_vm.CalorieCumulation), (obj) =>
            {
                resetTodayCaloriesFood((int)health_vm.CalorieCumulation.today_total);
                txt_accumCal.text = health_vm.CalorieCumulation.total.ToString("N0");
            });

            _bindingManager.makeBinding(health_vm, nameof(health_vm.DistanceCumulation), (obj) =>
            {
                int distance_unit = profile_vm.getSettingWithDefault(ClientAccountSetting.ConfigID.distance_unit, UnitDefine.DistanceType.km);

                if (distance_unit == UnitDefine.DistanceType.mil)
                {
                    txt_accumDis.text = health_vm.getTotalCumulatedDistance_mi().ToString("N0");
                    txt_accumDisDesc.text = StringCollection.get("statistics.accum.mi", 0);
                }
                else
                {
                    txt_accumDis.text = health_vm.DistanceCumulation.total.ToString("N0");
                    txt_accumDisDesc.text = StringCollection.get("statistics.accum.km", 0);
                }
            });

            _bindingManager.makeBinding(health_vm, nameof(health_vm.StepCumulation), (obj) =>
            {
                txt_accumSteps.text = health_vm.StepCumulation.total.ToString("N0");
            });

            // 단위가,, 바뀐다면??
            _bindingManager.makeBinding(profile_vm, nameof(profile_vm.SettingMap), (obj) =>
            {
                int distance_unit = profile_vm.getSettingWithDefault(ClientAccountSetting.ConfigID.distance_unit, UnitDefine.DistanceType.km);

                if (distance_unit == UnitDefine.DistanceType.mil)
                {
                    txt_accumDis.text = health_vm.getTotalCumulatedDistance_mi().ToString("N0");
                    txt_accumDisDesc.text = StringCollection.get("statistics.accum.mi", 0);
                }
                else
                {
                    txt_accumDis.text = health_vm.DistanceCumulation.total.ToString("N0");
                    txt_accumDisDesc.text = StringCollection.get("statistics.accum.km", 0);
                }
            });
        }

        #region main/sub tab button clicks
        public void onClickToday()
        {
            if (_quering)
            {
                return;
            }

            GraphType = GraphTypes.Daily;
        }

        public void onClickWeek()
        {
            if (_quering)
            {
                return;
            }

            GraphType = GraphTypes.Weekly;
        }

        public void onClickMonth()
        {
            if (_quering)
            {
                return;
            }

            GraphType = GraphTypes.Monthly;
        }

        public void onClickInsightTab()
        {
            if (_quering)
            {
                return;
            }

            TabType = TabTypes.Insight;
        }

        public void onClickHistoryTab()
        {
            if (_quering)
            {
                return;
            }

            TabType = TabTypes.History;
        }

        public void onClickLifeStatTab()
        {
            if (_quering)
            {
                return;
            }

            TabType = TabTypes.LifeStat;
        }
        #endregion

        #region insight

        #region server/data process
        private void loadTopRankFromServer()
        {
            GetStepTodayTotalRankProcessor step = GetStepTodayTotalRankProcessor.create();
            step.run(result => {
                if (result.succeeded())
                {
                    resetMevsAllChart();
                }
            });
        }

        private void loadFriendRankFromServer()
        {
            GetStepTodayFriendRankProcessor step = GetStepTodayFriendRankProcessor.create();
            step.run(result => {
                if (result.succeeded())
                {
                    resetMevsFriendsChart();
                }
            });
        }

		private void loadTripLogFromServer()
        {
            // 일단 한번만 해주자
            if( ViewModel.Trip.LastQueryLogTime != 0)
            {
                return;
            }

            QueryTripHistoryProcessor step = QueryTripHistoryProcessor.create( Network.getAccountID(), 0, 10);
            step.run(result => { 
                if( result.succeeded())
                {
                    ViewModel.Trip.LastQueryLogTime = TimeUtil.unixTimestampUtcNow();
                    ViewModel.Trip.AppendTripLog(step.getLogList());
                }
            });
        }

		private void calcQueryParam(out long begin, out long end, out int interval_type)
        {
            DateTime begin_time = DateTime.Now;
            DateTime end_time = DateTime.Now;
            interval_type = 1;

            if (_graphType == GraphTypes.Daily)
            {
                begin_time = TimeUtil.todayBeginUTC();
                end_time = DateTime.UtcNow;
                interval_type = 1;
            }
            else if (_graphType == GraphTypes.Weekly)
            {
                DateTime now = DateTime.Now;

                DateTime begin_date = new DateTime(now.Year, now.Month, now.Day);
                begin_date = begin_date.AddDays(-(int)now.DayOfWeek);

                begin_time = begin_date.ToUniversalTime();
                end_time = DateTime.UtcNow;
                interval_type = 2;
            }
            else if (_graphType == GraphTypes.Monthly)
            {
                DateTime now = DateTime.Now;

                DateTime begin_date = new DateTime(now.Year, now.Month, 1);

                begin_time = begin_date.ToUniversalTime();
                end_time = DateTime.UtcNow;
                interval_type = 2;
            }

            begin = TimeUtil.unixTimestampFromDateTime(begin_time);
            end = TimeUtil.unixTimestampFromDateTime(end_time);
        }

        // 서버에서는 빈칸을 주지 않는다
        private void postProcessData(List<ClientHealthLogData> list, long begin, long end, int interval_type)
        {
            long step = 0;
            if (interval_type == 1)
            {
                step = TimeUtil.msHour;
            }
            else
            {
                step = TimeUtil.msDay;
            }

            Dictionary<long, ClientHealthLogData> dataMap = new Dictionary<long, ClientHealthLogData>();
            foreach (ClientHealthLogData log in list)
            {
                dataMap.Add(TimeUtil.unixTimestampFromDateTime(log.record_time), log);
            }

            for (long time = begin; time <= end; time += step)
            {
                if (dataMap.ContainsKey(time) == false)
                {
                    ClientHealthLogData log = new ClientHealthLogData();
                    log.record_time = TimeUtil.dateTimeFromUnixTimestamp(time);
                    log.value = 0;
                    list.Add(log);
                }
            }

            list.Sort((a, b) => {
                if (a.record_time < b.record_time)
                {
                    return -1;
                }
                else if (a.record_time > b.record_time)
                {
                    return 1;
                }

                return 0;
            });
        }

        #endregion

        #region chart/board setup

        private void setChartType()
        {
            week_average_chart.ChartType = WeeklyBarChart.ChartTypes.weeklyAverage;
            me_vs_all_chart.ChartType = WeeklyBarChart.ChartTypes.meVsAll;
            me_vs_friends_chart.ChartType = WeeklyBarChart.ChartTypes.meVsFriends;
            daily_byTime_chart.ChartType = WeeklyBarChart.ChartTypes.dailyByTime;
        }

        private void resetMonthChart()
        {
            if (calendar_cell_parent == null)
            {
                return;
            }

//            // 이번달 1일부터
//            long queryBeginDay = (TimeUtil.unixTimestampFromDateTime(new DateTime(_currentMonth.Year, _currentMonth.Month, 1)) + TimeUtil.timezoneOffset()) / TimeUtil.msDay;
//            // 이번달 마지막 날까지 (다음달 1일 빼기 1)
//            long queryEndDay = (TimeUtil.unixTimestampFromDateTime(new DateTime(_currentMonth.AddMonths(1).Year, _currentMonth.AddMonths(1).Month, 1)) + TimeUtil.timezoneOffset()) / TimeUtil.msDay - 1;

//            Debug.Log($"query daily health : begin[{TimeUtil.dateFromDayCount(queryBeginDay)}] end[{TimeUtil.dateFromDayCount(queryEndDay)}]");

//            UIBlockingInput.getInstance().open();
//            QueryDailyHealthLogProcessor step = QueryDailyHealthLogProcessor.create(HealthDataType.step, queryBeginDay, queryEndDay);
//            step.run(result => {
//                UIBlockingInput.getInstance().close();

//                if (result.failed())
//                {
//                    return;
//                }

//                DateTime today = DateTime.Now;
//                DateTime monthBeginDay = TimeUtil.dateFromDayCount(queryBeginDay);

//                long uiBeginDay = queryBeginDay - (int)monthBeginDay.DayOfWeek;
//                int weekCount = Mathf.CeilToInt((float)(queryEndDay - uiBeginDay + 1) / 7.0f);
//                long uiEndDay = uiBeginDay + weekCount * 7 - 1;

//                if (uiEndDay - uiBeginDay + 1 < calendar_cell_parent.childCount)
//                    uiEndDay = uiBeginDay + calendar_cell_parent.childCount - 1;
//                Debug.Log($"ui : begin[{TimeUtil.dateFromDayCount(uiBeginDay)}] end[{TimeUtil.dateFromDayCount(uiEndDay)}]");

//                int successCount = 0;
//                int cellCount = 0;
//                for (long day = uiBeginDay; day <= uiEndDay; day++)
//                {
//                    int x = (int)(day - uiBeginDay) % 7;
//                    int y = (int)(day - uiBeginDay) / 7;
//                    int dayOfMonth = (int)(day - queryBeginDay + 1);
//                    bool dayIsInThisMonth = day >= queryBeginDay && day <= queryEndDay;

//                    ClientHealthLogDaily log = step.getLog(day);

//                    // 있는 셀은 재사용할 거야~
//                    RectTransform rect = null;
//                    if (calendar_cell_parent.childCount > cellCount)
//                        rect = calendar_cell_parent.GetChild(cellCount).GetComponent<RectTransform>();
//                    else
//                        rect = Instantiate(calendar_cell_prefab, calendar_cell_parent).GetComponent<RectTransform>();

//                    ++cellCount;

//                    // 만들어보자,, 크기 30, 여백 20... 피봇은 좌상단!
//                    rect.anchoredPosition = new Vector2(22 + 50 * x, -42 * y);

//                    var cell = rect.GetComponent<UIMonthCalendarCell>();
//                    if (cell != null)
//                    {
//                        if (dayIsInThisMonth == false)
//                        {
//                            // 이달에 없는 날짜
//                            if(dayOfMonth <= 0)
//                            {
//                                // 지난 달이네
//                                DateTime lastMonth = today.AddMonths(-1);
//                                int lastMonthDays = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
//                                cell.Initialize(dayOfMonth + lastMonthDays, null, true, font_regular);
//                            }
//                            else
//                            {
//                                // 다음 달이네
//                                int thisMonthDays = DateTime.DaysInMonth(today.Year, today.Month);
//                                cell.Initialize(dayOfMonth - thisMonthDays, null, true, font_regular);
//                            }
//                        }
//                        else
//                        {
//                            if (dayOfMonth > today.Day && _currentMonth.Month == today.Month)
//                            {
//                                // 있지만 기록되지 않은 날짜 (오늘 이후)
//                                cell.Initialize(dayOfMonth, null, false, font_regular);
//                            }
//                            else
//                            {
//                                int value = 0;
//                                float rate = 0;
//                                if (log != null)
//                                {
//                                    value = (int)log.value;
//                                    rate = log.getAchivementRate();
//                                }

//                                int dayState;
//                                if (rate > 0.7f)
//                                {
//                                    dayState = UIMonthCalendarCell.MonthlyCellState.high;
//                                }
//                                else if (rate > 0.3f)
//                                {
//                                    dayState = UIMonthCalendarCell.MonthlyCellState.medium;
//                                }
//                                else
//                                {
//                                    dayState = UIMonthCalendarCell.MonthlyCellState.low;
//                                }

//                                if (rate >= 1.0f)
//                                {
//                                    successCount++;
//                                }

//                                cell.Initialize(dayOfMonth, dayState, !dayIsInThisMonth, dayOfMonth == today.Day ? font_bold : font_regular);
//                            }
//                        }
//                    }
//                }

///*                // 이럴수가! 마지막 줄에도 날짜가 기록되었다면?? (캘린더가 5줄보다 길다면 == 6줄이라면)
//                if (calendar_cell_parent.childCount > 35 && calendar_cell_parent.GetChild(35).GetComponent<UIMonthCalendarCell>().hasDate())
//                    // 영역을 한 칸 늘려요
//                    go_insightMonthly.GetComponent<RectTransform>().sizeDelta = new Vector2(375f, 401f + 42f);
//                else
//                    go_insightMonthly.GetComponent<RectTransform>().sizeDelta = new Vector2(375f, 401f);*/

//                // 텍스트 세팅
//                txt_monthSwitch.text = StringCollection.get("calendar.month.unit", _currentMonth.Month - 1);
//                txt_monthlySteps_upper.text = StringCollection.getFormat("statistics.monthly.steps.desc", 0, StringCollection.get("calendar.month.unit", _currentMonth.Month - 1), successCount.ToString());

//                // 여우 채우기
//                float fillAmount = (float)successCount / (float)DateTime.DaysInMonth(today.Year, today.Month);
//                setMontlyFox(fillAmount);

///*                // 이래야 높이 세팅이 반영되네 실화야,,??
//                if (_graphType == GraphTypes.Monthly)
//                {
//                    go_insightMonthly.SetActive(false);
//                    go_insightMonthly.SetActive(true);
//                }*/
//            });
        }

        private void setMontlyFox(float fillAmount)
        {
            img_foxFill.fillAmount = fillAmount;

            if (fillAmount >= 1f)
            {
                img_foxFill.gameObject.SetActive(false);
                go_foxColor.SetActive(true);
            }
            else
            {
                img_foxFill.gameObject.SetActive(true);
                go_foxColor.SetActive(false);

                if (fillAmount > 0.7f)
                    img_foxFill.color = ColorChart.gray_750;
                else if (fillAmount > 0.5f)
                    img_foxFill.color = ColorChart.gray_400;
                else
                    img_foxFill.color = ColorChart.gray_250;
            }
        }

        private int getCalorieStepLevel(int calorie)
        {
            int lastLevel = 0; // 최대 범위를 넘어선경우..
            var calorieDic = GlobalRefDataContainer.getInstance().getMap<RefCaloriesStep>();
            foreach (var pair in calorieDic)
            {
                var step = pair.Value as RefCaloriesStep;
                lastLevel = step.level;
                if (step.daily_min_calorie <= calorie && calorie <= step.daily_max_calorie)
                {
                    return step.level;
                }
            }

            // 그냥 마지막껄로..
            return lastLevel;
        }

        private void resetTodayCaloriesFood(int calories)
        {
            int level = getCalorieStepLevel(calories);

            txt_caloriesFood_upper.text = StringCollection.getFormat("statistics.today.calorie.description", 0, StringCollection.get("RefCaloriesStep.foodName", level));
            txt_caloriesFood_cal.text = $"{StringCollection.get("RefCaloriesStep.foodCal", level)}kcal";
        }

        private void resetDailyStepsChart(int todayStepCount, bool updateChartAnim = false)
        {
            if (GoalCount > 0)
                _fillRatio = (float)todayStepCount / (float)GoalCount;
            else
                _fillRatio = 0;

            _fillRatio = Mathf.Clamp(_fillRatio, 0f, 1f);

            if (_fillRatio == 1f)
            {
                if (!dailyFoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("idle") && dailyFoxAnimator.gameObject.activeInHierarchy)
                    dailyFoxAnimator.Play("idle", -1, 1f);

                txt_dailySteps_big.text = $"{todayStepCount.ToString("N0")}!";
                txt_dailySteps_big.color = ColorChart.white;
                go_dailyStepGaugeFull.SetActive(true);
                img_dailyStepGauge.gameObject.SetActive(false);
            }
            else
            {
                if (!dailyFoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("walk") && dailyFoxAnimator.gameObject.activeInHierarchy)
                    dailyFoxAnimator.Play("walk", -1, 1f);

                txt_dailySteps_big.text = todayStepCount.ToString("N0");
                txt_dailySteps_big.color = ColorChart.primary_300;
                go_dailyStepGaugeFull.SetActive(false);
                img_dailyStepGauge.gameObject.SetActive(true);

                if (updateChartAnim)
                    img_dailyStepGauge.fillAmount = 0f;

                _updateDailyChartAnimation = true;
            }

            txt_goalSteps.text = GoalCount.ToString("N0");
        }

        private void resetTodayTopTodayYestChart()
        {
            MapPacket req = Network.createReq(CSMessageID.HealthData.QueryStatisticsReq);
            req.put("id", Network.getAccountID());
            req.put("timezone_offset", TimeUtil.timezoneOffset());
            req.put("health_data_type", HealthDataType.step);

            long begin_time;
            long end_time;
            int interval_type;

            calcQueryParam(out begin_time, out end_time, out interval_type);

            Debug.Log($"UTCNow {DateTime.UtcNow} {end_time} {begin_time}");
            req.put("begin", begin_time);
            req.put("end", end_time);
            req.put("type", interval_type);

            UIBlockingInput.getInstance().open();
            _quering = true;

            Network.call(req, ack => {
                _quering = false;

                if (ack.getResult() == ResultCode.ok)
                {
                    // 오늘 시간별 차트~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    List<ClientHealthLogData> list = ack.getList<ClientHealthLogData>("data");
                    postProcessData(list, begin_time, end_time, interval_type);

                    int max_value = 0;
                    int max_index = 0;

                    var todayDiffData = daily_byTime_chart.GetChartData();
                    var dataset = todayDiffData.DataSets[0];
                    for (int i = 0; i < dataset.Entries.Count; ++i)
                    {
                        if (i < list.Count)
                        {
                            var value = list[i].value;
                            dataset.Entries[i].Value = value;
                            if (value > max_value)
                            {
                                max_value = value;
                                max_index = i;
                            }
                        }
                        else
                        {
                            dataset.Entries[i].Value = 0;
                        }
                    }
                    daily_byTime_chart.setDataMaxValue(max_value);
                    daily_byTime_chart.SetDirty();

                    var sc = GlobalRefDataContainer.getStringCollection();
                    txt_dailyByTime_upper.text = sc.getFormat("statistics.realtime.dailyTop", 0, max_index < 12 ? sc.get("am", 0) : sc.get("pm", 0), max_index >= 13 ? max_index - 12 : max_index);

                    // 오늘 vs 어제 차트 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    // 어제껄 얻어와보자.
                    MapPacket req = Network.createReq(CSMessageID.HealthData.QueryStatisticsReq);
                    req.put("id", Network.getAccountID());
                    req.put("timezone_offset", TimeUtil.timezoneOffset());
                    req.put("health_data_type", HealthDataType.step);

                    interval_type = 1;

                    begin_time = TimeUtil.unixTimestampFromDateTime(TimeUtil.todayBeginUTC().AddDays(-1));
                    end_time = TimeUtil.unixTimestampFromDateTime(TimeUtil.todayBeginUTC());

                    req.put("begin", begin_time);
                    req.put("end", end_time);
                    req.put("type", interval_type);

                    _quering = true;

                    Network.call(req, ack =>
                    {
                        UIBlockingInput.getInstance().close();
                        _quering = false;

                        if (ack.getResult() == ResultCode.ok)
                        {
                            List<ClientHealthLogData> yesterdayList = ack.getList<ClientHealthLogData>("data");

                            postProcessData(yesterdayList, begin_time, end_time, interval_type);

                            var yesterdayDiffData = today_vs_yest_chart.GetChartData();

                            var todayDataSet = yesterdayDiffData.DataSets[1];
                            todayDataSet.Entries.Clear();
                            int todayWalk = 0;
                            for (int i = 0; i < list.Count; ++i)
                            {
                                LineEntry entry = new LineEntry(i, list[i].value);
                                todayDataSet.Entries.Add(entry);
                                todayWalk = Mathf.Max(list[i].value, 0);
                            }
                            var yesterdayDataSet = yesterdayDiffData.DataSets[0];
                            yesterdayDataSet.Entries.Clear();
                            for (int i = 0; i < yesterdayList.Count; ++i)
                            {
                                LineEntry entry = new LineEntry(i, yesterdayList[i].value);
                                yesterdayDataSet.Entries.Add(entry);
                            }

                            int diffWalk = (int)(todayDataSet.Entries[todayDataSet.Entries.Count - 1].Value - yesterdayDataSet.Entries[todayDataSet.Entries.Count - 1].Value);
                            txt_dailyUpDownSteps.text = Mathf.Abs(diffWalk).ToString("N0");
                            img_dailyUpDownArrow.sprite = upDownArrows[diffWalk > 0 ? 0 : 1];

                            txt_todayVsYest_upper.text = GlobalRefDataContainer.getStringCollection().getFormat(diffWalk > 0 ? "statistics.realtime.todayVsYesterday.more" : "statistics.realtime.todayVsYesterday.less", 0, Mathf.Abs(diffWalk).ToString("N0"));
                            today_vs_yest_chart.SetValueFocusIndex(Mathf.Max(0, todayDataSet.Entries.Count - 1));
                            today_vs_yest_chart.SetDirty();
                        }
                    });

                }
                else
                {
                    UIBlockingInput.getInstance().close();
                }
            });
        }

        private void resetWeekAverageChart()
        {
            if (_graphType != GraphTypes.Weekly)
                return;

            //// 현재주의 시작일
            //long currentWeekBeginDay = (_currentWeek_beginTime + TimeUtil.timezoneOffset()) / TimeUtil.msDay;
            //long todayDayCount = TimeUtil.todayDayCount();

            //long lastWeekBeginDay = currentWeekBeginDay - 7;
            //long lastWeekEndDay = currentWeekBeginDay - 1;

            //long queryBeginDay = lastWeekBeginDay;
            //long queryEndDay;

            //if (_currentWeek_beginTime < _thisWeek_beginTime)
            //{
            //    queryEndDay = currentWeekBeginDay + 6;
            //}
            //else
            //{
            //    queryEndDay = TimeUtil.todayDayCount();
            //}

            //Debug.Log($"query daily health : begin[{TimeUtil.dateFromDayCount(queryBeginDay)}] end[{TimeUtil.dateFromDayCount(queryEndDay)}]");

            //UIBlockingInput.getInstance().open();
            //QueryDailyHealthLogProcessor step = QueryDailyHealthLogProcessor.create(HealthDataType.step, queryBeginDay, queryEndDay);
            //step.run(result =>
            //{
            //    UIBlockingInput.getInstance().close();

            //    if (result.failed())
            //    {
            //        return;
            //    }

            //    week_average_chart.setDataMaxValue(GoalCount);
            //    var weekAverageData = week_average_chart.GetChartData();
            //    var dataset = weekAverageData.DataSets[0];

            //    // 지난주 이번주 평균 구하기
            //    int lastWeekAverage = step.calcAverage(lastWeekBeginDay, lastWeekEndDay);
            //    int currentWeekAverage = step.calcAverage(currentWeekBeginDay, queryEndDay);

            //    // 이번주 데이터 그래프에 적용
            //    for (int i = 0; i < dataset.Entries.Count; ++i)
            //    {
            //        var entry = dataset.Entries[i];

            //        ClientHealthLogDaily log = step.getLog(currentWeekBeginDay + i);

            //        entry.IsToday = (currentWeekBeginDay + i) == todayDayCount;
            //        entry.Value = log != null ? log.value : 0;
            //    }
            //    week_average_chart.SetDirty();

            //    // 평균선 그리기
            //    txt_weeklyAverageValue.text = currentWeekAverage.ToString("N0");
            //    rect_weeklyAverageLabel.sizeDelta = new Vector2(txt_weeklyAverageValue.preferredWidth + 31.5f, 41f);
            //    rect_weeklyAverageLine.anchoredPosition = new Vector2(0f, Mathf.Clamp(currentWeekAverage, 0f, GoalCount) * (160f / GoalCount) + 3f);

            //    // 문자열 세팅
            //    // 요기 스트링 먹이기 냠!!~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //    int weekPassed = (int)((_thisWeek_beginTime - _currentWeek_beginTime) / (1000 * 60 * 60 * 24 * 7));
            //    string weekPassedText = weekPassed.ToString();
            //    if (weekPassed == 0 || weekPassed == 1)
            //    {
            //        txt_weeklySteps_upper.text = StringCollection.getFormat("statistics.weekly.steps.desc", 0, StringCollection.get("calendar.week.unit", weekPassed), currentWeekAverage.ToString("N0"));
            //    }
            //    else
            //    {
            //        txt_weeklySteps_upper.text = StringCollection.getFormat("statistics.weekly.steps.desc", 1, weekPassed, currentWeekAverage.ToString("N0"));
            //    }

            //    DateTime beginDate = TimeUtil.dateTimeFromUnixTimestamp(_currentWeek_beginTime).ToLocalTime();
            //    txt_weekSwitch.text = $"{beginDate.Month.ToString("D2")}.{beginDate.Day.ToString("D2")} - {beginDate.AddDays(6).Month.ToString("D2")}.{beginDate.AddDays(6).Day.ToString("D2")}";

            //    if (lastWeekAverage <= currentWeekAverage)
            //    {
            //        // 이번주에 더 많이 걸었다네
            //        img_weeklyUpDownArrow.sprite = upDownArrows[0];
            //        txt_weeklyUpDownSteps.text = $"{(currentWeekAverage - lastWeekAverage).ToString("N0")}";
            //    }
            //    else
            //    {
            //        // 지난주에 더 많이 걸었다네
            //        img_weeklyUpDownArrow.sprite = upDownArrows[0];
            //        txt_weeklyUpDownSteps.text = $"{(lastWeekAverage - currentWeekAverage).ToString("N0")}";
            //    }
            //});
        }

        private void resetMevsAllChart()
        {
            HealthTodayTotalRankViewModel vm = ViewModel.Health.TodayStepTotalRank;

            // 데이터!
            float me = ViewModel.Health.TodayStepCount; // 나의 오늘의 걸음 수
            float allAv = vm.Average;                   // 전체 유저의 오늘의 걸음 수 평균
            float percentage = vm.RankRatio * 100.0f;   // 전체 유저대비 나의 걸음수 랭킹 비율 (낮을 수록 상위권)

            // 보여주기!
            var mevsAllData = me_vs_all_chart.GetChartData();
            var dataset = mevsAllData.DataSets[0];
            dataset.Entries[0].Value = allAv;
            dataset.Entries[1].Value = me;

            meVsAllValues[0].text = allAv.ToString("N0");
            meVsAllName[0].text = StringCollection.get("statistics.realtime.allMean", 0);
            meVsAllValues[1].text = me.ToString("N0");
            meVsAllName[1].text = ViewModel.Profile.Profile.name;

            if (me >= allAv)
            {
                me_vs_all_chart.setDataMaxValue(me);
            }
            else
            {
                me_vs_all_chart.setDataMaxValue(allAv);
            }
            me_vs_all_chart.SetDirty();

            txt_meVsAll_upper.text = StringCollection.getFormat("statistics.realtime.meVsAll", 0, percentage.ToString("N0"));
        }

        private void resetMevsFriendsChart()
        {
            HealthTodayFriendRankViewModel vm = ViewModel.Health.TodayStepFriendRank;

            int myIndex = vm.MyIndex;
            List<ClientFriendHealthData> rankList = vm.FriendList;

            // 아직 데이터 로딩전
            if (myIndex == -1 || rankList == null)
            {
                return;
            }

            var mevsFriendsData = me_vs_friends_chart.GetChartData();
            var dataset = mevsFriendsData.DataSets[0];
            me_vs_friends_chart.setDataMaxValue(vm.MaxValue);

            // 4개만 표시?
            if (rankList.Count <= 1)
            {
                // 친구 없음ㅜㅡㅜ
                go_noFriends.SetActive(true);
                go_meVsFriendsChart.SetActive(false);

                txt_meVsFriends_upper.text = StringCollection.get("statistics.noFriends.desc", 0);

                go_realtimePanels[RealtimeGroupID.MeVsFriends].GetComponent<RectTransform>().sizeDelta = new Vector2(375f, 359f);
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    ClientFriendHealthData data = null;
                    if (i < rankList.Count)
                    {
                        data = rankList[i];
                    }

                    if (data != null)
                    {
                        // 만들어진 애들은 재활용~!
                        BarEntry entry = null;
                        if (i < dataset.Entries.Count)
                        {
                            entry = dataset.Entries[i];
                        }
                        else
                        {
                            entry = new BarEntry();
                            dataset.AddEntry(entry);
                        }

                        entry.Position = i;
                        entry.Value = data.value;
                        entry.IsMe = data.account_id == Network.getAccountID();

                        if (data._profile != null)
                        {
                            entry.ThumbnailUrl = data._profile.Profile.getPicktureURL(GlobalConfig.fileserver_url);

                            meVsFriendsValues[i].text = data.value.ToString("N0");
                            meVsFriendsName[i].text = data._profile.Profile.name;
                        }
                        else
                        {
                            entry.ThumbnailUrl = "";

                            meVsFriendsValues[i].text = "";
                            meVsFriendsName[i].text = "";
                        }

                        meVsFriendsName[i].font = entry.IsMe ? font_bold : font_regular;
                    }
                    else
                    {
                        // 없는 친구
                        if (i < dataset.Entries.Count)
                        {
                            // 우선 포지션을 리스트 인덱스에 맞춰줘야해,, 헷갈리네요
                            dataset.Entries[i].Position = i;
                            dataset.RemoveEntry(i);
                        }
                        meVsFriendsValues[i].text = "";
                        meVsFriendsName[i].text = "";
                    }
                }
                // 친구 명수에 따라 차트를 조절할 거야
                rect_meVsFriendsChart.sizeDelta = new Vector2((40 + 48) * dataset.Entries.Count + 40, me_vs_friends_chart.AxisConfig.VerticalAxisConfig.Bounds.Max + 44);
                rect_meVsFriendsLabel.sizeDelta = new Vector2(24 + 88 * dataset.Entries.Count, 46);

                go_noFriends.SetActive(false);
                go_meVsFriendsChart.SetActive(true);

                me_vs_friends_chart.SetDirty();

                go_meVsFriends_dotdotdot.SetActive(myIndex > 4);
                txt_meVsFriends_upper.text = StringCollection.getFormat("statistics.realtime.meVsFriends", 0, myIndex + 1);

                // 높이를 조절할 거야
                float chartHeight = me_vs_friends_chart.AxisConfig.VerticalAxisConfig.Bounds.Max + (dataset.Entries[0].IsMe ? 0 : 44f);
                go_realtimePanels[RealtimeGroupID.MeVsFriends].GetComponent<RectTransform>().sizeDelta = new Vector2(375f, chartHeight + 162f + 60f);   // 60 은 마진
            }

            go_realtimePanels[RealtimeGroupID.MeVsFriends].SetActive(false);
            go_realtimePanels[RealtimeGroupID.MeVsFriends].SetActive(true);
        }

        #endregion

        public void onClickWeekAveragePrev()
        {
            CurrentWeekBeginTime -= (1000 * 60 * 60 * 24 * 7);
        }

        public void onClickWeekAverageNext()
        {
            if (CurrentWeekBeginTime < _thisWeek_beginTime)
                CurrentWeekBeginTime += (1000 * 60 * 60 * 24 * 7);
        }

        public void onClickMonthChartPrev()
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            CurrentMonthInt = _currentMonth.Month;
        }

        public void onClickMonthChartNext()
        {
            if (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1) > new DateTime(_currentMonth.Year, _currentMonth.Month, 1))
            {
                _currentMonth = _currentMonth.AddMonths(1);
                CurrentMonthInt = _currentMonth.Month;
            }
        }

        #endregion

        #region explore

        private void resetExploreScrollData()
        {
            exploreScrollDelegate.setupData();
            exploreScrollDelegate.loadList(_listType);
        }

        public void changeListType(int type)
        {
            _listType = type;
        }

        #endregion

        #region life stat

        private void drawStatGraph()
        {
            // nullity check!!!!!!!!!!!!!!!!!
            var exp = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.STR);
            var fit = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.EXC);
            var emo = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.EMO);
            var iint = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.INT);
            var ins = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.ENT);
            var soc = ClientMain.instance.getViewModel().Stature.getStat(ClientAccountStat.StatType.REL);

            lifeStatGraphRenderer.setLevel(exp.level, fit.level, emo.level, iint.level, ins.level, soc.level);
            lifeStatGraphRenderer.setScore(exp.exp, fit.exp, emo.exp, iint.exp, ins.exp, soc.exp);

            // 테스트용
            //lifeStatGraphRenderer.setScore(1, 2, 4, 10, 6, 14);
            lifeStatGraphRenderer.draw();
        }

        #endregion

        private void Update()
        {
            if (_updateDailyChartAnimation)
            {
                img_dailyStepGauge.fillAmount += dailyChartAnimationSpeed * Time.deltaTime;

                if (img_dailyStepGauge.fillAmount >= _fillRatio)
                {
                    img_dailyStepGauge.fillAmount = _fillRatio;
                    _updateDailyChartAnimation = false;
                }
            }
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable");
        }
    }
}
