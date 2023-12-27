using Festa.Client.Module;
using Festa.Client.Module.UI;
using PolyAndCode.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module.Events;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public class UIGalleryPickerScrollDelegate : IEnhancedScrollerDelegate
    {
        private EnhancedScroller _scroller;
        private EnhancedScrollerCellView _rowItemPrefab;
        private List<NativeGallery.NativePhotoContext> _imagePathList;
        private UIGalleryPickerItem _prevPickedItem = null;             // 하나만 골라야 하는 경우에만 사용

        private int _maximumAvaliableCount = 10;
        public void setMaxCount(int count)
        {
            _maximumAvaliableCount = count;
        }
        private int _currentLoadedCount = 0; // 현재 로드한 사진 개수,

        private List<int> _selectionList;
        private Action<int> _selectCallback;
        public int getCurrentLoadedCount() => _currentLoadedCount;

		public List<int> getSelectionList() => _selectionList;

        private bool _isVerticalScroller = true;

        public UIGalleryPickerScrollDelegate(EnhancedScroller scroller, EnhancedScrollerCellView prefab, bool isVertical = true)
        {
            _isVerticalScroller = isVertical;
            _scroller = scroller;
            _rowItemPrefab = prefab;
            _selectionList = new List<int>();

			_scroller.Delegate = this;

			_imagePathList = new List<NativeGallery.NativePhotoContext>();
		}

        public void setSelectCallback(Action<int> callback)
        {
			_selectCallback = callback;
        }

        public void clear()
        {
			_selectionList.Clear();
			_currentLoadedCount = 0;
            _imagePathList.Clear();
        }

		public void setMaximumAvaliableCount( int count )
        {
			_maximumAvaliableCount = count;
		}

		public NativeGallery.NativePhotoContext getPhotoContextWithID( int id )
        {
			if (_imagePathList.Count <= id)
				return null;

			return _imagePathList[id];
        }

		public void loadImagePaths(int requestCount, bool clear = false)
        {
            List<NativeGallery.NativePhotoContext> photoContext = NativeGallery.GetRecentImagePaths(_currentLoadedCount, requestCount);
            if (photoContext == null)
            {
#if UNITY_EDITOR
                if (clear)
                {
                    _imagePathList.Clear();
                    // 2022.05.20 이강희 아직 초기화 전이면 skip
                    if ( _scroller.Container != null)
					{
                        _scroller.ScrollPosition = 0;
                    }
                }

                photoContext = new List<NativeGallery.NativePhotoContext>();

                for (int i = 0; i < requestCount / 4; ++i)
                {
                    photoContext.Add(new NativeGallery.NativePhotoContext(Application.dataPath + "/Source/UI/image/party.png"));
                    photoContext.Add(new NativeGallery.NativePhotoContext(Application.dataPath + "/Source/UI/image/photo_01.png"));
                    photoContext.Add(new NativeGallery.NativePhotoContext(Application.dataPath + "/Source/UI/image/photo_02.png"));
                    photoContext.Add(new NativeGallery.NativePhotoContext(Application.dataPath + "/Source/UI/image/photo_03.png"));
                }
#else
                return;
#endif
            }

            _currentLoadedCount += photoContext.Count;

            if (clear)
            {
                _imagePathList.Clear();
                // 2022.05.20 이강희 아직 초기화 전이면 skip
                if( _scroller.Container != null)
				{
                    _scroller.ScrollPosition = 0;
                }
            }

            _imagePathList.AddRange(photoContext);

            // 2022.05.20 이강희 아직 초기화 전이면 skip
            if (_scroller.Container != null)
            {
                float prevScrollPosition = _scroller.ScrollPosition;
                _scroller.ReloadData();
                _scroller.SetScrollPositionImmediately(prevScrollPosition);
            }
        }

        public int toggleSelection(int id, UIGalleryPickerItem item)
        {
            if (_selectionList.Contains(id) == false)
            {
                if(_maximumAvaliableCount == 1)
                {
                    // 하나만 선택할 수 있는 경우
                    // 2022.08.19 기준 프로필 사진 선택에만 적용
                    _selectionList.Clear();

                    if(_prevPickedItem != null)
                    {
                        _prevPickedItem.onUnclicked();
                    }
                    _prevPickedItem = item;
                }

                // 최대 개수 제한..
                else if (_selectionList.Count >= _maximumAvaliableCount)
                {
                    UIPopup.spawnOK(GlobalRefDataContainer.getStringCollection().getFormat("gallery.max", 0, _maximumAvaliableCount));
                    return -1;
                }

                _selectionList.Add(id);

                _selectCallback?.Invoke(_selectionList.Count);
                return _selectionList.Count - 1;
            }
            else
            {
                _selectionList.Remove(id);
                _scroller.RefreshActiveCellViews();
                _selectCallback?.Invoke(_selectionList.Count);
                return -1;
            }
        }

        public int getSelectionIndex(int id)
        {
            return _selectionList.IndexOf(id);
        }

        public void getResult(Handler<AsyncResult<List<NativeGallery.NativePhotoContext>>> handler)
        {
            List<NativeGallery.NativePhotoContext> selectResult = new List<NativeGallery.NativePhotoContext>();
            foreach (int id in _selectionList)
            {
                selectResult.Add(getPhotoContextWithID(id));
            }

            justifyPhotoList(selectResult, result =>
            {
                handler(result);
            });
        }

        private void justifyPhotoList(List<NativeGallery.NativePhotoContext> photo_list, Handler<AsyncResult<List<NativeGallery.NativePhotoContext>>> handler)
        {
            List<NativeGallery.NativePhotoContext> result_list = new List<NativeGallery.NativePhotoContext>();
            justifyPhotoIter(photo_list.GetEnumerator(), result_list, result =>
            {
                if (result.failed())
                {
                    handler(Future.failedFuture<List<NativeGallery.NativePhotoContext>>(result.cause()));
                }
                else
                {
                    handler(Future.succeededFuture(result_list));
                }

            });
        }

        private void justifyPhotoIter(List<NativeGallery.NativePhotoContext>.Enumerator e, List<NativeGallery.NativePhotoContext> result_list, Handler<AsyncResult<Module.Void>> handler)
        {
            if (e.MoveNext() == false)
            {
                handler(Future.succeededFuture());
                return;
            }

            NativeGallery.NativePhotoContext context = e.Current;
   //         NativeGallery.ImageProperties imageProperties;

   //         // 2022.05.27 crash나는 친구가 있다
   //         try
			//{
   //             imageProperties = NativeGallery.GetImageProperties(context.path);
   //         }
   //         catch(Exception ex)
			//{
   //             justifyPhotoIter(e, result_list, handler);
   //             return;
			//}

   //         int newWidth = imageProperties.width;
   //         int newHeight = imageProperties.height;

   //         if (newWidth <= 1440 && newHeight <= 1440)
   //         {
   //             result_list.Add(context);
   //             justifyPhotoIter(e, result_list, handler);
   //             return;
   //         }

   //         // 220617 소현: 긴 쪽을 1440 으로 맞춰서 축소
   //         // 가로
   //         if (imageProperties.width <= imageProperties.height)
   //         {
   //             newHeight = 1440;
   //             newWidth = 1440 * imageProperties.width / imageProperties.height;
   //         }
   //         // 세로
   //         else
   //         {
   //             newWidth = 1440;
   //             newHeight = 1440 * imageProperties.height / imageProperties.width;
   //         }

            // 2022.07.04 이강희 iCloud이슈를 해결하기 위해 resize를 native 코드 안으로 옮김
            // width, height를 -1로 주면 1440으로 resize해줌

            NativeGallery.GetImageThumbnailPathFromPhotoContextAsync(context, -1, -1, (result_path, unique_id) =>
            {
                MainThreadDispatcher.dispatch(() =>
                {
                    //Debug.Log($"justify photo [{context.path}][{context.identifier}][{imageProperties.width}x{imageProperties.height}] -> [{result_path}][{unique_id}][{newWidth}x{newHeight}]");

                    result_list.Add(new NativeGallery.NativePhotoContext(result_path, unique_id, true));
                    justifyPhotoIter(e, result_list, handler);
                });
            });
        }

        #region scroller_delgate
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if ( _isVerticalScroller)
            {
                UIGalleryPickerRowItem rowItem = scroller.GetCellView(_rowItemPrefab) as UIGalleryPickerRowItem;
                rowItem.setup(dataIndex, _imagePathList, _maximumAvaliableCount == 1 ? true : false, this);

                return rowItem;
            }
            else
            {
                UIGalleryPickerItem item = scroller.GetCellView(_rowItemPrefab) as UIGalleryPickerItem;
                item.setup(dataIndex, _imagePathList[dataIndex], _maximumAvaliableCount == 1 ? true : false, this);
                return item;
            }
            //return null;
        }
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (_isVerticalScroller)
            {
                // 요거 부모 사이즈에 따라 비율이 달라질 수 있다
                // xd 상 123 * 166 비율임.
                // 좌우 패딩 1씩 해서 2, 사이 간격 2씩 해서 4
                float ratio = 166.0f / 123.0f;
                float width = (scroller.Container.rect.width - 6.0f) / 3.0f;
                return width * ratio;
            }
            else
            {
                float ratio = 123.0f/ 166.0f;
                float height = (scroller.Container.rect.height - 4.0f);
                return height * ratio;
            }

            //return 166.0f;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (_isVerticalScroller)
                return Mathf.CeilToInt((float)_imagePathList.Count / 3.0f);

            return _imagePathList.Count;
        }

        #endregion
    }

    public class UIGalleryPicker : UISingletonPanel<UIGalleryPicker>
	{
		[SerializeField]
		private EnhancedScroller _scroller;

        [SerializeField]
		private UIGalleryPickerRowItem _rowItemPrefab;

        [SerializeField]
        private Button btn_select;

		private UIGalleryPickerScrollDelegate _scroll;

		private Action<List<NativeGallery.NativePhotoContext>> _finishCallback;

		// 토글된 아이템의 개수를 반환
		private Action<int> _toggleCallback;

        // 최대 가능 개수
        [SerializeField]
        private int MaximumAvaliableCount = 10;

        [SerializeField]
        private Image img_moreIndicator;
		
		// 매번 생성하기 그러니깐, 만들어두자.
        private Color _moreIndicatorColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        private int _requestLoadCount = 100;

        private bool _moreRequest = false;

        private void Start()
        {
			_scroller.ScrollRect.onValueChanged.AddListener(onScrollScrolled);
        }

        public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			_scroll = new UIGalleryPickerScrollDelegate(_scroller, _rowItemPrefab);
			_scroll.setMaximumAvaliableCount(MaximumAvaliableCount);
            _scroll.setSelectCallback((int count) =>
            {
                _toggleCallback?.Invoke(count);
                btn_select.interactable = count > 0;
            });
        }

        public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
            {
                var permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read);

                Debug.Log($"current permission status : {permission}");

                if (permission == NativeGallery.Permission.Granted)
                {
                    _scroll.loadImagePaths(_requestLoadCount, true);
                }
                else
                {
                    close();

                    NativeGallery.Permission permissionType = NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);

                    Debug.Log($"changed permission status : {permissionType}");

                    // 더블체킹
                    if (permissionType != NativeGallery.Permission.Granted)
                    {
                        var sc = GlobalRefDataContainer.getInstance().getStringCollection();
                        UIPopup.spawnYesNo(sc.get("gallery.noaccess.poprup.title", 0), sc.get("gallery.noaccess.poprup.desc", 0), () =>
                        {
                            if (NativeGallery.CanOpenSettings())
                            {
                                NativeGallery.OpenSettings();
                            }
                            else
                            {
                                // 설정 앱을 열 수 없을 때, 유저에게 직접 설정앱을 열어달라고 요청
                                UIPopup.spawnOK(sc.get("gallery.noaccesssetting.poprup.title", 0), sc.get("gallery.noaccesssetting.poprup.desc", 0), () =>
                                {

                                });
                            }
                        }, () => { });
                    }
                }
            }
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);
			
			_scroll.clear();

            btn_select.interactable = false;
            // 디폴트
            setMaxCount(10);
		}

        public void setMaxCount(int count)
        {
            _scroll.setMaxCount(count);
            MaximumAvaliableCount = count;
        }

		public void setFinishCallback(Action<List<NativeGallery.NativePhotoContext>> callback)
		{
			_finishCallback = callback;
		}

		public void setToggleCallback(Action<int> callback)
        {
			_toggleCallback = callback;
        }

        private void onScrollScrolled(Vector2 position)
        {
            if (position.y < 0.0f )
            {
                if (img_moreIndicator.gameObject.activeSelf == false )
                    img_moreIndicator.gameObject.SetActive(true);

				float delta = -position.y * _scroller.ScrollSize;
                float alpha = delta / img_moreIndicator.rectTransform.rect.height;

				_moreIndicatorColor.a = alpha;
				img_moreIndicator.color = _moreIndicatorColor;

				// indicator가 나와야 할 것 같은데? 인디케이터 크기가 40 쯤 되려나?
				if (delta >= img_moreIndicator.rectTransform.rect.height)
                {
					// 좀 더 사진을 땡겨와보자
					// drag를 끝냈으면 하는데,
					_moreRequest = true;
                }
				else
                {
					_moreRequest = false;
				}
            }
			else
            {
				if ( img_moreIndicator.gameObject.activeSelf )
					img_moreIndicator.gameObject.SetActive(false);
			}
        }

		public void onScrollEndDrag(BaseEventData e)
        {
			if ( _moreRequest)
            {
				_scroll.loadImagePaths(_requestLoadCount, false);
				_moreRequest = false;
			}
        }

        public void onClickSelect()
		{
			if ( _finishCallback != null)
			{
				btn_select.interactable = false;

                _scroll.getResult(result =>
                {
                    btn_select.interactable = true;
                    _finishCallback(result.result());
                });
			}

			close();
		}
	}
}
