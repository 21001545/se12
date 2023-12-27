using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	// 확대 축소에 따라서 사진을 합치고 해야되서
	public class UMBTripPhotoManager
	{
		private List<UMBActor_Photo> _actorList;
		private List<UMBActor_Photo> _visibleList;

		private UnityMapBox _mapBox;
		private UMBControl _control;

		public static UMBTripPhotoManager create(UnityMapBox mapBox)
		{
			UMBTripPhotoManager m = new UMBTripPhotoManager();
			m.init(mapBox);
			return m;
		}

		private void init(UnityMapBox mapBox)
		{
			_mapBox = mapBox;
			_control = mapBox.getControl();
			_actorList = new List<UMBActor_Photo>();
			_visibleList = new List<UMBActor_Photo>();
		}

		public void clear(bool justList = false)
		{
			if( justList == false)
			{
				foreach (UMBActor_Photo actor in _actorList)
				{
					_mapBox.removeActor(actor);
				}
			}

			_actorList.Clear();
			_visibleList.Clear();
		}

		public List<UMBActor_Photo> getPhotoList()
        {
			return _actorList;
        }

		public void add(List<ClientTripPhoto> photoList)
		{
			foreach(ClientTripPhoto item in photoList)
			{
				add(item);
			}
		}

		public void add(ClientTripPhoto photo)
		{
			UMBActor_Photo actor = (UMBActor_Photo)_mapBox.spawnActor("photo", photo.getLocation());
			actor.setup(photo);
			_actorList.Add(actor);
		}

		public void remove(ClientTripPhoto photo)
		{
			foreach(UMBActor_Photo actor in _actorList)
			{
				if( actor.getData() == photo)
				{
					_mapBox.removeActor(actor);
					_actorList.Remove( actor);
					break;
				}
			}
		}

		public void onUpdateZoom()
		{
			UMBActor_Photo mergeTarget = null;
			_visibleList.Clear();

			for (int i = _actorList.Count - 1; i >= 0; i--)
			{
				if( i == _actorList.Count - 1)
				{
					mergeTarget = _actorList[i];
					mergeTarget.clearMergedList();

					_visibleList.Add(mergeTarget);
				}
				else
				{
					UMBActor_Photo actor = _actorList[i];
					actor.clearMergedList();

					if ( isTooClose( mergeTarget, actor))
					{
						actor.show(false);
						mergeTarget.addMerged( actor);
					}
					else
					{
						mergeTarget = actor;
						_visibleList.Add(mergeTarget);
					}
				}
			}

			foreach(UMBActor_Photo photo in _visibleList)
			{
				photo.show(true);
			}
		}

		private bool isTooClose(UMBActor_Photo a,UMBActor_Photo b)
		{
			Vector2 posA = _mapBox.getTargetCamera().WorldToScreenPoint(a.transform.position);
			Vector2 posB = _mapBox.getTargetCamera().WorldToScreenPoint(b.transform.position);

			return (posA - posB).magnitude < 60;
		}
	}
}
