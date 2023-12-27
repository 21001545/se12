using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class TripGetLatestCheeringListProcessor : BaseStepProcessor
	{
		private int _trip_id;
		private int _slot_id;

		private bool _skipQuery;
		private List<ClientTripCheering> _cheeringList;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public static TripGetLatestCheeringListProcessor create()
		{
			TripGetLatestCheeringListProcessor p = new TripGetLatestCheeringListProcessor();
			p.init();
			return p;
		}

		protected override void init()
		{
			base.init();

			_trip_id = ViewModel.Trip.Data.next_trip_id;
			_slot_id = ViewModel.Trip.LatestCheeringID + 1;
			_skipQuery = false;
		}

		protected override void buildSteps()
		{
			_stepList.Add(checkLatest);
			_stepList.Add(query);
			_stepList.Add(setViewModel);
		}

		private void checkLatest(Handler<AsyncResult<Module.Void>> handler)
		{
			if( (ViewModel.Trip.LatestCheeringID + 1) >= ViewModel.Trip.CheeringConfig.next_slot_id)
			{
				_skipQuery = true;
			}
			else
			{
				_skipQuery = false;
			}
			handler(Future.succeededFuture());
		}

		private void query(Handler<AsyncResult<Module.Void>> handler)
		{
			if(_skipQuery)
			{
				handler(Future.succeededFuture());
				return;
			}

			Debug.Log($"query trip cheering: begin_slot_id[{_slot_id}]");

			MapPacket req = Network.createReq(CSMessageID.Trip.GetTripCheeringListReq);
			req.put("id", Network.getAccountID());
			req.put("sub_id", _trip_id);
			req.put("slot_id", _slot_id);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_cheeringList = ack.getList<ClientTripCheering>("data");

					handler(Future.succeededFuture());
				}
			});
		}

		private void setViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			if(_skipQuery)
			{
				handler(Future.succeededFuture());
				return;
			}

			ViewModel.Trip.appendCurrentUnreadTripCheeringList(_cheeringList);

			TripCheeringUtil.saveLatestCheeringID(GlobalConfig.gameserver_url, ViewModel.Trip.LatestCheeringID);

			handler(Future.succeededFuture());
		}
	}
}
