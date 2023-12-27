using Festa.Client.Data;
using Festa.Client.Module.FSM;
using UnityEngine;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Festa.Client.Module.Events;
using Festa.Client.Module;
using System.Collections;

namespace Festa.Client
{
	public class StateFirebaseLogin : ClientStateBehaviour
	{
		private FirebaseAuth _fbAuth; 

		public override int getType()
		{
			return ClientStateType.firebase_login;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("firebase login...", 40);

			//UIFirebaseLogin.getInstance().open();
			_fbAuth = FirebaseAuth.DefaultInstance;

            bool is_first_run = checkFirstRun();
#if UNITY_IPHONE
			bool is_already_login = _fbAuth.CurrentUser != null;
			 
			Debug.Log(string.Format("checkFirstRun:{0}", is_first_run));

			if( is_already_login && is_first_run)
			{
				_fbAuth.SignOut();
				is_already_login = false;
			}
#endif
			// iOS는.. 게임을 지웠다 깔아도, firebase auth가 남나보구나?
            _data.getStartupContext().is_first_run = is_first_run && _fbAuth.CurrentUser == null;


            if ( _fbAuth.CurrentUser != null)
			{
				fillStartupContext(_fbAuth.CurrentUser);
				changeToNextState();
			}
			else
			{
				UIFirebaseLogin.getInstance().open();
				UIFirebaseLogin.getInstance().setLoginSuccessCallback(onLoginSuccess);
			}
		}

		//public override void onExit(StateBehaviour<object> next_state)
		//{
		//	if( UILoading.getInstance().gameObject.activeSelf == false)
		//	{
		//		UILoading.getInstance().open();
		//	}
		//}

		private bool checkFirstRun()
		{
			// 테스트용,, 매번 새로 로그인 하는 코드
			//PlayerPrefs.DeleteKey("CheckFirstRun");

			if ( PlayerPrefs.HasKey("CheckFirstRun") == false)
			{
				PlayerPrefs.SetInt("CheckFirstRun", 0);
				return true;
			}

			return false;
		}


		private void fillStartupContext(FirebaseUser user)
		{
			StartupContext ctx = _data.getStartupContext();
			ctx.firebase_id = user.UserId;
			ctx.provider_id = user.ProviderId;
			ctx.phone_number = user.PhoneNumber;
		}

		private void onLoginSuccess(FirebaseUser user)
        {
#if !UNITY_EDITOR
			fillStartupContext(user);
#endif
            changeToNextState();
        }


		//private IEnumerator getCountryCode(Handler<AsyncResult<string>> callback)
		//{
		//	_data.getStartupContext().
		//}

		//private IEnumerator getPublicIP(Handler<AsyncResult<string>> callback)
		//{
		//	string uri = "https://api64.ipify.org?format=json";
		//	UnityWebRequest request = UnityWebRequest.Get(uri);

		//	yield return request.SendWebRequest();

		//	if( request.result != UnityWebRequest.Result.Success)
		//	{
		//		callback(Future.failedFuture<string>(new Exception(request.error)));
		//	}
		//	else
		//	{
		//		try
		//		{
		//			JsonObject json = new JsonObject(request.downloadHandler.text);
		//			string ip = json.getString("ip");
		//			callback(Future.succeededFuture(ip));
		//		}
		//		catch(System.Exception e)
		//		{
		//			callback(Future.failedFuture<string>(e));
		//		}
		//	}
		//}
	}
}
