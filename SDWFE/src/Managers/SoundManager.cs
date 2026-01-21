using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Engine;

public static class SoundManager
{
    private static float _masterVolume = 1.0f;
    public static float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Math.Clamp(value, 0.0f, 1.0f);
            MediaPlayer.Volume = _masterVolume;
        }
    }

    private static readonly Dictionary<string, SoundEffect> _sfx = new();
    private static readonly Dictionary<string, Song> _music = new();

    public static void StopMusic()
    {
        MediaPlayer.Stop();
    }

    public static void PlayMusic(Song song, bool isRepeating = true)
    {
        MediaPlayer.Stop();
        MediaPlayer.Volume = _masterVolume;
        MediaPlayer.IsRepeating = isRepeating;
        MediaPlayer.Play(song);
    }

    public static void PlaySound(SoundEffect soundEffect, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
    {
        soundEffect.Play(volume * _masterVolume, pitch, pan);
    }

    // Load individual assets into the manager
    public static void LoadSfx(string key, string assetPath = "SFX/")
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(assetPath)) return;
        _sfx[key] = ExtendedGame.AssetManager.LoadSoundEffect(key, assetPath);
    }

    public static void LoadMusic(string key, string assetPath = "SFX/")
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(assetPath)) return;
        _music[key] = ExtendedGame.AssetManager.LoadSong(key, assetPath);
    }

    // Play by key
    public static bool PlaySound(string key, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        LoadSfx(key); // Ensure it's loaded
        if (_sfx.TryGetValue(key, out var sfx))
        {
            sfx.Play(volume * _masterVolume, pitch, pan);
            return true;
        }
        return false;
    }

    public static bool PlayMusic(string key, bool isRepeating = true)
    {
        LoadMusic(key); // Ensure it's loaded
        if (_music.TryGetValue(key, out var song))
        {
            PlayMusic(song, isRepeating);
            return true;
        }
        return false;
    }

    // Optional helpers
    public static void UnloadAll()
    {
        _sfx.Clear();
        _music.Clear();
    }
}
