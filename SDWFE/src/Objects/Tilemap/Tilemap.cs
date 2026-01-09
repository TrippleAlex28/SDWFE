using System;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Sprite;
using System.Linq;
using Engine.Hitbox;
using SDWFE.Objects.Entities.PlayerEntity;

#nullable enable
public class Tilemap : GameObject
{
    public List<Rectangle> Colliders;
    public List<RoomDoor> Doors => _doors;
    public List<Stair> Stairs = new();

    private HashSet<int> _topWallId = new HashSet<int>() {1, 22, 23, 24, 25, 26, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 87, 88};
    private HashSet<int> _ySortables = new HashSet<int>() {2, 3, 4, 5, 85, 86, 89, 90, 91};
    private Dictionary<string, Dictionary<Vector2, TileData>> _layers = new();
    private List<RoomDoor> _doors = new();
    private int tileSize = 16;
    private string basePath = "../../../src/Objects/Tilemap/levels/";
    
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

        LoadAllTileLayers(map);
        Colliders = new List<Rectangle>();

        Dictionary<int, TiledObject> objectsById = GetObjectLookup(map);
        InitializeStairs(objectsById);

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
    private void InitializeStairs(Dictionary<int, TiledObject> objectsById)
    {
        foreach (var obj in objectsById.Values)
        {
            if (obj.name != "Stair")
                continue;

            Stair stair = new(
                new Vector2(obj.x, obj.y)
            );

            Stairs.Add(stair);
        }
    }
    public void SetStairHitboxes(HitboxManager hitboxManager)
    {
        foreach (var stair in Stairs)
        {
            stair.SetHitboxes(hitboxManager);
        }
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
            if (obj.name != "Door" || obj.properties == null)
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
        foreach (var layer in _layers)
        {
            foreach (var tile in layer.Value)
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
    }
    private void AddDoorsToScene()
    {
        foreach (RoomDoor door in _doors)
            AddChild(door);
    }
    private void LoadTextureSheets()
    {
        _tileSheets = new()
        {
            ExtendedGame.AssetManager.LoadTexture("TM_Main", "Tilemap/"),
            ExtendedGame.AssetManager.LoadTexture("TM_Collision", "Tilemap/"),
            ExtendedGame.AssetManager.LoadTexture("TM_Door_Anim", "Tilemap/")
        };
    }
    
    /// <summary>
    /// Loads all tile layers from the Tiled map (excludes object layers and collision)
    /// </summary>
    private void LoadAllTileLayers(TiledMap map)
    {
        if (map.layers == null) return;

        // Define which layer names to skip (not tile data)
        HashSet<string> skipLayers = new() { "Collision", "Objects" };

        foreach (var layer in map.layers)
        {
            if (layer.type != "tilelayer" || layer.name == null || skipLayers.Contains(layer.name))
                continue;

            _layers[layer.name] = LoadLayerData(layer);
        }
    }

    /// <summary>
    /// Loads tile data from a single layer
    /// </summary>
    private Dictionary<Vector2, TileData> LoadLayerData(Layer layer)
    {
        Dictionary<Vector2, TileData> result = new();

        List<int> data = layer.data ?? new();

        for (int i = 0; i < data.Count; i++)
        {
            int x = i % layer.width;
            int y = i / layer.width;
            int value = data[i] - 1;
            if (value >= 0)
            {
                Texture2D source = TileSheetUsed(value);
                bool ySortEnabled = _ySortables.Contains(value) || _topWallId.Contains(value);
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
        if (globalTileId <= 231)
        {
            return _tileSheets[0];
        }
        else
        {
            return _tileSheets[1];
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
    public string? name { get; set; }
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