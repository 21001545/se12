
#import <Foundation/Foundation.h>
#import "LocationManager.h"
#import "DBManager.h"

static CLLocationDistance const minDistanceMetters = 1.0;

@interface LocationManager ()

@property (nonatomic , strong)  CLLocationManager *locationManager;
@property (nonatomic , strong)  CLLocation *lastLocation;
@property CLAuthorizationStatus authorizationStatus;
@end

@implementation LocationManager

-(id) init {
    self = [super init];
    NSLog(@"iOS -> Location manager: init");
    
    self.locationManager =[[CLLocationManager alloc]init];
    self.locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation; // 최고 정확도
    self.locationManager.distanceFilter = kCLDistanceFilterNone;    // 필터링 끔
    self.locationManager.allowsBackgroundLocationUpdates = true;     // Background수집
    self.locationManager.pausesLocationUpdatesAutomatically = false;  // 자동 수신 중지 끔
    self.locationManager.delegate = self;
    self.authorizationStatus = kCLAuthorizationStatusNotDetermined;

    // 2021.12.16 이강희 - 권한 낮춤
    //[self.locationManager requestAlwaysAuthorization];
    [self.locationManager requestWhenInUseAuthorization];
    
    return self;
}

-(NSString *) getLocationsJson: (double) time {
    //NSLog(@"iOS -> Location manager: getLocationsString, time = %f", time);
    NSArray *arr = [[DBManager getInstance] selectLocationsAfter:time];
    
    //NSLog(@"iOS -> Location manager: getLocationsString, got elements in Array %lu", (unsigned long)arr.count);
    NSError* error;
    NSData* json = [NSJSONSerialization
                    dataWithJSONObject:arr
                    options:kNilOptions
                    error:&error];
    NSString *jsonString = [[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding];
    //NSLog(@"iOS -> Location manager: getLocationsString, json= %@", jsonString);
    
    return jsonString;
};


-(void) startLocationService {
    NSLog(@"iOS -> Location manager: startLocationService");
    if([CLLocationManager locationServicesEnabled]) {
        [self.locationManager startUpdatingLocation];
    } else {
        NSLog(@"iOS -> Location manager: location services disabled");
    }
};


-(void) stopLocationService {
    NSLog(@"iOS -> Location manager: stopLocationService");
    if([CLLocationManager locationServicesEnabled]) {
        [self.locationManager stopUpdatingLocation];
    } else {
        NSLog(@"iOS -> Location manager: location services disabled");
    }
};

-(void) deleteLocationsBefore:(double)time {
    NSLog(@"iOS -> Location manager: deleteLocationsBefore time = %f", time);
    int records = [[DBManager getInstance] deleteLocationsBefore:time];
    NSLog(@"iOS -> Location manager: deleted records = %d", records);
}

-(void) requestLocation {
    NSLog(@"iOS -> Location manager: requestLocation");
    if([CLLocationManager locationServicesEnabled]) {
        [self.locationManager requestLocation];
    } else {
        NSLog(@"iOS -> Location manager: location service disabled");
    }
};

#pragma mark - CLLocationManagerDelegate

- (void) locationManager:(CLLocationManager *)manager didFailWithError:(nonnull NSError *)error
{
    NSLog(@"%@",error);
}

- (void) locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray<CLLocation *> *)locations
{
    if(locations.count == 0) {
        return;
    }
    
    // 2021.12.06 이강희
    // 1. 자체 위치 이동 filter가 있다 (1미터 미만 무시) (제거)
    // 2. 시간값을 임의로 세팅하고 있다 (제거)
    
    // for(CLLocation *newLocation in locations) {
    //     if(!self.lastLocation ||
    //        [newLocation distanceFromLocation:self.lastLocation] > minDistanceMetters) {
    //         NSLog(@"iOS -> CLLocationManagerDelegate: Got new location: %@", newLocation);
    //         self.lastLocation = newLocation;
    //         //save to DB
    //         [[DBManager getInstance] insertLocationAt:([[NSDate date] timeIntervalSince1970] * 1000)
    //                                          latitude: newLocation.coordinate.latitude
    //                                         longitude:newLocation.coordinate.longitude
    //                                        h_accuracy: newLocation.horizontalAccuracy
    //                                        v_accuracy: newLocation.verticalAccuracy
    //          ];
    //     }
    // }
    //NSLog(@"didUpdateLocations: count:%d", locations.count);

    for(CLLocation* newLocation in locations)
    {
        //NSLog(@"%@", newLocation);

        // if (@available(iOS 15.0, *)) {
        //     NSLog(@"Lon[%f] Lat[%f] Accuracy[%f] Speed[%f] SpeedAccuracy[%f] Source[%d,%d]", newLocation.coordinate.longitude,
        //           newLocation.coordinate.latitude,
        //           newLocation.horizontalAccuracy,
        //           newLocation.speed,newLocation.speedAccuracy,
        //           newLocation.sourceInformation.isProducedByAccessory,
        //           newLocation.sourceInformation.isSimulatedBySoftware);
        // } else {
        //     // Fallback on earlier versions
        // }
        
        self.lastLocation = newLocation;
        [[DBManager getInstance] insertLocationAt:([newLocation.timestamp timeIntervalSince1970] * 1000)
                                        latitude: newLocation.coordinate.latitude
                                        longitude: newLocation.coordinate.longitude
                                        altitude: newLocation.altitude  // 2022.08.08 이강희
                                        h_accuracy: newLocation.horizontalAccuracy
                                        v_accuracy: newLocation.verticalAccuracy
                                        speed: newLocation.speed
                                        speed_accuracy: newLocation.speedAccuracy
        ];

    }
}

-(void) locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status {
    self.authorizationStatus = status;
    NSLog(@"iOS -> CLLocationManagerDelegate: Authorization status changed: %d", status);
}

-(void) locationManagerDidPauseLocationUpdates:(CLLocationManager *)manager {
    NSLog(@"iOS -> CLLocationManagerDelegate: locationManagerDidPauseLocationUpdates");
}

-(void) locationManagerDidResumeLocationUpdates:(CLLocationManager *)manager {
    NSLog(@"iOS -> CLLocationManagerDelegate: locationManagerDidResumeLocationUpdates");
}

-(int) currentAuthorizationStatus
{
    return self.authorizationStatus;
}

-(void) requestAlwaysAuthorization
{
    [self.locationManager requestAlwaysAuthorization];
}
@end
