using Festa.Client.Module.MsgPack;
using System;
using System.IO;
using UnityEngine.Networking;

namespace Festa.Client.Module.Net
{
	class HttpPacketCodec
	{
		private ObjectFactory _objectFactory;

		private MemoryStream _writeStream;
		private MessagePacker _msgPacker;
		private ObjectPacker _objPacker;

		private MessageUnpacker _msgUnpacker;
		private ObjectUnpacker _objUnpacker;

		public static HttpPacketCodec create(ObjectFactory of,int option_mask)
		{
			HttpPacketCodec codec = new HttpPacketCodec();
			codec.init(of, option_mask);
			return codec;
		}

		private void init(ObjectFactory objectFactory,int option_mask)
		{
			_objectFactory = objectFactory;

			_writeStream = new MemoryStream();
			_msgPacker = MessagePacker.create( _writeStream);
			_objPacker = ObjectPacker.create(_objectFactory, option_mask);

			_msgUnpacker = MessageUnpacker.create(null);
			_objUnpacker = ObjectUnpacker.create(_objectFactory, option_mask);
		}

		public UnityWebRequest makeRequest(String url,MapPacket packet)
		{
			byte[] payload = encode(packet);

			UnityWebRequest request = new UnityWebRequest(url);
			UploadHandlerRaw uploader = new UploadHandlerRaw(payload);
			uploader.contentType = "application/octet-stream";

			request.uploadHandler = uploader;
			request.downloadHandler = new DownloadHandlerBuffer();
			request.method = UnityWebRequest.kHttpVerbPOST;
			request.timeout = 10;

			return request;
		}

		public byte[] encode(MapPacket packet)
		{
			_msgPacker.resetStream();
			_objPacker.pack(_msgPacker, packet);

			byte[] payload = new byte[_msgPacker.getTotalWrittenBytes()];
			Array.Copy(_writeStream.GetBuffer(), payload, _msgPacker.getTotalWrittenBytes());

			return payload;
		}

		public MapPacket decode(byte[] payload)
		{
			using (MemoryStream stream = new MemoryStream(payload))
			{
				_msgUnpacker.reset(stream);
				MapPacket packet =  _objUnpacker.unpack(_msgUnpacker) as MapPacket;

				//Debug.Log(string.Format("total read bytes:{0}", _msgUnpacker.getTotalReadBytes()));

				return packet;
			}
		}
	}
}
