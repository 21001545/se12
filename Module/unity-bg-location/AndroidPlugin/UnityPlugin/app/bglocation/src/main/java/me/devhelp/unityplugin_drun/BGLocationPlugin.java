package me.devhelp.unityplugin_drun;

import android.Manifest;
import android.app.Activity;
import android.app.PendingIntent;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.location.Location;
import android.location.LocationManager;
import android.net.Uri;
import android.os.Build;
import android.util.Log;

import com.google.gson.Gson;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;

public class BGLocationPlugin {
    public final static String LOG_TAG = "BGLocation.UnityPlugin";
    private static final int REQUEST_LOCATION = 1;
    private static final int LOCATION_REQUEST_CODE = 1010;

    private Activity _activity;
    private Intent locationIntent;

    public static BGLocationPlugin create(Activity activity) {
        BGLocationPlugin plugin = new BGLocationPlugin();
        plugin.init(activity);
        return plugin;
    }

    private void init(Activity activity) {
        _activity = activity;
    }

    public void initDevice() {
        Log.i(LOG_TAG, "initDevice");
        locationIntent = new Intent(_activity.getApplicationContext(), LocationService.class);
    }

    public void startLocationService() {
        Log.d(LOG_TAG, "UnityPluginActivity: startLocationService");
        // 2021.12.07 swpark 시작할때 권한을 물어보지 않는다.
        //checkPermissions();
        Log.i(LOG_TAG, "startLocationService");
        PendingIntent pendingIntent = _activity.createPendingResult(REQUEST_LOCATION, new Intent(), PendingIntent.FLAG_UPDATE_CURRENT);
        locationIntent.putExtra(LocationService.PENDING_INTENT, pendingIntent);
        _activity.startService(locationIntent);
    }

    public String getLocationsJson(long time) {
        // Log.d(LOG_TAG, "UnityPluginActivity: getLocationsJson after " + time);
        Cursor cursor = _activity.getContentResolver().query(LocationContentProvider.CONTENT_URI, null,
                LocationContentProvider.LOCATION_TIME + " > " + time,
                null, null);
        List<LocationDto> locationUpdates = new ArrayList<>();
        if (cursor != null) {
            while (cursor.moveToNext()) {
                LocationDto dto = new LocationDto();
                dto.setTime(cursor.getLong(cursor.getColumnIndex(LocationContentProvider.LOCATION_TIME)));
                dto.setLongitude(cursor.getDouble(cursor.getColumnIndex(LocationContentProvider.LOCATION_LONGITUDE)));
                dto.setLatitude(cursor.getDouble(cursor.getColumnIndex(LocationContentProvider.LOCATION_LATITUDE)));
                dto.setAltitude(cursor.getDouble(cursor.getColumnIndex(LocationContentProvider.LOCATION_ALTITUDE)));
                dto.setH_accuracy(cursor.getDouble(cursor.getColumnIndex(LocationContentProvider.LOCATION_HORIZONTAL_ACCURACY)));
                dto.setV_accuracy(cursor.getDouble(cursor.getColumnIndex(LocationContentProvider.LOCATION_VERTICAL_ACCURACY)));

                locationUpdates.add(dto);
            }
            cursor.close();
        }
        String json = new Gson().toJson(locationUpdates);
        //Log.d(LOG_TAG, "Json: " + json);
        return json;
    }

    public void stopLocationService() {
        Log.i(LOG_TAG, "stopLocationervice");
        _activity.stopService(locationIntent);
    }

    public void deleteLocationsBefore(long time) {
        Log.i(LOG_TAG, "UnityPluginActivity: deleteLocationsBefore " + time);
        int count = _activity.getContentResolver().delete(LocationContentProvider.CONTENT_URI,
                LocationContentProvider.LOCATION_TIME + " <= " + time,
                null);
        Log.i(LOG_TAG, "Deleted: " + count + "rows");
    }

    public void checkPermissions() {
        Log.d(LOG_TAG, "UnityPluginActivity: checkPermissions");
        if (!hasPermission()) {
            _activity.requestPermissions(new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION}, LOCATION_REQUEST_CODE);
        }
    }

    public void requestOneTime() {
        LocationManager locationManager = (LocationManager) _activity.getApplicationContext().getSystemService(Context.LOCATION_SERVICE);

        if (_activity.getApplicationContext().checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && _activity.checkSelfPermission(Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            Log.w(LOG_TAG, "checkSelfPermission fail: ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION");
            return;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            locationManager.getCurrentLocation(
                    LocationManager.GPS_PROVIDER,
                    null,
                    _activity.getApplication().getMainExecutor(),
                    new Consumer<Location>() {
                        @Override
                        public void accept(Location location) {
                            // code
                            Log.d(LOG_TAG,"acceptLocation : " + location);
                            saveLocation(location);
                        }
                    });
        }
        else
        {
            Log.d(LOG_TAG,"getLastKnownLocation");
            Location location = locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
            saveLocation(location);
        }
    }

    public boolean hasPermission() {
        Log.d(LOG_TAG, "UnityPluginActivity: hasPermission");
        return PackageManager.PERMISSION_GRANTED == _activity.checkSelfPermission(android.Manifest.permission.ACCESS_FINE_LOCATION);
    }

    private void saveLocation(Location location) {
        if (location == null) return;

        ContentValues values = new ContentValues();
        values.put(LocationContentProvider.LOCATION_TIME, System.currentTimeMillis());
        values.put(LocationContentProvider.LOCATION_LATITUDE, location.getLatitude());
        values.put(LocationContentProvider.LOCATION_LONGITUDE, location.getLongitude());
        values.put(LocationContentProvider.LOCATION_ALTITUDE, location.getAltitude());
        values.put(LocationContentProvider.LOCATION_HORIZONTAL_ACCURACY, location.getAccuracy());
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            values.put(LocationContentProvider.LOCATION_VERTICAL_ACCURACY, location.getVerticalAccuracyMeters());
        }
        else
        {
            values.put(LocationContentProvider.LOCATION_VERTICAL_ACCURACY, location.getAccuracy());
        }
        Uri uri = _activity.getContentResolver().insert(LocationContentProvider.CONTENT_URI, values);
        Log.d(LOG_TAG, "inserted new location, location: " + location);
    }
}

