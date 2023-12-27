using Festa.Client.Module;
using Festa.Client.MapBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class LocationViewModel : AbstractViewModel
	{
		private MBLongLatCoordinate _currentLocation;
		private double _currentAltitude;
		private string _currentAddress;

		public MBLongLatCoordinate CurrentLocation
		{
			get
			{
				return _currentLocation;
			}
			set
			{
				Set(ref _currentLocation, value);
			}
		}

		public double CurrentAltitude
		{
			get
			{
				return _currentAltitude;
			}
			set
			{
				Set(ref _currentAltitude, value);
			}
		}

		public string CurrentAddress
		{
			get
			{
				return _currentAddress;
			}
			set
			{
				Set(ref _currentAddress, value);
			}
		}

		public static LocationViewModel create()
		{
			LocationViewModel vm = new LocationViewModel();
			vm.init();
			return vm;
		}

		public PlaceData createCurrentPlaceData()
		{
			return PlaceData.create(_currentLocation, _currentAddress, GlobalRefDataContainer.getStringCollection().getCurrentLangType());
		}

		protected override void init()
		{
			base.init();
			_currentLocation = new MBLongLatCoordinate(127.0543616485475, 37.50637354896867);
			_currentAltitude = 30.0f;
		}
	}
}
