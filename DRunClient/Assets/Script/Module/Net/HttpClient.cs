using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Festa.Client.Module.MsgPack;
using System;

namespace Festa.Client.Module.Net
{
    public class HttpClient : MonoBehaviour
    {
		private HttpPacketCodec _codec;
		private Queue<Action> _callQueue;
		private bool _calling;

		public static HttpClient create(GameObject target,ObjectFactory objectFactory,int option_mask)
		{
			HttpClient client = target.AddComponent<HttpClient>();
			client.init(objectFactory,option_mask);
			return client;
		}

		private void init(ObjectFactory objectFactory,int option_mask)
		{
			_codec = HttpPacketCodec.create(objectFactory, option_mask);
			_callQueue = new Queue<Action>();
		}

        public void call(string url,MapPacket req,System.Action<MapPacket> callback)
		{
			_callQueue.Enqueue(() => {

				StartCoroutine(_Call(url, req, callback));

			});


//			StartCoroutine(_Call(url,req, callback));
		}

		public void callParallel(string url,MapPacket req,System.Action<MapPacket> callback)
		{
			StartCoroutine(_Call(url, req, callback));
		}

		public void update()
		{
			if( _callQueue.Count > 0 && _calling == false)
			{
				Action call = _callQueue.Dequeue();
				call();
			}
		}

		private IEnumerator _Call(string url,MapPacket req,System.Action<MapPacket> callback)
		{
			_calling = true;
			UnityWebRequest request = _codec.makeRequest(url, req);
			yield return request.SendWebRequest();

			MapPacket ack = null;

			if( request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(request.error);
				ack = MapPacket.createWithMsgID(CSMessageID.ReqError);
				ack.put(MapPacketKey.source_msg_id, req.getID());
				ack.put(MapPacketKey.result, ResultCode.error_http_call_error);
			}
			else
			{
				//Debug.Log(string.Format("http ack : {0}", request.downloadHandler.data.Length));

				try
				{

					ack = _codec.decode(request.downloadHandler.data);


					//if (ack.contains("vectortile_data"))
					//{
					//	BlobData blob = (BlobData)ack.get("vectortile_data");
					//	Debug.Log(string.Format("vectortile : {0}", blob.getData().Length));
					//}

					if( ack.getResult() != ResultCode.ok)
					{
						Debug.LogException(ack.makeErrorException(req.getID()));
					}

					ack.put(MapPacketKey.source_msg_id, req.getID());
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					ack = MapPacket.createWithMsgID(CSMessageID.ReqError);
					ack.put(MapPacketKey.source_msg_id, req.getID());
					ack.put(MapPacketKey.result, ResultCode.error);
				}
			}

			request.Dispose();
			_calling = false;

			callback(ack);
		}
	}
}

