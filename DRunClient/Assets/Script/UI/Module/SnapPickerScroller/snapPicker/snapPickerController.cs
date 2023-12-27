using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using TMPro;
using UnityEngine.Events;

namespace Festa.Client
{
    /// <summary>
    /// This class controls one slot scroller. We could have shared the slot data between the 
    /// three slot controllers, but for demonstration purposes we gave each slot controller their 
    /// own set of data.
    /// </summary>
    public class snapPickerController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        // 스크롤러에서 사용할 picker data view 프리팹
        [SerializeField]
        private EnhancedScrollerCellView pickerDataViewPrefab;

        private SmallList<pickerData>   _data;        // picker 셀 리스트
        public EnhancedScroller         scroller;     // 셀을 표시할 스크롤러

        private float cellSize;
        private UnityAction<EnhancedScroller, int> _onSnappedCallback;

        public void setCellSize(float _cellSize)
        {
            cellSize = _cellSize;
        }

        // 2022.4.15 이강희 초기화 타이밍 이슈로 외부에서 직접 초기화 하도록 변경
        public void init(UnityAction<EnhancedScroller,int> onSnappedCallback)
		{
            // create a new data list for the slots
            _data = new SmallList<pickerData>();
            // set this controller as the scroller's delegate
            scroller.Delegate = this;

            scroller.GetComponent<PickerSnap>().setSnappedCallback(onSnappedCallback);
            _onSnappedCallback = onSnappedCallback;

        }

        //void Awake()
        //{
        //    // create a new data list for the slots
        //    _data = new SmallList<pickerData>();
        //}

        //void Start()
        //{
        //    // set this controller as the scroller's delegate
        //    scroller.Delegate = this;
        //}

        public void Reload(SmallList<pickerData> _dataList)
        {
            // 데이터 리셋
            _data.Clear();

            // 데이터는 snapPicker_work 를 상속받는 스크립트에서 전달해 준다~~
            _data = _dataList;

            // 앞에 두 개, 뒤에 두 개 더미
            _data.AddStart(new pickerData() { stringData = "" });
            _data.AddStart(new pickerData() { stringData = "" });

            _data.Add(new pickerData() { stringData = "" });
            _data.Add(new pickerData() { stringData = "" });

			// reload the scroller
			scroller.ReloadData();
		}


		// 휘리릭 자동으로 돌아가게 만드는 함수
		public void AddVelocity(float amount)
        {
            scroller.LinearVelocity = amount;
        }

        #region EnhancedScroller Callbacks

        /// <summary>
        /// This callback tells the scroller how many slot cells to expect
        /// </summary>
        /// <param name="scroller">The scroller requesting the number of cells</param>
        /// <returns>The number of cells</returns>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        /// <summary>
        /// This callback tells the scroller what size each cell is.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell size</param>
        /// <param name="dataIndex">The index of the data list</param>
        /// <returns>The size of the cell (Height for vertical scrollers, Width for Horizontal scrollers)</returns>
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellSize;
        }

        /// <summary>
        /// This callback gets the cell to be displayed by the scroller
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell</param>
        /// <param name="dataIndex">The index of the data list</param>
        /// <param name="cellIndex">The cell index (This will be different from dataindex if looping is involved)</param>
        /// <returns>The cell to display</returns>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            // get the cell view from the scroller, recycling if possible
            pickerDataView dataView = scroller.GetCellView(pickerDataViewPrefab) as pickerDataView;

            // set the data for the cell
            dataView.SetData(_data[dataIndex], dataIndex => {

                if( dataIndex >= 2)
				{
                    _onSnappedCallback(scroller, dataIndex - 2);
                    scroller.JumpToDataIndex(dataIndex - 2, 0, 0, false, EnhancedScroller.TweenType.easeOutQuad, 0.2f);
                }
            });

            // return the cell view to the scroller
            return dataView;
        }

        #endregion
    }
}