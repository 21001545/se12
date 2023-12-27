using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module.UI
{
	public class LayerJobClose : AbstractLayerJob
	{
		private UIAbstractPanel	_target;
		private float _remain_time;
		private float _transitionType;

		public static LayerJobClose create(UILayer owner,UIAbstractPanel target, int transitionType)
		{
			LayerJobClose job = new LayerJobClose();
			job.init( owner, target, null, transitionType);
			return job;
		}

		private void init(UILayer owner,UIAbstractPanel target,UIPanelOpenParam param, int transitionType)
		{
			_openParam = param;
			_owner = owner;
			_target = target;
			_transitionType = transitionType;
		}

		public override void start()
		{
			//_prev = 
			if (_target.getTransition() != null)
            {
                if (_transitionType == TransitionEventType.openImmediately)
                    _remain_time = _target.getTransition().closeImmediately(0f);
                else
                    _remain_time = _target.getTransition().startClose();
            }
            else
                _remain_time = 0.0f;
        }

        public override bool run()
		{
			_remain_time -= Time.deltaTime;
			// 실제로 완전히 닫힐 때까지 기다려 준다
			if( _remain_time <= 0 && (_target.getTransition() == null ||_target.getTransition().isActive() == false))
			{
				// 2022.6.24 이강희 UIPopup같은 instance layer에 Scene에 계속 쌓이는 이슈가 있어서 아래 코드 다시 살림
				// 2022.07.03 소현 : 같은 fixed 레이어 내에서 전환할 때 직전 레이어가 닫히지 않아서 일단 이렇게 수정했습니다!!
				if(!_owner.isFixed())
					_owner.setCurrentPanel(null);
				return true;
			}

			return false;
		}
	}
}

