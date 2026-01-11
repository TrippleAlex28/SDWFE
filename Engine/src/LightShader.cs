using Engine;
using Engine.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class LightShader
{
    public Effect LightEffect { get; private set; }

    public LightShader()
    {
        LightEffect = ExtendedGame.AssetManager.LoadEffect("LightEffect", "Shaders/");
    }

    public void SetLights(List<PointLight> allWorldLights)
    {
        if (LightEffect == null) return;

        Vector4[] lightData = new Vector4[16];
        Vector4[] lightColors = new Vector4[16];
        int currentLightCount = 0;

        foreach (var light in allWorldLights)
        {
            Vector2 screenPos = ExtendedGame.WorldToScreen(light.WorldPosition);

            Vector2 normalizedPos = new Vector2(
                screenPos.X / SettingsManager.Instance.WindowSettings.ScreenWidth,
                screenPos.Y / SettingsManager.Instance.WindowSettings.ScreenHeight
            );

            float normalizedRad = light.WorldRadius / ExtendedGame.DrawResolution.X; 

            lightData[currentLightCount] = new Vector4(normalizedPos.X, normalizedPos.Y, normalizedRad, 0);
            lightColors[currentLightCount] = light.LightColor.ToVector4();

            currentLightCount++;
            if (currentLightCount >= 16) break;
        }

        LightEffect.Parameters["AspectRatio"]?.SetValue((float)ExtendedGame.DrawResolution.Y / ExtendedGame.DrawResolution.X);
        LightEffect.Parameters["LightCount"]?.SetValue(currentLightCount);

        LightEffect.Parameters["LightData"]?.SetValue(lightData);
        LightEffect.Parameters["LightColors"]?.SetValue(lightColors);
    }
}
public struct PointLight
{
    public Vector2 WorldPosition; 
    public float WorldRadius;     
    public Color LightColor;      
}