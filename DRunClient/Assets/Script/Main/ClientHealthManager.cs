using Festa.Client.Module.Net;
using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace Festa.Client
{
	public class ClientHealthManager
	{
		private AbstractHealthDevice _device;

		private Dictionary<int, List<ClientHealthLogData>> _pendedHealthData;
		private Dictionary<int, HealthLogAppender> _logAppenderMap;
		private ClientNetwork _clientNetwork;
		private ClientViewModel _viewModel;

		private IntervalTimer _timer;
		private IntervalTimer _flushTimer;

		private List<ClientHealthLogData> _tempList = new List<ClientHealthLogData>();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public AbstractHealthDevice getDevice()
		{
			return _device;
		}

		public static ClientHealthManager create()
		{
			ClientHealthManager manager = new ClientHealthManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_pendedHealthData = new Dictionary<int, List<ClientHealthLogData>>();
			_logAppenderMap = new Dictionary<int,HealthLogAppender>();
			_tempList = new List<ClientHealthLogData>();
			_clientNetwork = ClientMain.instance.getNetwork();
			_viewModel = ClientMain.instance.getViewModel();

#if UNITY_EDITOR
			_timer = IntervalTimer.create(3.0f, false, true);
			_flushTimer = IntervalTimer.create(1.0f, false, true);
#else
			_timer = IntervalTimer.create(2.0f, false, true);
			_flushTimer = IntervalTimer.create(2.0f, false, true);
#endif


			createDevice();
		}

		private void createDevice()
		{

#if UNITY_EDITOR
			_device = AbstractHealthDevice.create<HealthDevice_EditorDev>();
#else

#if UNITY_ANDROID
			_device = AbstractHealthDevice.create<HealthDevice_Android>();
#elif UNITY_IPHONE
			_device = AbstractHealthDevice.create<HealthDevice_iOS>();
#endif
#endif

		}

		public void initialQuery(UnityAction callback)
		{
			_tempList.Clear();

			// 최초 가입시 일주일치 걷기 정보를 얻기 위해 
			// 또는 앱이 완전히 종료되었다가 올라왔을 경우에 대해

			ClientHealthLogCumulation cumulation = _viewModel.Health.StepCumulation;

			Debug.Log($"health device init query: last_recorded_time[{cumulation.last_recorded_time}]");

			long lastRecordedTime = TimeUtil.unixTimestampFromDateTime(cumulation.last_recorded_time);

			if( lastRecordedTime < _viewModel.SignIn.LoginTime)
			{
				Debug.LogWarning($"clamp lastRecordTime by LoginTime: {TimeUtil.dateTimeFromUnixTimestamp(lastRecordedTime)} -> {TimeUtil.dateTimeFromUnixTimestamp(_viewModel.SignIn.LoginTime)}");
				lastRecordedTime = _viewModel.SignIn.LoginTime;
			}

			_device.setLastRecordTime(lastRecordedTime);
			_device.queryNewStepRecord(true, _tempList, () => {

				Debug.Log(string.Format("initialQuery record count : {0}", _tempList.Count));

				if (_tempList.Count > 0)
				{
					appendLog(HealthDataType.step, _tempList);

					if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.trip)
					{
						recordCalories_n_Distance(_tempList);
					}
				}

				DateTime nowLocal = DateTime.Now;
				DateTime latestLogTime = TimeUtil.dateTimeFromUnixTimestamp(lastRecordedTime);

				foreach(ClientHealthLogData log in _tempList)
				{
					if( log.record_time > latestLogTime)
					{
						latestLogTime = log.record_time;
					}
				}

				// 날짜가 바뀌었다, 그런데 오늘자 로그가 없다 0이라도 만들어 보내자
				if (latestLogTime.ToLocalTime().Day != nowLocal.Day)
				{
					appendLog(HealthDataType.step, ClientHealthLogData.create(0));
					appendLog(HealthDataType.calories, ClientHealthLogData.create(0));
					appendLog(HealthDataType.distance, ClientHealthLogData.create(0));
				}

				flushToServer(() => {
					callback();
				});
			});
		}

		public void queryAndFlushNow(UnityAction completeHandler)
		{
			_timer.stop();
			_flushTimer.stop();

			queryNewRecord(() => {

				flushToServer(() => {

					_timer.setNext();
					_flushTimer.setNext();

					completeHandler();
				
				});
			});
		}

		public void update()
		{
			_device.update();

			if( _timer.update())
			{
				queryNewRecord(() => {
					_timer.setNext();
				});
			}

			if (_flushTimer.update())
			{
				flushToServer(() => {
					_flushTimer.setNext();
				});
			}
		}

		private void queryNewRecord(UnityAction completeHandler)
		{
			_tempList.Clear();
			_device.queryNewStepRecord(false, _tempList, () =>
			{
				if (_tempList.Count > 0)
				{
					appendLog(HealthDataType.step, _tempList);
					if (ViewModel.Trip.Data.status != ClientTripConfig.StatusType.trip)
					{
						recordCalories_n_Distance(_tempList);
					}
				}

				completeHandler();
			});
		}

		private void flushToServer(UnityAction completeHandler)
		{
			if( _pendedHealthData.Count == 0)
			{
				completeHandler();
				return;
			}

			Dictionary<int, List<ClientHealthLogData>> health_data = _pendedHealthData;
			MapPacket req = _clientNetwork.createReq(CSMessageID.HealthData.RecordHealthDataReq);
			req.put("health_data", health_data);

			_pendedHealthData = new Dictionary<int, List<ClientHealthLogData>>();

			_clientNetwork.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					_viewModel.updateFromPacket(ack);
				}
				else
				{
					// 전송에 실패하면 merge해서 다시 보낼 수 있도록 해보자
					mergeHealthData(_pendedHealthData, health_data);
				}

				completeHandler();
			});
		}

		public void appendLogDouble(int type,DateTime time,double value)
		{
			HealthLogAppender appender = getAppender(type);
			ClientHealthLogData log = appender.append(time, value);
			if( log != null)
			{
				appendLog(type,log);
			}
		}

		private HealthLogAppender getAppender(int type)
		{
			HealthLogAppender appender;
			if (_logAppenderMap.TryGetValue(type, out appender) == false)
			{
				appender = HealthLogAppender.create();
				_logAppenderMap.Add(type, appender);
			}

			return appender;
		}

		public void appendLog(int type,ClientHealthLogData log)
		{
			List<ClientHealthLogData> list = null;
			if( _pendedHealthData.TryGetValue(type, out list) == false)
			{
				list = new List<ClientHealthLogData>();
				_pendedHealthData.Add(type, list);
			}

			list.Add(log);

			//Debug.Log(string.Format("type:[{0}] value[{1}]", type, value));
		}

		public void appendLog(int type,List<ClientHealthLogData> add_list)
		{
			List<ClientHealthLogData> list = null;
			if (_pendedHealthData.TryGetValue(type, out list) == false)
			{
				list = new List<ClientHealthLogData>();
				_pendedHealthData.Add(type, list);
			}

			list.AddRange(add_list);
		}

		private void mergeHealthData(Dictionary<int, List<ClientHealthLogData>> target,Dictionary<int,List<ClientHealthLogData>> data)
		{
			foreach(KeyValuePair<int,List<ClientHealthLogData>> item in data)
			{
				List<ClientHealthLogData> data_list;
				if( target.TryGetValue(item.Key,out data_list) == false)
				{
					data_list = new List<ClientHealthLogData>();
					target.Add(item.Key, data_list);
				}

				data_list.AddRange(item.Value);
			}

			// sorting
			foreach(KeyValuePair<int,List<ClientHealthLogData>> item in target)
			{
				item.Value.Sort((a, b) => { 
					if( a.record_time < b.record_time)
					{
						return -1;
					}
					else if( a.record_time > b.record_time)
					{
						return 1;
					}

					return 0;
				});
			}
		}

		// 걸음수 기반 이동거리, 칼로리 로그 (여정기록중이 아닐때)
		private void recordCalories_n_Distance(List<ClientHealthLogData> log_list)
		{
			double stride = ViewModel.Health.Body.stride;
			if( stride == 0)
			{
				// 68cm
				stride = 68 / 100.0 / 1000.0;
			}

			// 
			double weight = ViewModel.Health.Body.getWeightWithKG();
			if (weight == 0)
			{
				weight = 60.0f;
			}

			foreach (ClientHealthLogData log in log_list)
			{
				int step_count = log.value;

				double distance = (double)step_count * stride;

				// 평균 4km/h
				double speed = 4.0;

				appendLogDouble(HealthDataType.distance, log.record_time, distance * 1000.0);

				double duration_minutes = distance * 60 / speed;
				double calories = ViewModel.Health.calcCalories(ClientTripConfig.TripType.walking, speed, weight, duration_minutes);

				appendLogDouble(HealthDataType.calories, log.record_time, calories * 1000.0);
			}
		}
	}
}
