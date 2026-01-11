using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Engine;

/// <summary>
/// Handles asset loading. Feel free to expand/change this.
/// </summary>
public sealed class AssetManager
{
    private readonly ContentManager _contentManager;

    public AssetManager(ContentManager contentManager) : base()
    {
        this._contentManager = contentManager;
    }

    public Texture2D LoadTexture(string spriteName, string basePath = "")
    {
        return this._contentManager.Load<Texture2D>(basePath + spriteName);
    }

    public SpriteFont LoadFont(string fontName, string basePath = "")
    {
        return this._contentManager.Load<SpriteFont>(basePath + fontName);
    }
    public Effect LoadEffect(string effectName, string basePath = "")
    {
        return this._contentManager.Load<Effect>(basePath + effectName);
    }
    public SoundEffect LoadSoundEffect(string soundEffectName, string basePath = "")
    {
        return this._contentManager.Load<SoundEffect>(basePath + soundEffectName);
    }

    public Song LoadSong(string songName, string basePath = "")
    {
        return this._contentManager.Load<Song>(basePath + songName);
    }
}