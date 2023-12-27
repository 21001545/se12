using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	// open에는 transition기능이 포함되어 있다
	public class LayerJobOpen : AbstractLayerJob
	{
		public class Step
		{
			public const int wait_close = 1;
			public const int wait_open = 2;
		}

		private UIAbstractPanel	_prev;
		private UIAbstractPanel _next;
		private int		_step;
		private float 	_step_end_time;
		private int _transitionType;
		private int _closeType;

		public static LayerJobOpen create(UILayer owner,UIAbstractPanel panel,UIPanelOpenParam param, int transitionType, int closeType)
		{
			LayerJobOpen job = new LayerJobOpen();
			job.init( owner, panel, param, transitionType, closeType);
			return job;
		}

		private void init(UILayer owner,UIAbstractPanel panel,UIPanelOpenParam param, int transitionType, int closeType)
		{
			_owner = owner;
			_openParam = param;
			_next = panel;
			_transitionType = transitionType;
			_closeType = closeType;
		}
		public override void start()
		{
			_prev = _owner.getCurrentPanel();

            if (_prev == null)
            {
                _step = Step.wait_open;
                if (_next.getTransition() != null)
                {
                    if (_transitionType != TransitionEventType.openImmediately)
                        _next.getTransition().startOpen();
                    else
                    {
                        _next.getTransition().openImmediately();
                    }
                }

                _next.getBindingManager().updateAllBindings();
            }
            else
            {
				if(_owner == _next.getLayer())
                {
					if(_closeType == TransitionEventType.openImmediately)
                    {
						_step_end_time = _prev.getTransition().closeImmediately(0);
					}
					else if (_transitionType == TransitionEventType.openImmediately)
                    {
						_step_end_time = _prev.getTransition().startClose();
					}
					else
					{
						//float duration = _next.getTransition().getDuration() + 0.5f;
						float duration = _next.getTransition().getDuration() + _next.getTransition().getDuration();
						_step_end_time = _prev.getTransition().closeImmediately(duration);
					}

				}


                    // 깜빡거릴 수 있네, 
                    // 즉시 오픈 시키자.
                    _step = Step.wait_open;
                    if (_transitionType != TransitionEventType.openImmediately)
                        _step_end_time = _next.getTransition().startOpen();
                    else
                    {
                        _next.getTransition().openImmediately();
                    }
                    _next.getBindingManager().updateAllBindings();

            }
        }

        public override bool run()
		{
/*			_step_end_time -= Time.deltaTime;
			if( _step_end_time <= 0)
			{*/
				if( _step == Step.wait_close)
				{
					_step = Step.wait_open;
					if(_transitionType != TransitionEventType.openImmediately)
						_next.getTransition().startOpen();
					else
                    {
						_next.getTransition().openImmediately();
					}
					_next.getBindingManager().updateAllBindings();
				}
				else
				{
					_owner.setCurrentPanel( _next);
					return true;
				}
			//}

			return false;
		}
	}
}
