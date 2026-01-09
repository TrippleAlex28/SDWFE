using System;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Sprite;
using System.Linq;
using Engine.Hitbox;
using SDWFE.Objects.Entities.PlayerEntity;
using Engine.Collision;

#nullable enable
public class Tilemap : GameObject
{
    public List<Rectangle> Colliders;
    public List<RoomDoor> Doors => _doors;
    public List<Stair> Stairs = new();

    private HashSet<int> _topWallId = new HashSet<int>() {1, 22, 23, 24, 25, 26, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 87, 88};
    private HashSet<int> _ySortables = new HashSet<int>() {2, 3, 4, 5, 85, 86, 89, 90, 91};
    
    // Stair tile IDs - background tiles that should draw BEHIND player when on stairs
    private HashSet<int> _stairBackgroundIds = new HashSet<int>() 
    { 
        219, 220, 221, 222, 223, 224, 225, 226, 227,
        197, 198, 199, 200, 201, 202, 203, 204, 205,
        176, 177, 178, 179, 180, 181, 182, 183, 184,
        155, 156, 157, 158, 159, 160, 161, 162, 163,
        134, 135, 136, 137, 138, 139, 140, 141, 142
    };
    // Stair front railing tile IDs - should ALWAYS draw in front of player when on stairs
    private HashSet<int> _stairForegroundIds = new HashSet<int>()
    {
        228, 229, 230, 231, 232,
        207, 208, 209,
        186, 187, 188,
        165, 166, 167

    };
    
    private HashSet<int> _stairBackRailings = new HashSet<int>()
    {
        9, 10, 11, 12, 13,
        30, 31, 32, 33, 34,
    };
    private Dictionary<string, Dictionary<Vector2, TileData>> _layers = new();
    private List<RoomDoor> _doors = new();
    public int tileSize = 16;
    private string basePath = "../../../src/Objects/Tilemap/levels/";
    
    private List<Texture2D> _tileSheets = null!;
    private List<TilesetRef> _tilesetRefs = new();
    
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
        
