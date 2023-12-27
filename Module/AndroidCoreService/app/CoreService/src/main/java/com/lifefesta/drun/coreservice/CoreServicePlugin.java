package com.lifefesta.drun.coreservice;

import static com.lifefesta.drun.coreservice.CoreService.LOG_TAG;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Intent;
import android.database.Cursor;
import android.os.Build;
import android.util.Log;

import com.google.gson.Gson;
import com.lifefesta.drun.coreservice.data.LocationContentProvider;
import com.lifefesta.drun.coreservice.data.LocationDTO;
import com.lifefesta.drun.coreservice.data.StepCountContentProvider;
import com.lifefesta.drun.coreservice.data.StepCountDTO;

import java.util.ArrayList;
import java.util.List;

public class CoreServicePlugin {

    private Activity _activity;

    public static CoreServicePlugin create(Activity activity)
    {
        CoreServicePlugin plugin = new CoreServicePlugin();
        plugin.init(activity);
        return plugin;
    }

    private void init(Activity activity)
    {
        _activity = activity;
    }

    public void startService(String notificationTitle,String notificationText) {
        Log.d(LOG_TAG, "Plugin:startService");

        PendingIntent pendingIntent = _activity.createPendingResult( 1, new Intent(), PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT);
        Intent intent = new Intent(_activity.getApplicationContext(), CoreService.class);
        intent.setAction(CoreService.Actions.StartService.name());

        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
        {
            _activity.startForegroundService(intent);
        }
        else
        {
            _activity.startService(intent);
        }
    }

    public void stopService() {
        Log.d(LOG_TAG, "Plugin:stopService");

        if( CoreServiceTracker.getServiceState(_activity.getApplicationContext()) == CoreServiceTracker.ServiceState.STOPPED)
        {
            return;
        }

        Intent intent = new Intent(_activity.getApplicationContext(), CoreService.class);
        intent.setAction(CoreService.Actions.StopService.name());
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            _activity.startForegroundService(intent);
        }
        else
        {
            _activity.startService(intent);
        }
    }

    public void startLocation() {
        Log.d(LOG_TAG, "Plugin:startService");

        if( CoreServiceTracker.getLocationState(_activity.getApplicationContext()) == CoreServiceTracker.LocationState.STARTED)
        {
            Log.e(LOG_TAG, "Plugin:location already started");
            return;
        }

        Intent intent = new Intent(_activity.getApplicationContext(), CoreService.class);
        intent.setAction(CoreService.Actions.StartLocation.name());
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            _activity.startForegroundService(intent);
        }
        else
        {
            _activity.startService(intent);
        }
    }

    public void stopLocation() {
        Log.d(LOG_TAG, "Plugin:stopLocation");

        if( CoreServiceTracker.getLocationState(_activity.getApplicationContext()) == CoreServiceTracker.LocationState.STOPPED)
        {
            Log.e(LOG_TAG, "Plugin:location already stopped");
            return;
        }

        Intent intent = new Intent(_activity.getApplicationContext(), CoreService.class);
        intent.setAction(CoreService.Actions.StopLocation.name());
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            _activity.startForegroundService(intent);
        }
        else
        {
            _activity.startService(intent);
        }
    }

    public String getStepCountRange(long begin,long end)
    {
        return StepCountDTO.queryRangeJson( _activity, begin, end);
    }

    public String getStepCountFrom(long begin)
    {
        return StepCountDTO.queryFromJson( _activity, begin);
    }

    public String getLocationRange(long begin,long end)
    {
        return LocationDTO.queryRangeJson( _activity, begin, end);
    }

    public String getLocationFrom(long begin)
    {
        return LocationDTO.queryFromJson( _activity, begin);
    }
}
