using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client.Module.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadePanelTransition : AbstractPanelTransition
	{
		private CanvasGroup _canvas_group;

		private FloatSmoothDamper _alpha_damper;
		private FloatSmoothDamper _scale_damper;

		private bool _active;
		private bool _isClosingNow;
		private float _closeWaitTime;
		private ITransitionEventHandler _eventHandler;

		[SerializeField]
		private Transform scale_target;
		private RectTransform rect_scaleTarget;
		public bool scaling = true;
		public bool noChangeSibling = false;
		public float duration = 0.1f;

        public override float getDuration()
        {
			return duration;
        }

        public override void init(ITransitionEventHandler eventHandler)
		{
			_canvas_group = GetComponent<CanvasGroup>();

			_alpha_damper = FloatSmoothDamper.create(0.0f, duration);
			_scale_damper = FloatSmoothDamper.create(scaling == false ? 1.0f : 0.5f, duration);

			_canvas_group.alpha = _alpha_damper.getCurrent();
			_canvas_group.interactable = false;

			_active = false;

			_eventHandler = eventHandler;
			if( scale_target == null)
			{
				scale_target = transform;
				rect_scaleTarget = scale_target.gameObject.GetComponent<RectTransform>();
			}

			//RectTransform rt = transform as RectTransform;
			//RectTransform rtParent = transform.parent as RectTransform;

			//Debug.Log(string.Format("offset:{0},{1}", rt.offsetMin, rt.offsetMax));
			//Debug.Log(string.Format("anchorPos:{0}", rt.anchoredPosition));
			//Debug.Log(string.Format("parentSize:{0}", rtParent.rect));
		}

		public override float startOpen()
		{
			_isClosingNow = false;
			gameObject.SetActive(true);

			if(noChangeSibling == false)
			{
				transform.SetAsLastSibling();
			}

			_alpha_damper.setTarget(1.0f);
			_scale_damper.setTarget(1.0f);

			_active = true;

			_eventHandler?.onTransitionEvent(TransitionEventType.start_open);

			return duration / 2.0f;
		}

        public override float openImmediately()
        {
			_isClosingNow = false;

			gameObject.SetActive(true);
			_canvas_group.interactable = true;

			if (noChangeSibling == false)
			{
				transform.SetAsLastSibling();
			}

			_alpha_damper.setTarget(1.0f);
			_scale_damper.setTarget(1.0f);
			_alpha_damper.reset(1.0f);
			_scale_damper.reset(1.0f);

			scale_target.localScale = Vector3.one * _scale_damper.getCurrent();
			_canvas_group.alpha = _alpha_damper.getCurrent();

			_active = true;

			_eventHandler?.onTransitionEvent(TransitionEventType.start_open);
			_eventHandler?.onTransitionEvent(TransitionEventType.end_open);

			return 0f;
        }

		public override float closeImmediately(float duration)
		{
			if (noChangeSibling == false)
			{
				transform.SetAsFirstSibling();
			}
			_isClosingNow = true;
			_closeWaitTime = duration;
			return 0f;
		}

		private void closePanel()
		{
			if (noChangeSibling == false)
			{
				transform.SetAsFirstSibling();
			}
			gameObject.SetActive(false);
			_canvas_group.interactable = false;

            _alpha_damper.reset(0.0f);
            _scale_damper.reset(scaling == false ? 1.0f : 0.5f);

            _alpha_damper.setTarget(0.0f);
			_scale_damper.setTarget(scaling == false ? 1.0f : 0.5f);

			scale_target.localScale = Vector3.one * _scale_damper.getCurrent();
			_canvas_group.alpha = _alpha_damper.getCurrent();

			_eventHandler?.onTransitionEvent(TransitionEventType.start_close);
			_eventHandler?.onTransitionEvent(TransitionEventType.end_close);
		}	

		public override float startClose()
		{
			if (noChangeSibling == false)
			{
				transform.SetAsLastSibling();
			}
			_canvas_group.interactable = false;

			_alpha_damper.setTarget(0.0f);
			_scale_damper.setTarget(scaling == false ? 1.0f : 0.5f);

			_active = true;
			_isClosingNow = false;

			_eventHandler?.onTransitionEvent(TransitionEventType.start_close);

			return duration / 2.0f;
		}

		public override bool isActive()
		{
			return _active;
		}

		public override void update()
		{
			if(_isClosingNow)
            {
				if(_closeWaitTime <= 0f)
                {
					closePanel();
					_isClosingNow = false;
                }
				else
                {
					_closeWaitTime -= Time.deltaTime;
                }
            }

			if( _active == false)
			{
				return;
			}

			_active = false;

			if ( _alpha_damper.update())
			{
				_canvas_group.alpha = _alpha_damper.getCurrent();
				_active = true;
			}

			if( _scale_damper.update())
			{
				scale_target.localScale = Vector3.one * _scale_damper.getCurrent();
				_active = true;
			}

			if( _active == false)
			{
				if( _alpha_damper.getTarget() == 0.0f)
				{
					_eventHandler?.onTransitionEvent(TransitionEventType.end_close);
					// 임시코드
					gameObject.SetActive(false);
				}
				else
				{
					_canvas_group.interactable = true;

					_eventHandler?.onTransitionEvent(TransitionEventType.end_open);
				}
			}
		}
	}
}

