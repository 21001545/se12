//using Festa.Client.Module;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using Unity.Collections;
//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;

//// 렌더 요소들을 파일로 저장하자.
//public class Capture
//{
//    private static string _temporaryImagePath = null;
//    private static string temporaryImagePath
//    {
//        get
//        {
//            if (_temporaryImagePath == null)
//            {
//                _temporaryImagePath = Path.Combine(Application.temporaryCachePath, "captrue");
//                try
//                {
//                    Directory.CreateDirectory(_temporaryImagePath);
//                }
//                catch(Exception e)
//                {
//                    Debug.LogError(e);
//                }
//            }

//            return _temporaryImagePath;
//        }
//    }

//    /// <summary>
//    ///  Texture를 jpeg로 저장한다.
//    /// </summary>
//    /// <param name="target"></param>
//    /// <param name="callback">저장된 파일의 경로 전달, 생성하지 못하였다면 null</param>
//    /// <returns></returns>
//    public static void takeCapture(Texture2D target, Action<string> callback )
//    {
//        if ( target == null )
//        {
//            Debug.Log("takeCapture : texture is null");
//            callback?.Invoke(null);
//            return;
//        }

//        byte[] bytes = target.EncodeToJPG();

//        if ( bytes == null )
//        {
//            Debug.Log("takeCapture : invalid texture data");
//            callback?.Invoke(null);
//            return;
//        }

//        string fileName = ((TimeUtil.unixTimestampUtcNow().ToString() + UnityEngine.Random.value.ToString()) + ".jpg");
//        fileName = Path.Combine(temporaryImagePath, fileName);

//        File.WriteAllBytes(fileName, bytes);

//        callback?.Invoke(fileName);
//    }

//    /// <summary>
//    /// ARCameraManager에서 직접 읽어오기, 원본의 데이터를 가져올 수 있음
//    /// </summary>
//    /// <param name="arCameraManager"></param>
//    /// <param name=""></param>
//    /// <param name="callback"></param>
//    public static void takeCapture(ARCameraManager arCameraManager, Action<string> callback)
//    {
//        if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image) == false)
//        {
//            Debug.Log("takeCapture : TryAcquireLatestCpuImage -> false");
//            callback?.Invoke(null);
//            return;
//        }

//        Debug.Log($"takeCapture plane count ${image.planeCount}");
//        for (int planeIndex = 0; planeIndex < image.planeCount; ++planeIndex)
//        {
//            // 이 plane들은.. 음 카메라별 차이점에서 오는건가?
//            var plane = image.GetPlane(planeIndex);
//            Debug.LogFormat("Plane {0}:\n\tsize: {1}\n\trowStride: {2}\n\tpixelStride: {3}",
//                planeIndex, plane.data.Length, plane.rowStride, plane.pixelStride);
//        }

//        Debug.Log($"{image.width} * {image.height}");

//        image.ConvertAsync(new XRCpuImage.ConversionParams
//        {
//            inputRect = new RectInt(0, 0, image.width, image.height),
//            outputDimensions = new Vector2Int(image.width, image.height), // 스케일 다운은 필요하지 않을 듯..
//            outputFormat = TextureFormat.RGB24,
//            transformation = XRCpuImage.Transformation.MirrorX
//        }, (XRCpuImage.AsyncConversionStatus status, XRCpuImage.ConversionParams conversionParams, NativeArray<byte> data) =>
//        {
//            if (status != XRCpuImage.AsyncConversionStatus.Ready)
//            {
//                Debug.Log($"takeCapture image convert failed");
//                Debug.Log(status);
//                callback?.Invoke(null);
//                return;
//            }

//            var texture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);
//            texture.LoadRawTextureData(data);
//            texture.Apply();

//            byte[] bytes = texture.EncodeToJPG();
//            string fileName = ((TimeUtil.unixTimestampUtcNow().ToString() + UnityEngine.Random.value.ToString()) + ".jpg");
//            fileName = Path.Combine(temporaryImagePath, fileName);

//            File.WriteAllBytes(fileName, bytes);
//            callback?.Invoke(fileName);
//        });

//        // It's safe to dispose the image before the async operation completes.
//        // 비동기 작업이 완료 되기전에, image를 dispose하는게 안전하다고 함..?
//        image.Dispose();

//    }
//}
