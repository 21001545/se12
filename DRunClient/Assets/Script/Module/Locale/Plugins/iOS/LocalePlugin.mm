#import <Foundation/Foundation.h>

@interface LocalePlugin : NSObject
{
//You have to put your variables here
}
@end

@implementation LocalePlugin
//Converting String to char for UNITY
//Rememeber that unity can't handle NSString variables
char* cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}
@end

// This is the function we call from unity script
extern "C"
{
    char* IOSgetPhoneCountryCode()
    {
        NSLocale *locale = [NSLocale currentLocale];
        NSString *countryCode = [locale objectForKey: NSLocaleCountryCode];
        //NSString *countryName = [locale displayNameForKey: NSLocaleCountryCode value: countryCode];
        return cStringCopy([countryCode UTF8String]);
    }
}
