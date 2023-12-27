void ScreenSpaceMasking_float(float2 ScreenPos,float4 clip,out float Alpha)
{
	if (ScreenPos.x < clip.x ||
		ScreenPos.y < clip.y ||
		ScreenPos.x > clip.z ||
		ScreenPos.y > clip.w)
	{
		Alpha = 0;
	}
	else
	{
		Alpha = 1;
	}
}