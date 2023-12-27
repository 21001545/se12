#if UNITY_ANDROID

using DG.Tweening.Plugins;
using DRun.Client.Android;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;

namespace Festa.Client
{
	public class StateCheckPermissionAndroid : ClientStateBehaviour
	{
		private static string[] _req_permissions_q = new string[] {
			"android.permission.ACTIVITY_RECOGNITION",
			"android.permission.ACCESS_FINE_LOCATION",
			"android.permission.ACCESS_COARSE_LOCATION"
		};

		private static string[] _req_permissions_under_q = new string[] {
			"android.permission.ACCESS_FINE_LOCATION",
			"android.permission.ACCESS_COARSE_LOCATION"
		};

		public class Status
		{
			public const int granted = 0;
			public const int denied = 1;
			public const int dont_ask_again = 2;
			public const int unknown = -1;
		}

		private string[] _req_permissions;
		private Dictionary<string, int> _status;

		protected override void init()
		{
			base.init();

			int sdkInt = getSDKInt();

			Debug.Log($"android api level:{sdkInt}");
			
			if( sdkInt <= 28)
			{
				_req_permissions = _req_permissions_under_q;
			}
			else
			{
				_req_permissions = _req_permissions_q;
			}

			_status = new Dictionary<string, int>();
			foreach(string per in _req_permissions)
			{
				_status.Add(per, Status.unknown);
			}
		}

		public override int getType()
		{
			return ClientStateType.check_permission_android;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			validatePermission();

			UILoading.getInstance().setProgress("check permission...", 35);
		}

		public override void onExit(StateBehaviour<object> next_state)
		{
			// 권한 획득이 되었다면 서비스 다시 시작
			CoreServicePlugin.getInstance().startService();
		}

		private void validatePermission()
		{
			bool need_request = hasAllPermissions();
			if( need_request)
			{
				StringBuilder sb = new StringBuilder();

				List<string> request_list = new List<string>();

				int not_granted_count = 0;
				foreach(string per in _req_permissions)
				{
					if( _status[ per] != Status.granted)
					{
						string label = getPermissionLabel(per);

						Debug.Log(label);

						if( not_granted_count > 0)
						{
							sb.Append("\n");
						}

						sb.AppendFormat("<B>[{0}]</B>", label);
						not_granted_count++;

						request_list.Add(per);
					}
				}

				Debug.Log(sb.ToString());

				//string message = "D-Run을 설치해주셔서 감사합니다.\n앱을 실행하기 위해 다음 권한이 필요합니다\n\n{0}\n";
				string message = GlobalRefDataContainer.getStringCollection().getFormat("startup.android.request_permission", 0, sb.ToString());
				UIPopup popup = UIPopup.spawnOK(message, () => {

					requestPermissionIter(request_list, 0, () => {


						changeToNextState();

					});

					//requestPermissions();
				});
			}
			else
			{
				changeToNextState();
			}
		}

		private bool hasAllPermissions()
		{
			bool need_reequest = false;

			foreach(string per in _req_permissions)
			{
				if( Permission.HasUserAuthorizedPermission(per) == false)
				{
					need_reequest = true;
					Debug.Log(string.Format("permission[{0}] not authorized", per));

					_status[per] = Status.denied;
				}
				else
				{
					_status[per] = Status.granted;
				}
			}

			return need_reequest;
		}

		private void requestPermissionIter(List<string> list,int index,Action complete_callback)
		{
			if( index >= list.Count)
			{
				complete_callback();
				return;
			}

			PermissionCallbacks callbacks = new PermissionCallbacks();
			callbacks.PermissionGranted += per=> {
				_status[per] = Status.granted;
				Debug.Log(string.Format("granted:{0}", per));

				requestPermissionIter(list, index + 1, complete_callback);
			};
			callbacks.PermissionDenied += per => {
				_status[per] = Status.denied;
				Debug.Log(string.Format("denied:{0}", per));

				requestPermissionIter(list, index + 1, complete_callback);
			};
			callbacks.PermissionDeniedAndDontAskAgain += per => {
				_status[per] = Status.dont_ask_again;
				Debug.Log(string.Format("denied dont ask again:{0}", per));

				requestPermissionIter(list, index + 1, complete_callback);
			};
			
			Permission.RequestUserPermission( list[index], callbacks);
		}

		/*
		                PermissionInfo info = getPackageManager().getPermissionInfo(permission, 0);
                Log.i(TAG, String.format( "%s:name[%s]", permission, info.name));

                String label = info.loadLabel( getPackageManager()).toString();
                String desc = info.loadDescription( getPackageManager()).toString();

                Log.i(TAG, label);
                Log.i(TAG, desc);

		 */

		private string getPermissionLabel(string permission)
		{
			try
			{
				AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject mainActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

				AndroidJavaObject packageManager = mainActivity.Call<AndroidJavaObject>("getPackageManager");
				AndroidJavaObject permissionInfo = packageManager.Call<AndroidJavaObject>("getPermissionInfo", permission, 0);

				AndroidJavaObject label_charseq = permissionInfo.Call<AndroidJavaObject>("loadLabel", packageManager);
				string label = label_charseq.Call<string>("toString");

				return label;
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return permission;
			}
		}

		public int getSDKInt()
		{
#if UNITY_EDITOR
			return 31;
#else
			using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return version.GetStatic<int>("SDK_INT");
			}
#endif
		}
	}
}

#endif
