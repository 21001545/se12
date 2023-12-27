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

namespace DRun.Client.Logic.Firebase.EMailPassword
{
	public class CreateUserProcessor : BaseStepProcessor
	{
		private string _email;
		private string _password;

		private FirebaseUser _newUser;

		private FirebaseAuth FBAuth => FirebaseAuth.DefaultInstance;

		public static CreateUserProcessor create(string email,string password)
		{
			CreateUserProcessor p = new CreateUserProcessor();
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
			FBAuth.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWithOnMainThread(task => { 
				if( task.IsCanceled)
				{
					handler(Future.failedFuture(new Exception("canceled")));
					return;
				}

				if( task.IsFaulted)
				{
					handler(Future.failedFuture(task.Exception));
				}
				else
				{
					_newUser = task.Result;
					Debug.Log($"user created: email[{_email}] name[{_newUser.DisplayName}] id[{_newUser.UserId}]");
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
