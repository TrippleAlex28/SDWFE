using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Collision;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Tiles;

#nullable enable

namespace SDWFE.Objects.Tilemap;

public class Tilemap : GameObject
{
    #region Constants
    
    private const string LEVELS_PATH = "../../../src/Objects/Tilemap/levels/";
    private const int TILE_SIZE = 16;
    
    #endregion

    #region Tile Categories
    
    private static readonly HashSet<int> WallTileIds = new()
    {
        0, 1, 21, 22, 23, 24, 25, 26,
        63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
        87, 88
    };

    private static readonly HashSet<int> YSortableTileIds = new()
    {
        2, 3, 4, 5, 15, 16, 36, 37,
        84, 85, 86, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
        100, 81, 82, 83, 102, 103, 104, 123, 124, 125,
        144, 145, 146
    };

    private static readonly HashSet<int> StairBackgroundTileIds = new()
    {
        219, 220, 221, 222, 223, 224, 225, 226, 227,
        197, 198, 199, 200, 201, 202, 203, 204, 205,
        176, 177, 178, 179, 180, 181, 182, 183, 184,
        155, 156, 157, 158, 159, 160, 161, 162, 163,
        134, 135, 136, 137, 138, 139, 140, 141, 142
    };

    private static readonly HashSet<int> StairForegroundTileIds = new()
    {
        228, 229, 230, 231, 232,
        207, 208, 209,
        186, 187, 188,
        165, 166, 167
    };

    private static readonly HashSet<int> StairBackRailingTileIds = new()
    {
        9, 10, 11, 12, 13,
        30, 31, 32, 33, 34
    };

    private static readonly HashSet<int> ShadowTileIds = new()
    {
        128, 129, 149, 150, 170, 171, 210, 211, 212, 213, 214
    };

    #endregion
    #region Data From Tiled
    // All portals in the tilemap
    public List<PortalData> Portals { get; private set; } = new();

    // All doors in the tilemap
    public List<DoorData> Doors { get; private set; } = new();

    // All enemy spawn points in the tilemap
    public List<EnemyData> Enemies { get; private set; } = new();

    public Vector2 SpawnPoint { get; set; } = Vector2.Zero;

    #endregion
    #region Public Properties
    
    public List<Rectangle> Colliders { get; private set; } = new();
    public Dictionary<int, RoomDoor> DoorsById { get; } = new();

    public List<Stair> Stairs { get; } = new();
    public int TileSize => TILE_SIZE;
    
    #endregion

    #region Private Fields
    
    private readonly Dictionary<string, Dictionary<Vector2, TileData>> _layers = new();
    private readonly List<RoomDoor> _doors = new();
    private List<Texture2D> _tileSheets = null!;
    private List<TilesetRef> _tilesetRefs = new();
    private readonly HitboxManager _hitboxManager;

    //Extra Textures for special tiles can be added here
    private Texture2D _roomDoorSheet;
    
    #endregion

    #region Constructor
    
    public Tilemap(string levelFile, HitboxManager hitboxManager)
    {
        LoadTileSheets();
        _hitboxManager = hitboxManager;

        _roomDoorSheet = ExtendedGame.AssetManager.LoadTexture("TM_Door_Anim", "Tilemap/");
        var map = JsonManager.Load<TiledMap>(LEVELS_PATH + levelFile);
        _tilesetRefs = map.tilesets?.OrderByDescending(t => t.firstgid).ToList() ?? new();

        var objectsById = GetObjectLookup(map);
        
        LoadTileLayers(map);
        BuildColliders(map);
        BuildAllObjects(objectsById);
        BuildSprites();
        
        AddDoorsAsChildren();
    }
    
    #endregion

    #region Public Methods
    
    public void RegisterHitboxes(HitboxManager hitboxManager)
    {
        foreach (var collider in Colliders)
        {
            hitboxManager.AddStatic(collider, HitboxLayer.Environment, HitboxLayer.All);
        }
        
        foreach (var stair in Stairs)
        {
            stair.RegisterHitboxes(hitboxManager);
        }
    }

    public Texture2D GetTileSheet(int tileId)
    {
        if (_tileSheets.Count < 3)
            LoadTileSheets();

        int tilesetIndex = GetTilesetIndex(tileId + 1);
        tilesetIndex = Math.Clamp(tilesetIndex, 0, _tileSheets.Count - 1);
        
        return _tileSheets[tilesetIndex];
    }

    public int GetTilesetFirstGid(int tileId)
    {
        int tilesetIndex = GetTilesetIndex(tileId + 1);
        
        if (tilesetIndex < 0 || tilesetIndex >= _tilesetRefs.Count)
            return 1;

        return _tilesetRefs[_tilesetRefs.Count - 1 - tilesetIndex].firstgid;
    }
    
    #endregion

    #region Loading
    
