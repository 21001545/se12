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

    // ��.. ������� �̺�Ʈ �����ϱⰡ... �׳�...
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
        // LoadImage�� �ſ�ſ� ������, Decode �ܰ谡 ���� ũ��.
        // �Դٰ� ������ Main thread������ �ǹǷ�, Threading, Awaiter������ �ȵ�.
        // 1. ���� ������ Native texture�� �����,�̸� �̿��Ͽ� Texture2D.CreateExternalTexture() ��� �ϴ°�
        // ������ glTexture�� ����°�.. �ᱹ main thread... 
        // 2. ��°�δ� Native���� Decode�� �����͸� ���� �޾� LoadRawTextureData()�� ����ϴ� ���
        // 3. UnityWebRequest�� �̿��� coroutine��..¦�� �񵿱� ó���ϱ�.. �̰� �׳���? 
        // 4. �׳� LoadImage ����.

        // Texture2D�� ���� �޸�Ǯ�� ����ϸ鼭..?
        // ��.. �񵿱������� �ε��� �� �ִ� ���� ���� ����Ʈ�ε� ��������...

        // ���̵��� 1 > 2 > 3 > 4 ...
        // 3������ �غ���..

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
