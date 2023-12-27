using DRun.Client.Logic;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;

namespace DRun.client.Logic.Account
{
	public class WriteLogProcessor : BaseLogicStepProcessor
	{
		private List<JsonObject> _logList;

		public static class LogType
		{
			public const int normal = 400;
			public const int debug = 401;
		}

		public JsonObject appendLog(int type)
		{
			JsonObject jsonObject = new JsonObject();

			jsonObject.put("event_time", DateTime.UtcNow.ToString("o"));
			jsonObject.put("event_type", type);

			_logList.Add(jsonObject);

			return jsonObject;
		}

		public static WriteLogProcessor create()
		{
			WriteLogProcessor p = new WriteLogProcessor();
			p.init();
			return p;
		}

		protected override void init()
		{
			base.init();

			_logList = new List<JsonObject>();
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Festa.Client.Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.WriteLogReq);
			req.put("data", _logList);

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
