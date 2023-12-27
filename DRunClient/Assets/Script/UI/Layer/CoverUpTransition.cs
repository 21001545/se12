using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module.UI;
using Festa.Client.Module;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasGroup))]
	public class CoverUpTransition : AbstractPanelTransition
	{
		private RectTransform _rt;
		private CanvasGroup _canvas_group;
		private Vector2SmoothDamper _offsetMin;
		private Vector2SmoothDamper _offsetMax;

		private bool _active;
		private bool _is_closing;
		private const float duration = 0.1f;
		private float _closeWaitTime;
		private bool _isClosingNow;		// 당장닫는거
		private ITransitionEventHandler _eventHandler;

		public static class Direction
		{
			public const int next = 0;
			public const int prev = 1;
		}

        public override float getDuration()
        {
			return duration;
        }

        public override void init(ITransitionEventHandler eventHandler)
		{
			_canvas_group = GetComponent<CanvasGroup>();
			_rt = transform as RectTransform;

			_offsetMin = Vector2SmoothDamper.create(Vector2.zero, duration);
			_offsetMax = Vector2SmoothDamper.create(Vector2.zero, duration);

			_canvas_group.interactable = false;

			_active = false;
			_eventHandler = eventHandler;
		}

		public float startOpen(int direction)
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();

			RectTransform parent = _rt.parent as RectTransform;

			Vector2 offset = parent.rect.size;

			if( direction == Direction.next)
			{
				_offsetMin.reset(new Vector2(offset.x, 0));
				_offsetMax.reset(new Vector2(offset.x, 0));
			}
			else
			{
				_offsetMin.reset(new Vector2(-offset.x, 0));
				_offsetMax.reset(new Vector2(-offset.x, 0));
			}

			_offsetMin.setTarget(Vector2.zero);
			_offsetMax.setTarget(Vector2.zero);

			_rt.offsetMin = _offsetMin.getCurrent();
			_rt.offsetMax = _offsetMax.getCurrent();

			_active = true;
			_is_closing = false;
			_isClosingNow = false;

			_eventHandler.onTransitionEvent( TransitionEventType.start_open);

			return duration;
		}

        public float openImmediately(int direction)
        {
			gameObject.SetActive(true);
			transform.SetAsLastSibling();

			RectTransform parent = _rt.parent as RectTransform;

            Vector2 offset = parent.rect.size;

            _offsetMin.reset(Vector2.zero);
            _offsetMax.reset(Vector2.zero);

            _offsetMin.setTarget(Vector2.zero);
            _offsetMax.setTarget(Vector2.zero);

			_rt.offsetMin = _offsetMin.getCurrent();
			_rt.offsetMax = _offsetMax.getCurrent();

			_is_closing = false;
			_isClosingNow = false;

			_canvas_group.interactable = true;

			_eventHandler.onTransitionEvent(TransitionEventType.start_open);
			_eventHandler.onTransitionEvent(TransitionEventType.end_open);

			return 0f;
        }

		public float startClose(int direction)
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();

			RectTransform parent = _rt.parent as RectTransform;

			Vector2 offset = parent.rect.size;

            /*            _offsetMin.reset(Vector2.zero);
                        _offsetMax.reset(Vector2.zero);*/
            _offsetMin.init(Vector2.zero, duration);
            _offsetMax.init(Vector2.zero, duration);

            if (direction == Direction.next)
			{
				_offsetMin.setTarget(new Vector2(-offset.x, 0));
				_offsetMax.setTarget(new Vector2(-offset.x, 0));
			}
			else
			{
				_offsetMin.setTarget(new Vector2(offset.x, 0));
				_offsetMax.setTarget(new Vector2(offset.x, 0));
			}

			_rt.offsetMin = _offsetMin.getCurrent();
			_rt.offsetMax = _offsetMax.getCurrent();

			_active = true;
			_is_closing = true;
			_isClosingNow = false;

			_eventHandler.onTransitionEvent( TransitionEventType.start_close);

			return duration;
		}

		public float closeImmediately(int direction)
		{
			gameObject.SetActive(false);

			RectTransform parent = _rt.parent as RectTransform;

			Vector2 offset = parent.rect.size;

			if (direction == Direction.next)
			{
				_offsetMin.reset(new Vector2(-offset.x, 0));
				_offsetMax.reset(new Vector2(-offset.x, 0));
				_offsetMin.setTarget(new Vector2(-offset.x, 0));
				_offsetMax.setTarget(new Vector2(-offset.x, 0));
			}
			else
			{
				_offsetMin.reset(new Vector2(offset.x, 0));
				_offsetMax.reset(new Vector2(offset.x, 0));
				_offsetMin.setTarget(new Vector2(offset.x, 0));
				_offsetMax.setTarget(new Vector2(offset.x, 0));
			}

			_rt.offsetMin = _offsetMin.getCurrent();
			_rt.offsetMax = _offsetMax.getCurrent();

			_is_closing = false;

			_eventHandler.onTransitionEvent(TransitionEventType.start_close);
			_eventHandler.onTransitionEvent(TransitionEventType.end_close);
			return 0f;
		}

		public void openNow()
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
			_canvas_group.interactable = true;

			_rt.offsetMin = Vector2.zero;
			_rt.offsetMax = Vector2.zero;

			// 
			_eventHandler.onTransitionEvent(TransitionEventType.start_open);
			_eventHandler.onTransitionEvent(TransitionEventType.end_open);
		}

		public override float startOpen()
		{
			transform.SetAsLastSibling();
			return startOpen(Direction.next);
		}

		public override float startClose()
		{
			transform.SetAsFirstSibling();
			return startClose(Direction.next);
		}

        public override float openImmediately()
        {
			transform.SetAsLastSibling();
			openImmediately(Direction.next);
			return 0f;
        }

        public override float closeImmediately(float duration)
        {
			transform.SetAsFirstSibling();
			_closeWaitTime = duration;
			_isClosingNow = true;
			_is_closing = false;
			return 0f;
		}


        public override bool isActive()
		{
			return _active;
		}

		public override void update()
		{
			if (_isClosingNow)
			{
				if (_closeWaitTime <= 0f)
                {
					closeImmediately(Direction.next);
					_isClosingNow = false;
				}
				else
					_closeWaitTime -= Time.deltaTime;
			}

			if ( _active == false)
			{
				return;
			}

			_active = false;
			if( _offsetMin.update())
			{
				_active = true;
				_rt.offsetMin = _offsetMin.getCurrent();
			}

			if( _offsetMax.update())
			{
				_active = true;
				_rt.offsetMax = _offsetMax.getCurrent();
			}

			if ( _active == false)
			{
				if(_is_closing)
				{
					_eventHandler.onTransitionEvent(TransitionEventType.end_close);

					gameObject.SetActive(false);

					_canvas_group.interactable = false;
				}
				else
				{
					_eventHandler.onTransitionEvent(TransitionEventType.end_open);

					_canvas_group.interactable = true;
				}
			}
		}
	}
}
