using DRun.Client.Logic.Firebase.EMailPassword;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client.Logic.Firebase
{
	public class SignInProcessor : BaseStepProcessor
	{
		private string _email;
		private string _password;

		private FirebaseUser _user;
		private FirebaseAuth FBAuth => FirebaseAuth.DefaultInstance;

		public static SignInProcessor create(string email,string password)
		{
			SignInProcessor p = new SignInProcessor();
			p.init(email, password);
			return p;
		}

		private void init(string email,string password)
		{
			base.init();
			_email = email;
			_password = password;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Festa.Client.Module.Void>> handler)
		{
			FBAuth.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWithOnMainThread(task => { 
				if( task.IsCanceled)
				{
					handler(Future.failedFuture(new Exception("cancelled")));
					return;
				}

				if( task.IsFaulted)
				{
					handler(Future.failedFuture(task.Exception));
				}
				else
				{
					_user = task.Result;

					Debug.Log($"user sign-in success: email[{_email}] user_id[{_user.UserId}");
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
