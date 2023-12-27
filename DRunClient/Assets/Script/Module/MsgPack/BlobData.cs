using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module.MsgPack
{
	public class BlobData : CustomSerializer
	{
		protected byte[] _data;

		public static BlobData create(byte[] data)
		{
			BlobData blobData = new BlobData();
			blobData.setData(data);
			return blobData;
		}

		public void pack(MessagePacker packer)
		{
			packer.packBinaryHeader( _data.Length);
			packer.addPayload( _data, 0, _data.Length);
		}

		public void unpack(MessageUnpacker unpacker)
		{
			int length = unpacker.unpackBinaryHeader();

			// unpacker�� buffer�� �ӽ÷� �ִ°Ŷ�
			// �޾Ƽ� �����ؾ� �Ѵ�
			//_data = unpacker.readPayload( length);

			byte[] payload = unpacker.readPayload(length);
			_data = new byte[length];

			System.Array.Copy(payload, _data, length);
		}

		public void setData(byte[] data)
		{
			_data = data;
		}

		public byte[] getData()
		{
			return _data;
		}
	}
}