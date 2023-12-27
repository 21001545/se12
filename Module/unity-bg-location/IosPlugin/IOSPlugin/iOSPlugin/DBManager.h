#import <Foundation/Foundation.h>
#import <sqlite3.h>

@interface DBManager : NSObject
+(DBManager*) getInstance;
-(void) insertLocationAt: (double) time latitude:(double) latitude longitude:(double) longitude h_accuracy:(double) h_accuracy v_accuracy:(double) v_accuracyß;
-(NSArray*) selectLocationsAfter: (double) time;
-(int) deleteLocationsBefore:(double) time;
@end
