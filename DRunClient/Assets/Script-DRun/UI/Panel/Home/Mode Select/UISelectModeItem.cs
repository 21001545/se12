using TMPro;
using UnityEngine;

namespace DRun.Client
{
    [System.Serializable]
    public class UISelectModeItem
    {
        [SerializeField] private TMP_Text _txt_modeName;

        private static readonly Color _highlightTextColor = UIStyleDefine.ColorStyle.gray200;
        private static readonly Color _normalTextColor = UIStyleDefine.ColorStyle.gray500;

        public void setup(bool isDefaultMode = false)
        {
            // select default color.
            _txt_modeName.color = isDefaultMode ? _highlightTextColor : _normalTextColor; 
        }

        public void select()
        {
            this._txt_modeName.color = _highlightTextColor;
        }

        public void deselect()
        {
            this._txt_modeName.color = _normalTextColor;
        }
    }
}