    private void LoadTileSheets()
    {
        _tileSheets = new List<Texture2D>
        {
            ExtendedGame.AssetManager.LoadTexture("TM_Main", "Tilemap/"),
            ExtendedGame.AssetManager.LoadTexture("TM_Collision", "Tilemap/"),
            ExtendedGame.AssetManager.LoadTexture("TM_Collision", "Tilemap/")
        };
    }

    private void LoadTileLayers(TiledMap map)
    {
        if (map.layers == null) return;

        HashSet<string> skipLayers = new() { "Collision", "Objects" };

        foreach (var layer in map.layers)
        {
            if (layer.type != "tilelayer" || layer.name == null || skipLayers.Contains(layer.name))
                continue;

            _layers[layer.name] = ParseLayerData(layer);
        }
    }

    private Dictionary<Vector2, TileData> ParseLayerData(Layer layer)
    {
        var result = new Dictionary<Vector2, TileData>();
        var data = layer.data ?? new List<int>();

        for (int i = 0; i < data.Count; i++)
        {
            int x = i % layer.width;
            int y = i / layer.width;
            int tileId = data[i] - 1;

            if (tileId < 0) continue;

            var texture = GetTileSheet(tileId);
            bool isYSortable = YSortableTileIds.Contains(tileId) || WallTileIds.Contains(tileId);
            int ySortOffset = WallTileIds.Contains(tileId) ? 24 : 8;
            
            result[new Vector2(x, y)] = new TileData(tileId, texture, isYSortable, ySortOffset);
        }
        
        return result;
    }
    
    #endregion

    #region Building
    
    private void BuildColliders(TiledMap map)
    {
        var collisionLayer = map.layers?.FirstOrDefault(l => l.name == "Collision");
        if (collisionLayer?.data == null) return;

        var colliderBuilder = new TileColliderBuilder(this);
        Colliders = colliderBuilder.BuildColliders(collisionLayer.data, collisionLayer.width, collisionLayer.height);
    }

    private void BuildAllObjects(Dictionary<int, TiledObject> objectsById)
    {
        foreach (var obj in objectsById.Values)
        {
            if (obj.name == "StairL")
            {
                Stairs.Add(new Stair(new Vector2(obj.x, obj.y), StairFacing.Left));
            }
            else if (obj.name == "StairR")
            {
                Stairs.Add(new Stair(new Vector2(obj.x, obj.y), StairFacing.Right));
            }
            else if (obj.name == "RoomDoor")
            {
                DoorData doorData = new DoorData()
                {
                    Position = new Vector2(obj.x, obj.y - 32),
                    WaveNumber = GetPropertyByName(obj, "wave")?.value ?? 0
                };
                Doors.Add(doorData);
            }
            else if (obj.name == "Portal")
            {
                PortalData portalData = new PortalData()
                {
                    Position = new Vector2(obj.x, obj.y - 32),
                    WaveNumber = GetPropertyByName(obj, "wave")?.value ?? 0,
                    LevelIndex = GetPropertyByName(obj, "level")?.value ?? -1
                };
                Portals.Add(portalData);
            }
            else if (obj.name == "SpawnPoint")
                SpawnPoint = new Vector2(obj.x, obj.y - 16);              
            else if (obj.name == "Enemy"){
        
                EnemyData enemyData = new EnemyData()
                {                    Position = new Vector2(obj.x, obj.y - 32),
                EnemyType = GetPropertyByName(obj, "type")?.value ?? 0,
                WaveNumber = GetPropertyByName(obj, "wave")?.value ?? 0
                };
                Enemies.Add(enemyData);              
            }
        }
    }

    private TiledProperty? GetPropertyByName(TiledObject obj, string propertyName)
    {
        if (obj.properties == null)
            return null;

        return obj.properties.FirstOrDefault(p => p.name == propertyName);
    }

    // private void BuildDoors(Dictionary<int, TiledObject> objectsById)
    // {
    //     foreach (var obj in objectsById.Values)
    //     {
    //         if (obj.name != "Door" || obj.properties == null)
    //             continue;

    //         foreach (var prop in obj.properties)
    //         {
    //             if (prop.name != "Hitbox" || prop.Hitbox == null)
    //                 continue;

    //             var door = new RoomDoor(
    //                 GetTileSheet(81),
    //                 new Vector2(obj.x, obj.y - 32),
    //                 tileSize: 32,
    //                 animationTotalFrames: 6,
    //                 sourceRect: null,
    //                 hitbox: prop.Hitbox.Value
    //             );

    //             _doors.Add(door);
    //         }
    //     }
    // }

