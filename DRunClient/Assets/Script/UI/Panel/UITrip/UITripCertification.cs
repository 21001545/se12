using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Festa.Client.Module.UI;
using TMPro;
using Festa.Client.NetData;
using Festa.Client.Module;

namespace Festa.Client
{
    public class UITripCertification : UISingletonPanel<UITripCertification>
    {
        [Header("trip data")]
        [SerializeField]
        private TMP_Text txt_steps;
        [SerializeField]
        private TMP_Text txt_distance;
        [SerializeField]
        private TMP_Text txt_speed;
        [SerializeField]
        private TMP_Text txt_cal;
        [SerializeField]
        private TMP_Text txt_alt;
        [SerializeField]
        private TMP_Text txt_date;
        [SerializeField]
        private TMP_Text txt_location;

        [SerializeField]
		public UITripPath tripPath;

		[SerializeField]
        Canvas _canvas;

        [SerializeField]
        private UIPhotoThumbnail _photo;

        [SerializeField]
        private GameObject[] _photoOverlay;
        [SerializeField]
        private GameObject[] _highlightType;

        private int _photoOverlayType = 0;
        private ClientTripLog _tripLog;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            tripPath.init();
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            base.open(param, transitionType, closeType);

            _photoOverlayType = 0;
            _photoOverlay[0].SetActive(true);
            _photoOverlay[1].SetActive(false);
            _photoOverlay[2].SetActive(false);
            _highlightType[0].SetActive(true);
            _highlightType[1].SetActive(false);
            _highlightType[2].SetActive(false);

            txt_steps.text = UITripEndResult.getInstance().data_totalSteps.ToString("N0");
            txt_distance.text = UITripEndResult.getInstance().data_distance.ToString("F2");
            txt_speed.text = UITripEndResult.getInstance().data_speed.ToString("F2");
            txt_cal.text = UITripEndResult.getInstance().data_calories.ToString("N0");
            txt_alt.text = UITripEndResult.getInstance().data_altitude.ToString("F2");
            txt_date.text = UITripEndResult.getInstance().data_tripDate.ToString("yyyy.MM.dd");
            txt_location.text = UITripEndResult.getInstance().data_location;

            _tripLog = UITripEndResult.getInstance().getTripLog();

            tripPath.setup(_tripLog.path_data);
		}

        public void setPhoto(List<NativeGallery.NativePhotoContext> photoList)
        {
            _photo.setImageFromFile(photoList[0]);
        }

        public void onClickOverlay(int type)
        {
            if (_photoOverlayType == type)
                return;

            _photoOverlay[_photoOverlayType].SetActive(false);
            _highlightType[_photoOverlayType].SetActive(false);

            _photoOverlayType = type;

            _photoOverlay[_photoOverlayType].SetActive(true);
            _highlightType[_photoOverlayType].SetActive(true);
        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        public void onClickApply()
        {
            StartCoroutine(applyUI());
        }

        private IEnumerator applyUI()
        {
            yield return new WaitForEndOfFrame();

            try
            {
                string directory = Application.temporaryCachePath + "/CameraImg";

                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }

                string file_path = directory + $"/screenshot{Time.time.ToString()}.jpg";

                Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

                float shrinkRate = 0.915f;                  // 약간 작아진 비율
                float scale = 0.75f;                        // 이미지 비율 3:4 (0.75)
                Texture2D croppedTexture = new Texture2D((int)(shrinkRate * Screen.width), (int)(shrinkRate * Screen.width / scale));

                float textureScale = screenShot.height / Screen.height;
                float heightRatio = 264f / 812f;    // 높이 대비 비율
                //(int)((Screen.height - (top_root + shrinkRate * Screen.width / scale)) * textureScale)
                croppedTexture.SetPixels(screenShot.GetPixels((int)(0.043f * Screen.width), (int)(Screen.height * heightRatio),
                                                                (int)(shrinkRate * Screen.width * textureScale), (int)(shrinkRate * Screen.width * textureScale / scale)));
                croppedTexture.Apply();

                byte[] data = croppedTexture.EncodeToJPG(75);

                File.WriteAllBytes(file_path, data);
                DestroyImmediate(screenShot);
                DestroyImmediate(croppedTexture);

                // 모먼트로 넘기기
                List<NativeGallery.NativePhotoContext> photo_list = new List<NativeGallery.NativePhotoContext>();
                photo_list.Add(new NativeGallery.NativePhotoContext(file_path));

                ClientMain.instance.getViewModel().MakeMoment.PhotoList = MakeMomentViewModel.makePhotoList(photo_list);
                UIMakeMomentCommit.getInstance().open();
                ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentCommit.getInstance());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}