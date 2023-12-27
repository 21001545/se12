using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientTripPhoto
	{
		public int trip_id;
		public int slot_id;
		public JsonObject photo_data;
		public DateTime create_time;

		[SerializeOption(SerializeOption.NONE)]
		private bool _dataParsed;

		[SerializeOption(SerializeOption.NONE)]
		private MBLongLatCoordinate _location;

		[SerializeOption(SerializeOption.NONE)]
		private string _photoURL;
		
		private void parseData()
		{
			if( _dataParsed)
			{
				return;
			}

			_dataParsed = true;
			_location = MBLongLatCoordinate.fromJson(photo_data.getJsonObject("location"));
			_photoURL = photo_data.getString("url");
		}

		public MBLongLatCoordinate getLocation()
		{
			parseData();
			return _location;
		}

		public string getPhotoURL(string baseURL)
		{
			return string.Format("{0}/{1}/{2}", baseURL, "momentphoto", _photoURL);
		}
	}
}
