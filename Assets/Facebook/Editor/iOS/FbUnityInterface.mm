//
//  FbUnityInterface.mm
//  Unity-iPhone
//
//  Created by Benjamin Padget on 6/4/13.
//
//

#include "FbUnityInterface.h"
#import <FacebookSDK/FacebookSDK.h>
#import <Foundation/NSJSONSerialization.h>
#include <string>
#include "RegisterMonoModules.h"

static FbUnityInterface *_instance = nil;
const char *g_fbObjName = "UnityFacebookSDKPlugin";

@implementation FbUnityInterface

+(FbUnityInterface *)sharedInstance {
  return _instance;
}

+ (void)initialize {
  if(!_instance) {
    _instance = [[FbUnityInterface alloc] init];
  }
}


- (id)init {
  if(_instance != nil) {
    return _instance;
  }
  
  self = [super init];
  if(!self)
    return nil;
  
  _instance = self;
  
  self.isInitializing = YES;
  self. dialogMode = NativeDialogModes::FAST_APP_SWITCH_SHARE_DIALOG;
  
  [[NSNotificationCenter defaultCenter]
   addObserver:self
   selector:@selector(didBecomeActive:)
   name:UIApplicationDidBecomeActiveNotification
   object:nil];
  
  [[NSNotificationCenter defaultCenter]
   addObserver:self
   selector:@selector(willTerminate:)
   name:UIApplicationWillTerminateNotification
   object:nil];

  return self;
}

- (id)initWithCookie:(bool)cookie
             logging:(bool)_logging
              status:(bool)_status
frictionlessRequests:(bool)_frictionlessRequests {
  self = [self init];
  
  self.useFrictionlessRequests = _frictionlessRequests;
  
  //since this class is a singleton, I don't know how we would ever have an open session here, but handle anyway
  if (self.session.isOpen) {
    [self handleSessionChange:self.session state:self.session.state error:nil];
    return self;
  }
  
  // create a fresh session object
  _session = [[FBSession alloc] init];
  
  // if we don't have a cached token, a call to open here would cause UX for login to
  // occur; we don't want that to happen unless the user clicks the login button, and so
  // we check here to make sure we have a token before calling open
  if (self.session.state == FBSessionStateCreatedTokenLoaded) {
    // even though we had a cached token, we need to login to make the session usable
    [self.session openWithCompletionHandler:^(FBSession *session,
                                              FBSessionState state,
                                              NSError *error) {
      [self handleSessionChange:session state:state error:error];
    }];
  } else {
    UnitySendMessage(g_fbObjName, "OnInitComplete", "");
  }
  return self;
}




- (void)handleSessionChange:(FBSession *)session
                      state:(FBSessionState) state
                      error:(NSError *)error
{
  switch (state) {
    case FBSessionStateOpen: {
      [FBSession setActiveSession:session];

      if(self.useFrictionlessRequests) {
        self.friendCache = [[FBFrictionlessRecipientCache alloc] init];
        [self.friendCache prefetchAndCacheForSession:nil];
      } else {
        self.friendCache = nil;
      }
      
      
      //lets fire off another request while in the completion handler for the previous request
      //what can possibly go wrong?
      [FBRequestConnection startForMeWithCompletionHandler:
       ^(FBRequestConnection *connection, id result, NSError *error) {
        std::string strRes = "";
        id<FBGraphUser> user = result;
        if(user && session != nil && session.accessTokenData != nil && session.accessTokenData.accessToken != nil) {
          strRes += [user.id cStringUsingEncoding:NSUTF8StringEncoding];
          strRes += ":";
          strRes += [session.accessTokenData.accessToken cStringUsingEncoding:NSUTF8StringEncoding];
        }

        if(self.isInitializing) {
          UnitySendMessage(g_fbObjName, "OnInitComplete", strRes.c_str());
          self.isInitializing = NO;
        } else {
          UnitySendMessage(g_fbObjName, "OnLogin", strRes.c_str());
        }
      }];
    }
      break;

    case FBSessionStateClosedLoginFailed:
      [FBSession.activeSession closeAndClearTokenInformation];
      UnitySendMessage(g_fbObjName, "OnLogin", "");
      break;
    default:
      break;
  }  
}

