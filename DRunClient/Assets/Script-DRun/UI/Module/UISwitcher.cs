using Festa.Client.Module;
using UnityEngine;

namespace DRun.Client
{
    /// <summary>
    /// UI Toggle Switcher
    /// </summary>
    public class UISwitcher : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Min / Max accessing index range")]
        [Space(10)]
        [SerializeField, ReadOnly]
        private int _minIndex;

        [SerializeField, ReadOnly]
        private int _maxIndex;

        [Space(10)]
        [Header("Currently switched UI")]
        [SerializeField]
        [ReadOnly]
        private RectTransform _switchedUI;

        // warning없애기용 ㅋㅋ
        public int getMinIndex()
        {
            return _minIndex;
        }
#endif

        [Space(10)]
        [Header("Switching targets")]
        [SerializeField]
        private RectTransform[] _switchTargetUI;

        /// <summary>
        /// Enable only one GameObject at the given index, otherwise disable.
        /// </summary>
        /// <param name="index"></param>
        public void @switch(int index)
        {
            for (var i = 0; i < _switchTargetUI.Length; i++)
            {
                // index 항목만 켜기.
                if (i == index)
                {
                    _switchTargetUI[i].gameObject.SetActive(true);
#if UNITY_EDITOR
                    _switchedUI = _switchTargetUI[i];
#endif
                    continue;
                }

                _switchTargetUI[i].gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        void OnEnable() => validateRange();
        void OnValidate() => validateRange();

        private void validateRange()
        {
            int maxLen = _switchTargetUI.Length;
            if (_switchTargetUI != null && maxLen > 0)
            {
                _minIndex = 0;
                _maxIndex = maxLen - 1;
            }
        }
#endif
    }
}