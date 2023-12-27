float RGBToL(float3 color)
{
    float fmin = min(min(color.r, color.g), color.b);    
    float fmax = max(max(color.r, color.g), color.b);    
    
    return (fmax + fmin) / 2.0; 
}

float3 RGBToHSL(float3 color)
{
    float3 hsl;
    float fmin = min(min(color.r, color.g), color.b);    
    float fmax = max(max(color.r, color.g), color.b);    //Max. value of RGB
    float delta = fmax - fmin;             //Delta RGB value

    hsl.z = (fmax + fmin) / 2.0; // Luminance

    if (delta == 0.0)		//This is a gray, no chroma...
    {
        hsl.x = 0.0;	// Hue
        hsl.y = 0.0;	// Saturation
    }
    else                                    //Chromatic data...
    {
        if (hsl.z < 0.5)
            hsl.y = delta / (fmax + fmin); // Saturation
        else
            hsl.y = delta / (2.0 - fmax - fmin); // Saturation
        
        float deltaR = (((fmax - color.r) / 6.0) + (delta / 2.0)) / delta;
        float deltaG = (((fmax - color.g) / 6.0) + (delta / 2.0)) / delta;
        float deltaB = (((fmax - color.b) / 6.0) + (delta / 2.0)) / delta;
        
        if (color.r == fmax )
            hsl.x = deltaB - deltaG; // Hue
        else if (color.g == fmax)
            hsl.x = (1.0 / 3.0) + deltaR - deltaB; // Hue
        else if (color.b == fmax)
            hsl.x = (2.0 / 3.0) + deltaG - deltaR; // Hue
        
        if (hsl.x < 0.0)
            hsl.x += 1.0; // Hue
        else if (hsl.x > 1.0)
            hsl.x -= 1.0; // Hue
    }

    return hsl;
}

float HueToRGB(float f1, float f2, float hue)
{
    if (hue < 0.0)
        hue += 1.0;
    else if (hue > 1.0)
        hue -= 1.0;
    float res;
    if ((6.0 * hue) < 1.0)
        res = f1 + (f2 - f1) * 6.0 * hue;
    else if ((2.0 * hue) < 1.0)
        res = f2;
    else if ((3.0 * hue) < 2.0)
        res = f1 + (f2 - f1) * ((2.0 / 3.0) - hue) * 6.0;
    else
        res = f1;
    return res;
}

float3 HSLToRGB( float3 hsl)
{
    float3 rgb;
    if (hsl.y == 0.0)
        rgb = float3(hsl.z,hsl.z, hsl.z); 
    else
    {
        float f2;
        
        if (hsl.z < 0.5)
            f2 = hsl.z * (1.0 + hsl.y);
        else
            f2 = (hsl.z + hsl.y) - (hsl.y * hsl.z);
        
        float f1 = 2.0 * hsl.z - f2;
        
        rgb.r = HueToRGB(f1, f2, hsl.x + (1.0/3.0));
        rgb.g = HueToRGB(f1, f2, hsl.x);
        rgb.b = HueToRGB(f1, f2, hsl.x - (1.0/3.0));
    }

    return rgb;
}

void ColorBalance_float(float3 textureColor, float red, float green, float blue, out float3 color)
{
    float lightness = RGBToL(textureColor.rgb);
    float3 shift = float3(red, green, blue);

    const float a = 0.25;
    const float b = 0.333;
    const float scale = 0.7;

    float3 midtones = (clamp((lightness - b) /  a + 0.5, 0.0, 1.0) * clamp ((lightness + b - 1.0) / -a + 0.5, 0.0, 1.0) * scale) * shift;

    float3 newColor = textureColor.rgb + midtones;
    newColor = clamp(newColor, 0.0, 1.0);

    float3 newHSL = RGBToHSL(newColor);
    float oldLum = RGBToL(textureColor.rgb);
    color.rgb = HSLToRGB(float3(newHSL.x, newHSL.y, oldLum));    
}