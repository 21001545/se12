#import <Foundation/Foundation.h>
#import <Contacts/Contacts.h>
#import <MessageUI/MFMessageComposeViewController.h>

#include <objc/runtime.h>

extern UIViewController* UnityGetGLViewController();

@interface UNativeContacts:NSObject
+ (int)checkPermission;
+ (char*)loadContactList;
+ (int)openSMS:(NSString *)phoneNumber message:(NSString *)message;
@end

@implementation UNativeContacts

static MFMessageComposeViewController *messageComposeViewController;

// Credit: https://stackoverflow.com/a/37052118/2373034
+ (char *)getCString:(NSString *)source
{
    if( source == nil )
        source = @"";
    
    const char *sourceUTF8 = [source UTF8String];
    char *result = (char*) malloc( strlen( sourceUTF8 ) + 1 );
    strcpy( result, sourceUTF8 );
    
    return result;
}

+(int)checkPermission{
    if ( [CNContactStore authorizationStatusForEntityType:CNEntityTypeContacts] == CNAuthorizationStatusAuthorized )
        return 1;
    return 0;
}

+(char*)loadContactList {
    NSMutableArray<NSString *> *contactList = [[NSMutableArray alloc] init];
    CNAuthorizationStatus status = [CNContactStore authorizationStatusForEntityType:CNEntityTypeContacts];
    
    if( status == CNAuthorizationStatusDenied || status == CNAuthorizationStatusRestricted)
    {
        NSLog(@"access denied");
    }
    else
    {
        CNContactStore *contactStore = [[CNContactStore alloc] init];
        
        // 개수를 미리 알 수 없나?
        NSArray *keys = [[NSArray alloc]initWithObjects:CNContactIdentifierKey,  CNContactGivenNameKey, CNContactFamilyNameKey, CNContactMiddleNameKey, CNContactPhoneNumbersKey, nil];
        
        CNContactFetchRequest *request = [[CNContactFetchRequest alloc] initWithKeysToFetch:keys];
        
        request.predicate = nil;
        
        [contactStore enumerateContactsWithFetchRequest:request
                                                  error:nil
                                             usingBlock:^(CNContact* __nonnull contact, BOOL* __nonnull stop)
         {
             NSString *phoneNumber = @"";
             NSString *name = @"";
             if( contact.phoneNumbers)
                 phoneNumber = [[[contact.phoneNumbers firstObject] value] stringValue];
                 
            if (contact.givenName == nil || contact.givenName.length == 0 )
                name = contact.familyName;

            if ( phoneNumber == nil || phoneNumber.length == 0 )
                return;

            if ( name == nil || name.length == 0 )
                return;
                
             [contactList addObject:phoneNumber];
             [contactList addObject:name];
         }];
        
    }

    return [self getCString:[contactList componentsJoinedByString:@">"]];
}

+(int)openSMS:(NSString *)phoneNumber message:(NSString *)message
{
    messageComposeViewController = [[MFMessageComposeViewController alloc] init];
    if([MFMessageComposeViewController canSendText])
    {
        messageComposeViewController.body = message;
        messageComposeViewController.recipients = [NSArray arrayWithObjects:phoneNumber,nil];
        // 굳이 결과를 전달 받을 필요는 없지.
        messageComposeViewController.messageComposeDelegate = (id)self;

        [UnityGetGLViewController() presentViewController:messageComposeViewController animated:YES completion:nil];
        return 1;
    }

    // sms를 보낼 수 없는 기기인듯? 셀룰러가 아니라든가? 그런건가?
    return 0;
}

+(void)messageComposeViewController:(nonnull MFMessageComposeViewController *)controller didFinishWithResult:(MessageComposeResult)result {  
     // 딱히 뭐 할게 없는데? 컨트롤러나 닫자.
    switch (result) {
        case MessageComposeResultCancelled:
            break;
        case MessageComposeResultFailed:
            break;
        case MessageComposeResultSent:
            break;
        default:
            break;
    }

    messageComposeViewController = nil;
    [controller dismissViewControllerAnimated:true completion:nil];
}
@end

extern "C" int _NativeContacts_CheckPermission( )
{
    return [UNativeContacts checkPermission];
}

extern "C" char* _NativeContacts_GetContacts()
{
    return [UNativeContacts loadContactList];
}
extern "C" int _NativeContacts_OpenSMS( const char* phoneNumber, const char* message )
{
    return [UNativeContacts openSMS:[NSString stringWithUTF8String:phoneNumber] message:[NSString stringWithUTF8String:message]];
}

