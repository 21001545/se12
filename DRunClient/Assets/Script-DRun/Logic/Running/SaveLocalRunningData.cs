using DRun.Client.Running;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.MsgPack;
using System.Collections.Generic;
using System.IO;

// 2023-01-11 이강희 multithread가 3개가 돌고 있다보니 동시에 여러번 저장될 수 도 있다

namespace DRun.Client.Logic.Running
{
	public class SaveLocalRunningData : BaseStepProcessor
	{
		private string _basePath;
		private LocalRunningStatusData _statusData;
		private List<RunningPathData> _pathList;

		private MultiThreadWorker threadWorker => ClientMain.instance.getMultiThreadWorker();

		private static bool _saving = false;

		public static SaveLocalRunningData create(string basePath,LocalRunningStatusData statusData,List<RunningPathData> pathList)
		{
			SaveLocalRunningData processor = new SaveLocalRunningData();
			processor.init(basePath, statusData, pathList);
			return processor;
		}

		private void init(string basePath, LocalRunningStatusData statusData,List<RunningPathData> pathList)
		{
			base.init();
			_basePath = basePath;
			_statusData = statusData;

			// 멀티쓰레드로 write하는 중에 갱신되는 버그가 있다			
			_pathList = new List<RunningPathData>();
			foreach(RunningPathData path in pathList)
			{
				_pathList.Add(path.cloneForSave());
			}
		}

		protected override void buildSteps()
		{
			_stepList.Add(save);
		}

		public override void run(Handler<AsyncResult<Void>> handler)
		{
			// 그럴 수 있다
			if( _saving)
			{
				MainThreadDispatcher.dispatchFixedUpdate(() => {
					run(handler);
				});
			}
			else
			{
				runSteps(0, _stepList, _logStepTime, handler);
			}
		}

		private void save(Handler<AsyncResult<Void>> handler)
		{
			_saving = true;

			threadWorker.execute<Void>(promise => { 
			
				try
				{
					saveStatusData();
					savePathData();

					promise.complete();
				}
				catch(System.Exception e)
				{
					promise.fail(e);
				}
			}, result => {
				_saving = false;

				if ( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}

			});
		}

		private void saveStatusData()
		{
			string file_path = _basePath + "/" + "status";

			using (MemoryStream writeStream = new MemoryStream())
			{
				MessagePacker msgPacker = MessagePacker.create(writeStream);
				ObjectPacker objPacker = ObjectPacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

				msgPacker.resetStream();
				msgPacker.packInt(LocalRunningStatusData.VERSION);
				objPacker.pack(msgPacker, _statusData);

				byte[] payload = new byte[msgPacker.getTotalWrittenBytes()];
				System.Array.Copy(writeStream.GetBuffer(), payload, msgPacker.getTotalWrittenBytes());

				File.WriteAllBytes(file_path, payload);
			}
		}

		private void savePathData()
		{
			using (MemoryStream writeStream = new MemoryStream())
			{
				MessagePacker msgPacker = MessagePacker.create( writeStream);
				ObjectPacker objPacker = ObjectPacker.create( GlobalObjectFactory.getInstance(), SerializeOption.ALL);

				foreach (RunningPathData path in _pathList)
				{
					string file_path = _basePath + $"/path_{path.getPathID()}";
					
					msgPacker.resetStream();
					msgPacker.packInt(RunningPathData.VERSION);
					objPacker.pack(msgPacker, path);

					byte[] payload = new byte[msgPacker.getTotalWrittenBytes()];
					System.Array.Copy(writeStream.GetBuffer(), payload, msgPacker.getTotalWrittenBytes());
					File.WriteAllBytes(file_path, payload);

					//Debug.Log($"save path: version[{1}] id[{path.path_id}] count[{path.getLocationCount()}]");
				}
			}
		}

	}

}

