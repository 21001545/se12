using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	// update를 누군가 호출해줘야함
	public class UIAlignToTouchScreenKeyboard : MonoBehaviour
	{
		public RectTransform target;

		//private Vector2SmoothDamper _posDamper;
		private Vector2 _initPos;
		private Camera _targetCamera;
		//private IntervalTimer _timer;
		private bool _lastKeyboardVisible;
		private Canvas _rootCanvas;

		private TouchScreenKeyboardUtil KeyboardUtil => TouchScreenKeyboardUtil.getInstance();

		public void Awake()
		{
			_rootCanvas = GetComponentInParent<Canvas>();

			if(_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				_targetCamera = null;
			}
			else
			{
				_targetCamera = Camera.main;
			}

			Debug.Assert(target.anchorMin.y == 0);
			Debug.Assert(target.anchorMax.y == 0);
			Debug.Assert(target.pivot.y == 0);

			_initPos = target.anchoredPosition;
		}

		public void LateUpdate()
		{
			alignToTouchScreenKeyboard();
		}

#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if( Application.isPlaying)
			{
				bool visible = KeyboardUtil.isVisible();
				if (visible)
				{
					float height = KeyboardUtil.getHeight() / Screen.height;

					RectTransform rtParent = target.parent as RectTransform;

					Vector3 lLeft = new Vector3( -rtParent.rect.width / 2.0f, rtParent.rect.height * height - rtParent.rect.height / 2.0f, 0);
					Vector3 lRight = new Vector3(rtParent.rect.width / 2.0f, rtParent.rect.height * height - rtParent.rect.height / 2.0f, 0);

					Gizmos.color = Color.red;
					Gizmos.DrawLine(rtParent.TransformPoint(lLeft), rtParent.TransformPoint(lRight));
				}
			}
		}
#endif

		private void alignToTouchScreenKeyboard()
		{
			bool visible = KeyboardUtil.isVisible();

			float height = KeyboardUtil.getHeight();

			Vector2 screenPos = new Vector2(Screen.width / 2.0f, height);
			Vector2 prevPos = target.anchoredPosition;
			Vector2 targetPos;

			RectTransform rtParent = target.parent as RectTransform;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rtParent, screenPos, _targetCamera, out targetPos))
			{
				targetPos.y += rtParent.rect.height / 2.0f;

				if (targetPos.y < _initPos.y)
				{
					targetPos.y = _initPos.y;
				}

				if( prevPos != targetPos)
				{
					target.anchoredPosition = targetPos;
				}
			}

			//if ( visible)
			//{
			//	bool update_position = false;

			//	if( _lastKeyboardVisible == visible)
			//	{
			//		//if( _timer.update())
			//		//{
			//		//	_timer.setNext();
			//			update_position = true;
			//		//}
			//	}
			//	else
			//	{
			//		update_position = true;
			//	}

			//	if( update_position)
			//	{
			//		float height = KeyboardUtil.getHeight();

			//		Vector2 screenPos = new Vector2(Screen.width / 2.0f, height);
			//		Vector2 targetPos;

			//		RectTransform rtParent = target.parent as RectTransform;

			//		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rtParent, screenPos, _targetCamera, out targetPos))
			//		{
			//			targetPos.y += rtParent.rect.height / 2.0f;

			//			if( targetPos.y < _initPos.y)
			//			{
			//				targetPos.y = _initPos.y;
			//			}

			//			target.anchoredPosition = targetPos;
			//		}
			//	}
			//}
			//else
			//{
			//	//_posDamper.setTarget(_initPos);
			//	target.anchoredPosition = _initPos;
			//}

			_lastKeyboardVisible = visible;
		}
	}
}
