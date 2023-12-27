using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine.Events;

namespace Festa.Client
{
    public class pickerDataView : EnhancedScrollerCellView
    {            
        [SerializeField]
        private TextMeshProUGUI txt_pickData;

        private UnityAction<int> _onClickHandler;

        // 기본으로 있던 함수~!
        public void SetData(pickerData data,UnityAction<int> onClickHandler)
        {
            txt_pickData.text = data.stringData;
            _onClickHandler = onClickHandler;
        }

        public string getStringData()
        {
            return txt_pickData.text;
        }

        public void onClick()
		{
            if( _onClickHandler != null)
			{
                _onClickHandler(dataIndex);
			}
		}
    }
}