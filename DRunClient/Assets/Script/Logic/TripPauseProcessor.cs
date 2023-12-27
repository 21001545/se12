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
	public class TripPauseProcessor : TripProcessorBase
	{
		private ClientTripPathData _newPathData;

		public ClientTripPathData getNewPathData()
		{
			return _newPathData;
		}

		public static TripPauseProcessor create()
		{
			TripPauseProcessor p = new TripPauseProcessor();
			p.init();
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(flushHealth);
			_stepList.Add(flushLocation);
			_stepList.Add(reqPause);
		}

		private void reqPause(Handler<AsyncResult<Module.Void>> handler)
		{
			MBLongLatCoordinate currentLocation = ViewModel.Location.CurrentLocation;
			double currentAltitude = ViewModel.Location.CurrentAltitude;

			MapPacket req = Network.createReq(CSMessageID.Trip.PauseTripReq);
			req.put("longitude", currentLocation.lon);
			req.put("latitude", currentLocation.lat);
			req.put("altitude", currentAltitude);

			Network.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					// 마지막 시간 갱신
					long end_time = (long)ack.get("end");
					ViewModel.Trip.CurrentTripPathData.path_end_time = end_time;

					_newPathData = (ClientTripPathData)ack.get("trip_path");
					ViewModel.Trip.CurrentTripPathDataList.Add(_newPathData);

					ViewModel.updateFromPacket(ack);

					handler(Future.succeededFuture());
				}
				else
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
			});
		}
	}
}
