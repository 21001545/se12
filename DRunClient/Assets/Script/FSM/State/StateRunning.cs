using UnityEngine;
using Festa.Client.Module.FSM;
using Festa.Client.MapBox;
using System.Collections.Generic;
using Festa.Client.Module.Net;

namespace Festa.Client
{
	public class StateRunning : ClientStateBehaviour
	{
		private ClientHealthManager _health;
		private ClientLocationManager _location;

		public override int getType()
		{
			return ClientStateType.running;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_health = ClientMain.instance.getHealth();
			//_location = ClientMain.instance.getLocation();

			//MBLongLatCoordinate pos = new MBLongLatCoordinate(127.05441805010621, 37.50634312319362);

			//UMBOfflineRenderer.getInstance().mapBox.updateCurrentLocation(pos);
			//UMBOfflineRenderer.getInstance().mapBox.startMap();
			//UMBOfflineRenderer.getInstance().mapBox.getControl().moveTo(pos, 16);

			//List<string> file_list = new List<string>();

			//file_list.Add("C:\\work\\LifeFesta\\festa\\FestaClient\\FestaUnityClient\\Assets\\Source\\UI\\Image\\party.png");
			//file_list.Add("C:\\work\\LifeFesta\\festa\\FestaClient\\FestaUnityClient\\Assets\\Source\\UI\\Image\\photo_01.png");

			//HttpFileUploader uploader = _network.createFileUploader(file_list);

			//uploader.run(ack => {
			//	Debug.LogWarning(ack.encode());
			//});

			
		}

		public override void update()
		{
			_health.update();
			_location.update();
		}
	}
}
