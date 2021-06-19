#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED
//电介质的反射率平均约0.04
#define MIN_REFLECTIVITY 0.04

struct BRDF
{
	float3 diffuse;
	float3 specular;
	float roughness;
};

float OneMinusReflectivity(float metallic)
{
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}

//得到表面的BRDF数据
BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
	BRDF brdf;
	float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
	brdf.diffuse = surface.color * oneMinusReflectivity;
	if(applyAlphaToDiffuse)
	{
		brdf.diffuse *= surface.alpha;
	}
	brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);	//金属影响镜面反射的颜色，而非金属不影响，非金属的镜面反射应该是白色的
	float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
	brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
	return brdf;
}

//根据公式得到镜面反射强度  r2 / (d2 * max(0.1, (L·H)2) * n).  d = (N·H)2 * (r2 - 1) + 1.0001.
float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(light.direction, h)));
	float r2 = Square(brdf.roughness);
	float d2 = Square(nh2 * (r2 - 1.0) + 1.0001);
	float normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}

//直接光照的表面颜色
float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif