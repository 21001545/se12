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
	public class UIScrollBehaviour
	{
		private Vector2 _ratioConvert;
		private float _viewBound;
		private float _contentBound;
		private UnityAction<float> _scrollHandler;

		private float _velocity;
		private bool _dragging;
		private float _scrollPos;
		private float _prevScrollPos;
		private float _dragStartCursorPos;
		private float _dragStartScrollPos;
		private float _normalizedScrollPos;

		public float getNormalizedScrollPos()
		{
			return _normalizedScrollPos;
		}

		public static UIScrollBehaviour create(Vector2 ratioConvert, UnityAction<float> scrollHandler)
		{
			UIScrollBehaviour b = new UIScrollBehaviour();
			b.init(ratioConvert, scrollHandler);
			return b;
		}

		private void init(Vector2 ratioConvert, UnityAction<float> scrollHandler)
		{
			_ratioConvert = ratioConvert;
			//_viewBound = Screen.height / 2.0f;
			_viewBound = Screen.height;
			_contentBound = Screen.height + Screen.height / 2.0f;
			_velocity = 0;
			_dragging = false;
			_scrollHandler = scrollHandler;
			_prevScrollPos = _scrollPos = 0;
			_normalizedScrollPos = 0;
		}

		public void onBeginDrag(BaseEventData e)
		{
			PointerEventData pe = e as PointerEventData;
			_dragging = true;
			_dragStartCursorPos = pe.position.y;
			_dragStartScrollPos = _scrollPos;
		}

		public void onDrag(BaseEventData e)
		{
			PointerEventData pe = e as PointerEventData;

			float delta = pe.position.y - _dragStartCursorPos;
			float position = _dragStartScrollPos + delta;

			float offset = calculateOffset(position - _scrollPos);
			position -= offset;
			if( offset != 0)
			{
				position += rubberDelta(offset, _viewBound);
			}

			//Debug.Log($"delta[{delta}] offset[{offset}]");

			updateScroll(position);
		}

		public void onEndDrag(BaseEventData e)
		{
			PointerEventData pe = e as PointerEventData;
			_dragging = false;
		}

		public void update()
		{
			float offset = calculateOffset(0);
			float deltaTime = Time.unscaledDeltaTime;
			if( _dragging == false && (offset != 0 || _velocity != 0))
			{
				float position = _scrollPos;
				if( offset != 0)
				{
					float speed = _velocity;
					float smoothTime = 0.1f;

					position = Mathf.SmoothDamp(position, position - offset, ref speed, smoothTime, Mathf.Infinity, deltaTime);
					if( Mathf.Abs(speed) < 1)
					{
						speed = 0;
					}

					_velocity = speed;

				}
				else
				{
					_velocity *= Mathf.Pow(0.135f, deltaTime);
					if ( Mathf.Abs(_velocity) < 1)
						_velocity = 0;
					position += _velocity * deltaTime;
				}

				updateScroll(position);
			}

			if( _dragging)
			{
				float newVelocity = (_scrollPos - _prevScrollPos) / deltaTime;
				_velocity = Mathf.Lerp(_velocity, newVelocity, deltaTime * 10.0f);
			}

			if( _scrollPos != _prevScrollPos)
			{
				_prevScrollPos = _scrollPos;
			}
		}

		private void updateScroll(float position)
		{
			float scrollSize = _contentBound - _viewBound;

			//Debug.Log($"pos[{position}] size[{scrollSize}]");

			_scrollPos = position;

			float result;

			if ( _scrollPos < 0)
			{
				result = _ratioConvert.x + _scrollPos / scrollSize;
			}
			else if( _scrollPos > scrollSize)
			{
				result = _ratioConvert.y + (_scrollPos - scrollSize) * (1 - _ratioConvert.y) / scrollSize; 
			}
			else
			{
				result = _ratioConvert.x + _scrollPos * (_ratioConvert.y - _ratioConvert.x) / scrollSize;
			}

			result = Mathf.Clamp(result, 0, 1);

			_normalizedScrollPos = result;

			_scrollHandler(result);
		}

		private float calculateOffset(float delta)
		{
			float offset = 0;

			float min = 0 - _scrollPos;
			float max = _contentBound - _scrollPos;

			min -= delta;
			max -= delta;

			float maxOffset = _viewBound - max;
			float minOffset = 0 - min;

			if (maxOffset > 0.001f)
				offset = maxOffset;
			else if (minOffset < -0.001f)
				offset = minOffset;

			//Debug.Log($"scrollPos[{_scrollPos}] min[{min}] max[{max}] offset[{offset}]");


			//float min = 0 - _scrollPos;
			//float max = _contentBound - _scrollPos;

			//// min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)
			//min += delta;
			//max += delta;

			//float maxOffset = _viewBound - max;
			//float minOffset = 0 - min;

			//if (maxOffset > 0.001f)
			//	offset = maxOffset;
			//else if (minOffset < -0.001f)
			//	offset = minOffset;

			return offset;
		}

		private static float rubberDelta(float overStretching, float viewSize)
		{
			return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
		}
	}
}
