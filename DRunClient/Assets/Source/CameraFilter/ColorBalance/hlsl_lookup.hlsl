void LookUp_float(UnityTexture2D lookUp, UnitySamplerState lookupSampler, float4 textureColor, out float4 color)
{
    const float width = 128;
    const float height = 128;
    float blueColor = textureColor.b * 63.0;

    float2 quad1;
    quad1.y = floor(floor(blueColor) / 8.0);
    quad1.x = floor(blueColor) - (quad1.y * 8.0);

    float2 quad2;
    quad2.y = floor(ceil(blueColor) / 8.0);
    quad2.x = ceil(blueColor) - (quad2.y * 8.0);

    float2 texPos1;
    texPos1.x = (quad1.x * 0.125) + 0.5/width + ((0.125 - 1.0/width) * textureColor.r);
    texPos1.y = (quad1.y * 0.125) + 0.5/height + ((0.125 - 1.0/height) * textureColor.g);

    float2 texPos2;
    texPos2.x = (quad2.x * 0.125) + 0.5/width + ((0.125 - 1.0/width) * textureColor.r);
    texPos2.y = (quad2.y * 0.125) + 0.5/height + ((0.125 - 1.0/height) * textureColor.g);

    float4 newColor1 = SAMPLE_TEXTURE2D( lookUp, lookupSampler, texPos1);
    float4 newColor2 = SAMPLE_TEXTURE2D( lookUp, lookupSampler, texPos2);

    color = lerp(newColor1, newColor2, frac(blueColor));
}