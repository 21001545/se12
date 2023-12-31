using Festa.Client.MapBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class KalmanLatLong
	{
		private float _minAccuracy = 1;
		private float _qMetresPerSecond;
		private long _timeStampMilliseconds;
		private double _lat;
		private double _lng;
		private float _variance; // P matrix.  Negative means object uninitialised.  NB: units irrelevant, as long as same units used throughout

		private double _errorDistance = 200.0 / 1000.0; // 200미터이상 차이나면 에러
		private double _errorTimeLimit = 30.0f; // 30초이상 발생하면 순간이동

		private double _errorAccumulationTime;  // 에러 발생 누적 시간

		public KalmanLatLong(float Q_metres_per_second)
		{
			_qMetresPerSecond = Q_metres_per_second;
			_variance = -1;
		}

		public long TimeStamp { get { return _timeStampMilliseconds; } }
		public double Lat { get { return _lat; } }
		public double Lng { get { return _lng; } }
		public float Accuracy { get { return (float)Math.Sqrt(_variance); } }

		public void SetState(double lat, double lng, float accuracy, long TimeStamp_milliseconds)
		{
			_lat = lat;
			_lng = lng;
			_variance = accuracy * accuracy;
			_timeStampMilliseconds = TimeStamp_milliseconds;
			_errorAccumulationTime = 0;
		}

		/// <summary>
		/// Kalman filter processing for lattitude and longitude
		/// </summary>
		/// <param name="lat_measurement_degrees">new measurement of lattidude</param>
		/// <param name="lng_measurement">new measurement of longitude</param>
		/// <param name="accuracy">measurement of 1 standard deviation error in metres</param>
		/// <param name="TimeStamp_milliseconds">time of measurement</param>
		/// <returns>new state</returns>
		public void Process(double lat_measurement, double lng_measurement, float accuracy, long TimeStamp_milliseconds)
		{
			if (accuracy < _minAccuracy)
			{
				accuracy = _minAccuracy;
			}

			if (_variance < 0)
			{
				// if variance < 0, object is unitialised, so initialise with current values
				_timeStampMilliseconds = TimeStamp_milliseconds;
				_lat = lat_measurement; _lng = lng_measurement; _variance = accuracy * accuracy;
			}
			else
			{
				// else apply Kalman filter methodology

				long TimeInc_milliseconds = TimeStamp_milliseconds - _timeStampMilliseconds;
				if (TimeInc_milliseconds > 0)
				{
					// time has moved on, so the uncertainty in the current position increases
					_variance += TimeInc_milliseconds * _qMetresPerSecond * _qMetresPerSecond / 1000;
					_timeStampMilliseconds = TimeStamp_milliseconds;
					// TO DO: USE VELOCITY INFORMATION HERE TO GET A BETTER ESTIMATE OF CURRENT POSITION
				}

				// Kalman gain matrix K = Covarariance * Inverse(Covariance + MeasurementVariance)
				// NB: because K is dimensionless, it doesn't matter that variance has different units to lat and lng
				float K = _variance / (_variance + accuracy * accuracy);
				// apply K
				_lat += K * (lat_measurement - _lat);
				_lng += K * (lng_measurement - _lng);
				// new Covarariance  matrix is (IdentityMatrix - K) * Covarariance 
				_variance = (1 - K) * _variance;

				// 음영지역 처리 (임시)
				double distance = MapBoxUtil.distance(_lng, _lat, lng_measurement, lat_measurement);
				if( distance >= _errorDistance)
				{
					_errorAccumulationTime += (double)TimeInc_milliseconds / 1000.0;
				}
				else
				{
					_errorAccumulationTime = 0;
				}

				if(_errorAccumulationTime >= _errorTimeLimit)
				{
					_lat = lat_measurement;
					_lng = lng_measurement;
					_variance = accuracy * accuracy;
					_errorAccumulationTime = 0;
				}
			}
		}
	}
}
