using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.Module.Net;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client
{
	public class StateCheckSignIn : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.check_signin;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("trip status...", 40);

			_viewModel.SignIn.loadCache();

			// 로그인화면으로
			if( string.IsNullOrEmpty(_viewModel.SignIn.EMail))
			{
				_owner.changeState(ClientStateType.signin);
			}
			else
			{
				// 회원 가입여부 체크 (나중에는 유효성 검증도 필요)
				checkSignUp();
			}
		}

		private void checkSignUp()
		{
			MapPacket req = _network.createReq(CSMessageID.Auth.CheckSignUpReq);
			req.put("email", _viewModel.SignIn.EMail);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					_owner.changeState(ClientStateType.signin);
				}
				else
				{
					bool check_result = (bool)ack.get("check_result");

					// 회원가입 확인
					if( check_result == false)
					{
						_owner.changeState(ClientStateType.login);
					}
					else
					{
						_owner.changeState(ClientStateType.signin);
					}
				}
			});
		}
	}
}
