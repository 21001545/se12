using Festa.Client.MapBox;
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
	public class TripLogSnaphotProcessor : BaseStepProcessor
	{
		private ClientTripLog _log;
		private TextureCache _textureCache;
		private Vector2Int _size;

		private int _cacheKey;
		private string _name;
		private TextureCacheItemUsage _textureUsage;

		public ClientTripLog getLog()
		{
			return _log;
		}

		public TextureCacheItemUsage getTextureUsage()
		{
			return _textureUsage;
		}

		public static TripLogSnaphotProcessor create(ClientTripLog log, Vector2Int size,TextureCache textureCache)
		{
			TripLogSnaphotProcessor p = new TripLogSnaphotProcessor();
			p.init(log, size,textureCache);
			return p;
		}

		private void init(ClientTripLog log,Vector2Int size,TextureCache textureCache)
		{
			base.init();

			_size = size;
			_log = log;
			_textureCache = textureCache;
			_name = $"trip_log_{log.trip_id}_{size.x}_{size.y}";
			_cacheKey = EncryptUtil.makeHashCode(_name);
		}

		protected override void buildSteps()
		{
			_stepList.Add(loadFromCache);
			_stepList.Add(buildSnapshot);
		}

		private void loadFromCache(Handler<AsyncResult<Module.Void>> handler)
		{
			_textureUsage = _textureCache.makeUsage(_cacheKey);
			handler(Future.succeededFuture());
		}

		private void buildSnapshot(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _textureUsage != null)
			{
				handler(Future.succeededFuture());
				return;
			}

			UMBOfflineRenderer.getInstance().buildForTripPath(_log.path_data, _size,texture => { 
				if( texture == null)
				{
					handler(Future.failedFuture(new Exception("texture is null")));
				}
				else
				{
					texture.name = _name;

					_textureCache.registerCache(_cacheKey, texture);
					_textureUsage = _textureCache.makeUsage(_cacheKey);
					handler(Future.succeededFuture());
				}
			});
		}

	}
}
