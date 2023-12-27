void RGBToL_float(float3 color, out float L)
{
    float fmin = min(min(color.r, color.g), color.b);    
    float fmax = max(max(color.r, color.g), color.b);    
    
    L = (fmax + fmin) / 2.0; 
}