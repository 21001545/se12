using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AwesomeCharts {

    [ExecuteInEditMode]
    public class WeeklyBarChart : AxisBaseChart<BarData> {

        private int _chartType;
        public int ChartType
        {
            set { _chartType = value; }
        }

        public class ChartTypes
        {
            public static int weeklyAverage = 0;
            public static int meVsAll = 1;
            public static int meVsFriends = 2;
            public static int dailyByTime = 3;
        }

        [SerializeField]
        private BarChartAxisConfig axisConfig;
        [SerializeField]
        private BarChartConfig config;
        [SerializeField]
        internal BarData data;

        [SerializeField]
        private bool _hasProfile;

        [SerializeField]
        internal bool playAnimationInVisible = true;
        private bool updateAnimation = false;
        private bool visibleOnce = false; // 한번이라도 보여진적이 있나?
        private float _dataMaxValue;    // 데이터가 가질 수 있는 최대값 - 이 값을 넘어가면 클램핑

        [SerializeField]
        internal float animationSpeed = 100.0f;

        [SerializeField]
        internal bool showMaxValueLine = false;

        [SerializeField]
        private AxisLabel maxValueTextLabelPrefab;

        private BarCharPositioner positioner;
        private ChartValuePopupAbstract currentValuePopup = null;
        private BarEntry currentValuePopupEntry = null;
        private VerticalAxisLabelEntryProvider verticalLabelsProvider;
        private BarAxisLabelEntryProvider horizontalLabelsProvider;
        private BasicAxisValueFormatter verticalAxisValueFormatter;
        private BarAxisValueFormatter horizontalAxisValueFormatter;
        private AxisValueFormatter customVerticalAxisValueFormatter;
        private AxisValueFormatter customHorizontalAxisValueFormatter;

        private UIChartMaxValueLineRenderer maxValueLineRenderer;
        //private UIChartAverageLineRenderer averageLineRenderer;
        private List<Bar> barInstances;

        public void setDataMaxValue(float val)
        {
            _dataMaxValue = val;
        }

        
        public BarChartConfig Config {
            get { return config; }
            set {
                config = value;
                config.configChangeListener = OnConfigChanged;
                SetDirty ();
            }
        }

        public BarChartAxisConfig AxisConfig {
            get { return axisConfig; }
            set {
                axisConfig = value;
                SetDirty ();
            }
        }

        public AxisValueFormatter CustomVerticalAxisValueFormatter {
            get { return customVerticalAxisValueFormatter; }
            set {
                customVerticalAxisValueFormatter = value;
                SetDirty ();
            }
        }

        public AxisValueFormatter CustomHorizontalAxisValueFormatter {
            get { return customHorizontalAxisValueFormatter; }
            set {
                customHorizontalAxisValueFormatter = value;
                SetDirty ();
            }
        }

        public override BarData GetChartData () {
            return data;
        }

        void Reset () {
            data = new BarData (new BarDataSet ());
            Config = new BarChartConfig ();
        }

        protected override void Awake () {
            base.Awake ();

            if (data == null) {
                data = new BarData (new BarDataSet ());
            }
            if (config == null) {
                Config = new BarChartConfig ();
            }
            if (axisConfig == null) {
                axisConfig = new BarChartAxisConfig ();
            }

            positioner = new BarCharPositioner ();
            barInstances = new List<Bar> ();
            verticalLabelsProvider = new VerticalAxisLabelEntryProvider ();
            horizontalLabelsProvider = new BarAxisLabelEntryProvider ();
            verticalAxisValueFormatter = new BasicAxisValueFormatter ();
            horizontalAxisValueFormatter = new BarAxisValueFormatter ();
        }

        protected override void Start()
        {
            base.Start();

            // Content 다음에 생성 되어야 함.
            var newMaxValueLineRenderer = viewCreator.CreateBaseGameObject("MaxValueLineRenderer", transform, PivotValue.BOTTOM_LEFT);
            newMaxValueLineRenderer.GetComponent<RectTransform>().sizeDelta = GetSize();
            maxValueLineRenderer = newMaxValueLineRenderer.AddComponent<UIChartMaxValueLineRenderer>();

/*            var newAverageLineRenderer = viewCreator.CreateBaseGameObject("AverageLineRenderer", transform, PivotValue.BOTTOM_LEFT);
            newAverageLineRenderer.GetComponent<RectTransform>().sizeDelta = GetSize();
            averageLineRenderer = newAverageLineRenderer.AddComponent<UIChartAverageLineRenderer>();*/
        }

        private void OnConfigChanged () {
            SetDirty ();
        }

        protected override void OnInstantiateViews () {
            base.OnInstantiateViews ();
            chartDataContainerView.AddComponent<RectMask2D> ();
        }

        protected override AxisLabelEntryProvider GetVerticalAxisEntriesProvider () {
            return verticalLabelsProvider;
        }

        protected override AxisLabelEntryProvider GetHorizontalAxisEntriesProvider () {
            return horizontalLabelsProvider;
        }

        protected override SingleAxisConfig GetVerticalAxisConfig () {
            return axisConfig.VerticalAxisConfig;
        }

        protected override SingleAxisConfig GetHorizontalAxisConfig () {
            return axisConfig.HorizontalAxisConfig;
        }

        private AxisValueFormatter GetCorrectVerticalAxisValueFormatter () {
            return CustomVerticalAxisValueFormatter != null? CustomVerticalAxisValueFormatter : verticalAxisValueFormatter;
        }

        private AxisValueFormatter GetCorrectHorizontalAxisValueFormatter () {
            return CustomHorizontalAxisValueFormatter != null? CustomHorizontalAxisValueFormatter : horizontalAxisValueFormatter;
        }

        protected override void OnUpdateAxis () {
            base.OnUpdateAxis ();
            UpdateBarChartPositioner ();
            UpdateHorizontalAxisEntriesProvider ();
            UpdateVerticalAxisEntriesProvider ();
        }

        private void UpdateBarChartPositioner () {
            positioner.data = data;
            positioner.barChartConfig = Config;
            positioner.containerSize = GetSize ();
            positioner.axisBounds = GetAxisBounds ();
            positioner.RecalculatePositioner ();
        }

        private void UpdateVerticalAxisEntriesProvider () {
            verticalAxisValueFormatter.config = AxisConfig.VerticalAxisConfig.ValueFormatterConfig;

            AxisBounds axisBounds = GetAxisBounds ();
            verticalLabelsProvider.valueMin = axisBounds.YMin;
            verticalLabelsProvider.valueMax = axisBounds.YMax;
            verticalLabelsProvider.labelCount = AxisConfig.VerticalAxisConfig.LabelsCount;
            verticalLabelsProvider.firstEntryVisible = AxisConfig.VerticalAxisConfig.DrawStartValue;
            verticalLabelsProvider.lastEntryVisible = AxisConfig.VerticalAxisConfig.DrawEndValue;
            verticalLabelsProvider.labelsGravity = AxisConfig.VerticalAxisConfig.LabelsAlignment;
            verticalLabelsProvider.valueFormatter = GetCorrectVerticalAxisValueFormatter ();
            verticalLabelsProvider.axisLength = verticalAxisLabelRenderer.GetComponent<RectTransform> ().sizeDelta.y;
        }

        private void UpdateHorizontalAxisEntriesProvider () {
            horizontalAxisValueFormatter.config = AxisConfig.HorizontalAxisConfig.ValueFormatterConfig;

            horizontalLabelsProvider.barChartPositioner = positioner;
            horizontalLabelsProvider.valueFormatter = GetCorrectHorizontalAxisValueFormatter ();
            horizontalLabelsProvider.labelsGravity = AxisConfig.HorizontalAxisConfig.LabelsAlignment;
        }

        protected override List<LegendEntry> CreateLegendViewEntries () {
            List<LegendEntry> result = new List<LegendEntry> ();
            foreach (BarDataSet dataSet in data.DataSets) {
                result.Add (new LegendEntry (dataSet.Title,
                    dataSet.GetColorForIndex (0)));
            }

            return result;
        }

        protected override void OnDrawChartContent () {
            base.OnDrawChartContent ();

            HideCurrentValuePopup ();
            UpdateBarInstances (positioner.GetAllVisibleEntriesCount ());
            if (GetChartData ().HasAnyData ()) {
                ShowBars ();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (Application.isPlaying == false)
                return;

            // 애니메이션을 진행 해볼까요~
            if (updateAnimation)
            {
                if (visibleOnce == false )
                {
                    // 이거 일일이 여기서 검사하는건 좀 별로 같은데.. 처음 한번만 캐싱해두는걸로 변경해보자.
                    var mask = gameObject.GetComponentInParent<Mask>();
                    if ( mask != null )
                    {
                        Vector3[] corners = new Vector3[4];
                        mask.GetComponent<RectTransform>().GetWorldCorners(corners);

                        float y = corners[0].y;
                        GetComponent<RectTransform>().GetWorldCorners(corners);
                        // 1 2
                        // 0 3
                        // y만 확인 해보자.
                        if ( corners[1].y >= y)
                        {
                            Debug.Log("Visible!");
                            visibleOnce = true;
                        }
                    }
                    else
                    {
                        // 없으면 canvas 기준으로 하고 싶은데..
                    }
                    return;
                }
                List<BarEntry> barEntries = positioner.GetVisibleEntries(0);

                bool animateFinish = true;
                for (int i = 0; i < barEntries.Count; i++)
                {
                    var size = positioner.GetBarSize(barEntries[i].Value, _dataMaxValue);

                    var rect = barInstances[i].GetComponent<RectTransform>();
                    if (rect.sizeDelta.y < size.y)
                    {
                        float y = Mathf.Min(size.y, rect.sizeDelta.y + animationSpeed * Time.deltaTime);

                        animateFinish &= (y >= size.y);

                        size.y = y;
                        rect.sizeDelta = size;

                        if (y < animationSpeed * 0.1f)
                        {
                            animateFinish = false;
                            break;
                        }
                    }
                }

                updateAnimation = !animateFinish;
                if ( updateAnimation == false )
                {
                    Debug.Log("Finish Animation!");
                    if (showMaxValueLine)
                        maxValueLineRenderer?.gameObject.SetActive(true);
                }
            }
        }

        private void UpdateBarInstances (int requiredCount) {
            int currentBarsCount = barInstances.Count;

            // Add missing bars
            int missingBarsCount = requiredCount - currentBarsCount;
            while (missingBarsCount > 0) {
                Bar barInstance = viewCreator.InstantiateBar (chartDataContainerView.transform, Config.BarPrefab);
                barInstances.Add (barInstance);
                missingBarsCount--;
            }

            // Remove redundant bars
            int redundantBarsCount = currentBarsCount - requiredCount;
            while (redundantBarsCount > 0) {
                Bar target = barInstances[barInstances.Count - 1];
                DestroyDelayed (target.gameObject);
                barInstances.Remove (target);
                redundantBarsCount--;
            }
        }

        private void ShowBars () {
            int nextBarInstanceIndex = 0;

            for (int i = 0; i < data.DataSets.Count; i++) {
                nextBarInstanceIndex = UpdatedBars (i, nextBarInstanceIndex);
            }

            updateAnimation = true;
            Debug.Log("Update Animation!");
        }

        private int UpdatedBars (int dataSetIndex, int nextBarInstanceIndex) {
            List<BarEntry> barEntries = positioner.GetVisibleEntries (dataSetIndex);

            float maximumValue = int.MinValue;
            int maxIndex = int.MinValue;
            for (int i = 0; i < barEntries.Count; i++)
            {
                if (maximumValue < barEntries[i].Value)
                {
                    maximumValue = barEntries[i].Value;
                    maxIndex = i;
                }
            }

            if (_chartType == ChartTypes.weeklyAverage)
            {
                for (int i = 0; i < barEntries.Count; i++)
                {
                    Color barColor = ColorChart.primary_300;
                    if (!barEntries[i].IsToday)
                        barColor = barEntries[i].Value >= _dataMaxValue ? ColorChart.gray_400 : ColorChart.gray_250;

                    UpdateBarWithEntry(barInstances[nextBarInstanceIndex],
                        barEntries[i],
                        barColor,
                        dataSetIndex);

                    if (barEntries[nextBarInstanceIndex].IsToday && barInstances[nextBarInstanceIndex].button != null)
                        OnBarClick(barEntries[i], dataSetIndex);

                    if (barInstances[nextBarInstanceIndex].go_checkMark != null)
                    {
                        barInstances[nextBarInstanceIndex].go_checkMark.SetActive(barEntries[nextBarInstanceIndex].Value >= _dataMaxValue);
                    }

                    nextBarInstanceIndex++;
                }
            }
            else if (_chartType == ChartTypes.meVsAll)
            {
                // 두 개임!! 색 고정!!
                for (int i = 0; i < barEntries.Count; i++)
                {
                    UpdateBarWithEntry(barInstances[nextBarInstanceIndex],
                        barEntries[i],
                        i == 0 ? ColorChart.gray_250 : ColorChart.primary_300,
                        dataSetIndex);
                    nextBarInstanceIndex++;
                }
            }
            else if (_chartType == ChartTypes.meVsFriends)
            {
                for (int i = 0; i < barEntries.Count; i++)
                {
                    UpdateBarWithEntry(barInstances[nextBarInstanceIndex],
                        barEntries[i],
                        barEntries[i].IsMe ? ColorChart.primary_300 : ColorChart.gray_250,
                        dataSetIndex);

                    nextBarInstanceIndex++;
                }
            }
            else if (_chartType == ChartTypes.dailyByTime)
            {
                for (int i = 0; i < barEntries.Count; i++)
                {
                    UpdateBarWithEntry(barInstances[nextBarInstanceIndex],
                        barEntries[i],
                        barEntries[i].Value >= _dataMaxValue ? ColorChart.primary_300 : ColorChart.gray_250,
                        dataSetIndex);
                    nextBarInstanceIndex++;
                }

            }

            // 오잉..이거 데이터셋이 여러개 일 경우 겹치겠는걸?

            maxValueLineRenderer?.gameObject.SetActive(showMaxValueLine && playAnimationInVisible == false);
            if (showMaxValueLine && maxIndex >= 0)
            {
                var position = positioner.GetBarPosition((int)barEntries[maxIndex].Position, dataSetIndex);
                var size = positioner.GetBarSize(barEntries[maxIndex].Value, _dataMaxValue);
                Vector2 leftBottomPosition = new Vector2(position.x + config.BarWidth * 0.5f, position.y + size.y + 18f);
                maxValueLineRenderer?.Initialize(maxValueTextLabelPrefab, leftBottomPosition, barEntries[maxIndex].Value.ToString("N0"), data.DataSets[dataSetIndex].MaxBarColor);
            }

/*            averageLineRenderer?.gameObject.SetActive(showAverageValueLine);
            if (showAverageValueLine && averageValue > 0.0f)
            {
                var size = positioner.GetBarSize(averageValue, _dataMaxValue);
                //averageLineRenderer?.Initialize(size.y);
                averageLineRenderer?.Initialize(100f);
                //OnBarClick(barEntries[maxIndex], dataSetIndex);
            }*/

            // 왜 안되는데,,??ㅜㅜ 일단 걍 수동으로 올릴게!

            return nextBarInstanceIndex;
        }

        private Bar UpdateBarWithEntry (Bar barInstance, BarEntry entry, Color color, int dataSetIndex) {
            barInstance.transform.localPosition = positioner.GetBarPosition ((int) entry.Position, dataSetIndex);
            if (playAnimationInVisible == false)
            {
                barInstance.GetComponent<RectTransform>().sizeDelta = positioner.GetBarSize(entry.Value, _dataMaxValue);
            }
            else
            {
                var size = positioner.GetBarSize(entry.Value, _dataMaxValue);
                size.y = 0;
                barInstance.GetComponent<RectTransform>().sizeDelta = size;
            }

            if(_hasProfile)
            {
                if(!entry.IsMe)
                {
                    if (entry.ThumbnailUrl == "")
                        barInstance.go_profile.SetActive(false);
                    else
                    {
                        barInstance.go_profile.SetActive(true);
                        barInstance.profile_tumbnail.setImageFromCDN(entry.ThumbnailUrl);
                    }
                }
                else
                {
                    barInstance.go_profile.SetActive(false);
                }
            }

            barInstance.SetColor (color);
            if(barInstance.button != null)
            {
                barInstance.button.onClick.RemoveAllListeners();
                barInstance.button.onClick.AddListener(delegate { OnBarClick(entry, dataSetIndex); });
            }
            return barInstance;
        }

        public void OnBarClick (BarEntry entry, int dataSetIndex) {
            Debug.Log("weekly bar click");
            if (Config.BarChartClickAction != null) {
                Config.BarChartClickAction.Invoke (entry, dataSetIndex);
            }
            ShowHideValuePopup (entry, dataSetIndex);
        }

        private void ShowHideValuePopup (BarEntry entry, int dataSetIndex) {
            if (currentValuePopup == null) {
                currentValuePopup = viewCreator.InstantiateChartPopup (contentView.transform, Config.PopupPrefab);
            }

            if (entry != currentValuePopupEntry) {
                UpdateValuePopup (entry, dataSetIndex);
                currentValuePopupEntry = entry;
            } else {
                HideCurrentValuePopup ();
            }
        }

        private void HideCurrentValuePopup () {
            if (currentValuePopup != null) {
                currentValuePopup.gameObject.SetActive (false);
                currentValuePopupEntry = null;
            }
        }

        private void UpdateValuePopup (BarEntry entry, int dataSetIndex)
        {
            if (currentValuePopup != null)
            {
                Vector2 pos = positioner.GetValuePopupPosition(entry, dataSetIndex, axisConfig.VerticalAxisConfig.Bounds.Max + 29f);
                currentValuePopup.transform.localPosition = pos;
                currentValuePopup.SetText(entry.Value.ToString("N0"));
                currentValuePopup.gameObject.SetActive(true);
            }
        }
    }
}