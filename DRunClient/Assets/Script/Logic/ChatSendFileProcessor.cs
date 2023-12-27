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
	public class ChatSendFileProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private List<string> _file_list;
		private JobProgressItemViewModel _jobProgress;

		private ChatRoomViewModel _roomVM;
		private long _chatroom_id;
		private int _agent_upload_id;
		private int _agent_id;

		private JsonArray _keyList;

		public JobProgressItemViewModel getJobProgress()
		{
			return _jobProgress;
		}

		public static ChatSendFileProcessor create(ChatRoomViewModel roomVM,List<string> file_list)
		{
			ChatSendFileProcessor p = new ChatSendFileProcessor();
			p.init(roomVM, file_list);
			return p;
		}

		private void init(ChatRoomViewModel roomVM,List<string> file_list)
		{
			base.init();

			_roomVM = roomVM;
			_chatroom_id = _roomVM.ID;
			_file_list = file_list;

			_jobProgress = JobProgressItemViewModel.create(JobProgressItemViewModel.JobType.chat_sendfile);
			_roomVM.SendingJobList.add(_jobProgress);
		}

		protected override void buildSteps()
		{
			_stepList.Add(uploadToAgent);
			_stepList.Add(reqSendFile);
			_stepList.Add(sendMessage);
		}

		public override void run(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Status = ClientJobProgressData.Status.running;
			_jobProgress.Progress = 0;

			runSteps(0, _stepList, _logStepTime, result => {
				_roomVM.SendingJobList.remove(_jobProgress);
			});
		}

		private void uploadToAgent(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Status = ClientJobProgressData.Status.running;
			_jobProgress.Progress = 0;

			HttpFileUploader uploader = Network.createFileUploader(_file_list);
			uploader.setProgressCallback(uploadFileProgressCallback);
			uploader.run(ack => { 
				if( ack.getInteger("result") != ResultCode.ok)
				{
					handler(Future.failedFuture(new Exception("upload file fail")));
				}
				else
				{
					_agent_upload_id = ack.getInteger("id");
					_agent_id = ack.getInteger("agent_id");

					handler(Future.succeededFuture());
				}
			});
		}

		private void uploadFileProgressCallback(int progress)
		{
			_jobProgress.Progress = progress * 60 / 100;
		}

		private void reqSendFile(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Progress = 60;

			MapPacket req = Network.createReq(CSMessageID.Chat.SendFileReq);
			req.put("id", _chatroom_id);
			req.put("agent_upload_id", _agent_upload_id);
			req.put("agent_id", _agent_id);

			Network.call(req, ack => {
				_jobProgress.Status = ClientJobProgressData.Status.complete;
				_jobProgress.Progress = 80;
				_jobProgress.ResultCode = ack.getResult();

				if ( ack.getResult() != ResultCode.ok)
				{

					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					try
					{
						_keyList = new JsonArray((string)ack.get("data"));

						for(int i = 0; i < _keyList.size(); ++i)
						{
							Debug.Log(_keyList.getString(i));
						}

						handler(Future.succeededFuture());
					}
					catch (Exception e)
					{
						handler(Future.failedFuture(e));
					}
				}
			});
		}

		private void sendMessage(Handler<AsyncResult<Module.Void>> handler)
		{
			JsonObject payload = new JsonObject();
			payload.put("type", ChatMessageType.photo);
			payload.put("files", _keyList);

			SendDirectMessageProcessor step = SendDirectMessageProcessor.create(_roomVM.DMTargetProfile._accountID, payload, null);
			step.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
