package com.lifefesta.drun.coreservice.util;

import java.text.SimpleDateFormat;
import java.time.Instant;
import java.util.Date;

public class TimeUtil {
    public static final SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");

    public static String toString(long mills)
    {
        return formatter.format(new Date(mills));
    }
}
