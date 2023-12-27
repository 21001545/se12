using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class TripStartProcessor : TripProcessorBase
	{
		private int _tripType;

		//private ClientLocationManager Location => ClientMain.instance.getLocation();

		public static TripStartProcessor create(int tripType)
		{
			TripStartProcessor p = new TripStartProcessor();
			p.init(tripType);
			return p;
		}

		private void init(int tripType)
		{
			base.init();

			_tripType = tripType;
		}

		protected override void buildSteps()
		{
			_stepList.Add(waitGPS);
			_stepList.Add(flushHealth);
			_stepList.Add(flushLocation);
			_stepList.Add(reqStart);
		}

		private void waitGPS(Handler<AsyncResult<Module.Void>> handler)
		{
			ClientMain.instance.StartCoroutine(_waitGPS(handler));
		}

		private IEnumerator _waitGPS(Handler<AsyncResult<Module.Void>> handler)
		{
			yield break;

			//UITripStatus.getInstance().showGPSWait();

			//yield return new WaitForSeconds(2.0f);

			//UITripStatus.getInstance().hideGPSWait();

			//handler(Future.succeededFuture());

			//if (Location.getDevice().getLastQueryTime() > ViewModel.Trip.TryStartTripTime)
			//{
			//	handler(Future.succeededFuture());
			//	yield break;
			//}

			//yield return new WaitForSeconds(0.1f);

			//UITripStatus.getInstance().showGPSWait();

			//while(true)
			//{
			//	Debug.Log($"waitGPS:LastQuerTime[{Location.getDevice().getLastQueryTime()}], TryStartTripTime[{ViewModel.Trip.TryStartTripTime}]");

			//	yield return new WaitForSeconds(0.5f);

			//	if (Location.getDevice().getLastQueryTime() > ViewModel.Trip.TryStartTripTime)
			//	{
			//		UITripStatus.getInstance().hideGPSWait();
			//		handler(Future.succeededFuture());
			//		break;
			//	}
			//}
		}

		private void reqStart(Handler<AsyncResult<Module.Void>> handler)
		{
			MBLongLatCoordinate currentLocation = ViewModel.Location.CurrentLocation;
			double currentAltitude = ViewModel.Location.CurrentAltitude;

			MapPacket req = Network.createReq(CSMessageID.Trip.BeginTripReq);
			req.put("longitude", currentLocation.lon);
			req.put("latitude", currentLocation.lat);
			req.put("altitude", currentAltitude);
			req.put("type", _tripType);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					// 좀더 앞단에서 서비스를 시작해주기로함
					//ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.trip);

					ViewModel.Trip.CurrentTripPathDataList.Clear();
					ViewModel.Trip.CurrentTripPhotoList.Clear();

					ClientTripPathData newPathData = (ClientTripPathData)ack.get("trip_path");
					ViewModel.Trip.CurrentTripPathDataList.Add(newPathData);

					ViewModel.Trip.resetCurrentUnreadTripCheeringList();

					TripCheeringUtil.saveLatestCheeringID(GlobalConfig.gameserver_url, ViewModel.Trip.LatestCheeringID);

					ViewModel.updateFromPacket(ack);

					Screen.sleepTimeout = SleepTimeout.NeverSleep;

					handler(Future.succeededFuture());
				}
				else
				{
					// 실패나면 다시 되돌림
					//ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.normal);
					handler(Future.failedFuture(ack.makeErrorException()));
				}
			});
		}
	}
}