    private void BuildSprites()
    {
        var spritesByLayer = new Dictionary<string, Dictionary<Vector2, Sprite>>();

        foreach (var layer in _layers)
        {
            spritesByLayer[layer.Key] = new Dictionary<Vector2, Sprite>();

            foreach (var (gridPos, tileData) in layer.Value)
            {
                var destRect = GetDestinationRect(gridPos);
                float drawLayer = tileData.YSortEnabled
                    ? ExtendedGame.GetYSort(new Vector2(destRect.X, destRect.Y), new Vector2(0, tileData.YSortPoint))
                    : 0f;
                if (ShadowTileIds.Contains(tileData.Index))
                    drawLayer = 0.001f; // Slightly above base environment layer
                var sprite = new Sprite(tileData.Source)
                {
                    GlobalPosition = new Vector2(destRect.X, destRect.Y),
                    SourceRectangle = GetSourceRect(tileData),
                    BaseDrawLayer = drawLayer,
                    OriginType = OriginType.TopLeft,
                    YSortOrigin = new Vector2(0, tileData.YSortPoint)
                };

                spritesByLayer[layer.Key][gridPos] = sprite;
                AddChild(sprite);
            }
        }

        ConfigureStairSprites(spritesByLayer);
    }

    private void ConfigureStairSprites(Dictionary<string, Dictionary<Vector2, Sprite>> spritesByLayer)
    {
        foreach (var stair in Stairs)
        {
            var tileRegion = stair.GetTileRegion();
            float stairYSortBase = stair.GetYSortBase();

            for (int tx = tileRegion.X; tx < tileRegion.X + tileRegion.Width; tx++)
            {
                for (int ty = tileRegion.Y; ty < tileRegion.Y + tileRegion.Height; ty++)
                {
                    var tilePos = new Vector2(tx, ty);

                    foreach (var layerName in _layers.Keys)
                    {
                        if (!_layers[layerName].TryGetValue(tilePos, out var tileData))
                            continue;

                        if (!spritesByLayer[layerName].TryGetValue(tilePos, out var sprite))
                            continue;

                        int tileIndex = tileData.Index;

                        if (StairForegroundTileIds.Contains(tileIndex))
                        {
                            sprite.BaseDrawLayer = stairYSortBase - Stair.FRONT_RAILING_LAYER_OFFSET;
                            stair.FrontRailingSprites.Add(sprite);
                        }
                        else if (StairBackRailingTileIds.Contains(tileIndex))
                        {
                            sprite.BaseDrawLayer = stairYSortBase - Stair.BACK_RAILING_LAYER_OFFSET;
                            stair.BackRailingSprites.Add(sprite);
                        }
                        else if (StairBackgroundTileIds.Contains(tileIndex))
                        {
                            sprite.BaseDrawLayer = stairYSortBase - Stair.STAIR_LAYER_OFFSET;
                            stair.StairSprites.Add(sprite);
                        }
                    }
                }
            }
        }
    }

    private void AddDoorsAsChildren()
    {
        foreach (var door in DoorsById.Values)
        {
            AddChild(door);
        }
    }
    
    #endregion

    #region Helpers
    
    private Dictionary<int, TiledObject> GetObjectLookup(TiledMap map)
    {
        var objectLayer = map.layers?.FirstOrDefault(l => l.type == "objectgroup");
        return objectLayer?.objects?.ToDictionary(o => o.id) ?? new Dictionary<int, TiledObject>();
    }

    private int GetTilesetIndex(int globalTileId)
    {
        for (int i = 0; i < _tilesetRefs.Count; i++)
        {
            if (globalTileId >= _tilesetRefs[i].firstgid)
                return _tilesetRefs.Count - 1 - i;
        }
        return 0;
    }

    private Rectangle GetSourceRect(TileData data)
    {
        int columns = data.Source.Width / TILE_SIZE;
        return new Rectangle(
            data.Index % columns * TILE_SIZE,
            data.Index / columns * TILE_SIZE,
            TILE_SIZE,
            TILE_SIZE
        );
    }

    private Rectangle GetDestinationRect(Vector2 gridPos)
    {
        return new Rectangle(
            (int)gridPos.X * TILE_SIZE,
            (int)gridPos.Y * TILE_SIZE,
            TILE_SIZE,
            TILE_SIZE
        );
    }
    
    #endregion
}

#region Data Structures

public readonly struct TileData
{
    public int Index { get; }
    public Texture2D Source { get; }
    public bool YSortEnabled { get; }
    public int YSortPoint { get; }

    public TileData(int index, Texture2D source, bool ySortEnabled = false, int ySortPoint = 0)
    {
        Index = index;
        Source = source;
        YSortEnabled = ySortEnabled;
        YSortPoint = ySortPoint;
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
    public List<int>? data { get; set; }
    public List<TiledObject>? objects { get; set; }
}

public class TiledObject
{
    public int id { get; set; }
    public string? name { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public List<TiledProperty>? properties { get; set; }
}

public class TiledProperty
{
    public string? name { get; set; }
    public int value { get; set; }
    public string? type { get; set; }
    public Rectangle? Hitbox { get; set; }
}

#endregion
