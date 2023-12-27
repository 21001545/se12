using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public abstract class CoroutineStepProcessor
	{
		protected delegate IEnumerator StepProcessor(Action<AsyncResult<Void>> callback);
		protected List<StepProcessor> _stepList;
		protected MonoBehaviour _behaviour;

		protected virtual void init(MonoBehaviour behaviour)
		{
			_behaviour = behaviour;
			_stepList = new List<StepProcessor>();
			buildSteps();
		}

		protected abstract void buildSteps();

		protected virtual void runSteps(int index,Action<AsyncResult<Void>> handler)
		{
			if( index == _stepList.Count)
			{
				handler(Future.succeededFuture());
				return;
			}

			_behaviour.StartCoroutine(_stepList[index](result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					runSteps(index + 1, handler);
				}
			}));
		}
	}
}
