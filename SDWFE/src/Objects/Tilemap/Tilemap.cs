using System;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Sprite;
using System.Linq;

#nullable enable
public class Tilemap : GameObject
{
    public List<Rectangle> Colliders;
    public List<RoomDoor> Doors => _doors;

    private HashSet<int> _topWallId = new HashSet<int>() {2, 5, 9, 10, 15, 16, 18, 25, 29, 30, 31, 32, 56, 57, 58, 59};
    private HashSet<int> _ySortables = new HashSet<int>() {19, 24, 38, 39, 40, 41};
    private Dictionary<Vector2, TileData> _tilemap;
    private List<RoomDoor> _doors = new();
    private int tileSize = 16;
    private string basePath = "../../../Content/Level/Levels/";
    
    private List<Texture2D> _tileSheets = null!;
    /// <summary>
    /// Creates a functional Tilemap
    /// </summary>
    /// <param name="tilemapTexture">Tileset of the layoutLayer</param>
    /// <param name="layoutPath">Only need the extra path, base path is until the Levels folder</param>
    /// <param name="collisionPath">Only need the extra path, base path is until the Levels folder</param>
    /// <param name="collisionTexture">Tileset of the collisionLayer</param>
    public Tilemap(String? pathFile = null)
    {   
        LoadTextureSheets();
        TiledMap map = JsonManager.Load<TiledMap>(basePath + pathFile);

        _tilemap = LoadMap(map);
        Colliders = new List<Rectangle>();

        Dictionary<int, TiledObject> objectsById = GetObjectLookup(map);
        ResolveObjectHitboxes(objectsById);

        TryBuildTileColliders(map);

        BuildDoors(objectsById);

        BuildTileSprites();

        AddDoorsToScene();
    }
    private Dictionary<int, TiledObject> GetObjectLookup(TiledMap map)
    {
        Layer? objectLayer = map.layers?.FirstOrDefault(l => l.type == "objectgroup");

        return objectLayer?.objects?.ToDictionary(o => o.id)
            ?? new Dictionary<int, TiledObject>();
    }

