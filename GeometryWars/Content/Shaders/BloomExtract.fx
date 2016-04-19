// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.
#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

DECLARE_TEXTURE(TextureSampler, 0);

float BloomThreshold;


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original image color.
	float4 c = SAMPLE_TEXTURE(TextureSampler, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}


technique BloomExtract
{
	pass Pass1
	{
#if SM5
		PixelShader = compile ps_5_0 PixelShaderFunction();
#elif SM4
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#elif SM3
		PixelShader = compile ps_3_0 PixelShaderFunction();
#else
		PixelShader = compile ps_2_0 PixelShaderFunction();
#endif
	}
}
