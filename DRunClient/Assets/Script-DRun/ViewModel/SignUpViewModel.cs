using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.ViewModel
{
	public class SignUpViewModel : AbstractViewModel
	{
		private string _email;
		private DateTime _verifyEmailExpireTime;
		private DateTime _resendVerifyCodeTime;
		private int _verifyEmailResult;

		private string _code;
		private string _password;
		private string _name;

		private int _gender;
		private double _height;
		private double _weight;

		public string EMail
		{
			get
			{
				return _email;
			}
			set
			{
				Set(ref _email, value);
			}
		}

		public DateTime VerifyEmailExpireTime
		{
			get
			{
				return _verifyEmailExpireTime;
			}
			set
			{
				Set(ref _verifyEmailExpireTime, value);
			}
		}

		public DateTime ResendVerifyCodeTime
		{
			get
			{
				return _resendVerifyCodeTime;
			}
			set
			{
				Set(ref _resendVerifyCodeTime, value);
			}
		}

		public String Code
		{
			get
			{
				return _code;
			}
			set
			{
				Set(ref _code, value);
			}
		}

		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				Set(ref _password, value);
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				Set(ref _name, value);
			}
		}

		public int VerifyEmailResult
		{
			get
			{
				return _verifyEmailResult;
			}
			set
			{
				Set(ref _verifyEmailResult, value);
			}
		}

		public int Gender
		{
			get
			{
				return _gender;
			}
			set
			{
				Set(ref _gender, value);
			}
		}

		public double Height
		{
			get
			{
				return _height;
			}
			set
			{
				Set(ref _height, value);
			}
		}

		public double Weight
		{
			get
			{
				return _weight;
			}
			set
			{
				Set(ref _weight, value);
			}
		}

		public static SignUpViewModel create()
		{
			SignUpViewModel vm = new SignUpViewModel();
			vm.init();
			return vm;
		}

	


	}
}
