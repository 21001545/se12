using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace DRun.Client.NFT
{
	public class NFTMetadataCacheManager
	{
		private Dictionary<int, NFTMetadataCache> _cacheMap;

		public static NFTMetadataCacheManager create()
		{
			NFTMetadataCacheManager m = new NFTMetadataCacheManager();
			m.init();
			return m;
		}

		private void init()
		{
			_cacheMap = new Dictionary<int, NFTMetadataCache>();
		}

		public void getMetadata(int token_id,UnityAction<NFTMetadataCache> callback)
		{
			if( _cacheMap.ContainsKey(token_id) )
			{
				callback( _cacheMap[token_id]);
				return;
			}

			ClientMain.instance.StartCoroutine(getMetadataFromWeb(token_id, callback));
		}

		private IEnumerator getMetadataFromWeb(int token_id,UnityAction<NFTMetadataCache> callback)
		{
			ClientNFTMetadataConfig config = ClientMain.instance.getViewModel().Wallet.NFTMetadataConfig;
			if( config == null)
			{
				Debug.LogError("nft metadata config is null");
				callback(null);
				yield break;
			}

			string url = config.makeURL(token_id);
			UnityWebRequest request = UnityWebRequest.Get(url);
			request.SetRequestHeader("Content-Type", "application/json");

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(request.error);
				callback(null);
			}
			else
			{
				try
				{
					JsonObject json = new JsonObject(request.downloadHandler.text);

					string imageURL = json.getString("image");

					if( imageURL.StartsWith("ipfs"))
					{
						//imageURL = imageURL.Replace("ipfs://", "https://ipfs.io/ipfs/");
						imageURL = imageURL.Replace("ipfs://", "https://gateway.pinata.cloud/ipfs/"); 
					}

					NFTMetadataCache cache = new NFTMetadataCache();
					cache.metadata = json;
					cache.imageUrl = imageURL;

					if( _cacheMap.ContainsKey(token_id) == false)
					{
						_cacheMap.Add(token_id, cache);
					}

					callback(cache);
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
					callback(null);
				}
			}
		}
	}
}
