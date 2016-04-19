// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

DECLARE_TEXTURE(TextureSampler, 0);

#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += SAMPLE_TEXTURE(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }
    
    return c;
}


technique GaussianBlur
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
