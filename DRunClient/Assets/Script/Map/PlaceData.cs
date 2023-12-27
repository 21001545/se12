using Festa.Client.MapBox;
using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class PlaceData
	{
		public class PlaceAPI
		{
			public const int none = 0;
			public const int google = 1;
		}

		private MBLongLatCoordinate _location;	// 장소의 위치
		private string _placeID;				// 장소의 ID
		private int _placeAPI;                  // 장소정보제공 API

		private string _address;                // 장소 이름
		private int _addressLang;				// 장소 이름 언어	(LanguageType)
	
		public MBLongLatCoordinate getLocation()
		{
			return _location;
		}

		public string getPlaceID()
		{
			return _placeID;
		}

		public int getPlaceAPI()
		{
			return _placeAPI;
		}

		public string getAddress()
		{
			return _address;
		}

		public int getAddressLang()
		{
			return _addressLang;
		}

		public static PlaceData create(MBLongLatCoordinate location,string address,int language_type)
		{
			PlaceData place = new PlaceData();
			place.setup(location, address, language_type);
			return place;
		}

		// festa서버에 저장된 정보
		public static PlaceData fromJson(JsonObject data)
		{
			if (data == null)
				return null;

			PlaceData place = new PlaceData();
			place.setupJson(data);
			return place;
		}

		// google place api에서 얻어온 정보 (TextSearch에서 얻어온 정보)
		public static PlaceData fromGooglePlace(JsonObject data,int language_type)
		{
			PlaceData place = new PlaceData();
			place.setupGoogle(data, language_type);
			return place;
		}

		private void setup(MBLongLatCoordinate location,string address,int language_type)
		{
			_location = location;
			_placeID = null;
			_placeAPI = PlaceAPI.none;
			_address = address;
			_addressLang = language_type;
		}

		private void setupJson(JsonObject data)
		{
			_location = MBLongLatCoordinate.fromJson(data.getJsonObject("location"));
			_placeID = data.getString("placeID");
			_placeAPI = data.getInteger("placeAPI");
			_address = data.getString("address");
			_addressLang = data.getInteger("addressLang");
		}

		public JsonObject toJson()
		{
			JsonObject data = new JsonObject();
			data.put("location", _location.toJson());
			data.put("placeID", _placeID);
			data.put("placeAPI", _placeAPI);
			data.put("address", _address);
			data.put("addressLang", _addressLang);
			return data;
		}

		private void setupGoogle(JsonObject data,int language_type)
		{
			JsonObject geometry = data.getJsonObject("geometry");
			JsonObject location = geometry.getJsonObject("location");
			string name = data.getString("name");
			string formatted_address = data.getString("formatted_address");
			string place_id = data.getString("place_id");

			_location = new MBLongLatCoordinate(location.getDouble("lng"), location.getDouble("lat"));
			_placeID = place_id;
			_placeAPI = PlaceAPI.google;
			_addressLang = language_type;
			if (string.IsNullOrEmpty(name) == false)
			{
				_address = $"{name}, {formatted_address}";
			}
			else
			{
				_address = formatted_address;
			}
		}
		
		
	}
}
