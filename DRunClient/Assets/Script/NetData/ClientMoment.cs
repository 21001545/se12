using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientMoment
	{
		public int account_id;
		public int id;
		public List<string> photo_list;
		public int trip_log;
		public string story;
		public List<int> tag;
		public string place;
		public int comment_count;
		public int like_count;
		public DateTime create_time;
		public DateTime update_time;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfile _profile;

		[SerializeOption(SerializeOption.NONE)]
		public string _shortStory;

		[SerializeOption(SerializeOption.NONE)]
		public bool _isLiked;

        [SerializeOption(SerializeOption.NONE)]
        public List<ClientMomentComment> recent_comment;

		[SerializeOption(SerializeOption.NONE)]
		private bool _placeParsed = false;

		[SerializeOption(SerializeOption.NONE)]
		private PlaceData _placeData;

		[SerializeOption(SerializeOption.NONE)]
		private PlaceData[] _photoPlaceDataList;

		public string makePhotoURL(string baseURL, int index)
		{
			return makePhotoURL(baseURL, photo_list[index]);
		}

		public static string makePhotoURL(string baseURL,string photoURL)
		{
			return string.Format("{0}/{1}/{2}", baseURL, "momentphoto", photoURL);
		}
			
		public void makeShortStory(string more_thing)
		{
			string line1 = story.Split(new[] { '\r', '\n' }).FirstOrDefault();

			if (line1.Length > 15)
			{
				_shortStory = line1.Substring(0, 15) + more_thing;
			}
			else
			{
				if (line1.Length < story.Length)
				{
					_shortStory = line1 + more_thing;
				}
				else
				{
					_shortStory = line1;
				}
			}
		}
		public bool isLongStory()
		{
			return _shortStory != story;
		}

		public PlaceData getPlaceData()
		{
			if(_placeParsed == false)
			{
				parsePlaceData();
			}

			return _placeData;
		}

		public PlaceData getPhotoPlaceData(int index)
		{
			if(_placeParsed == false)
			{
				parsePlaceData();
			}

			if( _photoPlaceDataList == null || index >= _photoPlaceDataList.Length)
			{
				return null;
			}

			return _photoPlaceDataList[index];
		}

        // 서버로 부터 데이터를 받기 전에 임시로 셋팅해놓기 위한.... 그냥 서버한테 달라구 할까..
        public void setPlaceData(int index, PlaceData data)
        {
            JsonObject json = new JsonObject(place);

            if (index == 0)
            {
                _placeData = data;
				json.put("place", data.toJson());
			}
			else
            {
                JsonArray array = json.getJsonArray("photoPlaces");
				array.getList()[index] = data.toJson().getMap();
			}
			place = json.encode();
			_placeParsed = false;
		}

        private void parsePlaceData()
		{
			_placeParsed = true;

			try
			{
				JsonObject json = new JsonObject(place);
			
				if( json.contains("place"))
				{
					_placeData = PlaceData.fromJson(json.getJsonObject("place"));
				}
				if( json.contains("photoPlaces"))
				{
					JsonArray array = json.getJsonArray("photoPlaces");

					_photoPlaceDataList = new PlaceData[array.size()];

					for(int i = 0; i < array.size(); ++i)
					{
						if( array.getValue(i) == null)
						{
							_photoPlaceDataList[i] = null;
						}
						else
						{
							_photoPlaceDataList[i] = PlaceData.fromJson(array.getJsonObject(i));
						}
					}
				}
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}

		}

		public string getRepresentativePhotoURL(string baseURL)
		{
			if( photo_list.Count == 0)
			{
				return null;
			}

			return makePhotoURL(baseURL, 0);
		}
	}
}
