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
	public class TripGetPhotoNCheeringProcessor : BaseStepProcessor
	{
		private int _account_id;
		private ClientTripLog _log;
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public static TripGetPhotoNCheeringProcessor create(int account_id,ClientTripLog log)
		{
			TripGetPhotoNCheeringProcessor p = new TripGetPhotoNCheeringProcessor();
			p.init(account_id,log);
			return p;
		}

		private void init(int account_id,ClientTripLog log)
		{
			base.init();
			_account_id = account_id;
			_log = log;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryPhoto);
			_stepList.Add(queryCheering);
		}

		private void queryPhoto(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.GetTripPhotoListReq);
			req.put("id", _account_id);
			req.put("sub_id", _log.trip_id);
			req.put("slot_id", 0);	// 전체 목록

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_log._photoList = ack.getList<ClientTripPhoto>("data");
					handler(Future.succeededFuture());
				}
			});
		}

		private void queryCheering(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Trip.GetTripCheeringListReq);
			req.put("id", _account_id);
			req.put("sub_id", _log.trip_id);

			Network.call(req, ack =>
			{
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_log._cheeringList = ack.getList<ClientTripCheering>("data");
					handler(Future.succeededFuture());
				}
			});
		}

	}
}
