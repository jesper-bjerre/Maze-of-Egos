#import <AVFAudio/AVFAudio.h>
#import <UIKit/UIKit.h>

static BOOL interrupted = NO;
static BOOL observing = NO;

static BOOL ApplyPlaybackPolicy(BOOL activate) {
    AVAudioSession *session = AVAudioSession.sharedInstance;
    NSError *error = nil;
    if (![session.category isEqualToString:AVAudioSessionCategoryPlayback] ||
        !(session.categoryOptions & AVAudioSessionCategoryOptionMixWithOthers)) {
        if (![session setCategory:AVAudioSessionCategoryPlayback mode:AVAudioSessionModeDefault
                         options:AVAudioSessionCategoryOptionMixWithOthers error:&error]) {
            NSLog(@"Maze audio category could not be set: %@", error.localizedDescription); return NO;
        }
    }
    if (activate && ![session setActive:YES error:&error]) {
        NSLog(@"Maze audio session could not be activated: %@", error.localizedDescription); return NO;
    }
    return YES;
}

static void RestoreWhenActive() {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (!interrupted && UIApplication.sharedApplication.applicationState == UIApplicationStateActive)
            ApplyPlaybackPolicy(YES);
    });
}

extern "C" int MazeAudioConfigurePlayback() {
    if (!observing) {
        observing = YES;
        NSNotificationCenter *center = NSNotificationCenter.defaultCenter;
        [center addObserverForName:UIApplicationDidBecomeActiveNotification object:nil queue:NSOperationQueue.mainQueue
                       usingBlock:^(NSNotification *note) { RestoreWhenActive(); }];
        [center addObserverForName:AVAudioSessionRouteChangeNotification object:nil queue:NSOperationQueue.mainQueue
                       usingBlock:^(NSNotification *note) {
            // Unity can restore its own category after a route/interruption change.
            if (![AVAudioSession.sharedInstance.category isEqualToString:AVAudioSessionCategoryPlayback] ||
                !(AVAudioSession.sharedInstance.categoryOptions & AVAudioSessionCategoryOptionMixWithOthers)) RestoreWhenActive();
        }];
        [center addObserverForName:AVAudioSessionInterruptionNotification object:nil queue:NSOperationQueue.mainQueue
                       usingBlock:^(NSNotification *note) {
            interrupted = [note.userInfo[AVAudioSessionInterruptionTypeKey] unsignedIntegerValue] == AVAudioSessionInterruptionTypeBegan;
            if (!interrupted) RestoreWhenActive();
        }];
    }
    return ApplyPlaybackPolicy(YES) ? 1 : 0;
}
extern "C" int MazeAudioUsesPlayback() {
    AVAudioSession *session = AVAudioSession.sharedInstance;
    return [session.category isEqualToString:AVAudioSessionCategoryPlayback] &&
        (session.categoryOptions & AVAudioSessionCategoryOptionMixWithOthers) ? 1 : 0;
}
