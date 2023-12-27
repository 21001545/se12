package com.lifefesta.drun.coreservice;

import static com.lifefesta.drun.coreservice.CoreService.LOG_TAG;

import android.Manifest;
import android.content.ContentValues;
import android.content.Context;
import android.content.pm.PackageManager;
import android.location.Location;
import android.location.LocationManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

import androidx.core.content.ContextCompat;

import com.lifefesta.drun.coreservice.data.LocationContentProvider;
import com.lifefesta.drun.coreservice.util.KalmanFilter;

public class LocationTracker {
    private static final int LOCATION_INTERVAL = 1000;
    private static final float LOCATION_DISTANCE = 1.0f;

    private CoreService _owner;
    private LocationManager _locationManager;
    private LocationListener _gpsListener;
    private LocationListener _networkListener;

    private boolean _stared;
    private long _lastGPSRecordTime;

    public static LocationTracker create(CoreService owner)
    {
        LocationTracker tracker = new LocationTracker();
        tracker.init(owner);
        return tracker;
    }

    private void init(CoreService owner)
    {
        _owner = owner;
        _locationManager = (LocationManager)_owner.getApplicationContext().getSystemService(Context.LOCATION_SERVICE);
        _stared = false;
    }

    public void start()
    {
        if( _stared == true)
        {
            Log.e(LOG_TAG, "LocationTracker already started");
            return;
        }

        if( hasPermission() == false)
        {
            Log.e(LOG_TAG, "failed to start location tracker : permission not granted");
            return;
        }

        _gpsListener = new LocationListener();
        _networkListener = new LocationListener();
        _lastGPSRecordTime = 0;

        requestLocationUpdate(LocationManager.GPS_PROVIDER, _gpsListener);
        requestLocationUpdate(LocationManager.NETWORK_PROVIDER, _networkListener);
    }

    private void requestLocationUpdate(String provider, LocationListener listener)
    {
        try
        {
            _locationManager.requestLocationUpdates(provider,LOCATION_INTERVAL, LOCATION_DISTANCE, listener);

            Log.i(LOG_TAG, "requestLocationUpdate: " + provider);
        }
        catch(SecurityException ex)
        {
            Log.w(LOG_TAG,"fail to request location update, ignore", ex);
        }
        catch(IllegalArgumentException ex)
        {
            Log.w(LOG_TAG, "provider doesn't exist",ex);
        }

    }

    public void stop()
    {
        _stared = false;
        try
        {
            if( _gpsListener != null)
            {
                _locationManager.removeUpdates(_gpsListener);
                _gpsListener = null;
            }

            if( _networkListener != null)
            {
                _locationManager.removeUpdates(_networkListener);
                _networkListener = null;
            }
        }
        catch(SecurityException ex)
        {
            Log.w(LOG_TAG, "fail to remove location listeners", ex);
        }
    }

    public boolean hasPermission()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            return ContextCompat.checkSelfPermission(_owner, Manifest.permission.ACCESS_FINE_LOCATION) ==
                    PackageManager.PERMISSION_GRANTED;
        }
        else
        {
            int result = _owner.getPackageManager().checkPermission(Manifest.permission.ACCESS_FINE_LOCATION, _owner.getPackageName());
            return result == PackageManager.PERMISSION_GRANTED;
        }
    }

    private void saveLocation(Location location) {
        if( location == null)
            return;

        if( location.getProvider().equals(LocationManager.NETWORK_PROVIDER))
        {
            long timestamp = location.getTime();

            // 3초 이상 GPS가 없을때만 사용한다
            if( _lastGPSRecordTime != 0 &&  timestamp - _lastGPSRecordTime < 3000)
            {
                Log.d(LOG_TAG, "discard location from NetworkProvider");
                return;
            }

            _lastGPSRecordTime = location.getTime();
        }
        else
        {
            _lastGPSRecordTime = location.getTime();
        }

        ContentValues values = new ContentValues();
        values.put(LocationContentProvider.COLUMN_TIME, location.getTime());
        values.put(LocationContentProvider.COLUMN_LATITUDE, location.getLatitude());
        values.put(LocationContentProvider.COLUMN_LONGITUDE, location.getLongitude());
        values.put(LocationContentProvider.COLUMN_ALTITUDE, location.getAltitude());
        values.put(LocationContentProvider.COLUMN_H_ACCURACY, location.getAccuracy());
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
        {
            values.put(LocationContentProvider.COLUMN_V_ACCURACY, location.getVerticalAccuracyMeters());
        }
        else
        {
            values.put(LocationContentProvider.COLUMN_V_ACCURACY, 0);
        }
        _owner.getContentResolver().insert(LocationContentProvider.CONTENT_URI, values);
        Log.d(LOG_TAG, "insert new location: " + location);
    }

    private class LocationListener implements android.location.LocationListener {
        //private KalmanFilter _filter = null;

        @Override
        public void onLocationChanged(Location location)
        {
            Log.d(LOG_TAG,"LocationListener:onLocationChanged:" + location);

            location.setTime(System.currentTimeMillis());

//            if( _filter == null)
//            {
//                _filter = new KalmanFilter(5.0);
//                _filter.SetState( location.getLatitude(), location.getLongitude(), location.getAccuracy(), location.getTime());
//            }
//            else
//            {
//                _filter.Process( location.getLatitude(), location.getLongitude(), location.getAccuracy(), location.getTime());
//            }
//
//            double longitude = _filter.getLongitude();
//            double latitude = _filter.getLatitude();

//            Log.d(LOG_TAG, String.format("filter: from[%f,%f] -> to[%f,%f]", location.getLongitude(), location.getLatitude(), longitude, latitude));

//            location.setLongitude(longitude);
//            location.setLatitude(latitude);

            saveLocation(location);
        }

        @Override
        public void onProviderDisabled(String provider)
        {
            Log.w(LOG_TAG,"LocationListener: providerDisabled: " + provider);
        }

        @Override
        public void onProviderEnabled(String provider)
        {
            Log.i(LOG_TAG, "LocationListener: providerEnabled: " + provider);
        }

        @Override
        public void onStatusChanged(String provider, int i, Bundle bundle)
        {
            Log.d(LOG_TAG,"LocationListener: statusChanged: " + provider);
        }

        @Override
        public void onFlushComplete(int requestCode)
        {
            Log.d(LOG_TAG, "Locatoin Listener: onFlushComplete: " + requestCode);
        }

    }

}
