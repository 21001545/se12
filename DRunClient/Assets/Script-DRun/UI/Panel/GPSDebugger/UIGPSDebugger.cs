using DRun.client.Logic.Account;
using DRun.Client.Running;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client
{
	public class UIGPSDebugger : UISingletonPanel<UIGPSDebugger>, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UIGPSDebugger_Log cellLog;
		public GameObject loading;

		private List<ClientLocationLog> _logList;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			loading.SetActive(false);

			scroller.Delegate = this;
			_logList = new List<ClientLocationLog>();
		}

		private void queryLog(int lastHours)
		{
			DateTime begin = DateTime.UtcNow.AddHours(-lastHours);

			AbstractGPSTracker tracker = ClientMain.instance.getGPSTracker();
			
			_logList.Clear();
			tracker.getLocationFrom(TimeUtil.unixTimestampFromDateTime(begin), _logList);

			_logList.Sort((a, b) => {
				if (a.event_time < b.event_time)
				{
					return 1;
				}
				else if (a.event_time > b.event_time)
				{
					return -1;
				}

				return 0;
			});

			if( scroller.Container != null)
			{
				scroller.ReloadData();
			}
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				queryLog(1);
			}
		}

		public void onClick_Close()
		{
			close();
		}

		public void onClick_1Hour()
		{
			queryLog(1);
		}

		public void onClick_6Hour()
		{
			queryLog(6);
		}

		public void onClick_WriteLog()
		{
			if( _logList.Count == 0)
			{
				return;
			}

			WriteLogProcessor step = WriteLogProcessor.create();
			JsonObject log = step.appendLog(WriteLogProcessor.LogType.debug);

			JsonObject json_param = new JsonObject();
			log.put("json_param", json_param);

			JsonArray json_loglist = new JsonArray();
			json_param.put("locations", json_loglist);

			foreach(ClientLocationLog location in _logList)
			{
				JsonObject json_location = new JsonObject();
				json_location.put("event_time", location.event_time.ToString("o"));
				json_location.put("longitude", location.longitude);
				json_location.put("latitude", location.latitude);
				json_location.put("altitude", location.altitude);
				json_location.put("accuracy", location.accuracy);
				json_location.put("speed", location.speed);
				json_location.put("speed_accuracy", location.speed_accuracy);

				json_loglist.add(json_location);
			}

			loading.SetActive(true);

			step.run(result => {
				loading.SetActive(false);
			
			});
		}

		#region scroller

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UIGPSDebugger_Log instance = (UIGPSDebugger_Log)scroller.GetCellView(cellLog);
			instance.setup(_logList[dataIndex]);

			return instance;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 49.1f;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _logList.Count;
		}

		#endregion
	}
}
