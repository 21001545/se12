using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class TripEndProcessor : TripProcessorBase
	{
		//private ClientTripConfig _tripConfig;
		//private ClientTripLog _tripLog;

		//public ClientTripLog getTripLog()
		//{
		//	return _tripLog;
		//}

		//public static TripEndProcessor create(ClientTripConfig tripConfig)
		//{
		//	TripEndProcessor p = new TripEndProcessor();
		//	p.init(tripConfig);
		//	return p;
		//}

		//private void init(ClientTripConfig tripConfig)
		//{
		//	base.init();
		//	_tripConfig = tripConfig;
		//}

		//protected override void buildSteps()
		//{
		//	_stepList.Add(flushHealth);
		//	_stepList.Add(flushLocation);
		//	_stepList.Add(reqEnd);
		//}

		//private string makeDefaultName()
		//{
		//	return GlobalRefDataContainer.getStringCollection().getFormat("triproute.default_name", 0, _tripConfig.next_trip_id);
		//}

		//private void reqEnd(Handler<AsyncResult<Module.Void>> handler)
		//{
		//	MBLongLatCoordinate currentLocation = ViewModel.Location.CurrentLocation;
		//	double currentAltitude = ViewModel.Location.CurrentAltitude;

		//	MapPacket req = Network.createReq(CSMessageID.Trip.EndTripReq);
		//	req.put("longitude", currentLocation.lon);
		//	req.put("latitude", currentLocation.lat);
		//	req.put("altitude", currentAltitude);
		//	req.put("name", makeDefaultName());

		//	Network.call(req, ack => {
		//		//tripPlay.interactable = true;

		//		if (ack.getResult() == ResultCode.ok)
		//		{
		//			ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.normal);
		//			Screen.sleepTimeout = SleepTimeout.SystemSetting;

		//			ViewModel.Trip.CurrentTripPathDataList.Clear();
		//			ViewModel.Trip.CurrentTripPhotoList.Clear();
		//			ViewModel.updateFromPacket(ack);

		//			_tripLog = ack.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log)[0];
		//			_tripLog._cheeringList = ack.getList<ClientTripCheering>(MapPacketKey.ClientAck.trip_cheering);
		//			_tripLog._photoList = ack.getList<ClientTripPhoto>(MapPacketKey.ClientAck.trip_photo);

		//			handler(Future.succeededFuture());
		//		}
		//		else
		//		{
		//			handler(Future.failedFuture(ack.makeErrorException()));
		//		}
		//	});

		//}
		protected override void buildSteps()
		{

		}
	}
}
