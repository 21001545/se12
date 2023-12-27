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

namespace Festa.Client.Logic
{
	public class TripResumeProcessor : TripProcessorBase
	{
		protected override void buildSteps()
		{

		}
		//private int _tripType;
		//private ClientTripPathData _newPathData;

		//public ClientTripPathData getNewPathData()
		//{
		//	return _newPathData;
		//}

		//public static TripResumeProcessor create(int tripType)
		//{
		//	TripResumeProcessor p = new TripResumeProcessor();
		//	p.init(tripType);
		//	return p;
		//}

		//private void init(int tripType)
		//{
		//	base.init();
		//	_tripType = tripType;
		//}

		//protected override void buildSteps()
		//{
		//	_stepList.Add(flushHealth);
		//	_stepList.Add(flushLocation);
		//	_stepList.Add(reqResume);
		//}

		//private void reqResume(Handler<AsyncResult<Module.Void>> handler)
		//{
		//	MBLongLatCoordinate currentLocation = ViewModel.Location.CurrentLocation;
		//	double currentAltitude = ViewModel.Location.CurrentAltitude;

		//	MapPacket req = Network.createReq(CSMessageID.Trip.ResumeTripReq);

		//	req.put("longitude", currentLocation.lon);
		//	req.put("latitude", currentLocation.lat);
		//	req.put("altitude", currentAltitude);
		//	req.put("type", _tripType);

		//	Network.call(req, ack => {
		//		if (ack.getResult() == ResultCode.ok)
		//		{
		//			ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.trip);

		//			//새로운 path등록
		//			_newPathData = (ClientTripPathData)ack.get("trip_path");
		//			ViewModel.Trip.CurrentTripPathDataList.Add(_newPathData);

		//			ViewModel.updateFromPacket(ack);

		//			handler(Future.succeededFuture());
		//		}
		//		else
		//		{
		//			handler(Future.failedFuture(ack.makeErrorException()));
		//		}
		//	});
		//}
	}
}
