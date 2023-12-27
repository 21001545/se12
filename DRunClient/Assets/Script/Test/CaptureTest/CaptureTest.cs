//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.UI;

//public class CaptureTest : MonoBehaviour
//{
//    [SerializeField]
//    private CameraBackground _cameraBackground = null;
//    [SerializeField]
//    private RawImage _resultImage = null;

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    public void onCaptrue()
//    {
//        _cameraBackground.onCaptrue((string fileName) =>
//        {
//            _resultImage.texture = null;
//            if (fileName == null)
//            {
//                return;
//            }

//            try
//            {
//                Texture2D result = new Texture2D(2, 2);
//                if (result.LoadImage(File.ReadAllBytes(fileName)))
//                    _resultImage.texture = result;
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e);
//            }
//        });
//    }
//}
