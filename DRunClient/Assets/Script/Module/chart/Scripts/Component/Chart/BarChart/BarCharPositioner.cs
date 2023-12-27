using System.Collections.Generic;
using UnityEngine;
using Festa.Client;

namespace AwesomeCharts {
    [System.Serializable]
    public class BarCharPositioner {

        public AxisBounds axisBounds;
        public BarData data;
        public BarChartConfig barChartConfig;
        public Vector2 containerSize;

        private float calculatedBarWidth;

        public int GetVisibleEntriesRange () {
            if (axisBounds == null)
                return 0;
            else
                return (int) (axisBounds.XMax - axisBounds.XMin) + 1;
        }

        public void RecalculatePositioner () {
            switch (barChartConfig.SizingMethod) {
                case BarSizingMethod.STANDARD:
                    calculatedBarWidth = GetBarWidthWithStandardMethod ();
                    break;
                case BarSizingMethod.SIZE_TO_FIT:
                    calculatedBarWidth = GetBarWidthWithSizeToFitMethod ();
                    break;
            }
        }

        private float GetBarWidthWithStandardMethod () {
            float maxWidth = GetBarWidthWithSizeToFitMethod ();
            return Mathf.Min (maxWidth, barChartConfig.BarWidth);
        }

        private float GetBarWidthWithSizeToFitMethod () {
            int visibleBars = GetVisibleEntriesRange () * data.DataSets.Count;
            float barsSpacings = (GetVisibleEntriesRange () + 1) * barChartConfig.BarSpacing;
            float innerBarsSpacings = (GetVisibleEntriesRange () * Mathf.Max (0, data.DataSets.Count - 1)) *
                barChartConfig.InnerBarSpacing;

            return (containerSize.x - barsSpacings - innerBarsSpacings) / visibleBars;
        }

        public Vector3 GetBarPosition (int position, int dataSetIndex) {
            if (axisBounds == null)
                return Vector3.zero;

            int visiblePosition = position - (int) axisBounds.XMin;
            float x = ((visiblePosition * data.DataSets.Count) * calculatedBarWidth) +
                (dataSetIndex * calculatedBarWidth) +
                (visiblePosition + 1) * barChartConfig.BarSpacing +
                (visiblePosition * Mathf.Max (0, data.DataSets.Count - 1) * barChartConfig.InnerBarSpacing) +
                (dataSetIndex * barChartConfig.InnerBarSpacing);

            return new Vector3 (x, 0, 0);
        }

        public Vector3 GetBarCenterPosition (int position) {
            
            int dataSetsCount = Mathf.Max (1, data.DataSets.Count);
            float positionFullWidth = calculatedBarWidth * dataSetsCount +
                barChartConfig.InnerBarSpacing * (dataSetsCount - 1);
            float x = GetBarPosition (position, 0).x + positionFullWidth / 2;
            return new Vector3 (x, 0, 0);
        }

        public float GetMaxBarHeight () {
            return containerSize.y;
        }

        public Vector2 GetBarSize (float value, float maxVal) {
            return new Vector2 (calculatedBarWidth, CalculateBarHeight (value, maxVal));
        }

        private float CalculateBarHeight (float value, float maxVal) {

            if(maxVal != 0)
                return Mathf.Clamp(value, 0f, maxVal) * (axisBounds.YMax / maxVal);
            else
                return 0f;
        }

        public Vector3 GetValuePopupPosition (BarEntry entry, int dataSetIndex, float maxVal) {
            Vector3 barPosition = GetBarPosition ((int) entry.Position, dataSetIndex);
            //Vector2 barSize = GetBarSize (entry.Value, maxVal);

            return new Vector3 (barPosition.x + calculatedBarWidth / 2, maxVal, 0);
        }

        public int GetAllVisibleEntriesCount () {
            if (axisBounds == null || data == null || !data.HasAnyData ())
                return 0;

            int result = 0;
            data.DataSets.ForEach (dataSet => {
                result += FilterVisibleEntries (dataSet).Count;
            });

            return result;
        }

        public List<BarEntry> GetVisibleEntries (int dataSetIndex) {
            if (axisBounds == null || data == null || !data.HasAnyData ())
                return new List<BarEntry> ();

            return FilterVisibleEntries (data.DataSets[dataSetIndex]);
        }

        private List<BarEntry> FilterVisibleEntries (BarDataSet dataSet) {
            return dataSet.Entries.FindAll ((BarEntry entry) =>
                entry.Position >= axisBounds.XMin &&
                entry.Position <= axisBounds.XMax);
        }
    }
}