-(void)login:(const char *)scope {
  if (self.session == nil || self.session.state != FBSessionStateCreated) {
    NSString *scopeStr = [NSString stringWithUTF8String:scope];
    NSArray *permissions = nil;
    if(scope && strlen(scope) > 0) {
      permissions = [scopeStr componentsSeparatedByString:@","];
    }
    
    self.session = [[FBSession alloc] initWithAppID:nil
                                               permissions:permissions
                                           defaultAudience:FBSessionDefaultAudienceFriends
                                           urlSchemeSuffix:nil
                                        tokenCacheStrategy:nil];
  }
  
  
  [self.session openWithBehavior:FBSessionLoginBehaviorWithFallbackToWebView
          completionHandler:^(FBSession *session,
                              FBSessionState state,
                              NSError *error) {
    [self handleSessionChange:session state:state error:error];
  }];
}

-(void)logout {
  [self.session closeAndClearTokenInformation];
  UnitySendMessage(g_fbObjName, "OnLogout", "");
}

-(void)didBecomeActive: (NSNotification *)notification {
  [FBAppCall handleDidBecomeActiveWithSession:self.session];
}

-(void)willTerminate:(NSNotification *)notification {
  [self.session close];
}

- (BOOL)openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication {
  bool res = [FBAppCall handleOpenURL:url sourceApplication:sourceApplication withSession:self.session];
  [FBSession setActiveSession:self.session];
  return res;
}

@end





//helper function for stuffing c strings into NSDictionarys
void addCStrToNsDict(NSMutableDictionary *dict, const char *key, const char *val) {
  if(dict && key && val && val[0] != 0 && key[0] != 0) {
    [dict setObject:[NSString stringWithUTF8String:val] forKey:[NSString stringWithUTF8String:key]];
  }
}

void RequestCompletionHelper(int requestId, bool isError, const char *payload) {
  std::string temp = "";
  
  char idStr[16];
  sprintf(idStr, "%d", requestId);
  
  temp += idStr;
  temp += ":";
  if(payload) {
    temp += payload;
  }

  UnitySendMessage(g_fbObjName, "OnRequestComplete", temp.c_str());
}

NSDictionary *UnpackDict(int numVals, const char **keys, const char **vals)
{
  NSMutableDictionary *params = nil;
  if(numVals > 0 && keys && vals) {
    params = [NSMutableDictionary dictionaryWithCapacity:numVals];
    for(int i=0; i<numVals; i++) {
      [params setObject:[NSString stringWithUTF8String:vals[i]] forKey:[NSString stringWithUTF8String:keys[i]]];
    }
  }
  
  return params;
}


