using Festa.Client.Logic;
using Festa.Client.MapBox;
using Festa.Client.Module.FSM;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;

namespace Festa.Client
{
	public class StatePauseTrip : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.pause_trip;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("trip status...", 80);

			int status = _viewModel.Trip.Data.status;

			if( status == ClientTripConfig.StatusType.trip ||
				status == ClientTripConfig.StatusType.paused)
			{
				ClientMain.instance.getViewModel().Trip.LatestCheeringID = TripCheeringUtil.loadLatestCheeringID(GlobalConfig.gameserver_url);

				// 서비스를 시작 시켜준다 (이전 실행에서 실행 중일 수도 있다)
				//ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.trip);

				if( status == ClientTripConfig.StatusType.trip)
				{
					pauseTrip();
				}
				else
				{
					changeToNextState();
				}
			}
			else
			{
				// 기존 서비스를 종료한다
				//ClientMain.instance.getLocation().changeMode(ClientLocationManager.Mode.normal);

				changeToNextState();
			}
		}

		private void pauseTrip()
		{
			// 2022.08.08 기기를 다시 실행하기 까지 중간 데이터를 query
			//ClientMain.instance.getLocation().updateLocationNow();

			TripPauseProcessor step = TripPauseProcessor.create();
			step.run(result => {
				changeToNextState();
			});

			//MBLongLatCoordinate currentLocation = _viewModel.Location.CurrentLocation;

			//MapPacket req = _network.createReq(CSMessageID.Trip.PauseTripReq);
			//req.put("longitude", currentLocation.lon);
			//req.put("latitude", currentLocation.lat);

			//_network.call(req, ack => { 
			//	if( ack.getResult() == ResultCode.ok)
			//	{
			//		_viewModel.updateFromPacket(ack);
			//		changeToNextState();
			//	}
			//});
		}
	}
}
