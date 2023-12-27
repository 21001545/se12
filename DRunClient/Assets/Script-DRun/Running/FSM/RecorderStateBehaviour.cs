using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client.Running
{
	public abstract class RecorderStateBehaviour : StateBehaviour<object>
	{
		protected List<ClientLocationLog> _queryLocationList;

		public RunningRecorder Recorder => ClientMain.instance.getRunningRecorder();
		public RunningViewModel ViewModel => ClientMain.instance.getViewModel().Running;
		public ProModeViewModel ProVM => ClientMain.instance.getViewModel().ProMode;
		public HealthViewModel HealthVM => ClientMain.instance.getViewModel().Health;
		public AbstractGPSTracker GPSTracker => ClientMain.instance.getGPSTracker();
		public AbstractHealthDevice StepCounter => ClientMain.instance.getHealth().getDevice();

		public static RecorderStateBehaviour create<T>() where T : RecorderStateBehaviour, new()
		{
			T state = new T();
			state.init();
			return state;
		}

		protected virtual void init()
		{
			_queryLocationList = new List<ClientLocationLog>();
		}

		//protected bool isMinable()
		//{
		//	// 마라톤 모드는 채굴 않됨
		//	if( ViewModel.isMarathonMode())
		//	{
		//		return false;
		//	}

		//	return ViewModel.StatData.isMinable();

		//	//if( ProVM.EquipedNFTItem.distance <= 0)
		//	//{
		//	//	return false;
		//	//}

		//	//// heart는 무제한 모드가 있다
		//	//if( ProVM.EquipedNFTItem.max_heart != 0 &&
		//	//	ProVM.EquipedNFTItem.heart <= 0)
		//	//{
		//	//	return false;
		//	//}

		//	//return true;
		//}

		//public class StepCountSum
		//{
		//	public int count = 0;
		//}

		////
		//protected void queryStepCountIter(IEnumerator<RunningPathData> it,UnityAction callback)
		//{
		//	if( it.MoveNext() == false)
		//	{
		//		callback();
		//		return;
		//	}

		//	RunningPathData path = it.Current;
		//	List<RunningPathData.SubPath> subPathList = path.getCountableSubPathList();
		//	StepCountSum sum = new StepCountSum();

		//	queryStepCountIter(path, sum, subPathList.GetEnumerator(), () => {
		//		path.updateStepCount(sum.count);
		//		queryStepCountIter(it, callback);
		//	});
		//}

		//protected void queryStepCountIter(RunningPathData path,StepCountSum sum,IEnumerator<RunningPathData.SubPath> it,UnityAction callback)
		//{
		//	if( it.MoveNext() == false)
		//	{
		//		callback();
		//		return;
		//	}

		//	RunningPathData.SubPath subPath = it.Current;
		//	LongVector2 timeRange = path.getSubPathTimeRange(subPath);

		//	if( timeRange.x == timeRange.y)
		//	{
		//		queryStepCountIter(path, sum, it, callback);
		//		return;
		//	}

		//	StepCounter.queryStepCountRange(timeRange.x, timeRange.y - 1, stepCount => {
		//		sum.count += stepCount;
		//		queryStepCountIter(path, sum, it, callback);
		//	});
		//}

		//protected void updateAllPathStepCount(UnityAction callback)
		//{
		//	List<RunningPathData> pathList = new List<RunningPathData>();
		//	pathList.AddRange( ViewModel.PathList);

		//	queryStepCountIter(pathList.GetEnumerator(), callback);
		//}

		protected void updatePathStepCount(RunningPathData path,UnityAction callback)
		{
			List<RunningPathData.SubPath> subPathList = new List<RunningPathData.SubPath>();
			subPathList.AddRange(path.getTempSubPathList());

			updateSubPathStepCount(0, path, subPathList.GetEnumerator(), () => {
				path.processAfterUpdateStepCount();
				callback();
			});
		}

		protected void updateSubPathStepCount(int stack_count,RunningPathData path,IEnumerator<RunningPathData.SubPath> it,UnityAction callback)
		{
			if( it.MoveNext() == false)
			{
				callback();
				return;
			}

			RunningPathData.SubPath subPath = it.Current;
			LongVector2 timeRange = path.getSubPathTimeRange(subPath);

			if( stack_count > 0)
			{
				timeRange.x += TimeUtil.msSecond;
			}

			StepCounter.queryStepCountRange(timeRange.x, timeRange.y, stepCount => {
				
				subPath.stepCount = stepCount;

//#if UNITY_EDITOR
//				int stepCountPerOneKM = stepCount > 0 ? (int)((double)stepCount / subPath.distance) : 0;
//				DateTime beginTime = TimeUtil.dateTimeFromUnixTimestamp(timeRange.x);
//				DateTime endTime = TimeUtil.dateTimeFromUnixTimestamp(timeRange.y);

//				Debug.Log($"subPath[{stack_count}] stepCount[{stepCount}] stepCountPerOneKM[{stepCountPerOneKM}] timeRange[{beginTime.ToString("HH:mm:ss")},{endTime.ToString("HH:mm:ss")}]");
//#endif
				bool need_dispatch = (stack_count / 50) != ((stack_count + 1) / 50);

				if( need_dispatch)
				{
					MainThreadDispatcher.dispatch(() => {
						updateSubPathStepCount(stack_count + 1, path, it, callback);
					});
				}
				else
				{
					updateSubPathStepCount(stack_count + 1, path, it, callback);
				}
			});
		}
	}
}
