using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Festa.Client.Module.Net
{
	public class HttpException : Exception
	{
		private long _responseCode;

		public HttpException(UnityWebRequest request)
			: base(string.Format("URL[{0}] ERROR[{1}]", request.url,request.error))
		{
			_responseCode = request.responseCode;
		}

		public long getResponseCode()
		{
			return _responseCode;
		}
	}
}
