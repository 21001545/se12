#import <Foundation/Foundation.h>
#import <CoreText/CoreText.h>
#include "IUnityRenderingExtensions.h"

// 이모지 그릴 라벨 하나..
UILabel* getUILabel()
{
    static UILabel * label = nil;
    if (label == nil)
    {
        label = [[UILabel alloc]initWithFrame:CGRectMake(0, 0, 256, 256)];
        [label setOpaque:NO];
        label.font = [UIFont systemFontOfSize:999];
        label.textAlignment = NSTextAlignmentCenter;
        label.baselineAdjustment = UIBaselineAdjustmentAlignCenters;
        label.lineBreakMode = NSLineBreakByClipping;
        label.minimumScaleFactor = 0.0001;
        label.adjustsFontSizeToFitWidth = YES;
    }
    return label;
}

extern "C" {
    int TMP_EmojiTexture_Render(const char* text, unsigned char * buffer , int size)
    {
        int textLength = 0;

        NSUInteger bytesPerRow = 4 * size;
        NSUInteger bitsPerComponent = 8;
        CGColorSpaceRef colorSpace = CGColorSpaceCreateDeviceRGB();
        CGContextRef context = CGBitmapContextCreate(buffer, size, size,
                                                     bitsPerComponent, bytesPerRow, colorSpace,
                                                     kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);
        CGContextClearRect(context, CGRectMake(0, 0, size, size));
        if(text){
            UILabel* label = getUILabel();
            [label setFrame:CGRectMake(0,0,size,size)];
            
            NSMutableAttributedString *attributedString =
            [[NSMutableAttributedString alloc] initWithString:[NSString stringWithUTF8String: text]];
                        
            textLength = (int)[attributedString length];
            label.attributedText = attributedString;
            
            [label.layer renderInContext:context];
        }

        CGColorSpaceRelease(colorSpace);
        CGContextRelease(context);

        return textLength;
    }
}