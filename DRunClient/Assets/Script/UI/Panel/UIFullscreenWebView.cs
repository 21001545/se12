using Festa.Client.Module.UI;
using UnityEngine;

namespace Festa.Client
{
	public class UIFullscreenWebView : UIPanel
	{
		public WebViewObject webViewObject;

		private bool _initWebView = false;
		private string _targetURL;
		
		public static UIFullscreenWebView spawnURL(string url)
		{
#if UNITY_EDITOR && UNITY_EDITOR_WIN
			Application.OpenURL(url);
			return null;
#else
			UIFullscreenWebView view = UIManager.getInstance().spawnInstantPanel<UIFullscreenWebView>();
			view.initWebView();
			view.prepareOpen(url);
			
			return view;
#endif

		}

		private void prepareOpen(string url)
		{
			_targetURL = url;
			webViewObject.SetVisibility(false);
		}

		public override void onTransitionEvent(int type)
		{
			base.onTransitionEvent(type);

			if( type == TransitionEventType.end_open)
			{
				webViewObject.SetVisibility(true);
				setupMargin();
				setupURL(_targetURL);
			}
			else if( type == TransitionEventType.start_close)
			{
				webViewObject.SetVisibility(false);
			}
		}

		private void setupMargin()
		{
			RectTransform rt = webViewObject.transform as RectTransform;
			Vector3[] corners = new Vector3[4];
			rt.GetWorldCorners(corners);

			Vector3 bottom_left = Camera.main.WorldToScreenPoint(corners[0]);
			Vector3 top_right = Camera.main.WorldToScreenPoint(corners[2]);

			int left;
			int top;
			int right;
			int bottom;

			//			left = (int)bottom_left.x;
			//			right = Screen.width - (int)top_right.x - 1;
			left = right = 0;
			top = Screen.height - (int)top_right.y - 1;
			bottom = (int)bottom_left.y;

			Debug.Log($"webview margin: bottom_left[{bottom_left}] top_right[{top_right}] left[{left}] top[{top}] right[{right}] bottom[{bottom}]");

			webViewObject.SetMargins(left, top, right, bottom);
		}

		public void setupURL(string url)
		{
			webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
			webViewObject.LoadURL(url);
		}

		public void onClickClose()
		{
			close();
		}

		private void initWebView()
		{
			if(_initWebView)
			{
				return;
			}
			_initWebView = true;

			webViewObject.Init(
						cb: (msg) =>
						{
							Debug.Log(string.Format("CallFromJS[{0}]", msg));
						},
						err: (msg) =>
						{
							Debug.Log(string.Format("CallOnError[{0}]", msg));
						},
						httpErr: (msg) =>
						{
							Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
						},
						started: (msg) =>
						{
							Debug.Log(string.Format("CallOnStarted[{0}]", msg));
						},
						hooked: (msg) =>
						{
							Debug.Log(string.Format("CallOnHooked[{0}]", msg));
						},
						ld: (msg) =>
						{
							setupMargin();

							Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
							// NOTE: depending on the situation, you might prefer
							// the 'iframe' approach.
							// cf. https://github.com/gree/unity-webview/issues/189
#if true
							webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                webViewObject.EvaluateJS(
                    "window.Unity = {" +
                    "   call:function(msg) {" +
                    "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                    "   }" +
                    "};");
#endif
							webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
						}
						//transparent: false,
						//zoom: true,
						//ua: "custom user agent string",
						//// android
						//androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
						//// ios
						//enableWKWebView: true,
						//wkContentMode: 0,  // 0: recommended, 1: mobile, 2: desktop
						//wkAllowsLinkPreview: true,
						//// editor
						//separated: false
						);
		}

	}
}
