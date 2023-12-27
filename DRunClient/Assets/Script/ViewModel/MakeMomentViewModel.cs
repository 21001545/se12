using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class MakeMomentViewModel : AbstractViewModel
	{
		private string _story;
		private List<int> _tagList;
		private ClientTripLog _tripLog;
		private List<PhotoItem> _photoList;
		//private Texture2D _tripLogImage;
		private PlaceData _place;

		private int _mode;
		private int _upload_id;
		private int _agent_id;
		private int _moment_id;
		private int _trip_log_id;

		public int Mode => _mode;

		public string Story
		{
			get
			{
				return _story;
			}
			set
			{
				Set(ref _story, value);
			}
        }

        public PlaceData Place
        {
            get
            {
                return _place;
            }
            set
            {
                Set(ref _place, value);
            }
        }


        public List<int> TagList
		{
			get
			{
				return _tagList;
			}
			set
			{
				Set(ref _tagList, value);
			}
		}

		public ClientTripLog TripLog
		{
			get
			{
				return _tripLog;
			}
			set
			{
				Set(ref _tripLog, value);
			}
		}

		public List<PhotoItem> PhotoList
		{
			get
			{
				return _photoList;
			}
			set
			{
				Set(ref _photoList, value);
			}
		}

		//public Texture2D TripLogImage
		//{
		//	get
		//	{
		//		return _tripLogImage;
		//	}
		//	set
		//	{
		//		if( _tripLogImage != null)
		//		{
		//			UnityEngine.Object.DestroyImmediate(_tripLogImage);
		//			_tripLogImage = null;
		//		}

		//		Set(ref _tripLogImage, value);
		//	}
		//}

		public int UploadID
		{
			get
			{
				return _upload_id;
			}
			set
			{
				Set(ref _upload_id, value);
			}
		}

		public int AgentID
		{
			get
			{
				return _agent_id;
			}
			set
			{
				Set(ref _agent_id, value);
			}
		}

		public int MomentID => _moment_id;

		public static class EditMode
		{
			public const int make = 1;		// 모먼트 생성 모드
			public const int modify = 2;		// 모먼트 편집 모드
		}

		public class PhotoItem
		{
			public NativeGallery.NativePhotoContext photoContext;
			public string photoURL;
			public PlaceData placeData;

			public PhotoItem(NativeGallery.NativePhotoContext photoContext)
			{
				set(photoContext);
			}

			public PhotoItem(string photoURL)
			{
				set(photoURL);
			}

			public void set(NativeGallery.NativePhotoContext photoContext)
			{
				this.photoContext = photoContext;
				this.photoURL = null;
			}

			public void set(string photoURL)
			{
				this.photoContext = null;
				this.photoURL = photoURL;
			}
		}

		public static MakeMomentViewModel create()
		{
			MakeMomentViewModel vm = new MakeMomentViewModel();
			vm.init();
			return vm;
		}

		public static MakeMomentViewModel clone(MakeMomentViewModel src)
		{
			MakeMomentViewModel vm = new MakeMomentViewModel();
			vm.initFrom(src);
			return vm;
		}

		protected override void init()
		{
			base.init();

			reset(EditMode.make);
		}

		public void reset(int editMode,ClientMoment moment = null,PlaceData placeData = null)
		{
			_mode = editMode;

			if( editMode == EditMode.make)
			{
				Story = "";
				Place = placeData;
				TripLog = null;
				PhotoList = new List<PhotoItem>();
				TagList = new List<int>();
//				TripLogImage = null;

				_upload_id = 0;
				_agent_id = 0;
			}
			else if( editMode == EditMode.modify)
			{
				Story = moment.story;
				TagList = moment.tag;
//				TripLogImage = null;
				_upload_id = 0;
				_agent_id = 0;
				_place = moment.getPlaceData();
				_moment_id = moment.id;
				_trip_log_id = moment.trip_log;

				//
				List<PhotoItem> photoList = new List<PhotoItem>();
				for(int i = 0; i < moment.photo_list.Count; ++i)
				{
					PhotoItem item = new PhotoItem(moment.photo_list[i]);
					item.placeData = moment.getPhotoPlaceData(i);
					photoList.Add(item);
				}
				PhotoList = photoList;
			}
		}

		private void initFrom(MakeMomentViewModel src)
		{
			base.init();

			_mode = src._mode;
			_story = src._story;
			_place = src._place;
			_tripLog = src._tripLog;
			_photoList = new List<PhotoItem>(src._photoList);
			_tagList = new List<int>(src._tagList);
			_upload_id = src._upload_id;
			_agent_id = src._agent_id;
			_moment_id = src._moment_id;
			_trip_log_id = src._trip_log_id;
		}

		public void makeReq(MapPacket req)
		{
			req.put("agent_upload_id", _upload_id);
			req.put("agent_id", _agent_id);
			req.put("trip_log", _tripLog != null ? _tripLog.trip_id : 0);
			req.put("story", _story);
			req.put("tag", _tagList);

			req.put("place", makePlaceJson().encode());
		}

		public void modifyReq(MapPacket req)
		{
			req.put("id", _moment_id);
			req.put("trip_log", _trip_log_id);
			req.put("story", _story);
			req.put("tag", _tagList);
			req.put("place", makePlaceJson().encode());

			JsonArray photoArray = new JsonArray();
			foreach(PhotoItem item in _photoList)
			{
				photoArray.add(item.photoURL);
			}

			req.put("photo_list", photoArray);
		}

		private JsonObject makePlaceJson()
		{
			JsonObject json = new JsonObject();
			json.put("place", _place == null ? null : _place.toJson());

			JsonArray photoPlaces = new JsonArray();
			json.put("photoPlaces", photoPlaces);
			foreach(PhotoItem item  in _photoList)
			{
				photoPlaces.add(item.placeData == null ? null : item.placeData.toJson());
			}

			return json;
		}


		public static List<PhotoItem> makePhotoList(List<NativeGallery.NativePhotoContext> photoContextList)
		{
			List<PhotoItem> photoList = new List<PhotoItem>();
			foreach(NativeGallery.NativePhotoContext photoContext in photoContextList)
			{
				photoList.Add(new PhotoItem(photoContext));
			}
			return photoList;
		}
	}
}
