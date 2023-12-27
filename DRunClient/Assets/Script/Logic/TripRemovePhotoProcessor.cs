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
	public class TripRemovePhotoProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private ClientTripPhoto _data;

		public static TripRemovePhotoProcessor create(ClientTripPhoto photo)
		{
			TripRemovePhotoProcessor p = new TripRemovePhotoProcessor();
			p.init(photo);
			return p;
		}

		private void init(ClientTripPhoto photo)
		{
			base.init();
			_data = photo;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqRemovePhoto);
			_stepList.Add(updateViewModel);
		}

		private void reqRemovePhoto(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.RemovePhotoReq);
			req.put("id", _data.trip_id);
			req.put("slot_id", _data.slot_id);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		
		}

		private void updateViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Trip.CurrentTripPhotoList.Remove(_data);
			handler(Future.succeededFuture());
		}
	}
}
