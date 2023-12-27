using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PolyAndCode.UI;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using UnityEngine.Networking;

public class GalleryTestCell : MonoBehaviour, ICell
{
    [SerializeField]
    private RawImage _thumbnailImage = null;

    Coroutine _loadCoroutine = null;
    private string _currentImagePath;

    // 음.. 상단으로 이벤트 전달하기가... 그냥...
    public Action<string> ExifCallback = null;

    private string _thumbnailRequestId = null;

    public void onClicked()
    {
        if (string.IsNullOrEmpty(_currentImagePath))
            return;

        ExifCallback?.Invoke(_currentImagePath);       
    }

    public void setThumbnail(NativeGallery.NativePhotoContext photoContext)
    {
        _currentImagePath = photoContext.path;
        if (_loadCoroutine != null )
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        _thumbnailImage.texture  = null;
         _thumbnailRequestId = NativeGallery.GetImageThumbnailPathFromPhotoContextAsync(photoContext, 400, 400, onThumbnailPathCallback);

			Debug.Log($"Set : {_thumbnailRequestId}");
    }

    public void onThumbnailPathCallback ( string path, string uniqueId )
    {
        Debug.Log($"callback : {path} {_thumbnailRequestId} {uniqueId}");
        if ( _thumbnailRequestId == uniqueId)
        {
            _loadCoroutine = StartCoroutine(loadCoroutine(path));
            _thumbnailRequestId = null;
        }
    }

    private IEnumerator loadCoroutine(string thumbnailPath)
    {
        // LoadImage는 매우매우 느린데, Decode 단계가 가장 크다.
        // 게다가 무조건 Main thread에서만 되므로, Threading, Awaiter에서도 안됨.
        // 1. 가장 빠른건 Native texture를 만들고,이를 이용하여 Texture2D.CreateExternalTexture() 사용 하는것
        // 하지만 glTexture를 만드는건.. 결국 main thread... 
        // 2. 둘째로는 Native에서 Decode한 데이터를 전달 받아 LoadRawTextureData()를 사용하는 방법
        // 3. UnityWebRequest를 이용한 coroutine로..짝퉁 비동기 처리하기.. 이게 그나마? 
        // 4. 그냥 LoadImage 쓰기.

        // Texture2D에 대한 메모리풀도 사용하면서..?
        // 음.. 비동기적으로 로드할 수 있는 모델이 가장 베스트인데 말이지요...

        // 난이도는 1 > 2 > 3 > 4 ...
        // 3번으로 해보자..

#if UNITY_ANDROID || UNITY_IOS
        if (thumbnailPath.StartsWith("file://") == false )
        {
            thumbnailPath = "file://" + thumbnailPath;
        }
#endif
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(thumbnailPath, false))
        {
            yield return request.SendWebRequest();            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Thumbnail load error : {thumbnailPath} {request.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (_thumbnailImage.texture != null )
            {
                UnityEngine.Object.DestroyImmediate(_thumbnailImage.texture);
                _thumbnailImage.texture = null;
            }

            if (texture != null)
            {
                _thumbnailImage.texture = texture;
            }
        }
    }
}
