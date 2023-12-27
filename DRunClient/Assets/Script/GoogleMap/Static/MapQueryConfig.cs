using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Festa.GoogleMap.Static
{
	public class MapQueryConfig
	{
		public string APIKey { get; set; } = "AIzaSyC_S2FvrIIFhIT1-JE1C0-nhrAfYtr2VH8";
		public string BaseURL { get; set; } = "https://maps.googleapis.com/maps/api/staticmap";
		public double Longitude { get; set; } = 37.50634312319362;
		public double Latitude { get; set; } = 127.05441805010621;
		public int SizeX { get; set; } = 300;
		public int SizeY { get; set; } = 300;
		public string Format { get; set; } = "jpg";
		public int Zoom { get; set; } = 17;
		public string MapType { get; set; } = "roadmap";

		public string makeURL()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(BaseURL);
			sb.AppendFormat("?center={0},{1}", Longitude, Latitude);
			sb.AppendFormat("&size={0}x{1}", SizeX, SizeY);
			sb.AppendFormat("&maptype={0}", MapType);
			sb.AppendFormat("&format={0}", Format);
			sb.AppendFormat("&zoom={0}", Zoom);
			sb.AppendFormat("&key={0}", APIKey);

			return sb.ToString();
		}
	}

}

