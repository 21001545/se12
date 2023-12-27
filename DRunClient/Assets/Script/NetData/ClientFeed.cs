using Festa.Client.Module.MsgPack;

namespace Festa.Client.NetData
{
	public class ClientFeed
	{
		public int object_datasource_id;
		public int object_owner_account_id;
		public int object_type;
		public int object_id;

		[SerializeOption(SerializeOption.NONE)]
		public ClientMoment _moment;

		[SerializeOption(SerializeOption.NONE)]
		public bool _checkExpandable = true;

		[SerializeOption(SerializeOption.NONE)]
		public bool _expaneded;

		[SerializeOption(SerializeOption.NONE)]
		public bool _checkBookmark = false;

		[SerializeOption(SerializeOption.NONE)]
		public int _serverOrder;

		[SerializeOption(SerializeOption.NONE)]
		public bool _justUploadedMyMoment;
	}
}
