package com.lifefesta.drun.coreservice;

import android.content.Context;
import android.content.SharedPreferences;

public final class CoreServiceTracker {
    public static final String FILE_KEY = "CoreService_Key";
    public static final String KEY_SERVICE_STATE = "CoreService_State";
    public static final String KEY_LOCATION_SATE = "LocationTracking_State";

    public enum ServiceState
    {
        STARTED,
        STOPPED
    }

    public enum LocationState
    {
        STARTED,
        STOPPED
    }

    public static void setServiceState(Context context,ServiceState state)
    {
        SharedPreferences sharedPrefs = context.getSharedPreferences(FILE_KEY, Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPrefs.edit();
        editor.putString(KEY_SERVICE_STATE, state.name());
        editor.apply();
    }

    public static ServiceState getServiceState(Context context)
    {
        SharedPreferences sharedPrefs = context.getSharedPreferences(FILE_KEY, Context.MODE_PRIVATE);
        String value = sharedPrefs.getString(KEY_SERVICE_STATE, ServiceState.STOPPED.name());
        return ServiceState.valueOf(value);
    }

    public static void setLocationState(Context context,LocationState state)
    {
        SharedPreferences sharedPrefs = context.getSharedPreferences(FILE_KEY, Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPrefs.edit();
        editor.putString(KEY_LOCATION_SATE, state.name());
        editor.apply();
    }

    public static LocationState getLocationState(Context context)
    {
        SharedPreferences sharedPrefs = context.getSharedPreferences(FILE_KEY, Context.MODE_PRIVATE);
        String value = sharedPrefs.getString(KEY_LOCATION_SATE, LocationState.STOPPED.name());
        return LocationState.valueOf(value);
    }
}
