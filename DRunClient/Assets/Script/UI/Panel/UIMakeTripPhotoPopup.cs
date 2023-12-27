using System.Collections;
using System.Collections.Generic;
using Festa.Client.Module.UI;
using UnityEngine;
using TMPro;
using Festa.Client.NetData;
using Festa.Client.Module;
using Festa.Client.MapBox;
using Festa.Client.Logic;

namespace Festa.Client
{
    public class UIMakeTripPhotoPopup : UISingletonPanel<UIMakeTripPhotoPopup>
    {
        [SerializeField]
        private UIMessengerPhotoDetailCell _photoCellPrefab;

        [SerializeField]
        private RectTransform rt_photoContent;

        [SerializeField]
        private UIPhotoThumbnail _currentPhoto;

        [SerializeField]
        private GameObject btn_delete;

        [SerializeField]
        private TMP_Text txt_index;

        private UnityMapBox _mapBox;
        private List<ClientTripPhoto> _photoList;
		private List<UIMessengerPhotoDetailCell> _photoCellList;
        private ClientTripPhoto _currentPhotoData;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_photoCellList = new List<UIMessengerPhotoDetailCell>();
		}

		private void removeAllPhotoCells()
		{
			GameObjectCache.getInstance().delete(_photoCellList);
		}

		public void setup(UnityMapBox mapbox, List<ClientTripPhoto> photoList)
        {
            _mapBox = mapbox;
            _photoList = photoList;
            btn_delete.SetActive(_photoList.Count > 0);

			btn_delete.SetActive(_photoList.Count > 0);
			txt_index.gameObject.SetActive(_photoList.Count > 0);
			rt_photoContent.gameObject.SetActive(_photoList.Count > 0);

			removeAllPhotoCells();
            for(int i = 0; i < _photoList.Count; ++i)
			{
                int index = i;
                UIMessengerPhotoDetailCell cell = GameObjectCache.getInstance().make(_photoCellPrefab, rt_photoContent, GameObjectCacheType.ui);
				cell.setup(_photoList[i].getPhotoURL(GlobalConfig.fileserver_url), () => {
					selectPhoto(index);					
				});

                _photoCellList.Add(cell);
			}

            selectPhoto(0);
		}

        private void selectPhoto(int index)
        {
            // ??
            if( index >= _photoList.Count)
            {
                return;
            }

            _currentPhotoData = _photoList[index];
            txt_index.text = $"{index + 1}/{_photoList.Count}";

            _currentPhoto.setImageFromCDN(_currentPhotoData.getPhotoURL(GlobalConfig.fileserver_url));

            for(int i = 0; i < _photoCellList.Count; ++i)
            {
                _photoCellList[i].select(i == index);
            }
        }

        public void onClickDelete()
        {
            if( _currentPhoto == null)
            {
                return;
            }

            UIPopup.spawnYesNo("##사진을 지우시겠습니까?", () => {

                TripRemovePhotoProcessor step = TripRemovePhotoProcessor.create(_currentPhotoData);
                step.run(result => { 
                    if( result.succeeded())
                    {
						_mapBox.getTripPhotoManager().remove(_currentPhotoData);
						_mapBox.getTripPhotoManager().onUpdateZoom();

						_photoList.Remove(_currentPhotoData);

                        if( _photoList.Count == 0)
                        {
                            close();
                        }
                        else
                        {
							setup(_mapBox, _photoList);
						}
					}
				});

				//TripRemovePhotoProcessor step = TripRemovePhotoProcessor.create(photoList[0]);
				//step.run(result => { 
				//	if( result.succeeded())
				//	{
				//		mapbox.getTripPhotoManager().remove(photoList[0]);
				//		mapbox.getTripPhotoManager().onUpdateZoom();
				//	}
				//});

			});
        }
    }
}
