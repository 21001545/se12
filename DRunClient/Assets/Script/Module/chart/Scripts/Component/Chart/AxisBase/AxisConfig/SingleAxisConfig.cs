using UnityEngine;

namespace AwesomeCharts {
    [System.Serializable]
    public class SingleAxisConfig {

        [SerializeField]
        private bool showConfig = true;
        [SerializeField]
        private AxisLabelConfig labelsConfig = new AxisLabelConfig ();
        [SerializeField]
        private AxisValue bounds = new AxisValue ();
        [SerializeField]
        private AxisLabel axisLabelPrefab;

        public bool ShowConfig
        {
            get { return showConfig; }
            set { showConfig = value; }
        }

        public AxisLabelConfig LabelsConfig {
            get { return labelsConfig; }
            set { labelsConfig = value; }
        }

        public AxisValue Bounds {
            get { return bounds; }
            set { bounds = value; }
        }

        public AxisLabel AxisLabelPrefab {
            get { return axisLabelPrefab; }
            set { axisLabelPrefab = value; }
        }
    }
}