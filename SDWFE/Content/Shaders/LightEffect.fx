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
    
    float maxIntensity = 0.0f;
    
    for (int i = 0; i < LightCount; i++)
    {
        float2 center = LightData[i].xy;
        float radius = LightData[i].z;
        
        float2 adjustedCenter = center;
        adjustedCenter.y *= AspectRatio;

        float distance = length(adjustedPos - adjustedCenter);
        float distanceRatio = distance / radius;
        
        if (distanceRatio < 1.0)
        {
            float intensity = 1.0 - distanceRatio; 
            
            maxIntensity = max(maxIntensity, intensity);
        }
    }

    
    float darknessAlpha = 1.0 - maxIntensity;
    

    return float4(color.rgb, darknessAlpha); 
}
technique BasicHole
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}