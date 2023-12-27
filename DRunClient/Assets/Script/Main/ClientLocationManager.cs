using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using Festa.Client.MapBox;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Text;
using Festa.Client.RefData;
using Festa.Client.Module.UI;

/*

https://stackoverflow.com/questions/1134579/smooth-gps-data
 
 */

namespace Festa.Client
{
	public class ClientLocationManager
	{
		private AbstractLocationDevice _device;
		private IntervalTimer _queryTimer;
		private IntervalTimer _flushTimer;
		private IntervalTimer _queryAddressTimer;
		private IntervalTimer _queryWeatherTimer;
		private IntervalTimer _normalModeRequestTimer;

		private ClientNetwork _clientNetwork;
		private ClientViewModel _viewModel;
		private bool _flushing;

		private List<ClientLocationLog> _pendedLogList;
		private List<ClientLocationLog> _tempList;
		private MBLongLatCoordinate _lastQueryAddressLocation;
		private JsonObject _lastAddressJson;
		private ClientLocationLog _lastLocationLog;

		private UIBindingManager _bindingManager;
		private float _flushTimerInterval;
		private int _lastTripStatus;
		private int _mode;

		private ClientHealthManager HealthManager => ClientMain.instance.getHealth();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public class Mode
		{
			public const int none = 0;
			public const int normal = 1;
			public const int trip = 2;
		}

		public AbstractLocationDevice getDevice()
		{
			return _device;
		}

		public static ClientLocationManager create()
		{
			ClientLocationManager manager = new ClientLocationManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_flushTimerInterval = 60.0f;
			_lastTripStatus = 0;

			_queryTimer = IntervalTimer.create(0.5f, false, true);
			_flushTimer = IntervalTimer.create(_flushTimerInterval, false, true);
			_queryAddressTimer = IntervalTimer.create(3.0f, false, true);
			_queryWeatherTimer = IntervalTimer.create(3.0f, false, true);
			_normalModeRequestTimer = IntervalTimer.create(10.0f, false, true);
			_pendedLogList = new List<ClientLocationLog>();
			_tempList = new List<ClientLocationLog>();
			_clientNetwork = ClientMain.instance.getNetwork();
			_viewModel = ClientMain.instance.getViewModel();
			_flushing = false;
			_lastQueryAddressLocation = MBLongLatCoordinate.zero;
			_lastLocationLog = null;
			_bindingManager = UIBindingManager.create();
			_mode = Mode.none;

			createDevice();
			bindVM();
		}

		private void createDevice()
		{
#if UNITY_EDITOR
			_device = LocationDevice_Editor.create();
#else
			_device = LocationDevice_Mobile.create(ClientMain.instance.getMultiThreadWorker());
#endif
		}

		private void bindVM()
		{
			TripViewModel vm = _viewModel.Trip;
			_bindingManager.makeBinding(vm, nameof(vm.Data), onUpdateTripConfig);
		}

		public void changeMode(int mode)
		{
			if( mode == Mode.normal)
			{
				Debug.Log("change location mode : normal");

				_device.resetFilter();
				_device.stopService();
				_device.oneTimeRequest();

				_normalModeRequestTimer.setNext();
				_mode = mode;
			}
			else if( mode == Mode.trip)
			{
				Debug.Log("change location mode : trip");

				_device.resetFilter();
				_device.startService();
				_mode = mode;
			}
		}

		public void update()
		{
			if( _queryTimer.update())
			{
				updateLocation(_queryTimer);
			}

			if( _mode == Mode.normal && _normalModeRequestTimer.update())
			{
				_device.oneTimeRequest();
				_normalModeRequestTimer.setNext();
			}

			if (_queryAddressTimer.update())
			{
				updateAddress();
			}
			
			if(_queryWeatherTimer.update())
			{
				updateWeather();
			}

			flushPendedLog();
		}

		private void updateLocation(IntervalTimer timer)
		{
			_tempList.Clear();

			_device.queryLocationLog(_tempList, () => {
				timer.setNext();

				//Debug.Log($"updateLocation: lon[{_device.getLastLocation().lon}] lat[{_device.getLastLocation().lat}] alt[{_device.getLastAltitude()}]");

				ClientMain.instance.getViewModel().Location.CurrentLocation = _device.getLastLocation();
				ClientMain.instance.getViewModel().Location.CurrentAltitude = _device.getLastAltitude();

				_pendedLogList.AddRange(_tempList);

				//
				TripViewModel trip_vm = ClientMain.instance.getViewModel().Trip;
				if( trip_vm.Data.status == ClientTripConfig.StatusType.trip ||
					trip_vm.Data.status == ClientTripConfig.StatusType.paused)
				{
					trip_vm.appendLocationLogToCurrentPath(_tempList);

					recordCalories_n_Distance(_tempList);
				}
				else
				{
					_lastLocationLog = null;
				}

				// MapReveal
				ClientMain.instance.getMapReveal().reveal(_tempList);

			});
		}