    private void ResolveObjectHitboxes(Dictionary<int, TiledObject> objectsById)
    {
        foreach (var obj in objectsById.Values)
        {
            if (obj.properties == null)
                continue;

            foreach (var prop in obj.properties)
            {
                if (prop.type != "object")
                    continue;

                if (!objectsById.TryGetValue(prop.value, out var target))
                    continue;

                prop.Hitbox = new Rectangle(
                    target.x,
                    target.y,
                    target.width,
                    target.height
                );
            }
        }
    }
    private void TryBuildTileColliders(TiledMap map)
    {
        Layer? collisionLayer = GetLayerByName("Collision", map);
        if (collisionLayer?.data == null)
            return;

        // CollisionMap colliderMap = new(
        //     TileSheetUsed(73),
        //     tileSize
        // );

        // Colliders = colliderMap.GenerateMapWithColliders(
        //     collisionLayer.data,
        //     collisionLayer.width,
        //     collisionLayer.height
        // );

        // foreach (Rectangle col in Colliders)
        // {
        //     GameManager.Instance.Collision.AddStatic(col);
        // }
    }
    private void BuildDoors(Dictionary<int, TiledObject> objectsById)
    {
        foreach (var obj in objectsById.Values)
        {
            if (obj.type != "Door" || obj.properties == null)
                continue;

            foreach (var prop in obj.properties)
            {
                if (prop.name != "Hitbox" || prop.Hitbox == null)
                    continue;

                RoomDoor door = new(
                    TileSheetUsed(81),
                    new Vector2(obj.x, obj.y - 32),
                    32,
                    6,
                    null,
                    prop.Hitbox.Value
                );

                _doors.Add(door);
                // GameManager.Instance.Collision.AddStatic(prop.Hitbox.Value);
            }
        }
    }
    private void BuildTileSprites()
    {
        foreach (var tile in _tilemap)
        {
            Rectangle dest = getDestRect(tile.Key);

            float drawLayer = tile.Value.YSortEnabled
                ? 0.8f / ExtendedGame.DrawResolution.Y *
                (tile.Value.YSortPoint + dest.Y)
                : 0f;

            Sprite sprite = new(tile.Value.Source)
            {
                GlobalPosition = new Vector2(dest.X, dest.Y),
                SourceRectangle = getSourceRect(tile.Value),
                BaseDrawLayer = drawLayer,
                OriginType = OriginType.TopLeft
            };

            AddChild(sprite);
        }
    }
    private void AddDoorsToScene()
    {
        foreach (RoomDoor door in _doors)
            AddChild(door);
    }
    private void LoadTextureSheets()
    {
        _tileSheets = new();
        _tileSheets.Add(ExtendedGame.AssetManager.LoadTexture("Tilemap", "Tilemap/"));
        _tileSheets.Add(ExtendedGame.AssetManager.LoadTexture("CollisionTiles", "Tilemap/"));
        _tileSheets.Add(ExtendedGame.AssetManager.LoadTexture("SD_Doors_Normal", "Tilemap/"));
    }
    private Dictionary<Vector2, TileData> LoadMap(TiledMap map)
    {
        Dictionary<Vector2, TileData> result = new();

        Layer tileLayer = GetLayerByName("Layout", map) ?? new();

        List<int> layoutData = tileLayer.data ?? new();

        for (int i = 0; i < layoutData.Count; i++)
        {
            int x = i % tileLayer.width;
            int y = i / tileLayer.width;
            int value = layoutData[i] - 1;
            if (value >= 0)
            {
                Texture2D source = TileSheetUsed(value);
                bool ySortEnabled = _ySortables.Contains(value) || _topWallId.Contains(value) ? true : false;
                int ySortValue = _topWallId.Contains(value) ? 24 : 8;
                result[new Vector2(x, y)] = new TileData(value, source, ySortEnabled, ySortValue);
            }
        }
        return result;
    }
    private Layer? GetLayerByName(string name, TiledMap map)
    {
        return map.layers?.FirstOrDefault(l => l.name == name);
    }
    private Texture2D TileSheetUsed(int globalTileId)
    {
        if (_tileSheets.Count < 3)
        {
            LoadTextureSheets();
        }
        if (globalTileId <= 72)
        {
            return _tileSheets[0];
        }
        else if (globalTileId <= 80)
        {
            return _tileSheets[1];
        }
        else
        {
            return _tileSheets[2];
        }
    }
    private Rectangle getSourceRect(TileData data)
    {
        int amountOfColumns = data.Source.Width / tileSize;
    
        Rectangle sourceRect = new Rectangle(
            (int)data.Index % amountOfColumns * tileSize,
            (int)data.Index / amountOfColumns * tileSize,
            tileSize,
            tileSize
        );

        return sourceRect;
    }
    private Rectangle getDestRect(Vector2 gridPos)
    {
        Rectangle destRect = new Rectangle(
            (int)gridPos.X * tileSize,
            (int)gridPos.Y * tileSize,
            tileSize,
            tileSize
        );

        return destRect;
    }
}
public struct TileData
{
    public int Index;
    public Texture2D Source;
    public bool YSortEnabled;
    public int YSortPoint;

    public TileData(int index, Texture2D source, bool ySortEnabled = false, int ySortPoint = 0)
    {
        this.Index = index;
        this.Source = source;
        this.YSortEnabled = ySortEnabled;
        this.YSortPoint = ySortPoint;
    }
}
public class TiledMap
{
    public List<Layer>? layers { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}
public class Layer
{
    public string? name { get; set; }
    public string? type { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public List<int>? data { get; set; } // only needed when type is tilelayer
    public List<TiledObject>? objects { get; set;} // only needed when type is objectgroup
}
public class TiledObject
{
    public int id { get; set; }
    public string? type { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public List<CustomProperty>? properties { get; set; }
}
public class CustomProperty
{
    // will be set by the first check
    public string? name { get; set; }
    public int value { get; set; }
    public string? type { get; set; }

    public Rectangle? Hitbox { get; set; }
}