using Festa.Client.Module;
using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UIPhotoConfirm : UISingletonPanel<UIPhotoConfirm>
	{
		public Transform itemRoot;
		public UIPhotoConfirm_Item itemSource;
		public RectTransform rtViewport;

		private List<UIPhotoConfirm_Item> _itemList;
		private int _mode;
		private List<NativeGallery.NativePhotoContext> _photoList;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_itemList = new List<UIPhotoConfirm_Item>();
			_photoList = new List<NativeGallery.NativePhotoContext>();
			itemSource.gameObject.SetActive(false);
		}
        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
				// 내부에서 도는 코루틴이.. 팝업이 deactive 상태라서 실행이 안됨.
                foreach (NativeGallery.NativePhotoContext imagePath in _photoList)
                {
                    UIPhotoConfirm_Item item = itemSource.make<UIPhotoConfirm_Item>(itemRoot, GameObjectCacheType.actor);
                    item.setup(rtViewport, imagePath);
                    _itemList.Add(item);
                }
            }
        }

        public void setup(List<NativeGallery.NativePhotoContext> photo_list,int mode)
		{
			_mode = mode;

			GameObjectCache.getInstance().delete(_itemList);

			_photoList.Clear();
			_photoList.AddRange(photo_list);
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		public void onClickNext()
		{
			//if (_mode == UIPhotoTake.Mode.moment)
			//{
			//	//UIBackNavigation.getInstance().setup(this, UIMakeMomentCommit.getInstance());
			//	//UIBackNavigation.getInstance().open();

			//	ViewModel.MakeMoment.PhotoList = MakeMomentViewModel.makePhotoList(_photoList);

			//	close();
			//	UIMakeMomentCommit.getInstance().open();

			//	ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentCommit.getInstance());
			//}
			//else if( _mode == UIPhotoTake.Mode.profile_edit)
			//{
			//	UIEditProfile.getInstance().onSelectPhoto(_photoList);
			//	//UIBackNavigation.getInstance().backTo(UIEditProfile.getInstance(), this);
			//}
			//else if( _mode == UIPhotoTake.Mode.profile_direct)
			//{
				


			//}
		}
	}
}
