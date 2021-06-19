#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

//计算入射光照
float3 IncomingLight(Surface surface, Light light)
{
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

//入射光照乘以表面颜色，得到最终的照明颜色
float3 GetLighting(Surface surface, BRDF brdf, Light light)
{
	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

//获取最终照明效果
float3 GetLighting(Surface surface, BRDF drdf)
{
	float3 color = 0.0;
	for(int i = 0; i < GetDirectionalLightCount(); i++)
	{
		color += GetLighting(surface, drdf, GetDirectionalLight(i));
	}
	return color;
}

#endif