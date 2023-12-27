using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIPhotoTake : UISingletonPanel<UIPhotoTake>
	{
		public RawImage image_camera_target;
		//public FilterTab camera_filter_tab;
		public Button btn_next;

		public GameObject[] camera_contents;
		public GameObject[] hide_for_take;

        [SerializeField]
		// 바텀 루트 오브젝트, 화면이 찍힐 영역의 비율을 1:1로 맞추기 위하여, 바텀의 영역을 임의로 조절하기 위해서 가져옴.
		private RectTransform bottom_root; 

        [SerializeField]
		private GameObject filter_root; // 필터 콘텐츠가 모여있는 루트 오브젝트
		
		[SerializeField]
		private Button btn_take; // 촬영 버튼
        
		[SerializeField]
        private Button btn_filter; // 필터 버튼

		[SerializeField]
		private GameObject go_gallery;
        [SerializeField]
        private RawImage img_gallery_thumbnail; // 필터 버튼

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public static class Mode
		{
			public const int moment = 1;
			public const int profile_edit = 2;
			public const int profile_direct = 3;
			public const int trip_photo = 4;
			public const int trip_certify = 5;
		}

		public static class CameraType
		{
			public const int normal = 0;
			public const int face_ar = 1;
			public const int action = 2;
			public const int ar = 3;
		}

		private int _mode;
		private int _cameraType;
		private GameObject _cameraContent;
		private Camera _mainCamera;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			image_camera_target.gameObject.SetActive(false);
			//camera_filter_tab?.gameObject.SetActive(false);

			_cameraType = CameraType.normal;
			_mainCamera = Camera.main;
		}

		public void setGallery(bool on)
        {
			go_gallery.SetActive(on);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);

            filter_root.gameObject.SetActive(false);

            btn_filter.gameObject.SetActive(true);
            btn_take.gameObject.SetActive(true);


			if (_mode == Mode.trip_photo || _mode == Mode.trip_certify)
				go_gallery.SetActive(false);
			else
				go_gallery.SetActive(true);

			// 화면 찍힐 공간을 인식해보자.
			// 현재 프레임 크기를 어떻게 가져오지?
			// screen.height - (top_root + screen.width)를  bottom으로..
			// top_root = 97;
			// 2022.08.10 소현 : top_root 84
			float top_root = 84f;
            float scale = /*1.0f*/0.75f;
            var canvasScaler = GetComponentInParent<CanvasScaler>();
