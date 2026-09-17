#import "IosPreferencesBridge.h"

extern "C" int _GetInt(const char* key, int defaultValue)
{
    NSString* keyString = key ? [NSString stringWithUTF8String:key] : nil;
    if (keyString == nil)
    {
        return defaultValue;
    }

    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
    if ([defaults objectForKey:keyString] == nil)
    {
        return defaultValue;
    }

    return (int)[defaults integerForKey:keyString];
}
