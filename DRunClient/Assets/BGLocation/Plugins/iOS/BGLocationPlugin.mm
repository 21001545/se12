#import "UnityAppController.h"
#import <CoreLocation/CoreLocation.h>
#import "LocationManager.h"

extern "C" {
    LocationManager* locationManager;

    void initLocationDevice()
    {
        locationManager = [[LocationManager alloc] init];
    }

    void startLocationService() {
        [locationManager startLocationService];
    }

    void stopLocationService() {
        [locationManager stopLocationService];
    }

    void requestLocation() {
        [locationManager requestLocation];
    }

    char const* getLocationsJson(double time) {
        char const *str =  [[locationManager getLocationsJson: time] UTF8String];
        char* jsn = (char*) malloc(strlen(str) + 1);
        strcpy(jsn, str);
        return jsn;
    }

    void deleteLocationsBefore(double time) {
        [locationManager deleteLocationsBefore:time];
    }

    int authorizationStatus()
    {
        return [locationManager currentAuthorizationStatus];
    }

    void requestAlwaysAuthorization()
    {
        return [locationManager requestAlwaysAuthorization];
    }
}


