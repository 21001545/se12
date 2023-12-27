using Festa.Client;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static NativeGallery;

public class UISelectPhoto : UIPanel
{
    // 카메라에서 새로 찍은 사진이든, 사진첩에서 가져온 사진이던.. Context가 반환된다.
    private UnityAction<NativePhotoContext> _callback = null;

    public static UISelectPhoto spawn( UnityAction<NativePhotoContext> callback)
    {
        UISelectPhoto popup = UIManager.getInstance().spawnInstantPanel<UISelectPhoto>();
        
        popup._callback = callback;

        return popup;
    }

    public void onClickTakePhoto()
    {
        close();
        var permission = NativeCamera.TakePicture((string path) =>
        { 
            _callback?.Invoke(new NativeGallery.NativePhotoContext(path));
        }, 1024);
    }

    public void onClickLibrary()
    {
        close();
        UIGalleryPicker.getInstance().setFinishCallback(onSelectPhoto);
        UIGalleryPicker.getInstance().open();
        UIGalleryPicker.getInstance().setMaxCount(1);
    }

    private void onSelectPhoto(List<NativeGallery.NativePhotoContext> photoList)
    {
        if (photoList.Count > 0)
            _callback?.Invoke(photoList[0]);
        else
            _callback?.Invoke(null);
    }
}
