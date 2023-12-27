using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientMomentComment
	{
		public int slot_id;
		public int sub_id;
		public int comment_account_id;
		public int status;
		public string message;
		public int like_count;
		public int sub_count;
		public DateTime update_time;

        [SerializeOption(SerializeOption.NONE)]
        public ClientMoment _moment;

        [SerializeOption(SerializeOption.NONE)]
		public ClientProfile _profile;

		[SerializeOption(SerializeOption.NONE)]
		public bool _isLiked;

        [SerializeOption(SerializeOption.NONE)]
        public int _loaded_sub_count = 0; // 로드된 대댓글 개수

        [SerializeOption(SerializeOption.NONE)]
        public bool _isPosting = false;

        public static class Status
		{
			public static int deleted = 0;
			public static int active = 1;
		}
	}
}
