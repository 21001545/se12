package com.lifefesta.drun.coreservice;

import static com.lifefesta.drun.coreservice.CoreService.LOG_TAG;

import android.Manifest;
import android.content.ContentValues;
import android.content.Context;
import android.content.pm.PackageManager;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Build;
import android.util.Log;

import androidx.core.content.ContextCompat;

import com.lifefesta.drun.coreservice.data.StepCountContentProvider;
import com.lifefesta.drun.coreservice.util.TimeUtil;

import java.util.List;

public class StepCountTracker implements SensorEventListener {

    private CoreService _owner;
    private SensorManager _manager;

    private int _lastStepCount = -1;
    private boolean _registered;

    public static StepCountTracker create(CoreService owner)
    {
        StepCountTracker t = new StepCountTracker();
        t.init(owner);
        return t;
    }

    public boolean isRunning()
    {
        return _registered;
    }

    private void init(CoreService owner)
    {
        _owner = owner;
        _manager = (SensorManager)_owner.getSystemService(Context.SENSOR_SERVICE);
        _registered = false;
    }

    public void start()
    {
        if( hasPermission() == false)
        {
            Log.e(LOG_TAG, "failed to start tracking step count : permission not granted");
            return;
        }

        if( _registered)
        {
            Log.w(LOG_TAG,"step sensor listener already registered");
            return;
        }

        final Sensor sensor = _manager.getDefaultSensor(Sensor.TYPE_STEP_COUNTER);
        if( sensor == null)
        {
            Log.e(LOG_TAG, "failed to acquire step counter sensor");

            List<Sensor> deviceSensors = _manager.getSensorList(Sensor.TYPE_ALL);

            for(int i=0; i<deviceSensors.size(); i++)
            {
                Sensor ss = deviceSensors.get(i);
                //센서 이름 확인
                Log.d(LOG_TAG, ss.getName() + "[" + ss.getType() + "]");
            }

            return;
        }

        _manager.registerListener( this, sensor, SensorManager.SENSOR_DELAY_UI);
        _registered = true;
        Log.d(LOG_TAG, "register step count listener");
    }

    public void stop()
    {
        _manager.unregisterListener(this);
        _registered = false;
        Log.d(LOG_TAG, "unregister step count listener");
    }

    public boolean hasPermission()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            return ContextCompat.checkSelfPermission(_owner, Manifest.permission.ACTIVITY_RECOGNITION) ==
                    PackageManager.PERMISSION_GRANTED;
        }
        else
        {
            return true;
//            int result = _owner.getPackageManager().checkPermission("android.permission.ACTIVITY_RECOGNITION", _owner.getPackageName());
//            return result == PackageManager.PERMISSION_GRANTED;
        }
    }

    @Override
    public void onSensorChanged(SensorEvent sensorEvent) {
        int value = (int)sensorEvent.values[0];
        Log.d(LOG_TAG,String.format("onSensorChanged steps=%d",value));

        if( _lastStepCount == -1)
        {
            _lastStepCount = value;
            return;
        }

        int delta = value - _lastStepCount;
        _lastStepCount = value;

        if( delta <= 0)
        {
            return;
        }

        saveStepCount(System.currentTimeMillis(), delta, value);
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int i) {

    }

    private void saveStepCount(long time,int count,int sensor)
    {
        ContentValues values = new ContentValues();
        values.put(StepCountContentProvider.COLUMN_TIME, time);
        values.put(StepCountContentProvider.COLUMN_COUNT, count);
        values.put(StepCountContentProvider.COLUMN_SENSOR, sensor);
        _owner.getContentResolver().insert(StepCountContentProvider.CONTENT_URI, values);

        Log.d(LOG_TAG,String.format("insert step count: time[%s] count[%d] sensor[%d]", TimeUtil.toString(time), count, sensor));
    }
}
