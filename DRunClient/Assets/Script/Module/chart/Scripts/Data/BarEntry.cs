using UnityEngine;

namespace AwesomeCharts {

    [System.Serializable]
    public class BarEntry : Entry {

        [SerializeField]
        private long position;

        private string _thumbnailUrl;
        private bool _isMe;
        private bool _isToday;

        public BarEntry() : base() {
            this.position = 0;
        }

        public BarEntry(long position, float value) : base(value) {
            this.position = position;
        }

        public long Position {
            get { return position; }
            set { position = value; }
        }

        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { _thumbnailUrl = value; }
        }

        public bool IsMe
        {
            get { return _isMe; }
            set { _isMe = value; }
        }

        public bool IsToday
        {
            get { return _isToday; }
            set { _isToday = value; }
        }
    }
}