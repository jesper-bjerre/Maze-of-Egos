#import <UIKit/UIKit.h>
extern "C" void MazeHaptic(int kind) {
    @autoreleasepool {
        if (kind == 2) {
            UINotificationFeedbackGenerator *feedback = [[UINotificationFeedbackGenerator alloc] init];
            [feedback notificationOccurred:UINotificationFeedbackTypeWarning];
        } else {
            UIImpactFeedbackStyle style = kind == 1 ? UIImpactFeedbackStyleMedium : UIImpactFeedbackStyleLight;
            UIImpactFeedbackGenerator *feedback = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
            [feedback impactOccurred];
        }
    }
}
