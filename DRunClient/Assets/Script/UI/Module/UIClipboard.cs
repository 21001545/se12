using UnityEngine;
using System.Runtime.InteropServices;
using Festa.Client.Module.UI;
using Festa.Client.Module;
using TMPro;

namespace Festa.Client
{
    public class UIClipboard : UISingletonPanel<UIClipboard>
    {
        [SerializeField]
        private RectTransform rect_popup;
        [SerializeField]
        private RectTransform refRect;
        [SerializeField]
        private GameObject go_copy;
        [SerializeField]
        private GameObject go_paste;
        [SerializeField]
        private float _positionOffset;
 
        private TMP_InputField _inputField;

#if UNITY_IPHONE
    #region -xcode

        [DllImport("__Internal")]
        private static extern string _importString();

        // Xcode 클립보드에서 데이터를 얻어와요
        public static string ImportString()
        {
            Debug.Log($"import copied string : {_importString()}");
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return _importString();
            }
            else
            {
                return "";
            }
        }

        [DllImport("__Internal")]
        private static extern void _exportString(string exportData);

        // 선택된 스트링을 Xcode 로 보내요
        public static void ExportString(string exportData)
        {
            Debug.Log($"export selected string : {exportData}");
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _exportString(exportData);
            }
        }

    #endregion
#endif
        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);
        }

        public RectTransform getRefRect()
        {
            return refRect;
        }

        public RectTransform getPopupRect()
        {
            return rect_popup;
        }

        public void setInputField(TMP_InputField input)
        {
            _inputField = input;
        }

        public void setPopupPosition(Vector2 pos)
        {
            pos.y += _positionOffset;
            rect_popup.anchoredPosition = pos;
        }

        private void setType(UIClipboardHandler.ClipboardType type)
        {
            if(type == UIClipboardHandler.ClipboardType.copyOnly)
            {
                go_copy.SetActive(true);
                go_paste.SetActive(false);
            }
            else if(type == UIClipboardHandler.ClipboardType.copyPaste)
            {
                go_copy.SetActive(true);
                go_paste.SetActive(true);
            }

            rect_popup.gameObject.SetActive(false);
            rect_popup.gameObject.SetActive(true);
        }

        public void init(Vector2 position, TMP_InputField inputField, UIClipboardHandler.ClipboardType clipboardType)
        {
            setType(clipboardType);
            setPopupPosition(position);
            setInputField(inputField);

            open();
        }

        public override void close(int trasitionType = 0)
        {
            // 2022.06.10 이강희 crash 의심 코드 수정
            if( _inputField != null)
			{
                _inputField.caretPosition = _inputField.selectionStringAnchorPosition;
            }
            base.close(trasitionType);
        }

        public void onClickCopy()
        {
            sendStringToClipboard();
            close();
        }

        public void onClickPaste()
        {
            getStringFromClipboard();
            close();
        }

        public void getStringFromClipboard()
        {
            int a = _inputField.caretPosition;
            int b = _inputField.selectionAnchorPosition;
            if (a > b)
            {
                int temp = a;
                a = b;
                b = temp;
            }
            Debug.Log($"pasted text : {_inputField.text.Substring(0, a)}(sample text){_inputField.text.Substring(b, _inputField.text.Length - b)}");

#if !UNITY_EDITOR && UNITY_IPHONE
            _inputField.text = $"{_inputField.text.Substring(0, a)}{_importString()}{_inputField.text.Substring(b, _inputField.text.Length - b)}";
#endif
        }

        public void sendStringToClipboard()
        {
            int a = _inputField.caretPosition;
            int b = _inputField.selectionAnchorPosition;
            if (a > b)
            {
                int temp = a;
                a = b;
                b = temp;
            }
            Debug.Log($"copied text : {_inputField.text.Substring(a, b - a)}");

#if !UNITY_EDITOR && UNITY_IPHONE
            _exportString(_inputField.text.Substring(a, b - a));
#endif
        }
    }
}