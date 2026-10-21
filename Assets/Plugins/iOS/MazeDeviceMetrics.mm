#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <mach/mach.h>

static BOOL previousBatteryMonitoring;
extern "C" {
    void MazeMetricsBegin() {
        previousBatteryMonitoring = UIDevice.currentDevice.batteryMonitoringEnabled;
        UIDevice.currentDevice.batteryMonitoringEnabled = YES;
    }
    void MazeMetricsEnd() { UIDevice.currentDevice.batteryMonitoringEnabled = previousBatteryMonitoring; }
    int MazeThermalState() { return (int)NSProcessInfo.processInfo.thermalState; }
    int MazeLowPowerMode() { return NSProcessInfo.processInfo.lowPowerModeEnabled ? 1 : 0; }
    float MazeBatteryLevel() { return UIDevice.currentDevice.batteryLevel; }
    int MazeBatteryState() { return (int)UIDevice.currentDevice.batteryState; }
    long long MazeResidentBytes() {
        mach_task_basic_info_data_t info;
        mach_msg_type_number_t count = MACH_TASK_BASIC_INFO_COUNT;
        return task_info(mach_task_self(), MACH_TASK_BASIC_INFO, (task_info_t)&info, &count) == KERN_SUCCESS ? (long long)info.resident_size : -1;
    }
}
