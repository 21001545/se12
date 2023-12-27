using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using PolyAndCode.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMakeMomentCommit : UISingletonPanel<UIMakeMomentCommit>
	{
		[SerializeField]
        private TMP_InputField input_story;
        //[SerializeField]
        //private TMP_Text text_tag;
        [SerializeField]
        private TMP_Text text_place;

        [SerializeField]
        private UIPhotoThumbnail _photo;

        [SerializeField]
        private Image img_multiPhotoIcon;

        [SerializeField]
        private TMP_Text txt_multiPhotoDesc;

        [SerializeField]
		private Image img_locationIcon;

        [SerializeField]
		private GameObject btn_locationRemove;

        [SerializeField]
        private RectTransform tagList;

        [SerializeField]
        private GameObject momentTagPrefab;

        [SerializeField]
        private RectTransform contentRoot;


        private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);
		}

		public void onClickBack()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
			close();
        }

        private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}
			MakeMomentViewModel vm = ViewModel.MakeMoment;

            _bindingManager.makeBinding(vm, nameof(vm.Story), input_story, nameof(input_story.text), null);
			_bindingManager.makeBinding(vm, nameof(vm.TagList), updateTagPreview);
			//_bindingManager.makeBinding(vm, nameof(vm.PhotoList), updatePhoto);
			//_bindingManager.makeBinding(vm, nameof(vm.TripLogImage), updatePhoto);
			_bindingManager.makeBinding(vm, nameof(vm.Place), (obj) => {
				if (vm.Place == null)
				{
					img_locationIcon.color = ColorChart.gray_400;
					text_place.text = StringCollection.get("moment.menu.share.addLocation", 0);
					btn_locationRemove.SetActive(false);
				}
				else
				{
					img_locationIcon.color = ColorChart.gray_700;
					text_place.text = vm.Place.getAddress();
					btn_locationRemove.SetActive(true);
				}
            });
        }

        public override void onTransitionEvent(int type)
        {
            if( type == TransitionEventType.start_open)
            {
				updatePhoto(null);
            }
        }

        private void updatePhoto(object obj)
		{
			MakeMomentViewModel vm = ViewModel.MakeMoment;

			if( vm.PhotoList.Count > 0)
            {
                if (vm.PhotoList[0].photoContext != null)
                {
                    _photo.setImageFromFile(vm.PhotoList[0].photoContext);
                }
                else
                {
                    _photo.setImageFromCDN(ClientMoment.makePhotoURL(GlobalConfig.fileserver_url, vm.PhotoList[0].photoURL));
                }

                img_multiPhotoIcon.gameObject.SetActive(vm.PhotoList.Count > 1);
                txt_multiPhotoDesc.gameObject.SetActive(vm.PhotoList.Count > 1);
            }
			else
			{
				//thumbnail.setEmpty();
			}
		}

		private void updateTagPreview(object obj)
		{
            List<int> momentTagList= ViewModel.MakeMoment.TagList;

            // tagList갯수가 더 적으면 추가해줌
            // 디폴트로 empty tag가 이미 들어가 있음을 주의 하자.
            if ( tagList.childCount - 1 < momentTagList.Count)
			{
				int addCount = momentTagList.Count - (tagList.childCount - 1 );
				for (int i = 0; i < addCount; ++i)
				{
					Instantiate(momentTagPrefab, tagList);
				}
			}

			tagList.GetChild(0).gameObject.SetActive(momentTagList.Count == 0);

            for (int i = 0; i < tagList.childCount - 1; ++i)
            {
                var child = tagList.GetChild(i + 1);
                if (i >= momentTagList.Count)
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);

                    UIMomentTag uiTag = child.GetComponent<UIMomentTag>();
                    uiTag.Initialize(i, momentTagList[i]);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(tagList);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }

        public void onClickAddTag()
        {
			var list = ViewModel.MakeMoment.TagList;
			list.Add(RefMomentLifeLog.ID_Walking);
			ViewModel.MakeMoment.TagList = new List<int>(list);

			int index = ViewModel.MakeMoment.TagList.Count - 1;

			UIMakeMomentTagSelect.spawn(RefMomentLifeLog.ID_Walking, (int tag) =>
            {
                ClientMain.instance.getViewModel().MakeMoment.TagList[index] = tag;
				updateTagPreview(ClientMain.instance.getViewModel().MakeMoment.TagList);
            });
        }

        public void onEndEditStory(string text)
		{
			ViewModel.MakeMoment.Story = text;
		}

        public void onClickLifeLogType()
		{
			////UIBackNavigation.getInstance().setup(this, UIMakeMomentLifeLogType.getInstance());
			////UIBackNavigation.getInstance().open();
			//UIMakeMomentLifeLogType.getInstance().open();

			//ClientMain.instance.getPanelNavigationStack().push(this, UIMakeMomentLifeLogType.getInstance());

		}

		public void onClickShare()
		{
			string message = StringCollection.get("moment.commit.popup.message", 0);

			if( ViewModel.MakeMoment.Mode == MakeMomentViewModel.EditMode.make)
			{
				message = StringCollection.get("moment.commit.popup.message", 0);
			}
			else if( ViewModel.MakeMoment.Mode == MakeMomentViewModel.EditMode.modify)
			{
				// 수정하시겠습니까??
				message = StringCollection.get("moment.commit.popup.message", 0);
			}

			UIPopup.spawnYesNo(message, () => {
				reqMakeMoment();

			});
		}

		public void onClickLocation()
        {
            UIMakeMomentLocation.spawn((PlaceData data) =>
            {
				if ( data != null )
					ViewModel.MakeMoment.Place = data;
            });
        }

        private void reqMakeMoment()
		{
			UIBlockingInput.getInstance().open();

			UnityAction clear_ui = () => {

				UIBlockingInput.getInstance().close();

				if (ClientMain.instance.getViewModel().Trip.Data.status != ClientTripConfig.StatusType.none)
				{
					UIMap.getInstance().open();
					UITripStatus.getInstance().open();
				}
				else
				{
					UIMainTab.getInstance().open();
					UIMainTab.getInstance().changeTab(UIMainTab.Tab.feed);
				}

				close();
				//ClientMain.instance.getPanelNavigationStack().clear();
			};

			clear_ui();

			if( ViewModel.MakeMoment.Mode == MakeMomentViewModel.EditMode.make)
			{
				MakeMomentProcessor step = MakeMomentProcessor.create();
				step.run(result => {
				});
			}
			else if( ViewModel.MakeMoment.Mode == MakeMomentViewModel.EditMode.modify)
			{
				ModifyMomentProcessor step = ModifyMomentProcessor.create();
				step.run(result =>
				{
				});
			}
		}

		public void onClickRemoveLocation()
        {
			ViewModel.MakeMoment.Place = null;
		}

		public void onClickPhoto()
        {
			UIMakeMomentCommitDetailPhotoPopup.getInstance().open();
        }
    }
}
