using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Festa.Client.Module
{

	public static class EncryptUtil
	{
		public static int makeHashCode(string str)
		{
			if (str == null || str.Length == 0)
			{
				return 0;
			}

			int hash = 0;

			for (int i = 0; i < str.Length; ++i)
			{
				int c = str[i];
				hash = ((hash << 5) - hash) + c;
				hash = hash & hash;
			}

			return hash;
		}

		public static int makeHashCodePositive(string str)
		{
			int hash = makeHashCode(str);
			return System.Math.Abs(hash);
		}

		public static byte[] encrypt(byte[] src,byte[] key)
		{
			int seed = (int)((Random.value * 254) + 1);
			
			int add_length = seed % 5;
			int length = src.Length + (add_length + 1);
			byte[] dst = new byte[ length];

			dst[ 0] = (byte)((seed ^ key[ 0]) & 0xFF);
			
			for(int i = 0; i < add_length; ++i)
			{
				dst[ i + 1] = (byte)(Random.value * 255);
			}
			
			for(int i = 0; i < src.Length; ++i)
			{
				byte m = (byte)((src[ i] ^ (key[ i % key.Length] + seed)) & 0xFF);
				dst[ add_length + 1 + i] = m;
			}
			
			return dst;
		}
		
		public static byte[] decrypt(byte[] src,byte[] key)
		{
			int seed = (int)((src[ 0] ^ key[ 0]) & 0xFF);
			int add_length = seed % 5;
			int src_length = src.Length - (add_length + 1);
			byte[] dst = new byte[ src_length];
			
			for(int i = 0; i < src_length; ++i)
			{
				dst[ i] = (byte)(src[ add_length + 1 + i] ^ (key[i % key.Length] + seed));
			}
			
			return dst;
		}

		public static byte[] makeKey(string key)
		{
			return System.Text.Encoding.UTF8.GetBytes(key);
		}

		public static string password(string key)
		{
			byte[] keyArray = Encoding.UTF8.GetBytes(key);
			SHA1Managed enc = new SHA1Managed();
			byte[] encodedKey = enc.ComputeHash(enc.ComputeHash(keyArray));
			StringBuilder myBuilder = new StringBuilder(encodedKey.Length);

			foreach (byte b in encodedKey)
			{
				myBuilder.Append(b.ToString("X2"));
			}

			return "*" + myBuilder.ToString();
		}
	}


}
