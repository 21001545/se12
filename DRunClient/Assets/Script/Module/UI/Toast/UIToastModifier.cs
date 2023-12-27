using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;
using Object = UnityEngine.Object;

namespace Festa.Client.Module.UI
{
    /// <summary>
    /// Toast 는 어떻게 만들지 이걸로 정함.
    /// </summary>
    public struct UIToastModifier
    {
        private readonly UIToast _toast;
        
        public UIToast Toast => _toast;

        public UIToastModifier(UIToast toast) => _toast = toast;

        public UIToastModifier toggleTextWrap(bool useWrap = true)
        {
	        _toast.txt_message.textWrappingMode = useWrap switch
	        {
		        true => TextWrappingModes.Normal,
		        false => TextWrappingModes.NoWrap
	        };

	        return this;
        }

        public UIToastModifier autoClose(float delayInSec)
        {
            _toast.StartCoroutine(closeAutomatically(delayInSec));
            return this;
        }

        private IEnumerator closeAutomatically(float delayInSec)
        {
            yield return new WaitForSeconds(delayInSec);
            _toast.close(TransitionEventType.start_close);
        }

        public UIToastModifier toggleIcon(bool show = true)
        {
	        _toast.icon_layout_element.ignoreLayout = !show;
            _toast.img_icon.gameObject.SetActive(show);

	        return this;
        }

        public UIToastModifier setType(UIToastType toastType)
        {
			if (toastType == UIToastType.none)
				this.toggleIcon(true);

			_toast.setType(toastType);
			return this;
		}

		public UIToastModifier withSprite(Sprite icon, Rect rect)
        {
			
	        this.toggleIcon(true);
            _toast.img_icon.sprite = icon;
            return this;
        }

        public UIToastModifier withRawImage(string url, Rect rect)
        {
            _toast.url = url;
            _toast.raw_image.setImageFromCDN(url);
            return this;
        }

        public UIToastModifier withTransition<T>(
	        Action<T> transition = null
		) where T : AbstractPanelTransition
        {
	        if (!_toast.TryGetComponent(out T c))
				c = _toast.AddComponent<T>();

			transition?.Invoke(c);
            return this;
        }

        public UIToastModifier setBackdropColor(Color col)
        {
            _toast.BackdropImage.color = col;
            return this;
        }

        public UIToastModifier setBackdropColor(string hexCol)
        {
	        if (ColorUtility.TryParseHtmlString(hexCol, out var c))
		        _toast.BackdropImage.color = c;

	        return this;
        }

        public UIToastModifier useBackdrop(bool use = true)
        {
            _toast.BackdropImage.gameObject.SetActive(use);
            return this;
        }

        public UIToastModifier setPaddingY(Vector2Int py)
        {
	        _toast.Horizontal.padding.top = py.x;
	        _toast.Horizontal.padding.bottom = py.y;
	        return this;
        }

        public UIToastModifier setPaddingX(Vector2Int px)
        {
	        _toast.Horizontal.padding.left = px.x;
	        _toast.Horizontal.padding.right = px.y;
	        return this;
        }

        public UIToastModifier setSpacing(float spacing)
        {
	        _toast.Horizontal.spacing = spacing;
	        return this;
        }

        // public UIToastModifier slideDirection(SlideUpDownTransition.SlideDirection direction)
        // {
        //     if (_panelTransition == null)
        //         return this;
        //
        //     if (_panelTransition is SlideUpDownTransition tran)
        //     {
        //         tran.slideDirection = direction;
        //     }
        //
        //     return this;
        // }
        //
        // public UIToastModifier slideTransitionType(SlideUpDownTransition.TransitionType transitionType)
        // {
        //     if (_panelTransition == null)
        //         return this;
        //
        //     if (_panelTransition is SlideUpDownTransition tran)
        //     {
        //         tran.transitionType = transitionType;
        //     }
        //
        //     return this;
        // }
    }
}