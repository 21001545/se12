using DRun.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Globalization;
using UnityEditor.Rendering;
using UnityEngine;

namespace Festa.Client
{
	public class StateSelectServer : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.select_server;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			if( RemoteConfig.getBoolean(RemoteConfig.show_server_list) || Application.isEditor)
			{
				UILoading.getInstance().setProgress("select server...", 30);

				UISelectServer.getInstance().setSelectCallback(selectCallback);
				UISelectServer.getInstance().open();
			}
			else
			{
				selectDefaultServer();
			}
		}

		private void selectCallback(JsonObject server)
		{
			// 서버 점검 중이네
			if (server.contains("under_maintenance"))
			{
				processUnderMaintenance(server.getJsonObject("under_maintenance"));
			}
			else
			{
				GlobalConfig.setupServerAddress(server);

				ClientMain.instance.getNetwork().setPacketURL(GlobalConfig.gameserver_url);
				ClientMain.instance.getNetwork().setFileUploadURL(GlobalConfig.fileupload_url);
				changeToNextState();
			}
		}

		private void selectDefaultServer()
		{
			string default_server = GlobalConfig.buildConfig.getString("default_server");

			JsonArray array = RemoteConfig.getJsonArray(RemoteConfig.server_list);
			for(int i = 0; i < array.size(); ++i)
			{
				JsonObject server = array.getJsonObject(i);
				if( server.getString("name") == default_server)
				{
					selectCallback(server);
					return;
				}
			}

			Debug.LogError($"can't select default server : {default_server}");
			Debug.Log(array.encode());
		}

		private void processUnderMaintenance(JsonObject info)
		{
			string message = info.getString("message");
			DateTime endTime;

			if( DateTime.TryParseExact(info.getString("end_time"), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime) == false)
			{
				endTime = DateTime.Now.AddHours(6.0);
			}

			TimeSpan remainTime = endTime - DateTime.Now;

			string remainTimeMessage;
			if (remainTime.TotalHours < 1.0f)
			{
				remainTimeMessage = $"종료까지 {remainTime.Minutes}분 남았습니다.";
			}
			else
			{
				remainTimeMessage = $"종료까지 {remainTime.Hours}시간 {remainTime.Minutes}분 남았습니다.";
			}

			string popupMessage = $"{message}\n<size=80%>{remainTimeMessage}</size>";

			UIPopup.spawnOK(popupMessage, () => {
				_owner.changeState(ClientStateType.read_remote_config, ClientStateType.select_server);
			});
		}
	}
}
