using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module.MsgPack
{
	public class EncryptedData : BlobData
	{
		public string toString(byte[] key)
		{
			byte[] decrypted_data = EncryptUtil.decrypt(_data, key);
			return System.Text.Encoding.UTF8.GetString(decrypted_data);
		}
		
		public static EncryptedData create(string str_data,byte[] key)
		{
			EncryptedData data = new EncryptedData();
			data.setData( EncryptUtil.encrypt( System.Text.Encoding.UTF8.GetBytes(str_data), key));
			return data;
		}
	}
}