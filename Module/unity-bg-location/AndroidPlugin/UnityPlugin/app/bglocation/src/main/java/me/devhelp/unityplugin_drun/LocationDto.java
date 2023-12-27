package me.devhelp.unityplugin_drun;

public class LocationDto {
    private long time;
    private double latitude;
    private double longitude;
    private double altitude;
    private double h_accuracy;
    private double v_accuracy;

    public long getTime() {
        return time;
    }
    public void setTime(long time) {
        this.time = time;
    }
    public double getLatitude() {
        return latitude;
    }
    public void setLatitude(double latitude) {
        this.latitude = latitude;
    }
    public double getLongitude() {
        return longitude;
    }
    public void setLongitude(double longitude) {
        this.longitude = longitude;
    }
    public double getAltitude() { return altitude;}
    public void setAltitude(double altitude) { this.altitude = altitude;}
    public double getH_accuracy(){ return h_accuracy;}
    public void setH_accuracy(double h_accuracy) {this.h_accuracy = h_accuracy;}
    public double getV_accuracy(){ return v_accuracy;}
    public void setV_accuracy(double v_accuracy) {this.v_accuracy = v_accuracy;}
}