		private void updateAddress()
		{
			MBLongLatCoordinate location = _device.getLastLocation();

			// 10미터 이상 거리가 변하면 주소를 업데이트 해준다
			if ( _lastQueryAddressLocation.isZero() == false && _lastQueryAddressLocation.distanceFrom( location) < 0.01f)
			{
				_queryAddressTimer.setNext();
				return;
			}

			queryAddress( location, address_json => {
				_queryAddressTimer.setNext(3.0f);

				_lastAddressJson = address_json;
				_lastQueryAddressLocation = location;

				string address = addressFromGeoJson(address_json);
				ClientMain.instance.getViewModel().Location.CurrentAddress = address;
			});
		}

		// 언어가 바뀌어서 강제로 해야될때
		public void forceUpdateAdress()
		{
			MBLongLatCoordinate location = _device.getLastLocation();

			queryAddress(location, address_json => {
				_queryAddressTimer.setNext();

				_lastAddressJson = address_json;
				_lastQueryAddressLocation = location;

				string address = addressFromGeoJson(address_json);
				ClientMain.instance.getViewModel().Location.CurrentAddress = address;
			});
		}

		public void updateWeather()
		{
			MBLongLatCoordinate location = _device.getLastLocation();

			// 위치데이터를 아직 얻어 오는 중이다
			if( location.isZero())
			{
				_queryWeatherTimer.setNext(3.0f);
				return;
			}

			MapPacket req = _clientNetwork.createReq(CSMessageID.Weather.QueryWeatherReq);
			req.put("longitude", location.lon);
			req.put("latitude", location.lat);

			_clientNetwork.call(req, ack => {

				// 10분있다가
				_queryWeatherTimer.setNext(10.0f * 60.0f);

				if( ack.getResult() == ResultCode.ok)
				{
					_viewModel.updateFromPacket(ack);
				}
			});
		}

		private void flushPendedLog()
		{
			if( _flushTimer.update())
			{
				flushNow(false, () => {
					_flushTimer.setNext( _flushTimerInterval);
				});
			}
		}

		//public void flushNow()
		//{
		//	if (_pendedLogList.Count == 0)
		//	{
		//		_flushTimer.setNext(_flushTimerInterval);
		//		return;
		//	}

		//	if (_flushing)
		//	{
		//		return;
		//	}

		//	_flushing = true;
		//	List<ClientLocationLog> location_log_data = _pendedLogList;
		//	_pendedLogList = new List<ClientLocationLog>();

		//	MapPacket req = _clientNetwork.createReq(CSMessageID.Location.RecordLocationLogReq);
		//	req.put("location_log_data", location_log_data);

		//	_clientNetwork.call(req, ack => {
		//		_flushTimer.setNext(_flushTimerInterval);
		//		_flushing = false;

		//		if (ack.getResult() == ResultCode.ok)
		//		{
		//			ClientMain.instance.getViewModel().updateFromPacket(ack);

		//			// 테스트 로그 제거
		//			//Debug.Log($"flush location_log:[{location_log_data.Count}]");
		//		}
		//		else
		//		{
		//			mergeLocationData(_pendedLogList, location_log_data);
		//		}
		//	});
		//}

		public void updateLocationNow()
		{
			_queryTimer.stop();
			updateLocation(_queryTimer);
		}

		public void flushNow(bool waitFlushing,UnityAction complete)
		{
			ClientMain.instance.StartCoroutine(_flushNow(waitFlushing,complete));
		}

		private IEnumerator _flushNow(bool waitFlushing,UnityAction completeCallback)
		{
			if( _pendedLogList.Count == 0)
			{
				completeCallback();
				yield break;
			}

			if( _flushing)
			{
				if (waitFlushing == false)
				{
					completeCallback();
					yield break;
				}
				else
				{
					while(_flushing)
					{
						yield return new WaitForSeconds(0.05f);
					}
				}
			}

			_flushing = true;
			List<ClientLocationLog> location_log_data = _pendedLogList;
			_pendedLogList = new List<ClientLocationLog>();

			MapPacket req = _clientNetwork.createReq(CSMessageID.Location.RecordLocationLogReq);
			req.put("location_log_data", location_log_data);

			_clientNetwork.call(req, ack =>	{
				_flushing = false;

				if (ack.getResult() == ResultCode.ok)
				{
					ClientMain.instance.getViewModel().updateFromPacket(ack);

					// 테스트 로그 제거
					//Debug.Log($"flush location_log:[{location_log_data.Count}]");
				}
				else
				{
					mergeLocationData(_pendedLogList, location_log_data);
				}

				completeCallback();
			});
		}

		private void mergeLocationData(List<ClientLocationLog> target,List<ClientLocationLog> data)
		{
			target.AddRange(data);
			target.Sort((a, b) => { 
				if( a.event_time < b.event_time)
				{
					return -1;
				}
				else if( a.event_time > b.event_time)
				{
					return 1;
				}
				return 0;
			});
		}

