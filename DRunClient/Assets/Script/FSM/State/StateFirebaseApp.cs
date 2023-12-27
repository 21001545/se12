using Festa.Client.Module.FSM;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

namespace Festa.Client
{
	public class StateFirebaseApp : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.firebase_app;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("initialize health device...", 5);

			FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
				if (task.Result == DependencyStatus.Available)
				{
					Debug.Log($"Firebase.AppId:{FirebaseApp.DefaultInstance.Options.AppId}");

					changeToNextState();
				}
				else
				{
					UIPopup.spawnOK("firebase init fail!");

					_owner.changeState(ClientStateType.sleep);
				}
			});
		}
	}
}
