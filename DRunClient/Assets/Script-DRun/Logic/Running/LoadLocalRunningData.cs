using DRun.Client.NetData;
using DRun.Client.Running;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.MsgPack;
using Festa.Client.NetData;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DRun.Client.Logic.Running
{
	// 얘는 부분 저장이 필요 없다
	public class LoadLocalRunningData : BaseStepProcessor
	{
		private string _basePath;
		private LocalRunningStatusData _statusData;
		private List<RunningPathData> _pathList;

		private bool _skipLoading;
		private MultiThreadWorker threadWorker => ClientMain.instance.getMultiThreadWorker();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private ClientNFTItem _nftItem;
		private ClientNFTBonus _nftBonus;

		private string _statusFilePath;

		public LocalRunningStatusData getStatusData()
		{
			return _statusData;
		}

		public List<RunningPathData> getPathList()
		{
			return _pathList;
		}

		public static LoadLocalRunningData create(string basePath)
		{
			LoadLocalRunningData processor = new LoadLocalRunningData();
			processor.init(basePath);
			return processor;
		}

		private void init(string basePath)
		{
			base.init();
			_basePath = basePath;
			_skipLoading = false;

			_statusFilePath = _basePath + "/" + "status";
		}

		protected override void buildSteps()
		{
			_stepList.Add(check);
			_stepList.Add(loadStatus);
			_stepList.Add(loatPathList);
		}

		private void check(Handler<AsyncResult<Void>> handler)
		{
			if( File.Exists(_statusFilePath) == false)
			{
				_skipLoading = true;
			}

			_nftItem = ViewModel.ProMode.EquipedNFTItem;
			_nftBonus = ViewModel.ProMode.NFTBonus;

			if( _nftItem == null || _nftBonus == null)
			{
				_skipLoading = true;
			}

			handler(Future.succeededFuture());
		}

		private void loadStatus(Handler<AsyncResult<Void>> handler)
		{
			if(_skipLoading)
			{
				handler(Future.succeededFuture());
				return;
			}

			threadWorker.execute<Void>(promise => {

				try
				{
					byte[] byte_data = File.ReadAllBytes(_statusFilePath);

					MemoryStream readStream = new MemoryStream(byte_data);
					MessageUnpacker msgUnpacker = MessageUnpacker.create(readStream);
					ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

					int version = msgUnpacker.unpackInt();
					if( version != LocalRunningStatusData.VERSION)
					{
						throw new System.Exception($"local running status data version different: file[{version}] code[{LocalRunningStatusData.VERSION}]");
					}
					_statusData = (LocalRunningStatusData)objUnpacker.unpack(msgUnpacker);

					// 2023.04.03 마라톤 모드 이어하기 않되는 버그 수정
					if( _statusData.running_type == ClientRunningLogCumulation.RunningType.promode)
					{
						if (_statusData.nft_token_id != _nftItem.token_id)
						{
							throw new System.Exception($"local running status data nft token_id different: file[{_statusData.nft_token_id}] current[{_nftItem.token_id}]");
						}
					}

					Debug.Log($"load status: status[{_statusData.status}] path_count[{_statusData.path_count}]");

					promise.complete();
				}
				catch(System.Exception e)
				{
					// 지워 놓는게 좋겠다
					
					deleteStatusFile();

					promise.fail(e);
				}
			
			}, result => { 
				if(result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void loatPathList(Handler<AsyncResult<Void>> handler)
		{
			if (_skipLoading)
			{
				handler(Future.succeededFuture());
				return;
			}

			threadWorker.execute<Void>(promise => { 
			
				try
				{
					_pathList = new List<RunningPathData>();
					for(int i = 0; i < _statusData.path_count; ++i)
					{
						_pathList.Add(loadPath(i));
					}

					promise.complete();
				}
				catch(System.Exception e)
				{
					deleteStatusFile();

					promise.fail(e);
				}
			
			}, result => {
				if (result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void deleteStatusFile()
		{
			try
			{
				File.Delete(_statusFilePath);
			}
			catch(System.Exception e)
			{
				Debug.Log(e);	
			}
		}

		private RunningPathData loadPath(int id)
		{
			string file_path = _basePath + $"/path_{id+1}";
			byte[] byte_data = File.ReadAllBytes(file_path);

			MemoryStream readStream = new MemoryStream(byte_data);
			MessageUnpacker msgUnpacker = MessageUnpacker.create(readStream);
			ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

			int version = msgUnpacker.unpackInt();
			if( version != RunningPathData.VERSION)
			{
				throw new System.Exception($"path version different: file[{version}] code[{RunningPathData.VERSION}]");
			}

			RunningPathData path_data = (RunningPathData)objUnpacker.unpack(msgUnpacker);
			path_data.postProcessLoad(_nftItem, _nftBonus);						
			
			Debug.Log($"load path: version[{version}] id[{path_data.getPathID()}] count[{path_data.getRawList().Count}]");

			return path_data;
		}
	}
}
