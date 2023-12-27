package com.lifefesta.drun.coreservice.data;

import android.content.Context;
import android.database.Cursor;

import com.google.gson.Gson;

import java.util.ArrayList;
import java.util.List;

public class StepCountDTO {
    private long time;
    private int count;
    private int sensor;

    public long getTime(){ return time;}
    public void setTime(long time) { this.time = time;}
    public int getCount(){ return count;}
    public void setCount(int count) { this.count = count;}
    public int getSensor(){ return sensor;}
    public void setSensor(int sensor) { this.sensor = sensor;}

    public static StepCountDTO create(Cursor cursor)
    {
        int id_time = cursor.getColumnIndex( StepCountContentProvider.COLUMN_TIME );
        int id_count = cursor.getColumnIndex( StepCountContentProvider.COLUMN_COUNT );
        int id_sensor = cursor.getColumnIndex( StepCountContentProvider.COLUMN_SENSOR);

        if( id_time == -1 || id_count == -1 || id_sensor == -1)
        {
            return null;
        }

        StepCountDTO dto = new StepCountDTO();

        dto.time = cursor.getLong( id_time);
        dto.count = cursor.getInt( id_count);
        dto.sensor = cursor.getInt( id_sensor);

        return dto;
    }

    public static List<StepCountDTO> createList(Cursor cursor)
    {
        List<StepCountDTO> list = new ArrayList<>();
        if( cursor != null)
        {
            while(cursor.moveToNext()) {
                StepCountDTO dto = StepCountDTO.create(cursor);
                if( dto != null)
                {
                    list.add(dto);
                }
            }
        }

        return list;
    }

    public static String createJson(Cursor cursor)
    {
        return (new Gson()).toJson(createList(cursor));
    }

    public static String queryRangeJson(Context context,long begin, long end)
    {
        String[] args = new String[] {
                String.valueOf(begin),
                String.valueOf(end)
        };

        Cursor cursor = context.getContentResolver().query(StepCountContentProvider.CONTENT_URI, null,
                "time >= ? and time <= ?", args, null);

        return createJson(cursor);
    }

    public static String queryFromJson(Context context,long begin)
    {
        String[] args  = new String[] {
                String.valueOf(begin)
        };

        Cursor cursor = context.getContentResolver().query(StepCountContentProvider.CONTENT_URI, null,
                "time >= ?", args, null);
        return createJson(cursor);
    }
}