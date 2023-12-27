using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class JobProgressItemViewModel : AbstractViewModel
	{
		private int _type;
		private int _status;
		private int _progress;
		private int _param;
		private int _result_code;
		private NativeGallery.NativePhotoContext _photo;

		public int Type => _type;

		public int Status
		{
			get { return _status; }
			set { Set(ref _status, value); }
		}

		public int Progress
		{
			get { return _progress; }
			set {
				Set(ref _progress, value);
			}
		}

		public int Param
		{
			get { return _param; }
			set { Set(ref _param, value); }
		}

		public int ResultCode
		{
			get { return _result_code; }
			set { Set(ref _result_code, value); }
		}

		public NativeGallery.NativePhotoContext Photo
		{
			get { return _photo; }
			set { Set(ref _photo, value); }
		}

		public static class JobType
		{
			public const int make_moment = 1;
			public const int modify_moment = 2;
			public const int delete_moment = 3;
			public const int chat_sendfile = 10;
		}

		public static JobProgressItemViewModel create(int type)
		{
			JobProgressItemViewModel vm = new JobProgressItemViewModel();
			vm.init(type);
			return vm;
		}

		protected void init(int type)
		{
			base.init();

			_type = type;
			_status = ClientJobProgressData.Status.create;
			_progress = 0;
			_param = 0;
			_result_code = Festa.Client.ResultCode.ok;
		}
	}
}
