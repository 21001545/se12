using EnhancedUI.EnhancedScroller;
using PolyAndCode.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIGalleryPickerItem : EnhancedScrollerCellView
	{
		[SerializeField]
		private UIPhotoThumbnail _thumbnail;

        [SerializeField]
		private GameObject go_base;

        [SerializeField]
		private GameObject go_unselected;

        [SerializeField]
		private GameObject go_selected;

        [SerializeField]
		private TMP_Text txt_selectIndex;

		[SerializeField]
		private GameObject go_selectedCellMark;

        private NativeGallery.NativePhotoContext _photoContext;
		private int _id;
		private UIGalleryPickerScrollDelegate _scrollDelegate;

		private UnityAction<int> _clickCallback;

		public GameObject getGameObject()
        {
			if(go_selected != null)
				return go_selected;

			return go_base;
        }

		public void onClicked()
		{
			if (_scrollDelegate != null)
			{
				int sel_index = _scrollDelegate.toggleSelection(_id, this);
				updateSelection(sel_index);
			}

			_clickCallback?.Invoke(_id);
		}

		public void onUnclicked()
        {
			updateSelection(-1);
        }

		public void resetSelectIndex()
        {
			if (_scrollDelegate != null)
			{
				int sel_index = _scrollDelegate.getSelectionIndex(_id);
				updateSelection(sel_index);
			}
		}

		private void updateSelection(int sel_index)
		{
			if (sel_index == -1)
			{
				if(go_selectedCellMark.activeSelf)
                {
					go_unselected.SetActive(true);
				}
				else
                {
					go_unselected.SetActive(false);
                }

				go_selected.SetActive(false);
            }
			else
            {
                go_unselected.SetActive(false);
                go_selected.SetActive(true);

				if(go_selectedCellMark.activeSelf)
					txt_selectIndex.text = (sel_index+1).ToString();
			}
		}

		public void enable(bool enable)
        {
			go_base.SetActive(enable);
        }

		public void setup(int id, NativeGallery.NativePhotoContext photoContext, bool isSinglePick = false, UIGalleryPickerScrollDelegate scrollDelegate = null)
		{
			_scrollDelegate = scrollDelegate;
			_photoContext = photoContext;
			_id = id;
			go_selected.SetActive(false);
			_thumbnail.setImageFromFile( _photoContext);

			if(isSinglePick)
            {
				go_selectedCellMark.SetActive(false);
            }
			else
            {
				go_selectedCellMark.SetActive(true);
			}

			resetSelectIndex();
        }

		public void setClickCallback(UnityAction<int> callback)
        {
			_clickCallback = callback;
        }

		public void setup(int id, string url )
        {
            _id = id;
            _thumbnail.setImageFromCDN(url);
        }

        public override void RefreshCellView()
        {
			resetSelectIndex();
        }
    }
}
