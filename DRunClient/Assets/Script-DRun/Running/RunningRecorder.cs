using DRun.Client.Logic.Running;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DRun.Client.Running
{
	public class RunningRecorder
	{
		private RecorderFSM _fsm;
		private string _localDataPath;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RunningViewModel runningVM => ViewModel.Running;

		public static RunningRecorder create()
		{
			RunningRecorder recorder = new RunningRecorder();
			recorder.init();
			return recorder;
		}

		private void init()
		{
			_fsm = RecorderFSM.create();
			_fsm.start();

			setupDirectory();
		}

		public void update()
		{
			_fsm.run();
		}

		public void startRecordingProMode()
		{
			startRecording(ClientRunningLogCumulation.RunningType.promode, 1, 0);
		}
		
		public void startRecordingMarathonMode(int marathon_type,int goal)
		{
			startRecording(ClientRunningLogCumulation.RunningType.marathon, marathon_type, goal);
		}

		public void startRecording(int running_type,int running_sub_type,int goal)
		{
			runningVM.resetForStart(running_type,running_sub_type,goal, ViewModel.ProMode.Data, ViewModel.ProMode.EquipedNFTItem, ViewModel.ProMode.NFTBonus, (double)ViewModel.Health.Body.getWeightWithKG());
			runningVM.StartTime = DateTime.UtcNow;
			runningVM.TrackingStartTime = DateTime.UtcNow;
			runningVM.FirstMoveChecked = false;

			_fsm.changeState(StateType.wait_gps);
		}

		public void pause()
		{
			_fsm.changeState(StateType.paused);
		}

		public void resume()
		{
			if( runningVM.GPSChecked == false)
			{
				_fsm.changeState(StateType.wait_gps);
			}
			else if( runningVM.FirstMoveChecked == false)
			{
				_fsm.changeState(StateType.wait_first_move);
			}
			else
			{
				_fsm.changeState(StateType.tracking);
			}
		}

		public void stop()
		{
			_fsm.changeState(StateType.end);
		}

		public void continueFromLocalData(LocalRunningStatusData statusData,List<RunningPathData> pathList)
		{
			StateContinueFromLocalData.Param param = new StateContinueFromLocalData.Param();
			param.statusData = statusData;
			param.pathList = pathList;

			_fsm.changeState(StateType.continue_from_localdata, param);
			
			// 한 스텝 돌려줌
			_fsm.run();	
		}


		private void setupDirectory()
		{
			_localDataPath = Application.persistentDataPath + "/running";

			if( Directory.Exists(_localDataPath) == false)
			{
				try
				{
					Directory.CreateDirectory(_localDataPath);
				}
				catch(Exception e)
				{
					Debug.LogException(e);
				}
			}

			Debug.Log($"running local path: {_localDataPath}");
		}

		public void saveLocalData(bool currentPathOnly)
		{
			LocalRunningStatusData statusData = runningVM.createLocalRunningStatusData();
			List<RunningPathData> pathList = new List<RunningPathData>();

			if( currentPathOnly)
			{
				// 그럴 수 있다 !! tracking단계 까지 못했을때 
				if( runningVM.CurrentPathData != null)
				{
					pathList.Add(runningVM.CurrentPathData);
				}
			}
			else
			{
				pathList.AddRange(runningVM.PathList);
			}

			SaveLocalRunningData step = SaveLocalRunningData.create(_localDataPath, statusData, pathList);
			step.run(result => { });
		}

		public LoadLocalRunningData createLoadLocalData()
		{
			return LoadLocalRunningData.create(_localDataPath);
		}

		public void onBecomeActive(Handler<AsyncResult<Festa.Client.Module.Void>> handler)
		{
			if( _fsm.getCurrentState() is StateTracking)
			{
				StateTracking tracking = _fsm.getCurrentState() as StateTracking;
				tracking.updateNow(() => {
					handler(Future.succeededFuture());
				});
			}
			else
			{
				handler(Future.succeededFuture());
			}
		}
	}
}
