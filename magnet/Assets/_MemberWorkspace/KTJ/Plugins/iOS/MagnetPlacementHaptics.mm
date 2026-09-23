#import <UIKit/UIKit.h>

extern "C" void Magnet_PlayPlacementHaptic()
{
    static UIImpactFeedbackGenerator *generator;
    if (generator == nil)
        generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];

    [generator impactOccurred];
    [generator prepare];
}