//everything in the extern "C" section is callable from C# unity
extern "C" {

void iosInit(bool _cookie, bool _logging, bool _status, bool _frictionlessRequests) {
  //[FbUnityInterface sharedInstance];
  [[FbUnityInterface alloc] initWithCookie:_cookie logging:_logging status:_status frictionlessRequests:_frictionlessRequests];
}

void iosLogin(const char *scope) {
  [[FbUnityInterface sharedInstance] login:scope];
}

void iosLogout() {
  [[FbUnityInterface sharedInstance] logout];
}

void iosSetShareDialogMode(NativeDialogModes::eModes mode) {
  [[FbUnityInterface sharedInstance] setDialogMode:mode];
}

void iosAppRequest(int requestId,
                   const char *message,
                   const char **to,
                   int toLength,
                   const char *filters,
                   const char **excludeIds,
                   int excludeIdsLength,
                   bool hasMaxRecipients, //not supported on mobile
                   int maxRecipients, //not supported on mobile
                   const char *data,
                   const char *title) {
  NSMutableDictionary *params = [NSMutableDictionary dictionary];
  addCStrToNsDict(params, "message", message);
  addCStrToNsDict(params, "filters", filters);
  addCStrToNsDict(params, "data", data);
  addCStrToNsDict(params, "title", title);
  
  if(to && toLength) {
    NSMutableArray *tempArray = [NSMutableArray array];
    for(int i=0; i<toLength; i++) {
      [tempArray addObject:[NSString stringWithUTF8String:to[i]]];
    }
    NSString *tempString = [tempArray componentsJoinedByString:@","];
    [params setObject:tempString forKey:@"to"];
  }
  
  FBFrictionlessRecipientCache *fc = [[FbUnityInterface sharedInstance] friendCache];
  
  [FBWebDialogs
   presentRequestsDialogModallyWithSession:nil
   message:[NSString stringWithUTF8String:message]
   title:[NSString stringWithUTF8String:title]
   parameters:params handler:
   ^(FBWebDialogResult result, NSURL *resultURL, NSError *error) {
     RequestCompletionHelper(requestId, error != nil, [[resultURL absoluteString] UTF8String]);
   }
   friendCache:fc];
}

void iosFeedRequest(int requestId,
                    const char *toId,
                    const char *link,
                    const char *linkName,
                    const char *linkCaption,
                    const char *linkDescription,
                    const char *picture,
                    const char *mediaSource,
                    const char *actionName,
                    const char *actionLink,
                    const char *reference) {
  
  NSMutableDictionary *params = [NSMutableDictionary dictionary];
  
  addCStrToNsDict(params, "to", toId);
  addCStrToNsDict(params, "link", link);
  addCStrToNsDict(params, "name", linkName);
  addCStrToNsDict(params, "caption", linkCaption);
  addCStrToNsDict(params, "description", linkDescription);
  addCStrToNsDict(params, "picture", picture);
  addCStrToNsDict(params, "source", mediaSource);
  addCStrToNsDict(params, "ref", reference);
  
  //json should look like this:
  //[{'name': '$actionName', 'link': '$actionLink'}]
  if(actionName && actionLink && actionName[0] != 0 && actionLink[0] != 0) {
    NSDictionary *tempDict =
    [NSDictionary dictionaryWithObjectsAndKeys:
     [NSString stringWithUTF8String:actionName],
     @"name",
     [NSString stringWithUTF8String:actionLink],
     @"link",
     nil];
    NSArray *tempArray = [NSArray arrayWithObject:tempDict];
    
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:tempArray
                                                       options:0
                                                         error:&error];
    if (jsonData) {
      NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
      [params setObject:jsonString forKey:@"actions"];
    }
  }

  bool userWantsNative = [FbUnityInterface sharedInstance].dialogMode == NativeDialogModes::FAST_APP_SWITCH_SHARE_DIALOG;
  if(userWantsNative) {
    FBShareDialogParams *dialogParams = [[[FBShareDialogParams alloc] init] autorelease];
    
    NSString *strLink = [NSString stringWithUTF8String:link];
    NSURL *linkUrl = [NSURL URLWithString:strLink];
    if(linkUrl.scheme == nil)
    {
      NSString *prefixed = [NSString stringWithFormat:@"http://%@", strLink];
      linkUrl = [NSURL URLWithString:prefixed];
    }
    dialogParams.link = linkUrl;
    dialogParams.name = [NSString stringWithUTF8String:linkName];
    dialogParams.caption = [NSString stringWithUTF8String:linkCaption];
    dialogParams.description = [NSString stringWithUTF8String:linkDescription];
    dialogParams.picture = [NSURL URLWithString:[NSString stringWithUTF8String:picture]];

    bool canPresentNative = [FBDialogs canPresentShareDialogWithParams:dialogParams];
    if( canPresentNative ) {
      [FBDialogs presentShareDialogWithParams:dialogParams
                                  clientState:nil
                                      handler:
       ^(FBAppCall *call, NSDictionary *results, NSError *error) {
         RequestCompletionHelper(requestId, error != nil, "");
       }];
      return;
    }
  }
  
  // Invoke the dialog
  [FBWebDialogs presentFeedDialogModallyWithSession:nil
                                         parameters:params
                                            handler:
   ^(FBWebDialogResult result, NSURL *resultURL, NSError *error) {
     RequestCompletionHelper(requestId, error != nil, [[resultURL absoluteString] UTF8String]);
   }];
  
}
  
