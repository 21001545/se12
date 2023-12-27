using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class WeatherViewModel : AbstractViewModel
	{
		private ClientWeather _data;

		public ClientWeather Data
		{
			get
			{
				return _data;
			}
			set
			{
//				Debug.Log(value.payload.encode());

				Set(ref _data, value);
			}
		}

		public static WeatherViewModel create()
		{
			WeatherViewModel vm = new WeatherViewModel();
			vm.init();
			return vm;
		}
	}
}
