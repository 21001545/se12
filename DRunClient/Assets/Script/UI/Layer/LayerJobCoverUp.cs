using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Festa.Client.Module.UI;

namespace Festa.Client
{
	public class LayerJobCoverUp : AbstractLayerJob
	{
		public class Step
		{
			public const int wait_open = 1;
		}

		private UIAbstractPanel _prev;
		private UIAbstractPanel _next;
		private int _step;
		private float _step_end_time;
		private int _direction;

		public static LayerJobCoverUp create(UILayer owner,UIAbstractPanel panel,UIPanelOpenParam param, int transitionType)
		{
			LayerJobCoverUp job = new LayerJobCoverUp();
			job.init(owner, panel, param);
			return job;
		}

		private void init(UILayer owner,UIAbstractPanel panel,UIPanelOpenParam param)
		{
			_openParam = param;
			_owner = owner;
			_next = panel;

			if( param != null)
			{
				_direction = (int)param.getWithDefault("direction", 0);
			}
			else
			{
				_direction = 0;
			}
		}

		public override void start()
		{
			_prev = _owner.getCurrentPanel();

			_step = Step.wait_open;

			// 기존에 UI가 없다면 바로 뿅
			CoverUpTransition transition = _next.getTransition() as CoverUpTransition;
			if ( _prev == null)
			{
				transition.openNow();
				_step_end_time = 0;
			}
			else
			{
				CoverUpTransition prev_transition = _prev.getTransition() as CoverUpTransition;

				prev_transition.startClose(_direction);
				_step_end_time = transition.startOpen(_direction);

			}

			_next.getBindingManager().updateAllBindings();
		}


		public override bool run()
		{
			_step_end_time -= Time.deltaTime;
			if( _step_end_time <= 0)
			{
				if( _step == Step.wait_open && _next.getTransition().isActive() == false)
				{
					//if( _prev != null)
					//{
					//	_prev.getTransition().startClose();
					//}

					_owner.setCurrentPanel(_next);
					return true;
				}
			}

			return false;
		}


	}
}
