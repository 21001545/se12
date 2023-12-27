using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientStickerBoard
	{
		public BlobData data;
		public DateTime update_time;

		[SerializeOption(SerializeOption.NONE)]
		private JsonObject _jsonCache;

		private static JsonObject _emptyJsonData = new JsonObject("{\"version\":1,\"list\":[]}");

		public JsonObject getJsonData()
		{
			if( _jsonCache == null)
			{
				_jsonCache = parseData();
			}

			return _jsonCache;
		}

		private JsonObject parseData()
		{
			try
			{
				// 보드 편집을 아직 하지 않은 유저
				if( data == null)
				{
					return _emptyJsonData;
				}

				string string_data = Encoding.UTF8.GetString(data.getData());
				JsonObject json = new JsonObject(string_data);
				return json;
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return _emptyJsonData;
			}
		}

		public void setData(JsonObject json)
		{
			try
			{
				string string_data = json.encode();
				byte[] byte_data = Encoding.UTF8.GetBytes(string_data);
				data = BlobData.create(byte_data);
				_jsonCache = json;
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
		}

	}
}

