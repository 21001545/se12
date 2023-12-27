#import <Foundation/Foundation.h>
#import <MapKit/MapKit.h>

@interface LocationManager : NSObject <CLLocationManagerDelegate>

-(void) startLocationService;
-(void) stopLocationService;
-(void) requestLocation;
-(NSString *) getLocationsJson: (double) time;
-(void) deleteLocationsBefore: (double) time;
-(int) currentAuthorizationStatus;
-(void) requestAlwaysAuthorization;
@end