/*            if (canvasScaler != null)
                scale = canvasScaler.referenceResolution.y / Screen.height;*/

            float height = Screen.height - (top_root + Screen.width / scale);
            //bottom_root.sizeDelta = new Vector2(bottom_root.sizeDelta.x, height);
            var paths = NativeGallery.GetRecentImagePaths(0, 1);
			if ( paths != null && paths.Count > 0)
            {
                PhotoContextImageLoader loader = PhotoContextImageLoader.create(ClientMain.instance, ClientMain.instance.getTextureCache(), paths[0], 0);
                loader.run((id, textureUsage) =>
                {
                    if (textureUsage != null)
                    {
						img_gallery_thumbnail.texture = textureUsage.texture;
                    }
                });
            }
        }

        public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.end_open)
			{
				createCameraContent();
				checkPermission();
			}
			else if( type == TransitionEventType.start_close)
			{
				clearCameraContent();
			}
		}
		
		private void checkPermission()
        {
#if UNITY_IOS || UNITY_IPHONE
			// 음.. 어차피 ARSession에서 권한 요청을 한다.
			// 나는 권한이 denied일 경우에만 처리를 해주면 된다.
			var permission = NativeCamera.CheckPermission();
			Debug.Log($"UIPhotoTake, Camera Permission {permission}");
			if ( permission == NativeCamera.Permission.Denied)
            {
                var sc = GlobalRefDataContainer.getInstance().getStringCollection();
                UIPopup.spawnYesNo(sc.get("camera.noaccess.poprup.title", 0), sc.get("camera.noaccess.poprup.desc", 0), () =>
                {
					if (NativeCamera.CanOpenSettings())
					{
						NativeCamera.OpenSettings();
					}
					else
					{
						// 설정 앱을 열 수 없을 때, 유저에게 직접 설정앱을 열어달라고 요청
						UIPopup.spawnOK(sc.get("camera.noaccesssetting.poprup.title", 0), sc.get("camera.noaccesssetting.poprup.desc", 0), () =>
						{

						});
					}
                }, () => { });
            }
#else
			//notthing..
#endif
		}

		private void clearCameraContent()
		{
			if (_cameraContent != null)
			{
				UnityEngine.Object.DestroyImmediate(_cameraContent);
				_cameraContent = null;
			}

			image_camera_target.gameObject.SetActive(false);
			//camera_filter_tab?.gameObject.SetActive(false);
		}

		private void createCameraContent()
		{
			clearCameraContent();

			GameObject source = camera_contents[_cameraType];
			_cameraContent = Instantiate(source, ClientMain.instance.transform, false);

			if( _cameraType == CameraType.normal)
			{
				//CameraBackground cameraBackground = _cameraContent.GetComponent<CameraBackground>();
				//if( cameraBackground != null)
    //            {
    //                cameraBackground.setRenderTarget(image_camera_target);
    //                camera_filter_tab?.setCameraBackground(cameraBackground);
    //                image_camera_target.gameObject.SetActive(true);
    //                camera_filter_tab?.gameObject?.SetActive(true);
				//}
			}
		}

		public void setMode(int mode)
		{
			_mode = mode;
			//btn_next.gameObject.SetActive(mode == Mode.moment);
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
			close(); // 음...

			if (_mode == Mode.trip_photo)
			{
				UITripStatus.getInstance().open();
			}
		}

		public void onClickGallery()
		{
			// 탐험하기 사진찍기 모드에서는 갤러리 사진 사용 불가
			if( _mode == Mode.trip_photo)
			{
				return;
			}

			UIGalleryPicker.getInstance().setFinishCallback(onSelectPhoto);
			UIGalleryPicker.getInstance().open();
		}

		private void onSelectPhoto(List<NativeGallery.NativePhotoContext> photo_list)
		{
			if( photo_list.Count > 0)
			{
                //UIBackNavigation.getInstance().setup(this, UIPhotoConfirm.getInstance());
                //UIBackNavigation.getInstance().open();

                //UIPhotoConfirm.getInstance().setup(photo_list, _mode);
                //UIPhotoConfirm.getInstance().open();

                if (_mode == UIPhotoTake.Mode.moment)
                {
					//UIBackNavigation.getInstance().setup(this, UIMakeMomentCommit.getInstance());
					//UIBackNavigation.getInstance().open();

					ViewModel.MakeMoment.PhotoList = MakeMomentViewModel.makePhotoList(photo_list);

                    close();
                    UIMakeMomentCommit.getInstance().open();

                    ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentCommit.getInstance());
                }
                else if (_mode == UIPhotoTake.Mode.profile_edit)
                {
                    UIEditProfile.getInstance().onSelectPhoto(photo_list);
                    //UIBackNavigation.getInstance().backTo(UIEditProfile.getInstance(), this);
                }
                else if (_mode == UIPhotoTake.Mode.profile_direct)
                {

                }

                //ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoConfirm.getInstance());
			}
		}

		public void onClickFilter()
        {
			filter_root.gameObject.SetActive(true);

			btn_filter.gameObject.SetActive(false);
			btn_take.gameObject.SetActive(false);
        }

		public void onClickFilterCancel()
        {
            filter_root.gameObject.SetActive(false);

            btn_filter.gameObject.SetActive(true);
            btn_take.gameObject.SetActive(true);
        }

		private void setActivesForTake(bool active)
		{
			foreach(GameObject go in hide_for_take)
			{
				go.SetActive(active);
			}
		}

		public void onClickTake()
		{
			StartCoroutine(takeScreenShot());
		}

		private IEnumerator takeScreenShot()
		{
			setActivesForTake(false);
			//UIBackNavigation.getInstance().gameObject.SetActive(false);

			yield return new WaitForEndOfFrame();

			try
			{
				string directory = Application.temporaryCachePath + "/CameraImg";

				if( Directory.Exists( directory) == false)
				{
					Directory.CreateDirectory(directory);
				}

				// 2022.03.10 이강희 확장자를 jpg변경
				string file_path = directory + $"/screenshot{Time.time.ToString()}.jpg";

				Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

				// 1:1 비율... 현재는 Screen.width 기준으로 1:1을 만들었음.
				// 2022.08.10 소현 : 현재 기준으로 3:4 (0.75)
				float scale = /*1.0f*/0.75f;
				Texture2D croppedTexture = new Texture2D(Screen.width, (int)(Screen.width / scale));
                var canvasScaler = GetComponentInParent<CanvasScaler>();
/*                if (canvasScaler != null)
                    scale = canvasScaler.referenceResolution.y / Screen.height;*/

                // 97 : top_root의 고정 크기.. 
				// 2022.08.10 소현 : 크기 84 로 수정
				float top_root = 84f;
                float textureScale = screenShot.height / Screen.height;
                croppedTexture.SetPixels(screenShot.GetPixels(0, (int)((Screen.height - (top_root + Screen.width / scale)) * textureScale), (int)(Screen.width * textureScale), (int)(Screen.width * textureScale / scale)));
                croppedTexture.Apply();

				//// 2022.03.10 이강희 사이즈 축소
				//int newWidth;
				//int newHeight;
				//if( croppedTexture.width >= croppedTexture.height )
				//{
				//	newHeight = 1440;
				//	newWidth = 1440 * croppedTexture.width / croppedTexture.height;
				//}
				//else
				//{
				//	newWidth = 1440;
				//	newHeight = 1440 * croppedTexture.height / croppedTexture.width;
				//}

				//croppedTexture.Reinitialize(newWidth, newHeight, croppedTexture.format, false);

                byte[] data = croppedTexture.EncodeToJPG(75);

                File.WriteAllBytes(file_path, data);
                UnityEngine.Object.DestroyImmediate(screenShot);
				UnityEngine.Object.DestroyImmediate(croppedTexture);

                //UIBackNavigation.getInstance().setup(this, UIPhotoConfirm.getInstance());
                //UIBackNavigation.getInstance().open();

                List<NativeGallery.NativePhotoContext> photo_list = new List<NativeGallery.NativePhotoContext>();
				photo_list.Add(new NativeGallery.NativePhotoContext(file_path));

                //UIPhotoConfirm.getInstance().setup(photo_list, _mode);
                //UIPhotoConfirm.getInstance().open();

                //ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoConfirm.getInstance());

                if (_mode == UIPhotoTake.Mode.moment)
                {
					//UIBackNavigation.getInstance().setup(this, UIMakeMomentCommit.getInstance());
					//UIBackNavigation.getInstance().open();

					ViewModel.MakeMoment.PhotoList = MakeMomentViewModel.makePhotoList(photo_list);

					//close();
                    UIMakeMomentCommit.getInstance().open();

                    ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentCommit.getInstance());
                }
                else if (_mode == UIPhotoTake.Mode.profile_edit)
                {
                    UIEditProfile.getInstance().onSelectPhoto(photo_list);
                    //UIBackNavigation.getInstance().backTo(UIEditProfile.getInstance(), this);
                }
                else if (_mode == UIPhotoTake.Mode.profile_direct)
                {

                }
				else if( _mode == Mode.trip_photo)
				{
					UIMap.getInstance().addTripPhoto(photo_list);
					UITripStatus.getInstance().open(null, TransitionEventType.openImmediately);
					close();
					Invoke("savedMsg", 0.7f);
				}
				else if(_mode == Mode.trip_certify)
                {
					UITripCertification.getInstance().setPhoto(photo_list);
					UITripCertification.getInstance().open();
                    ClientMain.instance.getPanelNavigationStack().push(this, UITripCertification.getInstance());
                }
            }
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			setActivesForTake(true);
			//UIBackNavigation.getInstance().gameObject.SetActive(true);
		}

		private void savedMsg()
        {
			var text = GlobalRefDataContainer.getStringCollection().get("triproute.photoSaved", 0);
			UIToastNotification.spawn(text);
		}

		public void onClickNext()
		{
			//UIBackNavigation.getInstance().setup(this, UIMakeMomentCommit.getInstance());
			//UIBackNavigation.getInstance().open();

			close();
			UIMakeMomentCommit.getInstance().open();

			ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentCommit.getInstance());
		}

		public void onClickType_Normal()
		{
			_cameraType = CameraType.normal;
			createCameraContent();
		}

		public void onClickType_FaceAR()
		{
			_cameraType = CameraType.face_ar;
			createCameraContent();
		}

		public void onClickType_Action()
		{
			_cameraType = CameraType.action;
			createCameraContent();
		}

		public void onClickType_AR()
		{
			_cameraType = CameraType.ar;
			createCameraContent();
		}

		public void onClickFacingDirection()
		{
			if( _cameraType != CameraType.normal)
			{
				return;
			}
			if( _cameraContent != null)
			{
				//ARCameraManager am = _cameraContent.GetComponentInChildren<ARCameraManager>();
				//if( am != null)
				//{
				//	if (am.currentFacingDirection == CameraFacingDirection.User)
				//	{
				//		am.requestedFacingDirection = CameraFacingDirection.World;
				//	}
				//	else
				//	{
				//		am.requestedFacingDirection = CameraFacingDirection.User;
				//	}
				//}
			}
		}
	}
}
