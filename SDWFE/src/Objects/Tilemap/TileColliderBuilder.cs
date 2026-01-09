using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Tilemap;

namespace Engine.Collision;

/// <summary>
/// Generates optimized collision rectangles from tile-based collision layers.
/// Merges adjacent collidable pixels into larger rectangles for better performance.
/// </summary>
public class TileColliderBuilder
{
    #region Constants
    
    private const int MIN_ALPHA_THRESHOLD = 10;
    
    #endregion

    #region Fields
    
    private readonly Tilemap _tilemap;
    private readonly int _tileSize;
    
    #endregion

    #region Constructor
    
    public TileColliderBuilder(Tilemap tilemap)
    {
        _tilemap = tilemap;
        _tileSize = tilemap.TileSize;
    }
    
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Builds optimized collision rectangles from tile layer data.
    /// </summary>
    public List<Rectangle> BuildColliders(List<int> tileData, int mapWidth, int mapHeight)
    {
        bool[,] collisionMask = GenerateCollisionMask(tileData, mapWidth, mapHeight);
        return MergeIntoRectangles(collisionMask);
    }
    
    #endregion

    #region Collision Mask Generation
    
    private bool[,] GenerateCollisionMask(List<int> tileData, int mapWidth, int mapHeight)
    {
        int[,] tileGrid = ParseTileGrid(tileData, mapWidth, mapHeight);
        bool[,] collisionMask = new bool[mapWidth * _tileSize, mapHeight * _tileSize];

        int tileIndex = 0;
        foreach (int tileId in tileGrid)
        {
            if (tileId >= 0)
            {
                MarkTilePixels(collisionMask, tileId, tileIndex, mapWidth);
            }
            tileIndex++;
        }

        return collisionMask;
    }

    private int[,] ParseTileGrid(List<int> tileData, int mapWidth, int mapHeight)
    {
        int[,] grid = new int[mapHeight, mapWidth];
        
        for (int i = 0; i < tileData.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;
            grid[y, x] = tileData[i] - 1; // Tiled uses 1-based indexing
        }
        
        return grid;
    }

    private void MarkTilePixels(bool[,] collisionMask, int tileId, int tileIndex, int mapWidth)
    {
        Texture2D tileSheet = _tilemap.GetTileSheet(tileId);
        int firstGid = _tilemap.GetTilesetFirstGid(tileId);
        int localTileId = tileId - (firstGid - 1);

        Rectangle sourceRect = GetTileSourceRect(tileSheet, localTileId);
        Color[] pixels = ExtractPixels(tileSheet, sourceRect);

        int tileX = tileIndex % mapWidth * _tileSize;
        int tileY = tileIndex / mapWidth * _tileSize;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A > MIN_ALPHA_THRESHOLD)
            {
                int pixelX = tileX + (i % _tileSize);
                int pixelY = tileY + (i / _tileSize);
                collisionMask[pixelX, pixelY] = true;
            }
        }
    }

    private Rectangle GetTileSourceRect(Texture2D tileSheet, int localTileId)
    {
        int columns = tileSheet.Width / _tileSize;
        return new Rectangle(
            localTileId % columns * _tileSize,
            localTileId / columns * _tileSize,
            _tileSize,
            _tileSize
        );
    }

    private Color[] ExtractPixels(Texture2D texture, Rectangle sourceRect)
    {
        Color[] pixels = new Color[_tileSize * _tileSize];
        texture.GetData(0, sourceRect, pixels, 0, pixels.Length);
        return pixels;
    }
    
    #endregion

    #region Rectangle Merging
    
    /// <summary>
    /// Merges adjacent collidable pixels into larger rectangles using a greedy algorithm.
    /// </summary>
    private List<Rectangle> MergeIntoRectangles(bool[,] collisionMask)
    {
        int width = collisionMask.GetLength(0);
        int height = collisionMask.GetLength(1);
        bool[,] visited = new bool[width, height];
        var rectangles = new List<Rectangle>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!collisionMask[x, y] || visited[x, y])
                    continue;

                var rect = ExpandRectangle(collisionMask, visited, x, y, width, height);
                MarkAsVisited(visited, rect);
                rectangles.Add(rect);
            }
        }

        return rectangles;
    }

    private Rectangle ExpandRectangle(bool[,] collisionMask, bool[,] visited, int startX, int startY, int width, int height)
    {
        int rectWidth = 1;
        int rectHeight = 1;

        // Expand horizontally
        while (startX + rectWidth < width &&
               collisionMask[startX + rectWidth, startY] &&
               !visited[startX + rectWidth, startY])
        {
            rectWidth++;
        }

        // Expand vertically
        bool canExpandDown = true;
        while (canExpandDown && startY + rectHeight < height)
        {
            for (int i = 0; i < rectWidth; i++)
            {
                if (!collisionMask[startX + i, startY + rectHeight] ||
                    visited[startX + i, startY + rectHeight])
                {
                    canExpandDown = false;
                    break;
                }
            }

            if (canExpandDown)
                rectHeight++;
        }

        return new Rectangle(startX, startY, rectWidth, rectHeight);
    }

    private void MarkAsVisited(bool[,] visited, Rectangle rect)
    {
        for (int dy = 0; dy < rect.Height; dy++)
        {
            for (int dx = 0; dx < rect.Width; dx++)
            {
                visited[rect.X + dx, rect.Y + dy] = true;
            }
        }
    }
    
    #endregion
}
