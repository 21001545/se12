using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Festa.Client.Module.Net;
using Festa.Client.Module.MsgPack;

namespace Festa.Client
{
	public class ClientNetwork
	{
		private HttpClient _httpClient;
		private CDNClient _cdnClient;
		private string _packetURL;
		private string _fileUploadURL;

		private int _account_id;
		private int _token;

		public HttpClient getHttpClient()
		{
			return _httpClient;
		}

		public CDNClient getCDNClient()
		{
			return _cdnClient;
		}

		public void setSession(int account_id,int token)
		{
			_account_id = account_id;
			_token = token;

			Debug.Log($"setSession: account_id[{_account_id}] token[{_token}]");
		}

		public int getAccountID()
		{
			return _account_id;
		}

		public int getToken()
		{
			return _token;
		}

		public void setPacketURL(string url)
		{
			_packetURL = url;
		}

		public void setFileUploadURL(string url)
		{
			_fileUploadURL = url;
		}

		public static ClientNetwork create(GameObject target)
		{
			ClientNetwork net = new ClientNetwork();
			net.init(target);
			return net;
		}

		public void update()
		{
			_httpClient.update();
		}

		private void init(GameObject target)
		{
			_httpClient = HttpClient.create(target, GlobalObjectFactory.getInstance(), SerializeOption.CSNet);
			_packetURL = GlobalConfig.gameserver_url;
			_cdnClient = CDNClient.create(target);

			CDNImageDownloader.validateCacheDirectory();
		}

		public MapPacket createReq(int msg_id)
		{
			MapPacket req = MapPacket.createWithMsgID(msg_id);
			req.put("account_id", _account_id);
			req.put("token", _token);
			return req;
		}

		public MapPacket createReqWithoutSession(int msg_id)
		{
			MapPacket req = MapPacket.createWithMsgID(msg_id);
			return req;
		}

		public HttpFileUploader createFileUploader(List<string> file_list)
		{
			return HttpFileUploader.create( _httpClient, ClientMain.instance.getMultiThreadWorker(), _fileUploadURL, file_list);
		}

		public CDNImageDownloader createImageDownaloder(string base_url,int caller_id)
		{
			return CDNImageDownloader.create(_cdnClient, ClientMain.instance.getMultiThreadWorker(), ClientMain.instance.getTextureCache(), base_url, caller_id);
		}

		public void call(MapPacket req, System.Action<MapPacket> callback)
		{
			_httpClient.call(_packetURL, req, callback);
		}

		public void callParallel(MapPacket req, System.Action<MapPacket> callback)
		{
			_httpClient.callParallel(_packetURL, req, callback);
		}
	}
}
