#import "DBManager.h"

static DBManager *instance = nil;
static sqlite3 *database = nil;
static sqlite3_stmt *statement = nil;

@interface DBManager ()
-(void) createDB;
@property(nonatomic, strong) NSString *databasePath;
@end


@implementation DBManager

+(DBManager*) getInstance{
    if(!instance) {
        instance = [[super allocWithZone:nil] init];
        [instance createDB];
    }
    return instance;
}

-(void) createDB {
    NSLog(@"iOS -> DBManager createDB: Start");
    
    NSString *docsDir;
    NSArray *dirPaths;
    
    dirPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    docsDir = dirPaths[0];
    
    self.databasePath = [[NSString alloc] initWithString:
                         [docsDir stringByAppendingPathComponent: @"devhelp.db"]];
    
    NSFileManager *fileManager = [NSFileManager defaultManager];
    if([fileManager fileExistsAtPath: self.databasePath] == NO) {
        NSLog(@"iOS -> DBManager createDB: creating DB file");
        
        [self createTable];
    }
    else
    {
        [self prepareTable];
    }
    NSLog(@"iOS -> DBManager createDB: Finish");
}

-(void) createTable {
    const char *dbPath = [self.databasePath UTF8String];
    int openResult =  sqlite3_open(dbPath, &database);
    
    if(openResult == SQLITE_OK) {
        char *errMsg;
        const char *sql_stmt = "CREATE TABLE location (time REAL PRIMARY KEY, latitude REAL, longitude REAL, altitude REAL, h_accuracy REAL, v_accuracy REAL, speed REAL, speed_accuracy REAL);";
        int exec = sqlite3_exec(database, sql_stmt, NULL, NULL, &errMsg) ;
        if(exec != SQLITE_OK) {
            NSLog(@"iOS -> DBManager createDB: Failed to create TABLE, result = %d, errMsg: %s", exec, errMsg);
        }
        sqlite3_close(database);

        [[NSUserDefaults standardUserDefaults] setInteger:2 forKey:@"LocationTableVersion"];

    } else {
        NSLog(@"iOS -> DBManager createDB: Failed to open/create database, result = %d, error: %s",
                openResult, sqlite3_errmsg(database));
    }
}

-(void) prepareTable {
    NSInteger version = [[NSUserDefaults standardUserDefaults] integerForKey:@"LocationTableVersion"];
    if( version != 3)
    {
        NSLog(@"update DB Table");
        [self dropTable];
        [self createTable];
    }
}

-(void) dropTable {
    const char *dbPath = [self.databasePath UTF8String];
    int openResult =  sqlite3_open(dbPath, &database);
    
    if(openResult == SQLITE_OK) {
        char* errMsg;
        const char *sql_stmt = "DROP TABLE location;";
        int exec = sqlite3_exec(database, sql_stmt, NULL, NULL, &errMsg);
        if( exec != SQLITE_OK) { 
            NSLog(@"iOS -> DBManager dropTable: Failed to drop TABLE, result = %d, errMsg: %s", exec, errMsg);
        }
        sqlite3_close(database);
    }
    else
    {
        NSLog(@"iOS -> DBManager dropTable: Failed to open/create database, result = %d, error: %s",
                openResult, sqlite3_errmsg(database));
    }
}

-(void) insertLocationAt:(double)time latitude:(double)latitude longitude:(double)longitude altitude:(double)altitude
              h_accuracy:(double)h_accuracy v_accuracy:(double)v_accuracy speed:(double)speed speed_accuracy:(double)speed_accuracy
{
    //NSLog(@"iOS -> DBManager insert: start with params %f, %f, %f", time, latitude, longitude);
    const char *dbPath = [self.databasePath UTF8String];
    int openResult =  sqlite3_open(dbPath, &database);
    if(openResult == SQLITE_OK) {
        NSString *sql = [NSString stringWithFormat:
                         @"INSERT INTO location (time, latitude, longitude, altitude, h_accuracy, v_accuracy, speed, speed_accuracy) VALUES(\"%f\", \"%f\", \"%f\", \"%f\", \"%f\", \"%f\", \"%f\", \"%f\")",
                         time, latitude, longitude, altitude, h_accuracy, v_accuracy, speed, speed_accuracy ];
        const char *sql_stmt = [sql UTF8String];
        sqlite3_prepare_v2(database, sql_stmt, -1, &statement, NULL);
        int exec  = sqlite3_step(statement);
        if(exec != SQLITE_DONE) {
        //    NSLog(@"iOS -> DBManager insert: execution result = %d, msg: %s", exec, sqlite3_errmsg(database));
        }
        sqlite3_finalize(statement);
    } else {
        NSLog(@"iOS -> DBManager insert: Failed to open DB, result = %d, error : %s", openResult, sqlite3_errmsg(database));
    }
    sqlite3_close(database);
}

