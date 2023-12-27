using DRun.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
	class UISelectServer : UISingletonPanel<UISelectServer>
	{
		public Transform _item_root;
		public UISelectServerItem _item_source;

		private UnityAction<JsonObject> _selectCallback;

		private List<UISelectServerItem> _itemList;

		public override void initSingletonPostProcess(SingletonInitializer initializer)
		{
			base.initSingletonPostProcess(initializer);

			_item_source.gameObject.SetActive(false);
			_itemList = new List<UISelectServerItem>();

//			addServer("DRun Dev", "http://20.41.107.62:10100/packet", "http://20.41.107.62:10100/upload", "http://20.41.107.62:10101/file");
			
//			addServer("DRun InHouse Dev", "http://20.41.116.8:10100/packet", "http://20.41.116.8:10100/upload", "http://20.41.116.8:10101/file");

//#if UNITY_EDITOR
//			addServer("Local Host",	"http://localhost:8088/packet", "http://localhost:8088/upload", "http://localhost:8081/file");
//#endif

		// region A 52.231.54.250
		// region B 52.231.93.55

			//addServer("RegionA (Azure)", "http://52.231.54.250:10100/packet", "http://52.231.54.250:10100/upload", "https://festastorage.blob.core.windows.net");
			//addServer("RegionB (Azure)", "http://52.231.93.55:10100/packet", "http://52.231.93.55:10100/upload", "https://festastorage.blob.core.windows.net");

			//addServer("LKH Home", "http://192.168.0.118:8088/packet", "http://192.168.0.118:8088/upload", "http://192.168.0.118:8081/file");
			//addServer("LKH Office", "http://192.168.0.114:8088/packet", "http://192.168.0.114:8088/upload", "http://192.168.0.114:8081/file");
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);

			buildServerList();
		}

		private void buildServerList()
		{
			GameObjectCache.getInstance().delete(_itemList);

			JsonArray array = RemoteConfig.getJsonArray(RemoteConfig.server_list);

			for (int i = 0; i < array.size(); ++i)
			{
				JsonObject svr = array.getJsonObject(i);
				addServer(svr);
			}
		}

		private void addServer(JsonObject config)
		{
			UISelectServerItem item = _item_source.make<UISelectServerItem>(_item_root, GameObjectCacheType.ui);
			item.setup(config, onSelectServer);

			_itemList.Add(item);
		}

		private void onSelectServer(JsonObject config)
		{
			//GlobalConfig.setupServerAddress(config);

			//ClientMain.instance.getNetwork().setPacketURL(GlobalConfig.gameserver_url);
			//ClientMain.instance.getNetwork().setFileUploadURL(GlobalConfig.fileupload_url);

			_selectCallback(config);
			close();
		}

		public void setSelectCallback(UnityAction<JsonObject> callback)
		{
			_selectCallback = callback;
		}
	}
}
