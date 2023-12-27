using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;

namespace DRun.Client.Logic.Account
{
	public class ChangeBodyProcessor : BaseStepProcessor
	{
		private ClientBody _body;

		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public static ChangeBodyProcessor create(ClientBody body)
		{
			ChangeBodyProcessor p = new ChangeBodyProcessor();
			p.init(body);
			return p;
		}

		private void init(ClientBody body)
		{
			base.init();
			_body = body;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.ChangeBodyReq);
			req.put("gender", _body.gender);
			req.put("weight", _body.weight);
			req.put("height", _body.height);
			req.put("stride", _body.stride);
			req.put("weight_unit_type", _body.weight_unit_type);
			req.put("height_unit_type", _body.height_unit_type);
			req.put("stride_unit_type", _body.stride_unit_type);

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
	}
}
