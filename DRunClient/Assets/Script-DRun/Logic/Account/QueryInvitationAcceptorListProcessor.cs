using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.Logic.Account
{
	public class QueryInvitationAcceptorListProcessor : BaseLogicStepProcessor
	{
		private int _begin;
		private int _count;

		private List<ClientInvitationAccept> _list;

		public List<ClientInvitationAccept> getList()
		{
			return _list;
		}

		public static QueryInvitationAcceptorListProcessor create(int begin,int count)
		{
			QueryInvitationAcceptorListProcessor p = new QueryInvitationAcceptorListProcessor();
			p.init(begin, count);
			return p;
		}

		private void init(int begin,int count)
		{
			base.init();
			_begin = begin;
			_count = count;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
			_stepList.Add(queryProfile);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.QueryInvitationAcceptorListReq);
			req.put("begin", _begin);
			req.put("count", _count);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_list = ack.getList<ClientInvitationAccept>("data");

					handler(Future.succeededFuture());
				}
			});
		}

		private void queryProfile(Handler<AsyncResult<Void>> handler)
		{
			queryProfileIter(_list.GetEnumerator(), handler);
		}

		private void queryProfileIter(IEnumerator<ClientInvitationAccept> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientInvitationAccept data = it.Current;

			ClientMain.instance.getProfileCache().getProfileCache(data.acceptor_account_id, result => { 
				if( result.succeeded())
				{
					data._profileCache = result.result();
				}

				queryProfileIter(it, handler);
			});
		}
	}
}
