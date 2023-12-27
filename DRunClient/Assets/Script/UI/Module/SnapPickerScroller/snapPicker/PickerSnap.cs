using EnhancedUI.EnhancedScroller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public class PickerSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
	{
		public EnhancedScroller scroller;
		public UIScrollRectOverride scrollRect;
		public float cellSize = 44.0f;
		public float velocityThreshold = 20.0f;
		public int marginBegin = 0;
		public int marginEnd;

		private bool _isDragging = true;
		private bool _checkSnap = false;

		private UnityAction<EnhancedScroller, int> _snappedCallback;

		public void setSnappedCallback(UnityAction<EnhancedScroller,int> callback)
		{
			_snappedCallback = callback;
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
			if( _checkSnap)
			{
				if ( Mathf.Abs(scroller.Velocity.y) < velocityThreshold)
				{
					_checkSnap = false;

					float newIndexF = scroller.ScrollPosition / (cellSize + scroller.spacing);
					int newIndex = Mathf.FloorToInt(newIndexF + 0.5f);

					int total_count = scroller.Delegate.GetNumberOfCells(scroller);

					newIndex = System.Math.Clamp(newIndex, marginBegin, total_count - marginEnd);

					scroller.JumpToDataIndex(newIndex, 0, 0, false, EnhancedScroller.TweenType.easeOutQuad, 0.2f);

					if(_snappedCallback != null)
					{
						_snappedCallback(scroller, newIndex);
					}
				}
			}
		}
	}
}
