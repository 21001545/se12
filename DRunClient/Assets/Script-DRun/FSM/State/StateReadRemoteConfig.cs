using Festa.Client;
using Festa.Client.Module.FSM;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using UnityEngine;

namespace DRun.Client
{
	public class StateReadRemoteConfig : ClientStateBehaviour
	{
		private int _customNextState;

		public override int getType()
		{
			return ClientStateType.read_remote_config;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			readRemoteConfig();

			if( param == null)
			{
				_customNextState = _nextState;
			}
			else
			{
				_customNextState = (int)param;
			}
		}

		private void readRemoteConfig()
		{
			FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(task =>
			{
				if( task.IsFaulted)
				{
					UIPopup.spawnError("firebase remote config fetch fail!");
					_owner.changeState(ClientStateType.sleep);
				}
				else
				{
					FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activate_task => {
						_owner.changeState(_customNextState);
					});
				}
			});
		}
	}
}
