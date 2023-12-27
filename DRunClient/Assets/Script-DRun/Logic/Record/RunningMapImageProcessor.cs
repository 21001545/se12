using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using UnityEngine;

namespace DRun.Client.Logic.Record
{
	public class RunningMapImageProcessor : BaseStepProcessor
	{
		private ClientRunningLog _log;
		private TextureCache _textureCache;
		private Vector2Int _size;

		private int _cacheKey;
		private string _name;
		private TextureCacheItemUsage _textureUsage;

		public ClientRunningLog getLog()
		{
			return _log;
		}

		public TextureCacheItemUsage getTextureUsage()
		{
			return _textureUsage;
		}

		public static RunningMapImageProcessor create(ClientRunningLog log, Vector2Int size, TextureCache textureCache)
		{
			RunningMapImageProcessor p = new RunningMapImageProcessor();
			p.init(log, size, textureCache);
			return p;
		}

		private void init(ClientRunningLog log, Vector2Int size, TextureCache textureCache)
		{
			base.init();

			_size = size;
			_log = log;
			_textureCache = textureCache;
			_name = $"running_log_{log.running_type}_{log.running_id}_{size.x}_{size.y}";
			_cacheKey = EncryptUtil.makeHashCode(_name);
		}

		protected override void buildSteps()
		{
			_stepList.Add(loadFromCache);
			_stepList.Add(buildSnapshot);
		}

		private void loadFromCache(Handler<AsyncResult<Void>> handler)
		{
			_textureUsage = _textureCache.makeUsage(_cacheKey);
			handler(Future.succeededFuture());
		}

		private void buildSnapshot(Handler<AsyncResult<Void>> handler)
		{
			if (_textureUsage != null)
			{
				handler(Future.succeededFuture());
				return;
			}

			UMBRunningImageJob job = UMBRunningImageJob.create(UMBOfflineRenderer.getInstance(), _log, _size, texture =>
			{
				if (texture == null)
				{
					handler(Future.failedFuture(new System.Exception("texture is null")));
				}
				else
				{
					texture.name = _name;

					_textureCache.registerCache(_cacheKey, texture);
					_textureUsage = _textureCache.makeUsage(_cacheKey);
					handler(Future.succeededFuture());
				}
			});

			UMBOfflineRenderer.getInstance().enqueueJob(job);
		}


	}
}