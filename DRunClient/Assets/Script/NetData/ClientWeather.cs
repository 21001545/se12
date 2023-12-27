using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientWeather
	{
		public DateTime time;
		public JsonObject payload;

		public string getWeatherIcon()
        {
			string icon = payload.getJsonArray("weather").getJsonObject(0).getString("icon");
			return $"<sprite name=\"{icon}\">";
		}

		public string getTemperature()
        {
			int unit = ClientMain.instance.getViewModel().Profile.Setting_TemperatureUnit;
			float temp_kelvin = payload.getJsonObject("main").getFloat("temp");
			float temp_celsius = temp_kelvin - 273.15f;
			if (unit == UnitDefine.TemperatureType.c)
			{
				return string.Format($"{temp_celsius.ToString("N0")}°C");
			}
			else
			{
				float temp_fahrenheit = (float)UnitDefine.c_to_f(temp_celsius);
				return string.Format($"{temp_fahrenheit.ToString("N0")}°F");
			}
        }

		public string getDisplayName(int unit_type)
		{
			string name = "";

			try
			{
				int code = payload.getJsonArray("weather").getJsonObject(0).getInteger("id");
				string icon = payload.getJsonArray("weather").getJsonObject(0).getString("icon");
				float temp_kelvin = payload.getJsonObject("main").getFloat("temp");
				float temp_celsius = temp_kelvin - 273.15f;

				//string code_name = GlobalRefDataContainer.getStringCollection().get("WeatherCode", code);
				if( unit_type == UnitDefine.TemperatureType.c)
				{
					name = string.Format($"<sprite name=\"{icon}\">{temp_celsius.ToString("F1")}°C");
				}
				else
				{
					float temp_fahrenheit = (float)UnitDefine.c_to_f(temp_celsius);
					name = string.Format($"<sprite name=\"{icon}\">{temp_fahrenheit.ToString("F1")}°F");
				}
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				name = "N/A";
			}

			return name;
		}
	}
}
