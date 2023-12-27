using Festa.Client.LocalDB;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class QueryChatLogFromLocalProcessor : BaseStepProcessor
	{
		private LocalChatDataManager LocalChatCache => ClientMain.instance.getLocalChatData();
		
		private ChatRoomViewModel _roomViewModel;
		private int _begin;
		private int _end;

		private List<LDB_ChatRoomLog> _localLogList;

		public static QueryChatLogFromLocalProcessor create(ChatRoomViewModel vm,int begin,int end)
		{
			QueryChatLogFromLocalProcessor p = new QueryChatLogFromLocalProcessor();
			p.init(vm, begin, end);
			return p;
		}

		private void init(ChatRoomViewModel vm,int begin,int end)
		{
			base.init();

			_roomViewModel = vm;
			_begin = begin;
			_end = end;

			Debug.Log($"start query chatlog from local: room_id[{_roomViewModel.ID}] begin[{_begin}] end[{_end}]");
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryLog);
			_stepList.Add(applyViewModel);
		}

		private void queryLog(Handler<AsyncResult<Module.Void>> handler)
		{
			LocalChatCache.readChatLog(_roomViewModel.ID, _begin, _end, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_localLogList = result.result();
					handler(Future.succeededFuture());
				}
			});
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			List<ClientChatRoomLog> log_list = ClientChatRoomLog.create(_localLogList);
			_roomViewModel.appendLogs(log_list);

			handler(Future.succeededFuture());
		}
	}
}