NSString *ResponseHelper(id result, NSError *error) {
  NSError *serError = nil;
  if(result && [result isKindOfClass:[NSDictionary class]]) {
    NSDictionary *dict = (NSDictionary *)result;
    id nonJsonResponse = [dict objectForKey:FBNonJSONResponseProperty];
    
    NSData *jsonData;
    if(nonJsonResponse && [nonJsonResponse isKindOfClass:[NSString class]]) {
      return nonJsonResponse;
    }
    
    jsonData = [NSJSONSerialization dataWithJSONObject:result options:0 error:&serError];
    if (jsonData) {
      return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
  }
  else if(error) {
    NSObject * errorUserData = [[error userInfo] objectForKey:FBErrorParsedJSONResponseKey];
    if(errorUserData) {
      NSData *jsonData = [NSJSONSerialization dataWithJSONObject:errorUserData options:0 error:&serError];
      if (jsonData) {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
      }
    }
  }
  return nil;
}

  
void iosCallFbApi(int requestId,
                  const char *query,
                  const char *method,
                  const char **formDataKeys,
                  const char **formDataVals,
                  int formDataLen) {
  
  if(!query || !method)
    return;
  
  
  NSMutableDictionary *params = nil;
  if(formDataLen > 0 && formDataKeys && formDataVals) {
    params = [NSMutableDictionary dictionaryWithCapacity:formDataLen];
    for(int i=0; i<formDataLen; i++) {
      [params setObject:[NSString stringWithUTF8String:formDataVals[i]] forKey:[NSString stringWithUTF8String:formDataKeys[i]]];
    }
    
    [params setObject:@"json" forKey:@"format"];
  }
  
  
  
  FBRequest *req = [[FBRequest alloc] initWithSession:[[FbUnityInterface sharedInstance] session] graphPath:[NSString stringWithUTF8String:query] parameters:params HTTPMethod:[NSString stringWithUTF8String:method]];

  FBRequestConnection *con = [[FBRequestConnection alloc] init];
  [con addRequest:req completionHandler:
   ^(FBRequestConnection *connection, id result, NSError *error) {
     NSString *jsonString = ResponseHelper(result, error);
     RequestCompletionHelper(requestId, false, [jsonString UTF8String]);
   }];
  [con start];
}

void iosFBSettingsPublishInstall(int requestId, const char *appId) {
  [FBSettings publishInstall:[NSString stringWithUTF8String:appId] withHandler:
   ^(FBGraphObject *result, NSError *error) {
     NSString *jsonString = ResponseHelper(result, error);
     RequestCompletionHelper(requestId, error != nil, [jsonString UTF8String]);
   }];
}

void iosFBInsightsFlush() {
  [FBInsights flush];
}

void iosFBInsightsLogConversionPixel(const char *pixelID,
                                     double value) {
  [FBInsights logConversionPixel:[NSString stringWithUTF8String:pixelID] valueOfPixel:value];
}

void iosFBInsightsLogPurchase(double purchaseAmount,
                              const char *currency,
                              int numParams,
                              const char **paramKeys,
                              const char **paramVals) {
  NSDictionary *params = UnpackDict(numParams, paramKeys, paramVals);
  [FBInsights logPurchase:purchaseAmount currency:[NSString stringWithUTF8String:currency] parameters:params];
}

void iosFBInsightsSetAppVersion(const char *version) {
  [FBInsights setAppVersion:[NSString stringWithUTF8String:version]];
}

void iosFBInsightsSetFlushBehavior(int behavior) {
  [FBInsights setFlushBehavior:(FBInsightsFlushBehavior)behavior];
}

}


#if USE_NEW_APP_CLASS_NAME
#import "UnityAppController.h"
@implementation UnityAppController(FacebookURLHandler)
#else
#import "AppController.h"
@implementation AppController(FacebookURLHandler)
#endif



//older ios versions send this
- (BOOL)application:(UIApplication*)application handleOpenURL:(NSURL*)url {
	BOOL res = [[FbUnityInterface sharedInstance] openURL:url sourceApplication:nil];
	return res;
}


- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation {
  BOOL res = [[FbUnityInterface sharedInstance] openURL:url sourceApplication:sourceApplication];
  return res;
}

@end


