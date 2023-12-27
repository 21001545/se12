package com.lifefesta.drun.coreservice.data;


import static com.lifefesta.drun.coreservice.CoreService.LOG_TAG;

import android.content.ContentProvider;
import android.content.ContentUris;
import android.content.ContentValues;
import android.content.Context;
import android.content.UriMatcher;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.net.Uri;
import android.util.Log;

public class StepCountContentProvider extends ContentProvider {
    private static final String DB_NAME = "drun_stepcount";
    //private static final String AUTHORITY = "com.lifefesta.drun.coreservice.store.stepcount";

    private static final int DB_VERSION = 1;
    private static final String TABLE = "stepcount";

    public static final String COLUMN_TIME = "time";
    public static final String COLUMN_COUNT = "count";
    public static final String COLUMN_SENSOR = "sensor";

    private static final String DB_DROP_TABLE = "DROP TABLE " + TABLE + ";";
    private static final String DB_CREATE = "CREATE TABLE " + TABLE + "(" +
            COLUMN_TIME + " INTEGER PRIMARY KEY, " +
            COLUMN_COUNT + " INTEGER, " +
            COLUMN_SENSOR + " INTEGER " +
            ");";

    private static final String PATH = TABLE;
//    public static final Uri CONTENT_URI = Uri.parse("content://" + AUTHORITY + "/" + PATH);
//    private static final String CONTENT_TYPE = "vnd.android.cursor.dir/vnd."
//            + AUTHORITY + "." + PATH;

    private static final int URI_LOCATIONS = 1;

    private static UriMatcher URI_MATCHER;

    private DBHelper dbHelper;
    private String AUTHORITY;
    public static Uri CONTENT_URI;
    private String CONTENT_TYPE;

    @Override
    public boolean onCreate() {
        AUTHORITY = getContext().getPackageName() + ".coreservice.store.stepcount";
        CONTENT_URI = Uri.parse("content://" + AUTHORITY + "/" + PATH);
        CONTENT_TYPE = "vnd.android.cursor.dir/vnd." + AUTHORITY + "." + PATH;

        if( URI_MATCHER == null)
        {
            URI_MATCHER = new UriMatcher(UriMatcher.NO_MATCH);
            URI_MATCHER.addURI(AUTHORITY, PATH, URI_LOCATIONS);
        }

        dbHelper = new DBHelper(getContext());
        return true;
    }

    @Override
    public Cursor query(Uri uri, String[] projection, String selection, String[] selectionArgs, String sortOrder) {
        if (URI_MATCHER.match(uri) != URI_LOCATIONS) {
            throw new IllegalArgumentException("Wrong URI: " + uri);
        }
        Cursor cursor = dbHelper.getWritableDatabase().query(TABLE, projection, selection, selectionArgs, null, null, sortOrder);
        cursor.setNotificationUri(getContext().getContentResolver(), CONTENT_URI);
        return cursor;
    }

    @Override
    public String getType(Uri uri) {
        return (URI_MATCHER.match(uri) == URI_LOCATIONS) ? CONTENT_TYPE : null;
    }

    @Override
    public Uri insert(Uri uri, ContentValues values) {
        if (URI_MATCHER.match(uri) != URI_LOCATIONS) {
            throw new IllegalArgumentException("Wrong URI: " + uri);
        }
        long id = dbHelper.getWritableDatabase().insert(TABLE, null, values);
        Uri resultUri = ContentUris.withAppendedId(CONTENT_URI, id);
        getContext().getContentResolver().notifyChange(resultUri, null);
        return resultUri;
    }

    @Override
    public int delete(Uri uri, String selection, String[] selectionArgs) {
        if (URI_MATCHER.match(uri) != URI_LOCATIONS) {
            throw new IllegalArgumentException("Wrong URI: " + uri);
        }
        int count = dbHelper.getWritableDatabase().delete(TABLE, selection, selectionArgs);
        getContext().getContentResolver().notifyChange(uri, null);
        return count;
    }

    @Override
    public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {
        throw new UnsupportedOperationException("update not available");
    }

    private static class DBHelper extends SQLiteOpenHelper {

        public DBHelper(Context context) {
            super(context, DB_NAME, null, DB_VERSION);
            Log.i(LOG_TAG, "DBHelper init:" + DB_NAME);
        }

        public void onCreate(SQLiteDatabase db) {
            Log.i(LOG_TAG, "Creating table: " + DB_CREATE);
            db.execSQL(DB_CREATE);
        }

        public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
            db.execSQL(DB_DROP_TABLE);
            db.execSQL(DB_CREATE);
        }
    }

}
