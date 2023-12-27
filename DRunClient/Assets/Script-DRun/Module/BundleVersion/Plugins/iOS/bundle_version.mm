#import "UnityAppController.h"

extern "C" {

    int getVersionNumber_iOS()
    {
        NSString* version = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"];
        NSLog(@"bundle version:%@", version);
        return [version intValue];
    }

}