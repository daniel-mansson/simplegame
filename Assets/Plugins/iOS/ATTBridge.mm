/**
 * ATTBridge.mm
 *
 * Native bridge for Apple's App Tracking Transparency (ATT) framework.
 * Provides two C functions callable via Unity P/Invoke:
 *
 *   _RequestATTAuthorization()    — Calls requestTrackingAuthorizationWithCompletionHandler.
 *                                   The completion block is intentionally empty; the C# side
 *                                   polls _GetATTAuthorizationStatus() for the result.
 *
 *   _GetATTAuthorizationStatus()  — Returns the current tracking authorization status as int:
 *                                   0 = NotDetermined, 1 = Restricted, 2 = Denied, 3 = Authorized
 *
 * These values mirror ATTAuthorizationStatus.cs enum ordering.
 *
 * The AppTrackingTransparency framework is weak-linked so the app does not crash
 * on iOS < 14 — on older OS versions the status is always Authorized (pre-ATT behaviour).
 */

#import <Foundation/Foundation.h>

#if __has_include(<AppTrackingTransparency/AppTrackingTransparency.h>)
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#define ATT_AVAILABLE 1
#else
#define ATT_AVAILABLE 0
#endif

extern "C" {

void _RequestATTAuthorization()
{
#if ATT_AVAILABLE
    if (@available(iOS 14, *))
    {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            // C# side polls _GetATTAuthorizationStatus() for the result.
            (void)status;
        }];
    }
#endif
}

int _GetATTAuthorizationStatus()
{
#if ATT_AVAILABLE
    if (@available(iOS 14, *))
    {
        return (int)[ATTrackingManager trackingAuthorizationStatus];
    }
#endif
    // iOS < 14: tracking was unrestricted — treat as Authorized.
    return 3;
}

} // extern "C"
