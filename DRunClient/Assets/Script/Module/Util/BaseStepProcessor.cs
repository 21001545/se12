using Festa.Client.Module.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module
{
	public abstract class BaseStepProcessor
	{
		public delegate void StepProcessor(Handler<AsyncResult<Void>> callback);
		protected List<StepProcessor> _stepList;
		protected bool _logStepTime = false;

		protected virtual void init()
		{
			_stepList = new List<StepProcessor>();
			buildSteps();
		}

		protected abstract void buildSteps();

		public virtual void run(Handler<AsyncResult<Void>> handler)
		{
			runSteps(0, _stepList, _logStepTime,handler);
		}

		public static void runSteps(IEnumerator<BaseStepProcessor> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			BaseStepProcessor processor = it.Current;
			processor.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					runSteps(it, handler);
				}
			});
		}


		public static void runSteps(int index,List<StepProcessor> stepList,bool logStepTime,Handler<AsyncResult<Void>> handler)
		{
			if (index == stepList.Count)
			{
				handler(Future.succeededFuture());
				return;
			}

			StepProcessor step = stepList[index];
			float start_time = Time.realtimeSinceStartup;

			step(result => {
				if (logStepTime)
				{
					Debug.Log($"step[{step.Method.Name}] takes {Mathf.FloorToInt((Time.realtimeSinceStartup - start_time) * 1000)}ms");
				}

				if (result.failed())
				{
					Logger.logException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					runSteps(index + 1, stepList, logStepTime, handler);
				}
			});
		}

	}
}
