package com.lifefesta.drun.coreservice.data;

import android.content.Context;
import android.database.Cursor;

import com.google.gson.Gson;

import java.sql.Array;
import java.util.ArrayList;
import java.util.List;

public class LocationDTO {
    private long time;
    private double latitude;
    private double longitude;
    private double altitude;
    private double h_accuracy;
    private double v_accuracy;

    public long getTime(){ return time;}
    public void setTime(long time){this.time = time;}
    public double getLatitude(){ return latitude;}
    public void setLatitude(double latitude){this.latitude=latitude;}
    public double getLongitude(){ return longitude;}
    public void setLongitude(double longitude){this.longitude=longitude;}
    public double getAltitude(){ return altitude;}
    public void setAltitude(double altitude){this.altitude=altitude;}
    public double getH_accuracy(){ return h_accuracy;}
    public void setH_accuracy(double h_accuracy){this.h_accuracy=h_accuracy;}
    public double getV_accuracy(){ return v_accuracy;}
    public void setV_accuracy(double v_accuracy){this.v_accuracy=v_accuracy;}

    public static LocationDTO create(Cursor cursor)
    {
        int id_time = cursor.getColumnIndex( LocationContentProvider.COLUMN_TIME);
        int id_latitude = cursor.getColumnIndex( LocationContentProvider.COLUMN_LATITUDE);
        int id_longitude = cursor.getColumnIndex( LocationContentProvider.COLUMN_LONGITUDE);
        int id_altitude = cursor.getColumnIndex( LocationContentProvider.COLUMN_ALTITUDE);
        int id_h_accuracy = cursor.getColumnIndex( LocationContentProvider.COLUMN_H_ACCURACY);
        int id_v_accuracy = cursor.getColumnIndex( LocationContentProvider.COLUMN_V_ACCURACY);

        if( id_time == -1)
        {
            return null;
        }

        LocationDTO dto = new LocationDTO();
        dto.time = cursor.getLong(id_time);
        dto.latitude = cursor.getDouble(id_latitude);
        dto.longitude = cursor.getDouble(id_longitude);
        dto.altitude = cursor.getDouble(id_altitude);
        dto.h_accuracy = cursor.getDouble(id_h_accuracy);
        dto.v_accuracy = cursor.getDouble(id_v_accuracy);

        return dto;
    }

    public static List<LocationDTO> createList(Cursor cursor)
    {
        List<LocationDTO> list = new ArrayList<>();
        if( cursor != null)
        {
            while(cursor.moveToNext())
            {
                LocationDTO dto = create(cursor);
                if (dto != null)
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

    public static String queryRangeJson(Context context, long begin, long end)
    {
        String[] args = new String[] {
                String.valueOf(begin),
                String.valueOf(end)
        };

        Cursor cursor = context.getContentResolver().query(LocationContentProvider.CONTENT_URI, null,
                "time >= ? and time <= ?", args, null);

        return createJson(cursor);
    }

    public static String queryFromJson(Context context,long begin)
    {
        String[] args  = new String[] {
                String.valueOf(begin)
        };

        Cursor cursor = context.getContentResolver().query(LocationContentProvider.CONTENT_URI, null,
                "time >= ?", args, null);
        return createJson(cursor);
    }
}
