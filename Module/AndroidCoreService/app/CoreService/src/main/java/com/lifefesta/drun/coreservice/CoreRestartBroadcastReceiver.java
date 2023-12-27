package com.lifefesta.drun.coreservice;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.SystemClock;

public class CoreRestartBroadcastReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        CoreServiceTracker.ServiceState state = CoreServiceTracker.getServiceState(context.getApplicationContext());

        if( state == CoreServiceTracker.ServiceState.STARTED)
        {
            if( intent.getAction().equals( Intent.ACTION_BOOT_COMPLETED))
            {
                //restartService(context, intent);
                restartService2(context);
            }
            else if( intent.getAction().equals( Intent.ACTION_MY_PACKAGE_REPLACED))
            {
                //restartService(context, intent);
                restartService2(context);
            }
        }
    }

    // 바로 실행
    private void restartService(Context context,Intent intent)
    {
        Intent restartIntent = new Intent(context.getApplicationContext(), CoreService.class);
        restartIntent.setAction(CoreService.Actions.StartService.name());
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O)
        {
            context.startForegroundService(restartIntent);
        }
        else
        {
            context.startService(restartIntent);
        }
    }

    // 알림 받고 실행
    private void restartService2(Context context)
    {
        Intent intent = new Intent(context.getApplicationContext(), CoreService.class);
        intent.setAction(CoreService.Actions.StartService.name());
        intent.setPackage(context.getPackageName());

        PendingIntent restartServicePendingIntent = PendingIntent.getService(context, 1, intent, PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_ONE_SHOT);
        AlarmManager alarm = (AlarmManager) context.getApplicationContext().getSystemService(Context.ALARM_SERVICE);
        alarm.set(AlarmManager.ELAPSED_REALTIME, SystemClock.elapsedRealtime() + 1000, restartServicePendingIntent);
    }
}
