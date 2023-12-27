using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class SearchAccountProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManger;
		private ClientViewModel _viewModel;

		public class QueryType
		{
			public const int name = 1;
			public const int phone_number = 2;
		}

		private int _type;
		private string _name;
		private int _reqMessageID;

		private List<int> _accountList;
		private List<ClientProfileCache> _resultList;

		public List<ClientProfileCache> getResultList()
		{
			return _resultList;
		}

		public static SearchAccountProcessor create(int type, string name)
		{
			SearchAccountProcessor processor = new SearchAccountProcessor();
			processor.init(type, name);
			return processor;
		}

		private void init(int type,string name)
		{
			base.init();

			_network = ClientMain.instance.getNetwork();
			_profileCacheManger = ClientMain.instance.getProfileCache();
			_viewModel = ClientMain.instance.getViewModel();

			_type = type;
			_name = name;
			_reqMessageID = _type == QueryType.name ? CSMessageID.Account.SearchByNameReq : CSMessageID.Account.SearchByPhoneNumberReq;
		}

		protected override void buildSteps()
		{
			_stepList.Add(search);
			_stepList.Add(loadProfiles);
		}

		private void search(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(_reqMessageID);
			req.put("name", _name);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_accountList = ack.getList<int>("account_id");
					handler(Future.succeededFuture());
				}
			});
		}

		private void loadProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			_resultList = new List<ClientProfileCache>();
			loadProfileIter(_accountList.GetEnumerator(), handler);
		}

		private void loadProfileIter(List<int>.Enumerator e, Handler<AsyncResult<Module.Void>> handler)
		{
			if (e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			int account_id = e.Current;

			_profileCacheManger.getProfileCache(account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_resultList.Add(result.result());

					loadProfileIter(e, handler);
				}
			});
		}
	}
}
