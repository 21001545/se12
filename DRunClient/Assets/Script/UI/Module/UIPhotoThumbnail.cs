using Festa.Client.Module;
using Festa.Client.Module.Net;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIPhotoThumbnail : UIBehaviour
	{
		public enum ScaleType
        {
			fit, // 이미지의 종횡비를 유지하지 않고, 레이아웃 크기로 변경함.
			center, // 이미지 원본 사이즈 그대로, 중앙에 표시. 
			centerCrop, // 이미지의 종횡비를 유지하며, 이미지의 짧은 쪽을 레이아웃에 맞춤
			centerInside, // 이미지의 종횡비를 유지하지만, 이미지의 긴 쪽을 레이아웃에 맞춤			
        }

		public RawImage image_thumbnail;
		public Texture2D empty_texture;

		[SerializeField]
		private ScaleType _scaleType = ScaleType.fit;

		private Coroutine _loadingCoroutine = null;
		private Texture _loadedTexture;
		private TextureCacheItemUsage _loadedTextureUsage;
		
		private class SourceType
		{
			public const int empty = 0;
			public const int raw_texture = 1;
			public const int url = 2;
			public const int photo_context = 3;
		}

		private int		_currentSourceType = SourceType.empty;
		private string	_currentFilePath;
		private string	_currentURL;
		private NativeGallery.NativePhotoContext _currentPhotoContext;

		private TextureCache textureCache => ClientMain.instance.getTextureCache();

		public string getCurrentURL()
        {
			return _currentURL;
        }

		protected void clear()
		{
			if (_loadingCoroutine != null)
			{
				StopCoroutine(_loadingCoroutine);
				_loadingCoroutine = null;
			}

			if(_loadedTexture != null)
			{
				UnityEngine.Object.Destroy(_loadedTexture);
				_loadedTexture = null;
			}
			
			if( _loadedTextureUsage != null)
			{
				textureCache.deleteUsage(_loadedTextureUsage);
				_loadedTextureUsage = null;
			}

			//image_thumbnail.texture = empty_texture;
			//Debug.Log("clear",gameObject);
		}

		public void setEmpty()
		{
			clear();

			image_thumbnail.texture = empty_texture;
			_currentSourceType = SourceType.empty;
		}

		public void setImage(Texture2D texture,bool hasOwner)
		{
			// check same
			if( _currentSourceType == SourceType.raw_texture && texture == _loadedTexture)
			{
				return;
			}

			clear();

			if( hasOwner == false)
			{
				_loadedTexture = texture;
			}

			_currentSourceType = SourceType.raw_texture;

			applyTexture(texture);
		}

		public void setImageFromFile(string imagePath)
		{
			setImageFromFile(new NativeGallery.NativePhotoContext(imagePath));
		}

		public void setImageFromFile(NativeGallery.NativePhotoContext photoContext)
		{
			// check same
			if (_currentSourceType == SourceType.photo_context && _currentPhotoContext == photoContext)
            {
                return;
			}

			if( photoContext == null)
            {
                setEmpty();
                return;
			}

			clear();

			_currentSourceType = SourceType.photo_context;
			_currentPhotoContext = photoContext;

			// TODO: 확인필요
			_loadingCoroutine = ClientMain.instance.StartCoroutine(loadFromPhotoContext(photoContext));
		}

		public void setImageFromCDN(string url)
		{
			if( _currentSourceType == SourceType.url && _currentURL == url)
			{
				return;
			}

			//Debug.Log($"setImageFromCDN:id[{GetInstanceID()}] url[{url}]");

			if( string.IsNullOrEmpty(url))
            {
				setEmpty();
                return;
            }

            clear();

            _currentSourceType = SourceType.url;
			_currentURL = url;

			// 2022.02.28 이강희 
			if( empty_texture != null)
			{
				image_thumbnail.texture = empty_texture;
			}

			_loadingCoroutine = ClientMain.instance.StartCoroutine(loadFromCDN(url));
		}

		private static int _last_call_id = 0;

		public IEnumerator loadFromCDN(string url)
		{
			bool wait = true;

			int call_id = ++_last_call_id;

			CDNImageDownloader downloader = ClientMain.instance.getNetwork().createImageDownaloder(url, call_id);
			downloader.run((id, textureUsage) => {
				wait = false;
				if (textureUsage != null)
				{
					//Debug.Log($"loadFromCDN[Download or ReadFile]: {url} key:{textureUsage.key}");

					if (id != call_id)
					{
						textureCache.deleteUsage(textureUsage);
					}
					else
					{
						_loadedTextureUsage = textureUsage;
						applyTexture(textureUsage.texture);
					}
				}
				else
				{
					clear();
				}
			});

			yield return new WaitWhile(() => { return wait; });
		}

		public IEnumerator loadFromPhotoContext(NativeGallery.NativePhotoContext photoContext)
        {
			bool wait = true;

			int call_id = ++_last_call_id;

			PhotoContextImageLoader loader = PhotoContextImageLoader.create(ClientMain.instance, textureCache, photoContext, call_id);
			loader.run((id, textureUsage) => {
				wait = false;
				if( textureUsage != null)
                {
					if( id != call_id)
                    {
						textureCache.deleteUsage(textureUsage);
                    }
					else
                    {
						_loadedTextureUsage = textureUsage;
						applyTexture(textureUsage.texture);
                    }
                }
				else
                {
					clear();
                }
			});

			yield return new WaitWhile(() => { return wait; });
        }

//		public IEnumerator loadFromPhotoContext(NativeGallery.NativePhotoContext photoContext)
//        {
//			int textureKey = EncryptUtil.makeHashCode("PhotoContext:" + photoContext.path + photoContext.identifier);

//			Debug.Log($"loadFromPhotoContext:textureKey[{textureKey}]");

//			_loadedTextureUsage = textureCache.makeUsage(textureKey);
//			if( _loadedTextureUsage != null)
//			{
//				applyTexture(_loadedTextureUsage.texture);
//				yield break;
//			}
			
//			string thumbnailPath = null;

//			// 2021.12.15 이강희 버그 수정 : HEIC를 JPG로 이미 변환한거라서 다시 썸네일을 만들 필요 없다
//			if(photoContext.justifiedHEIC == false)
//            {
//				string requestId = NativeGallery.GetImageThumbnailPathFromPhotoContextAsync(photoContext, 400, 400, (string path, string uniqueId) =>
//				{
//					thumbnailPath = path;
//				});
//				yield return new WaitWhile(() => { return string.IsNullOrEmpty(thumbnailPath); });

//				if (thumbnailPath == null)
//				{
//					Debug.LogError($"Not Found thubnail : {photoContext.path}");
//					yield break;
//				}
//			}
//			else
//            {
//				thumbnailPath = photoContext.path;
//            }


//#if UNITY_ANDROID || UNITY_IOS
//			if (thumbnailPath.StartsWith("file://") == false)
//			{
//				thumbnailPath = "file://" + thumbnailPath;
//			}
//#endif
////			Debug.Log($"{GetInstanceID()} start loading:{thumbnailPath}");

//			using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(thumbnailPath, true))
//			{
//				yield return request.SendWebRequest();

////				Debug.Log($"{GetInstanceID()} end loading:{thumbnailPath}");

//				if (request.result != UnityWebRequest.Result.Success)
//				{
//					Debug.LogError($"Thumbnail load error : {thumbnailPath} {request.error}");
//					yield break;
//				}

//				Texture2D texture = DownloadHandlerTexture.GetContent(request);
//				if (texture != null)
//				{
//					textureCache.registerCache(textureKey, texture);
//					_loadedTextureUsage = textureCache.makeUsage(textureKey);
//					applyTexture(texture);
//				}
//			}
//		}

		private void applyTexture(Texture texture)
		{
			image_thumbnail.texture = texture;
			resetImageSize();
		}

		private void resetImageSize()
		{
			Texture texture = image_thumbnail.texture;
			if( texture == null)
			{
				return;
            }
            RectTransform rt_image = image_thumbnail.transform as RectTransform;
            RectTransform rt_parent = rt_image.parent as RectTransform;
            Rect rect_parent = rt_parent.rect;

            if ( _scaleType == ScaleType.fit)
            {
				// 할필요가 읍다!
            }
			else
            {
				// 초기화 해주고,
				rt_image.sizeDelta = Vector2.zero;

				Vector2 sizeDelta = new Vector2();

				if ( _scaleType == ScaleType.center)
                {
					// 이건.. 
					sizeDelta.x = texture.width - rt_image.rect.width;
					sizeDelta.y = texture.height - rt_image.rect.height;
                }
				else if ( _scaleType == ScaleType.centerInside)
                {
                    // 이미지의 긴 쪽을 레이아웃에 맞추자.
                    float ratio = Mathf.Min(rt_image.rect.width / (float)texture.width, rt_image.rect.height / (float)texture.height);

                    sizeDelta.x = texture.width * ratio - rt_image.rect.width;
                    sizeDelta.y = texture.height * ratio - rt_image.rect.height;
                }
				else if ( _scaleType == ScaleType.centerCrop)
                {
                    // 짧은 쪽을 맞추어 주자
                    float ratio = Mathf.Max(rt_image.rect.width / (float)texture.width, rt_image.rect.height / (float)texture.height);
                    sizeDelta.x = texture.width * ratio - rt_image.rect.width;
                    sizeDelta.y = texture.height * ratio - rt_image.rect.height;
                }
                rt_image.sizeDelta = sizeDelta;
            }
		}

		//protected override void OnDisable()
		//{
		//	base.OnDisable();
		//	setEmpty();
		//}
	}
}
