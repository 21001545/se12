using System;
using JetBrains.Annotations;
using TMPro;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Festa.Client.Module.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIToast : UIPanel
    {
        public TMP_Text txt_message;

        public Image img_icon;
        public LayoutElement icon_layout_element { get; private set; }

        public UIPhotoThumbnail raw_image;
        public string url;  

        public Image img_backdrop;

        [SerializeField]
        private UIToastType _toastType = UIToastType.none;

        [field: SerializeField] 
        public HorizontalLayoutGroup Horizontal { get; private set; }

        //private CanvasGroup _canvasGroup;

        //[SerializeField]
        //private ProceduralImage _proceduralImage;

        //[SerializeField]
        //private UniformModifier _uniformModifier;

        /// <summary>
        /// 0 - warning
        /// 1 - alert round
        /// 2 - check round
        /// </summary>
        [SerializeField]
        private Sprite[] _defaultIcons;

        [field: SerializeField]
        public RectTransform ToastRectTransform { get; private set; }

        public Image BackdropImage { get; private set; }

        private bool _closing;

        [CanBeNull] public Action OnStart;
        [CanBeNull] public Action OnComplete;

        public static readonly Vector2 DefaultPaddingX = new(20, 20);
        public static readonly Vector2 DefaultPaddingY = new(11, 11);
        public const float DefaultSpacingY = 8;

        #region override

        public override void onCreated(ReusableMonoBehaviour source)
        {
            base.onCreated(source);

            //_canvasGroup = GetComponent<CanvasGroup>();

            ToastRectTransform.pivot = new(0, 1);

            BackdropImage = transform.GetChild(0).GetComponent<Image>();
            Horizontal = transform.GetChild(1).GetComponent<HorizontalLayoutGroup>();
            _closing = true;
        }

        public override void onDelete()
        {
	        base.onDelete();

			OnComplete?.Invoke();
        }

        #endregion override

        #region behaviour

        public static UIToastModifier spawn(string contents, Vector2? position = null)
        {
            var toast = UIManager.getInstance().spawnInstantPanel<UIToast>();
			//Debug.Log($"{Time.frameCount}:spawn:{toast.gameObject.GetInstanceID()}", toast.gameObject);

			toast.icon_layout_element = toast.img_icon.GetComponent<LayoutElement>();

			// 시안의 위치 값을 그대로 쓰기 위해 pivot, anchor 조정.
			if (position.HasValue)
			{
				var rt = toast.ToastRectTransform;
				rt.anchorMax = new(0, 1);
				rt.anchorMin = new(0, 1);
				rt.pivot = new(0, 1);
                rt.anchoredPosition = position.Value;
			}

            toast.txt_message.text = contents;
            return new UIToastModifier(toast);
        }

		public static UIToastModifier spawnWithCallback(
			string contents,
			Vector2? position = null,
			[CanBeNull] Action onStart = null,
			[CanBeNull] Action onComplete = null)
		{
			onStart?.Invoke();
			var mod = spawn(contents, position);
			mod.Toast.OnComplete = onComplete;
			return mod;
		}

		/// <summary>
		/// Get default icon with UIToastType.
		/// </summary>
		/// <param name="toastType"></param>
		public void setType(UIToastType toastType)
        {
			img_icon.sprite = (_toastType = toastType) switch
			{
				UIToastType.warning => _defaultIcons[0], // warning
				UIToastType.error => _defaultIcons[1], // alertRound
				UIToastType.normal => _defaultIcons[2], // checkRound
				UIToastType.info => default, // 아직 없음
				_ => throw new NotImplementedException(),
			};
		}

		#endregion behaviour

		#region callback

		public void onClick_Backdrop()
        {
            if (_closing)
                return;

            // 사라지는 중에는 뒤가 눌릴 수 있도록 해줘봄
            img_backdrop.raycastTarget = false;    
			_closing = true;
            base.close();
        }

		public override void onTransitionEvent(int type)
		{
			base.onTransitionEvent(type);

            if( type == TransitionEventType.start_open)
            {
                _closing = false;
                img_backdrop.raycastTarget = true;
			}
		}

		#endregion callback

		#region test code

		//private void OnEnable()
		//{
		//	Debug.Log($"{Time.frameCount}:OnEnable:{gameObject.GetInstanceID()}", gameObject);
		//}

		//private void OnDisable()
		//{
		//	Debug.Log($"{Time.frameCount}:OnDisable:{gameObject.GetInstanceID()}", gameObject);
		//}

		//public override void onReused()
		//{
		//	Debug.Log($"{Time.frameCount}:onReused:{gameObject.GetInstanceID()}", gameObject);
		//}

		//public override void onDelete()
		//{
		//          Debug.Log($"{Time.frameCount}:onDelete:{gameObject.GetInstanceID()}", gameObject);
		//}

		#endregion test code
	}
}