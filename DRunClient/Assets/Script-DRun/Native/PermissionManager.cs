using System;
using System.Collections;

using Festa.Client;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Android;

// references
// https://mentum.tistory.com/327 - 유니티 안드로이드 스크린샷 / 스크린캡쳐 (Unity Android screenCapture)
// https://rito15.github.io/posts/unity-android-authority/
namespace Drun.Client
{
	public static class PermissionManager
	{
		// TODO: RefString 으로 변경하기! 기획 필요.
		static readonly string requestPermissionTitle = "# 러닝 결과 저장 권한";
		static readonly string requestPermissionContents = "# 러닝 결과를 저장하기 위해서 권한 승인이 필요합니다.";
		static readonly string requestPermissionDeniedContents = "# External Storage 쓰기 권한 획득 실패. 저장 취소. Drun 권한설정 창으로 이동합니다.";
		//static readonly string requestPermissionDeniedAndDontAskAgainContents = "# External Storage 쓰기 권한 획득 실패. 저장 취소.";

		public static void DefaultPermissionDenied(string response = null)
		{
			// 권한 획득 거절 당함!
			UIPopup.spawnError(message: requestPermissionDeniedContents, clickOK: () =>
			{
				// TODO: 저장 취소 처리.
				PermissionManager.openAppPermissionSettings();
			});
		}

		public static void DefaultPermissionDeniedAndDontAskAgain(string response = null)
		{
			// 권한 획득 거절 당함!
			UIPopup.spawnError(message: requestPermissionDeniedContents, clickOK: () =>
			{
				// TODO: 저장 취소 처리.
				PermissionManager.openAppPermissionSettings();
			});
		}

		private static PermissionCallbacks makePermissionCallbacks(Action<string> onPermissionGranted)
		{
			var permissionCallbacks = new PermissionCallbacks();

			permissionCallbacks.PermissionGranted -= onPermissionGranted;
			permissionCallbacks.PermissionGranted += onPermissionGranted;

			permissionCallbacks.PermissionDenied -= DefaultPermissionDenied;
			permissionCallbacks.PermissionDenied += DefaultPermissionDenied;

			permissionCallbacks.PermissionDeniedAndDontAskAgain -= DefaultPermissionDeniedAndDontAskAgain;
			permissionCallbacks.PermissionDeniedAndDontAskAgain += DefaultPermissionDeniedAndDontAskAgain;

			return permissionCallbacks;
		}

		public static void requestPermissionForExternalStorageAccess(Action<string> onPermissionGranted)
		{
			if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
			{
				Debug.Log("<color=teal>권한 이미 있음! 사진 저장 ㄱㄱ</color>");
				onPermissionGranted?.Invoke(string.Empty);
				return;
			}

			// 권한이 없어서 안내 팝업 띄우기.
			// try # 1
			UIPopup.spawnOK(title: requestPermissionTitle, message: requestPermissionContents, clickOK: () =>
			{
				// 권한 결과 콜백.
				var defaultPermissionCallbacks = makePermissionCallbacks(onPermissionGranted);
				Permission.RequestUserPermission(Permission.ExternalStorageWrite, defaultPermissionCallbacks);
			});
		}

		private static void openAppPermissionSettings()
		{
			try
			{
#if UNITY_ANDROID
				using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				using var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
				if (currentActivity == null)
					return;

				string packageName = currentActivity.Call<string>("getPackageName");

				using var uriClass = new AndroidJavaClass("android.net.Uri");
				using var uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);

				using var intent = new AndroidJavaObject("android.content.Intent",
					"android.settings.APPLICATION_DETAILS_SETTINGS", uriObject);
				intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
				intent.Call<AndroidJavaObject>("setFlags", 0x10000000);

				currentActivity.Call("startActivity", intent);

#elif UNITY_IOS
//
#endif
			}
			catch (Excetion ex)
			{
				Debug.LogException(ex);
			}
		}
	}
}