-(NSArray*) selectLocationsAfter: (double) time {
    //NSLog(@"iOS -> DBManager select: start with params %f", time);
    
    NSMutableArray *resultArray  =[[NSMutableArray alloc] init];
    
    const char *dbPath = [self.databasePath UTF8String];
    int openResult =  sqlite3_open(dbPath, &database);
    if(openResult == SQLITE_OK) {
        NSString *sql = [NSString stringWithFormat:
                         @"SELECT time, latitude, longitude, altitude, h_accuracy,v_accuracy,speed,speed_accuracy FROM location WHERE time > %f", time];
        char const *sql_stmt = [sql UTF8String];
        
        int prepare = sqlite3_prepare_v2(database, sql_stmt, -1, &statement, NULL);
        if(prepare == SQLITE_OK) {
            while(sqlite3_step(statement) == SQLITE_ROW) {
                double time = sqlite3_column_double(statement, 0);
                double latitude = sqlite3_column_double(statement, 1);
                double longitude = sqlite3_column_double(statement, 2);
                double altitude = sqlite3_column_double(statement, 3);
                double h_accuracy = sqlite3_column_double(statement, 4);
                double v_accuracy = sqlite3_column_double(statement, 5);
                double speed = sqlite3_column_double(statement, 6);
                double speed_accuracy = sqlite3_column_double(statement, 7);

                NSMutableDictionary *rowData = [NSMutableDictionary dictionaryWithObjectsAndKeys:
                                                [NSNumber numberWithDouble:time], @"time",
                                                [NSNumber numberWithDouble:latitude], @"latitude",
                                                [NSNumber numberWithDouble:longitude], @"longitude",
                                                [NSNumber numberWithDouble:altitude], @"altitude",
                                                [NSNumber numberWithDouble:h_accuracy], @"h_accuracy",
                                                [NSNumber numberWithDouble:v_accuracy], @"v_accuracy",
                                                [NSNumber numberWithDouble:speed], @"speed",
                                                [NSNumber numberWithDouble:speed_accuracy], @"speed_accuracy",
                                                nil];
                
                [resultArray addObject:rowData];
            }
        } else {
            //NSLog(@"iOS -> DBManager select: Failed prepare, result = %d, error : %s", prepare, sqlite3_errmsg(database));
        }
        sqlite3_finalize(statement);
    } else {
        NSLog(@"iOS -> DBManager select: Failed to open DB, result = %d, error : %s", openResult, sqlite3_errmsg(database));
    }
    
    sqlite3_close(database);
    return resultArray;
}

-(int) deleteLocationsBefore:(double)time {
    NSLog(@"iOS -> DBManager delete: start with param %f", time);
    int deletedRows;
    const char *dbPath = [self.databasePath UTF8String];
    int openResult =  sqlite3_open(dbPath, &database);
    if(openResult == SQLITE_OK) {
        NSString *sql = [NSString stringWithFormat:
                         @"DELETE FROM location_v2 WHERE time < \"%f\"", time];
        const char *sql_stmt = [sql UTF8String];
        sqlite3_prepare_v2(database, sql_stmt, -1, &statement, NULL);
        int exec  = sqlite3_step(statement);
        if(exec == SQLITE_DONE) {
            deletedRows = sqlite3_changes(database);
            NSLog(@"iOS -> DBManager delete: affected rows = %d", deletedRows);
        } else {
            NSLog(@"iOS -> DBManager delete: execution result = %d, msg: %s", exec, sqlite3_errmsg(database));
        }
        sqlite3_finalize(statement);
    } else {
        NSLog(@"iOS -> DBManager delete: Failed to open DB, result = %d, error : %s", openResult, sqlite3_errmsg(database));
    }
    sqlite3_close(database);
    return deletedRows;
}

@end
