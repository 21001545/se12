using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Festa.Client.Module;
using UnityEngine.Events;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
    [RequireComponent(typeof(SlideUpDownTransition))]
    public class UIPopup : UIPanel
    {
        [Header("===== Backdrop =====")]
        [SerializeField]
        private float _srcAlpha = 0;
        
        [SerializeField]
        private float _dstAlpha = 0.5f;
        
        [SerializeField]
        private Graphic _backdropGraphic;
        
        [SerializeField]
        [ReadOnly]
        private SlideUpDownTransition _slideUpDownTransition;
        
        [Space(10)]
        [Header("===== UI Ref =====")]
        public TMP_Text txt_message;
        public TMP_Text txt_title;
        public Image icon_title;

        public TMP_Text txt_btn_ok;
        //public TMP_Text txt_btn_yes;
        public TMP_Text txt_btn_delete;

        public GameObject image_root;
        public Image sprite_image;
        public UIPhotoThumbnail raw_image;

        public GameObject[] control_list;

        public UnityAction callbackPrimary;
        public UnityAction callbackOther;
        private TweenerCore<Color, Color, ColorOptions> _fadeInBackdropTweener;

        public class ControlType
        {
            public const int ok = 0;
            public const int yesno = 1;
            public const int deleteCancel = 2;
            public const int error = 3;
            public const int retry = 4;
        }

        public override void onCreated(ReusableMonoBehaviour source)
        {
            base.onCreated(source);

            _slideUpDownTransition = GetComponent<SlideUpDownTransition>();
            resetBackdrop();
        }

        public override void open(UIPanelOpenParam param, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);
            
        }

        public override void onReused()
        {
            base.onReused();
            
            disposeBackdropTween();
            resetBackdrop();
            fadeInBackdrop();
        }

        public override void close(int transitionType = 0)
        {
            base.close(transitionType);
            
            disposeBackdropTween();
            resetBackdrop();
        }

        private void resetBackdrop()
        {
            if (_backdropGraphic == null)
                return;
            
            Color initialColor = _backdropGraphic.color;
            initialColor.a = _srcAlpha;
            _backdropGraphic.color = initialColor;
        }
        
        private void fadeInBackdrop()
        {
            if (_backdropGraphic == null)
                return;

            resetBackdrop();
            
            _fadeInBackdropTweener = DOTween.ToAlpha(
                    () => _backdropGraphic.color,
                    newCol => _backdropGraphic.color = newCol,
                    _dstAlpha,
                    _slideUpDownTransition.alphaDuration
                )
                .SetEase(Ease.InOutQuint)
                .SetAutoKill();
        }
        
        private void disposeBackdropTween()
        {
            // dispose on closing ui popup if it still plays.
            if (_fadeInBackdropTweener != null && _fadeInBackdropTweener.IsPlaying())
            {
                _fadeInBackdropTweener.Kill();
                _fadeInBackdropTweener = null;
            }
        }

        public void clickOK()
        {
            close();
            if( callbackPrimary != null)
            {
                callbackPrimary();
            }
        }

        public void clickNO()
        {
            close();
            if( callbackOther != null)
            {
                callbackOther();
            }
        }

        private void setControlType(int type)
        {
            for(int i = 0; i < control_list.Length; ++i)
            {
                control_list[i].SetActive(i == type);
            }
        }

        public void hideImage()
        {
            image_root.gameObject.SetActive(false);
        }

        public void setImage(Sprite sprite)
        {
            sprite_image.gameObject.SetActive(true);
            sprite_image.sprite = sprite;

            raw_image.gameObject.SetActive(false);
        }

        public void setImageCDN(string url)
        {
            sprite_image.gameObject.SetActive(false);
            raw_image.gameObject.SetActive(true);
            raw_image.setImageFromCDN(url);
        }

        //
        public static UIPopup spawnError(string message,UnityAction clickOK = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.error);
            popup.hideImage();
            popup.txt_message.text = message;
            popup.txt_title.gameObject.SetActive(false);
            popup.callbackPrimary = clickOK;

            return popup;
        }

		public static UIPopup spawnError(string title,string message, UnityAction clickOK = null)
		{
			UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
			popup.fadeInBackdrop();
			popup.setControlType(ControlType.error);
			popup.hideImage();
			popup.txt_message.text = message;
			popup.txt_title.gameObject.SetActive(true);
            popup.txt_title.text = title;
			popup.callbackPrimary = clickOK;

			return popup;
		}

		public static UIPopup spawnOK(string message,UnityAction clickOK = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.ok);
            popup.hideImage();
            popup.txt_message.text = message;
            popup.txt_title.gameObject.SetActive(false);
            popup.callbackPrimary = clickOK;

            return popup;
        }

        public static UIPopup spawnOK(string title, string message, UnityAction clickOK = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.ok);
            popup.hideImage();
            popup.txt_title.gameObject.SetActive(true);
            popup.txt_title.text = title;
            popup.icon_title.gameObject.SetActive(false);
            popup.txt_message.text = message;
            popup.callbackPrimary = clickOK;
            return popup;
        }

        public static UIPopup spawnOK(Sprite icon_title,string title, string message, UnityAction clickOK = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.ok);
            popup.hideImage();
            popup.txt_title.gameObject.SetActive(true);
            popup.txt_title.text = title;
            popup.icon_title.gameObject.SetActive(true);
            popup.icon_title.sprite = icon_title;
            popup.txt_message.text = message;
            popup.callbackPrimary = clickOK;
            return popup;
        }


        public static UIPopup spawnYesNo(string message,UnityAction clickYes,UnityAction clickNo = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.yesno);
            popup.hideImage();
            popup.txt_message.text = message;
            popup.txt_title.gameObject.SetActive(false);
            popup.callbackPrimary = clickYes;
            popup.callbackOther = clickNo;

            return popup;
        }

        public static UIPopup spawnYesNo(string title, string message, UnityAction clickYes, UnityAction clickNo = null)
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.yesno);
            popup.hideImage();
            popup.txt_message.text = message;
            popup.txt_title.gameObject.SetActive(true);
            popup.txt_title.text = title;
            popup.icon_title.gameObject.SetActive(false);
            popup.callbackPrimary = clickYes;
            popup.callbackOther = clickNo;

            return popup;
        }

        public static UIPopup spawnDeleteCancel(string title, string message, UnityAction clickDelete, UnityAction clickCancel = null, string deleteButtonMsg = "")
        {
            UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
            popup.fadeInBackdrop();
            popup.setControlType(ControlType.deleteCancel);
            popup.hideImage();
            popup.txt_message.text = message;
            popup.txt_title.gameObject.SetActive(true);
            popup.txt_title.text = title;
            popup.icon_title.gameObject.SetActive(false);
            popup.callbackPrimary = clickDelete;
            popup.callbackOther = clickCancel;
			
            if(string.IsNullOrEmpty(deleteButtonMsg) == false)
            {
                popup.txt_btn_delete.text = deleteButtonMsg;
            }

            return popup;
        }

		public static UIPopup spawnRetry(string title, string message, UnityAction clickOK = null)
		{
			UIPopup popup = UIManager.getInstance().spawnInstantPanel<UIPopup>();
			popup.fadeInBackdrop();
			popup.setControlType(ControlType.retry);
			popup.hideImage();
			popup.txt_title.gameObject.SetActive(true);
			popup.txt_title.text = title;
			popup.icon_title.gameObject.SetActive(false);
			popup.txt_message.text = message;
			popup.callbackPrimary = clickOK;
			return popup;
		}

	}
}