		public void queryAddress(MBLongLatCoordinate location,UnityAction<JsonObject> callback)
		{
			ClientMain.instance.StartCoroutine(_queryAddress(location, callback));
		}

		private IEnumerator _queryAddress(MBLongLatCoordinate location,UnityAction<JsonObject> callback)
		{
			string uri = string.Format("https://nominatim.openstreetmap.org/reverse.php?format=geojson&lat={0}&lon={1}&zoom=18", location.lat, location.lon);

			//Debug.Log(uri);
			UnityWebRequest request = UnityWebRequest.Get(uri);

			// 언어별 설정
			//GlobalRefDataContainer.getStringCollection
			request.SetRequestHeader("Accept-Language",LanguageType.getAcceptLanguageValue(GlobalRefDataContainer.getStringCollection().getCurrentLangType()));

			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(request.error);
				callback(null);
			}
			else
			{
				try
				{
					//Debug.Log(request.downloadHandler.text);

					JsonObject json = new JsonObject(request.downloadHandler.text);
					callback(json);
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					callback(null);
				}
			}

			request.Dispose();
		}


		//municipality, city, town, village
		//city_district, district, borough, suburb, subdivision

		private string[] addressKeyList =
		{
			"subdivision",
			"suburb",
			"borough",
			"district",
			"city-district",
			"village",
			"town",
			"city",
			"municipality"
		};

		private string addressFromGeoJson(JsonObject json)
		{
			try
			{
				if( json == null)
				{
					return "";
				}

				JsonArray feature_array = json.getJsonArray("features");
				JsonObject feature = feature_array.getJsonObject(0);
				JsonObject properties = feature.getJsonObject("properties");
				JsonObject address = properties.getJsonObject("address");

				List<string> elements = new List<string>();
				foreach(string key in addressKeyList)
				{
					if( address.contains( key))
					{
						elements.Add(address.getString(key));
					}
				}

				StringBuilder sb = new StringBuilder();
				for(int i = 0; i < 2 && i < elements.Count; ++i)
				{
					if( i > 0)
					{
						sb.Append(", ");
					}

					sb.Append(elements[i]);
				}

				return sb.ToString();
				
				//string suburb = address.getString("suburb");
				//string district = address.getString("district");

				//string borough = address.getString("borough");
				//string city = address.getString("city");

				//if( string.IsNullOrEmpty(borough))
				//{
				//	return city;
				//}
				//else
				//{
				//	return string.Format("{0}, {1}", borough, city);
				//}
			}
			catch (System.Exception)
			{
				return "";
			}

		}

		private void recordCalories_n_Distance(List<ClientLocationLog> log_list)
		{
			double weight = ViewModel.Health.Body.getWeightWithKG();
			if (weight == 0)
			{
				weight = 60.0f;
			}

			// 이동거리, 칼로리도 데이터가 매우 많을 수 있다
			foreach (ClientLocationLog log in log_list)
			{
				if( _lastLocationLog == null)
				{
					_lastLocationLog = log;
					continue;
				}

				// 이동거리
				double distance = Haversine.calc(_lastLocationLog.latitude, _lastLocationLog.longitude, log.latitude, log.longitude);

				// 이동시간
				double time_duration = (log.event_time - _lastLocationLog.event_time).TotalHours;

				// 이동속도
				double speed = distance / time_duration;

				if( distance > 0)
				{
					HealthManager.appendLogDouble(HealthDataType.distance, log.event_time, distance * 1000.0);
				}

				if( time_duration > 0 && speed > 0)
				{
					//
					double duration_minutes = (log.event_time - _lastLocationLog.event_time).TotalMinutes;
					double calories = ViewModel.Health.calcCalories(ViewModel.Trip.Data.trip_type, speed, weight, duration_minutes);

					HealthManager.appendLogDouble(HealthDataType.calories, log.event_time, calories * 1000.0);					
				}

				_lastLocationLog = log;
			}
		}

		private void onUpdateTripConfig(object obj)
		{
			ClientTripConfig trip_config = _viewModel.Trip.Data;
			if (trip_config.status == _lastTripStatus)
			{
				return;
			}

			_lastTripStatus = trip_config.status;

			flushNow(true, () => {

				if (trip_config.status == ClientTripConfig.StatusType.trip ||
					trip_config.status == ClientTripConfig.StatusType.paused)
				{
					_flushTimerInterval = 0.1f;
				}
				else if (trip_config.status == ClientTripConfig.StatusType.none)
				{
					_flushTimerInterval = 60.0f;
				}

				_flushTimer.setNext(_flushTimerInterval);

				Debug.Log($"onUpdateTripConfig: {_viewModel.Trip.Data.status}, interval[{_flushTimerInterval}]");

			});
		}
	}
}

