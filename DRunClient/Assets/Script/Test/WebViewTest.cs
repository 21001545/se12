//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class WebViewTest : MonoBehaviour
//{
//    public TextAsset _embeddedHTML;
//    private WebViewObject _webViewObject;

//    void Start()
//    {
//        startWeb();
//    }

//    void Update()
//    {
        
//    }

//    public void startWeb()
//	{
//        _webViewObject = gameObject.AddComponent<WebViewObject>();

//        _webViewObject.Init(
//            cb: callback_CB,
//            err: callback_ERR,
//            started: callback_Started,
//            ld: callback_LD,
//            enableWKWebView: true);
//        //        _webViewObject.setImageChooserTitle(StringManager.FindUIString("WEBVIEWER_AOS_SELECT_JOB"));

//#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
//			_webViewObject.bitmapRefreshCycle = 1;
//#endif

//        _webViewObject.LoadHTML(_embeddedHTML.text, "");
//        //_webViewObject.LoadURL("https://www.google.com");

//        RectTransform rt = transform as RectTransform;

//        Vector3[] corners = new Vector3[4];
//        rt.GetWorldCorners(corners);

//        for (int i = 0; i < 4; ++i)
//        {
//            corners[i] = Camera.main.WorldToScreenPoint(corners[i]);

//            Debug.Log(string.Format("corner[{0}]:{1}", i, corners[i]));
//        }

//        int left = (int)corners[0].x;
//        int right = Screen.width - (int)corners[3].x;
//        int top = Screen.height - (int)corners[1].y;
//        int bottom = (int)corners[0].y;

//        Debug.Log(string.Format("left:{0} right:{1} top:{2} bottom:{3} width:{4} height:{5}", left, right, top, bottom, Screen.width, Screen.height));

//        _webViewObject.SetMargins(left, top, right, bottom);

//        _webViewObject.SetVisibility(true);
//    }


//    private void callback_CB(string msg)
//    {
//        Debug.Log(string.Format("CallFromJS[{0}]", msg));
//    }

//    private void callback_ERR(string msg)
//    {
//        Debug.Log(string.Format("CallOnError[{0}]", msg));
//    }

//    private void callback_Started(string msg)
//    {
//        Debug.Log(string.Format("CallOnStarted[{0}]", msg));
//    }

//    private void callback_LD(string msg)
//    {
//        Debug.Log(string.Format("CallOnLoaded[{0}]", msg));

//#if UNITY_EDITOR_OSX || !UNITY_ANDROID
//                // NOTE: depending on the situation, you might prefer
//                // the 'iframe' approach.
//                // cf. https://github.com/gree/unity-webview/issues/189
//#if true
//                _webViewObject.EvaluateJS(@"
//                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
//                    window.Unity = {
//                      call: function(msg) {
//                        window.webkit.messageHandlers.unityControl.postMessage(msg);
//                      }
//                    }
//                  } else {
//                    window.Unity = {
//                      call: function(msg) {
//                        window.location = 'unity:' + msg;
//                      }
//                    }
//                  }
//                ");
//#else
//                _webViewObject.EvaluateJS(@"
//                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
//                    window.Unity = {
//                      call: function(msg) {
//                        window.webkit.messageHandlers.unityControl.postMessage(msg);
//                      }
//                    }
//                  } else {
//                    window.Unity = {
//                      call: function(msg) {
//                        var iframe = document.createElement('IFRAME');
//                        iframe.setAttribute('src', 'unity:' + msg);
//                        document.documentElement.appendChild(iframe);
//                        iframe.parentNode.removeChild(iframe);
//                        iframe = null;
//                      }
//                    }
//                  }
//                ");
//#endif
//#endif
//        _webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
//    }
//}