        // Store tileset references sorted by firstgid descending for lookup
        _tilesetRefs = map.tilesets?.OrderByDescending(t => t.firstgid).ToList() ?? new();

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
            if (obj.name == "StairL")
            {
                Stair stair = new(
                    new Vector2(obj.x, obj.y),
                    1
                );

                Stairs.Add(stair);

            } else if (obj.name == "StairR")
            {
                Stair stair = new(
                    new Vector2(obj.x, obj.y),
                    0
                );
                Stairs.Add(stair);
            }
        }
    }
    public void SetHitboxes(HitboxManager hitboxManager)
    {
        foreach (var collider in Colliders)
        {
            hitboxManager.AddStatic(collider, HitboxLayer.Environment, HitboxLayer.All);
        }
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
                    (int)target.x,
                    (int)target.y,
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

        CollisionMap colliderMap = new(
            this
        );

        Colliders = colliderMap.GenerateMapWithColliders(
            collisionLayer.data,
            collisionLayer.width,
            collisionLayer.height
        );
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
        // Build all sprites and store them by layer and tile position
        Dictionary<string, Dictionary<Vector2, Sprite>> spritesByLayerAndTilePos = new();
        
        foreach (var layer in _layers)
        {
            spritesByLayerAndTilePos[layer.Key] = new Dictionary<Vector2, Sprite>();
            
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
                    OriginType = OriginType.TopLeft,
                    YSortOrigin = new Vector2(0, tile.Value.YSortPoint)
                };

                spritesByLayerAndTilePos[layer.Key][tile.Key] = sprite;
                AddChild(sprite);
            }
        }
        
        // Now configure stair sprites based on their tile region
        ConfigureStairSprites(spritesByLayerAndTilePos);
    }
    
    /// <summary>
    /// Configures stair tile sprites with fixed Y-sort values based on their stair region.
    /// </summary>
    private void ConfigureStairSprites(Dictionary<string, Dictionary<Vector2, Sprite>> spritesByLayerAndTilePos)
    {
        foreach (var stair in Stairs)
        {
            Rectangle tileRegion = stair.GetTileRegion();
            float stairYSortBase = stair.GetStairYSortBase();
            
            // Iterate through all tiles in the stair region
            for (int tx = tileRegion.X; tx < tileRegion.X + tileRegion.Width; tx++)
            {
                for (int ty = tileRegion.Y; ty < tileRegion.Y + tileRegion.Height; ty++)
                {
                    Vector2 tilePos = new Vector2(tx, ty);
                    
                    // Check all layers for tiles at this position
                    foreach (var layerName in _layers.Keys)
                    {
                        if (!_layers[layerName].TryGetValue(tilePos, out TileData tileData))
                            continue;
                        
                        if (!spritesByLayerAndTilePos[layerName].TryGetValue(tilePos, out Sprite? sprite))
                            continue;
                        
                        int tileIndex = tileData.Index;
                        
                        if (_stairForegroundIds.Contains(tileIndex))
                        {
                            // Front railing - draws in front of player
                            sprite.BaseDrawLayer = stairYSortBase - Stair.FRONT_RAILING_OFFSET;
                            stair.FrontRailingSprites.Add(sprite);
                            Console.WriteLine("Added front railing sprite at ysort " + sprite.BaseDrawLayer);
                        }
                        else if (_stairBackRailings.Contains(tileIndex))
                        {
                            // Check if it's in the top row (back railing) or main stair area
                            sprite.BaseDrawLayer = stairYSortBase - Stair.BACK_RAILING_OFFSET;
                            stair.BackRailingSprites.Add(sprite);
                        }
                        else if (_stairBackgroundIds.Contains(tileIndex))
                        {
                            sprite.BaseDrawLayer = stairYSortBase - Stair.STAIR_LAYER_OFFSET;
                            stair.StairSprites.Add(sprite);
                        }
                        if (sprite.BaseDrawLayer >= 0.8f){
                            Console.WriteLine("Warning: Stair sprite has invalid Y-sort layer: " + sprite.BaseDrawLayer);
                        }
                    }
                }
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
            ExtendedGame.AssetManager.LoadTexture("TM_Collision", "Tilemap/"),

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
    
    /// <summary>
    /// Gets the tileset index for a given global tile ID based on the tileset references.
    /// </summary>
    private int GetTilesetIndex(int globalTileId)
    {
        // _tilesetRefs is sorted by firstgid descending
        for (int i = 0; i < _tilesetRefs.Count; i++)
        {
            if (globalTileId >= _tilesetRefs[i].firstgid)
            {
                // Return the index from the end since we sorted descending
                return _tilesetRefs.Count - 1 - i;
            }
        }
        return 0; // Default to first tileset
    }
    
    /// <summary>
    /// Gets the firstgid (base index) for a given global tile ID.
    /// Used to convert global tile IDs to texture-relative indices.
    /// </summary>
    public int GetTilesetFirstGid(int globalTileId)
    {
        int tilesetIndex = GetTilesetIndex(globalTileId + 1); // +1 because tile IDs are 0-based in code but 1-based in Tiled
        
        if (tilesetIndex < 0 || tilesetIndex >= _tilesetRefs.Count)
            return 1; // Default to 1 (first tileset)
            
        return _tilesetRefs[_tilesetRefs.Count - 1 - tilesetIndex].firstgid;
    }
    
    public Texture2D TileSheetUsed(int globalTileId)
    {
        if (_tileSheets.Count < 3)
        {
            LoadTextureSheets();
        }
        
        int tilesetIndex = GetTilesetIndex(globalTileId + 1); // +1 because tile IDs are 0-based in code but 1-based in Tiled
        
        // Clamp to available tilesheets
        if (tilesetIndex >= _tileSheets.Count)
            tilesetIndex = _tileSheets.Count - 1;
        if (tilesetIndex < 0)
            tilesetIndex = 0;
            
        return _tileSheets[tilesetIndex];
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
    public List<TilesetRef>? tilesets { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}
public class TilesetRef
{
    public int firstgid { get; set; }
    public string? source { get; set; }
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