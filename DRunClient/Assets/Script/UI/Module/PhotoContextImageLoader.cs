using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Festa.Client
{
    public class PhotoContextImageLoader : CoroutineStepProcessor
    {
        private int _call_id;
        private TextureCache _textureCache;
        private NativeGallery.NativePhotoContext _photoContext;
        private int _textureKey;
        private string _filePath;

        private Texture _texture;
        private TextureCacheItemUsage _textureUsage;

        private static Dictionary<int, int> _loadingMap = new Dictionary<int, int>();

        public static bool tryLoading(int textureKey)
        {
            lock (_loadingMap)
            {
                if (_loadingMap.ContainsKey(textureKey) == false)
                {
                    _loadingMap.Add(textureKey, 1);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void endLoading(int textureKey)
        {
            lock (_loadingMap)
            {
                if (_loadingMap.ContainsKey(textureKey))
                {
                    _loadingMap.Remove(textureKey);
                }
            }
        }

        public static PhotoContextImageLoader create(MonoBehaviour behaviour,TextureCache textureCache,NativeGallery.NativePhotoContext photoContext,int call_id)
        {
            PhotoContextImageLoader loader = new PhotoContextImageLoader();
            loader.init(behaviour, textureCache, photoContext, call_id);
            return loader;
        }

        private void init(MonoBehaviour behaviour,TextureCache textureCache,NativeGallery.NativePhotoContext photoContext,int call_id)
        {
            base.init(behaviour);

            _call_id = call_id;
            _textureCache = textureCache;
            _photoContext = photoContext;

            _textureKey = EncryptUtil.makeHashCode("PhotoContext:" + photoContext.path + photoContext.identifier);
        }

        protected override void buildSteps()
        {
            _stepList.Add(loadFromCache);
        }

        public void run(UnityAction<int,TextureCacheItemUsage> callback)
        {
            if( tryLoading( _textureKey) == false)
            {
                MainThreadDispatcher.dispatchFixedUpdate(() => {
                    run(callback);
                });
            }
            else
            {
                runSteps(0, result => {
                    endLoading(_textureKey);
                    
                    if( result.failed())
                    {
                        Debug.LogException(result.cause());
                    }

                    callback(_call_id, _textureUsage);
                    _texture = null;
                });
            }
        }

        private IEnumerator loadFromCache(Action<AsyncResult<Module.Void>> handler)
        {
            _textureUsage = _textureCache.makeUsage(_textureKey);
            if( _textureUsage != null)
            {
                _texture = _textureUsage.texture;
                handler(Future.succeededFuture());
                yield break;
            }

            _stepList.Add(makeThumbnail);
            handler(Future.succeededFuture());
        }

        private IEnumerator makeThumbnail(Action<AsyncResult<Module.Void>> handler)
        {
            if( _photoContext.justifiedHEIC == true)
            {
                _filePath = _photoContext.path;

                _stepList.Add(loadFromFile);
                handler(Future.succeededFuture());
                yield break;
            }

            _filePath = null;
#if UNITY_IOS
            // ios... file path로 async로 얻어오는건 아직 구현이 안됨.
            if (_photoContext.path == _photoContext.identifier)
            {
                _filePath = NativeGallery.GetImageMiniThumbnailPathFromImagePath(_photoContext.path, 400, 400, true);
            }
            else
            {
                string requestID = NativeGallery.GetImageThumbnailPathFromPhotoContextAsync(_photoContext, 400, 400, (string path, string uniqueId) =>
                {
                    _filePath = path;
                });
            }
#else

            string requestID = NativeGallery.GetImageThumbnailPathFromPhotoContextAsync(_photoContext, 400, 400, (string path, string uniqueId) => {
                _filePath = path;
            });
#endif

            yield return new WaitWhile(() => { return string.IsNullOrEmpty(_filePath); });

            _stepList.Add(loadFromFile);
            handler(Future.succeededFuture());
        }

        private IEnumerator loadFromFile(Action<AsyncResult<Module.Void>> handler)
        {
#if UNITY_ANDROID || UNITY_IOS
            if( _filePath.StartsWith("file://") == false)
            {
                _filePath = "file://" + _filePath;
            }
#endif
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_filePath, true))
            {
                yield return request.SendWebRequest();

                if( request.result != UnityWebRequest.Result.Success)
                {
                    handler(Future.failedFuture( request.error));
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if( texture != null)
                {
                    _textureCache.registerCache(_textureKey, texture);
                    _textureUsage = _textureCache.makeUsage(_textureKey);
                    handler(Future.succeededFuture());
                }
                else
                {
                    handler(Future.failedFuture("texture is null"));
                }
            }
        }
    }
}
