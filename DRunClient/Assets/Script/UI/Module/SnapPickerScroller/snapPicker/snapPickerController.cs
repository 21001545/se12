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
        // ��ũ�ѷ����� ����� picker data view ������
        [SerializeField]
        private EnhancedScrollerCellView pickerDataViewPrefab;

        private SmallList<pickerData>   _data;        // picker �� ����Ʈ
        public EnhancedScroller         scroller;     // ���� ǥ���� ��ũ�ѷ�

        private float cellSize;
        private UnityAction<EnhancedScroller, int> _onSnappedCallback;

        public void setCellSize(float _cellSize)
        {
            cellSize = _cellSize;
        }

        // 2022.4.15 �̰��� �ʱ�ȭ Ÿ�̹� �̽��� �ܺο��� ���� �ʱ�ȭ �ϵ��� ����
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
            // ������ ����
            _data.Clear();

            // �����ʹ� snapPicker_work �� ��ӹ޴� ��ũ��Ʈ���� ������ �ش�~~
            _data = _dataList;

            // �տ� �� ��, �ڿ� �� �� ����
            _data.AddStart(new pickerData() { stringData = "" });
            _data.AddStart(new pickerData() { stringData = "" });

            _data.Add(new pickerData() { stringData = "" });
            _data.Add(new pickerData() { stringData = "" });

			// reload the scroller
			scroller.ReloadData();
		}


		// �ָ��� �ڵ����� ���ư��� ����� �Լ�
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