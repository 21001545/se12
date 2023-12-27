package com.lifefesta.drun.coreservice;

import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.os.IBinder;
import android.os.PowerManager;
import android.os.SystemClock;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.core.app.NotificationCompat;

public class CoreService extends Service {
    public final static String LOG_TAG = "CoreService";
    public final static String CHANNEL_ID = "CoreServiceChannel";

    //public final static String META_DATA_ACTIVITY_CLASS_NAME = "CoreService.ActivityClassName";

    public enum Actions
    {
        StartService,
        StopService,
        StartLocation,
        StopLocation
    }

    private PowerManager.WakeLock _wakeLock;
    private boolean _isServiceStarted;
    private StepCountTracker _stepCountTracker;
    private LocationTracker _locationTracker;

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        Log.d(LOG_TAG, "onCreate");

        _stepCountTracker = StepCountTracker.create(this);
        _locationTracker = LocationTracker.create(this);
        _wakeLock = null;
        _isServiceStarted = false;

    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        Log.d(LOG_TAG, "onDestroy");
        _stepCountTracker.stop();
        _locationTracker.stop();
    }

    @Override
    public void onTaskRemoved(Intent rootIntent)
    {
        Log.d(LOG_TAG, "onTaskRemoved");

        Intent intent = new Intent(getApplicationContext(), CoreService.class);
        intent.setAction(Actions.StartService.name());
        intent.setPackage(getPackageName());

        PendingIntent restartServicePendingIntent = PendingIntent.getService(this, 1, intent, PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_ONE_SHOT);
        AlarmManager alarm = (AlarmManager) getApplicationContext().getSystemService(Context.ALARM_SERVICE);
        alarm.set(AlarmManager.ELAPSED_REALTIME, SystemClock.elapsedRealtime() + 1000, restartServicePendingIntent);

        super.onTaskRemoved(rootIntent);
    }

    @Override
    public int onStartCommand(Intent intent,int flags,int startId) {
        Log.d(LOG_TAG, String.format("onStartCommand: flags[%d] startId[%d]", flags, startId));

        if( intent != null)
        {
            String actionName = intent.getAction();
            if( actionName != null)
            {
                Actions action = Actions.valueOf(intent.getAction());
                Log.d(LOG_TAG,String.format("onStartCommand: action[%s]", action.toString()));
                if( action == Actions.StartService)
                {
                    actionStartService(intent);
                }
                else if( action == Actions.StopService)
                {
                    actionStopService(intent);
                }
                else if( action == Actions.StartLocation)
                {
                    actionStartLocation(intent);
                }
                else if( action == Actions.StopLocation)
                {
                    actionStopLocation(intent);
                }
            }
            else
            {
                Log.w(LOG_TAG, "onStartCommand: action is null. will treat as StartService");
                actionStartService(null);
            }
        }
        else
        {
            Log.d(LOG_TAG, "onStartCommand: with a null intent. It has been probably restarted by the system. wll treat as StartService");
            actionStartService(null);
        }

        return START_STICKY;
    }

    private void createNotificationChannel() {
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
        {
            NotificationChannel serviceChannel = new NotificationChannel( CHANNEL_ID,
                    "StepCount Service Channel",
                    NotificationManager.IMPORTANCE_DEFAULT);
            serviceChannel.enableVibration(false);
            serviceChannel.setShowBadge(false);
            serviceChannel.enableLights(false);

            NotificationManager manager = getSystemService(NotificationManager.class);
            manager.createNotificationChannel(serviceChannel);
        }
    }

    private NotificationCompat.Builder buildNotification()
    {
        String title = getResources().getString(R.string.notification_title);
        String text = getResources().getString(R.string.notification_text);

/*
Notification notification = (new NotificationCompat.Builder(paramContext, d.c.d.a().a()))
    .setSmallIcon(2131231207)
    .setLargeIcon(i(paramContext))
    .setOngoing(true)
    .setCustomContentView(k(paramContext))
    .setCustomBigContentView(h(paramContext))
    .setStyle((NotificationCompat.Style)new NotificationCompat.DecoratedCustomViewStyle())
    .setCategory("service")
    .setContentIntent(pendingIntent)
    .setSilent(true)
    .setSound(null)
    .setVibrate(null)
    .setLights(0, 0, 0)
    .setGroup(b.b.b.a()).build();
n.f(notification, "Builder(context, Pedomet\n            .build()");
 */

        NotificationCompat.Builder notification = new NotificationCompat.Builder(this, CHANNEL_ID);
        notification.setContentTitle(title)
                .setContentText(text)
                .setContentIntent(createPendingIntent())
                .setSmallIcon(R.drawable.android_push_icon)
                .setCategory(NotificationCompat.CATEGORY_SERVICE)
                .setDefaults(Notification.DEFAULT_LIGHTS | Notification.DEFAULT_SOUND)
                .setVibrate(null)
                .setSound(null)
                .setPriority(NotificationCompat.PRIORITY_LOW)
                .setOngoing(true)
                .setLights(0,0,0)
        ;


        return notification;
    }

    // 알림 클릭하면 앱실행
    private PendingIntent createPendingIntent()
    {

        try
        {
            ApplicationInfo app = getApplicationContext().getPackageManager().getApplicationInfo( getApplicationContext().getPackageName(), PackageManager.GET_META_DATA);
            Bundle bundle = app.metaData;

            String className = bundle.getString("CoreService.ActivityClassName");

            Class<?> cls = Class.forName(className);
            Intent notificationIntent = new Intent( this, cls);
            return PendingIntent.getActivity(this, 0, notificationIntent, PendingIntent.FLAG_IMMUTABLE);
        }
        catch(Exception e)
        {
            Log.e(LOG_TAG, "create pendingIntent fail",e);
            return null;
        }
    }

    private void actionStartService(Intent intent)
    {
        if( _isServiceStarted)
        {
            if(_stepCountTracker.isRunning() == false)
            {
                _stepCountTracker.start();
            }
            return;
        }

        _isServiceStarted = true;
        CoreServiceTracker.setServiceState(getApplicationContext(),CoreServiceTracker.ServiceState.STARTED);

        PowerManager powerManager = (PowerManager)getSystemService(Context.POWER_SERVICE);
        _wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "CoreService::lock");
        _wakeLock.acquire();

        _stepCountTracker.start();

        //
        if( CoreServiceTracker.getLocationState(getApplicationContext()) == CoreServiceTracker.LocationState.STARTED)
        {
            Log.i(LOG_TAG,"try restart location tracker");
            _locationTracker.start();
        }

        createNotificationChannel();

        // 음
        try
        {
            startForeground(1, buildNotification().build());
        }
        catch(Exception e)
        {
            Log.w(LOG_TAG, "startForegroundError", e);
        }

        _wakeLock.release();
    }

    private void actionStopService(Intent intent)
    {
        try
        {
            if( _wakeLock != null && _wakeLock.isHeld())
            {
                _wakeLock.release();
            }

            stopForeground(false);
            stopSelf();
        }
        catch(Exception e)
        {
            Log.e(LOG_TAG, "service stopped without being started", e);
        }

        _isServiceStarted = false;
        CoreServiceTracker.setServiceState( this, CoreServiceTracker.ServiceState.STOPPED);
    }

    private void actionStartLocation(Intent intent)
    {
        _locationTracker.start();
        CoreServiceTracker.setLocationState( this, CoreServiceTracker.LocationState.STARTED);
    }

    private void actionStopLocation(Intent intent)
    {
        _locationTracker.stop();
        CoreServiceTracker.setLocationState( this, CoreServiceTracker.LocationState.STOPPED);
    }


}
