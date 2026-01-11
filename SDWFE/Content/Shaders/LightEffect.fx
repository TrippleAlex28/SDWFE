//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
#define MAX_LIGHTS 16
#define BASE_INTENSITY 0.8f

texture Texture;
sampler TextureSampler = sampler_state { Texture = <Texture>; };

float AspectRatio; 
int LightCount; 

float4 LightData[MAX_LIGHTS]; 
float4 LightColors[MAX_LIGHTS];


float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target
{
    float2 screenPos = texCoord;
    float2 adjustedPos = screenPos;
    adjustedPos.y *= AspectRatio;
    
    // Accumulate light intensity from all lights (additive blend)
    float totalIntensity = 0.0f;
    
    for (int i = 0; i < LightCount; i++)
    {
        float2 center = LightData[i].xy;
        
        float2 adjustedCenter = center;
        adjustedCenter.y *= AspectRatio;

        float dist = length(adjustedPos - adjustedCenter);
        float radius = LightData[i].z;
        
        // Exponential falloff for very soft edges (gaussian-like)
        float normalizedDist = dist / radius;
        float intensity = exp(-normalizedDist * normalizedDist * 3.0);
        
        // Add contributions from all lights
        totalIntensity += intensity;
    }
    
    // Clamp to 1.0 so overlapping lights don't over-brighten
    totalIntensity = saturate(totalIntensity);
    
    float darknessAlpha = 1.0 - totalIntensity;
    
    return float4(color.rgb, darknessAlpha); 
}
technique BasicHole
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}