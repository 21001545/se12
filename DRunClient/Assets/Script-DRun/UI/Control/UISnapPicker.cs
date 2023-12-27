using EnhancedUI.EnhancedScroller;
using Festa.Client;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DRun.Client
{
	public class UISnapPicker : MonoBehaviour,IEnhancedScrollerDelegate, IBeginDragHandler, IEndDragHandler
	{
		public UISnapPickerCell cellSource;
		public EnhancedScroller scroller;
		public int selectOffset;
		public int endDummyCount;

		[Serializable]
		public class ChangeSnapEvent : UnityEvent<int> { };


		[Header("Snap")]
		public float velocityThreshold = 20.0f;
		public ChangeSnapEvent onSnapped = new ChangeSnapEvent();

		private List<UISnapPickerData> _dataList;
		private bool _isDragging = true;
		private bool _checkSnap = false;
		private int _currentSnapIndex = -1;

		public List<UISnapPickerData> DataList
		{
			get
			{
				return _dataList;
			}
			set
			{
				_dataList = value;
				if( scroller.Container != null)
				{
					scroller.ReloadData();
				}
			}
		}

		public int CurrentSnapIndex => _currentSnapIndex;

		public void init()
		{
			_dataList = new List<UISnapPickerData>();
			scroller.Delegate = this;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UISnapPickerCell cell = (UISnapPickerCell)scroller.GetCellView(cellSource);
			cell.setup(_dataList[dataIndex],this);
			return cell;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return cellSource.height;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _dataList.Count;
		}

		public int getCurrentValue()
		{
			if(_currentSnapIndex < 0 || _currentSnapIndex > _dataList.Count)
			{
				return -1;
			}

			return _dataList[_currentSnapIndex + selectOffset].getValue();
		}

		public void jumpByValue(int value,bool isImmediate)
		{
			int dataIndex = dataIndexFromValue(value);
			if (dataIndex == -1)
				return;

			jumpTo(dataIndex - selectOffset, isImmediate);
		}

		public int dataIndexFromValue(int value)
		{
			for(int i = 0; i < _dataList.Count; ++i)
			{
				if(_dataList[i].getValue() == value)
				{
					return i;
				}
			}

			return -1;
		}

		public void jumpTo(int dataIndex,bool isImmeidate)
		{
			if( isImmeidate)
			{
				scroller.JumpToDataIndex(dataIndex, 0, 0, true, EnhancedScroller.TweenType.immediate, 0);
			}
			else
			{
				scroller.JumpToDataIndex(dataIndex, 0, 0, true, EnhancedScroller.TweenType.easeOutQuad, 0.2f);
			}

			_currentSnapIndex = dataIndex;
			onSnapped.Invoke(dataIndex);
		}

		public void OnBeginDrag(PointerEventData data)
		{
			if( scroller.IsTweening)
			{
				return;
			}

			_isDragging = true;
		}

		public void OnEndDrag(PointerEventData data)
		{
			if( _isDragging)
			{
				_isDragging = false;
				_checkSnap = true;
			}
		}

		void LateUpdate()
		{
			if (_checkSnap)
			{
				if (Mathf.Abs(scroller.Velocity.y) < velocityThreshold)
				{
					_checkSnap = false;

					float newIndexF = scroller.ScrollPosition / (cellSource.height + scroller.spacing);
					int newIndex = Mathf.FloorToInt(newIndexF + 0.5f);

					int total_count = scroller.Delegate.GetNumberOfCells(scroller);

					newIndex = System.Math.Clamp(newIndex, 0, total_count - endDummyCount);

					jumpTo(newIndex, false);
				}
			}
		}
	}
}
