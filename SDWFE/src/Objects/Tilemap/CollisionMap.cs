using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.Collision
{
    public class CollisionMap
    {
        private readonly int _tileSize;
        private Tilemap _tilemap;

        public CollisionMap(Tilemap tilemap)
        {
            _tilemap = tilemap;
            _tileSize = tilemap.tileSize;
        }
        
        /// <summary>
        /// Gives the colliders of the map
        /// </summary>
        /// <param name="path">Path to a csv file, most likely a collision csv</param>
        /// <returns></returns>
        public List<Rectangle> GenerateMapWithColliders(List<int> data, int mapWidth, int mapHeight)
        {
            // returns a reasonable amount of colliders, as they are merged based on pixels
            bool[,] collisionMap = Generate(data, mapWidth, mapHeight);
            return GenerateColliders(collisionMap);
        }

        /// <summary>
        /// Generate a bool map based on the csv file for colliders and pixels, it will return false or true for every pixel
        /// </summary>
        /// <param name="path">the path of a csv file, most likely a collision csv</param>
        /// <returns></returns>
        public bool[,] Generate(List<int> data, int mapWidth, int mapHeight)
        {
            int[,] tileMap = new int[mapHeight, mapWidth];
            
            for (int i = 0; i < data.Count; i++)
            {
                int x = i % mapWidth;
                int y = i / mapWidth;
                tileMap[y, x] = data[i] - 1; // Tiled uses 1-based indexing
            }

            // Make a collision bool[] of all pixels on the map
            bool[,] collision = new bool[mapWidth * _tileSize, mapHeight * _tileSize];

            // Index to know the position of the current tile
            int tileIndex = 0;
            foreach (int tile in tileMap)
            {
                // If the tile is not within the tileset continue
                if (tile < 0)
                {
                    tileIndex++;
                    continue;
                }
                Texture2D _tileset = _tilemap.TileSheetUsed(tile);

                // Convert global tile ID to texture-relative index
                int firstgid = _tilemap.GetTilesetFirstGid(tile);
                int relativeIndex = tile - (firstgid - 1); // -1 because firstgid is 1-based

                // Get the Texture from this specific tile
                int columns = _tileset.Width / _tileSize;
                Rectangle sourceRect = new Rectangle(
                    relativeIndex % columns * _tileSize,
                    relativeIndex / columns * _tileSize,
                    _tileSize,
                    _tileSize
                );
                
                // Get every pixel from that tile Texture 
                Color[] pixels = new Color[_tileSize * _tileSize];
                _tileset.GetData(0, sourceRect, pixels, 0, pixels.Length);

                // Locations of tile in the map
                int xPos = tileIndex % mapWidth * _tileSize;
                int yPos = tileIndex / mapWidth * _tileSize;

                // Check every pixel and save if the color isn't transperant
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].A > 10)
                    {
                        if (xPos == 176)
                        {
                            
                        }
                        // Extra pixels due to pixels array
                        collision[xPos + i % _tileSize, yPos + i / _tileSize] = true;
                    }
                }
                tileIndex++;
            }

            return collision;
        }

        /// <summary>
        /// Goes from pixel based bool[,] to simple rectangle colliders, it merges colliders based on pixels
        /// </summary>
        /// <param name="collision">a bool map of which pixels are colored</param>
        /// <returns></returns>
        public List<Rectangle> GenerateColliders(bool[,] collision)
        {
            // Get the size of the tileMap in tiles
            int width = collision.GetLength(0);
            int height = collision.GetLength(1);

            // Keep track of which pixels you already covered, and of the colliders created
            bool[,] visited = new bool[width, height];
            List<Rectangle> rects = new List<Rectangle>();
        
            // Loop through the full pixel map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Check if the pixel was in the collision layer, and that it hasn't been visited.
                    if (collision[x, y] && !visited[x, y])
                    {
                        // Start with basic 1x1 rectangle
                        int rectWidth = 1;
                        int rectHeight = 1;

                        // Expand the width until it isn't possible anymore
                        while (x + rectWidth < width && collision[x + rectWidth, y] && !visited[x + rectWidth, y])
                        {
                            rectWidth++;
                        }

                        // Expand the height until it is not possible
                        bool canExpand = true;
                        while (canExpand && y + rectHeight < height)
                        {
                            // Loop through all the neighbours of the rectwidth
                            for (int i = 0; i < rectWidth; i++)
                            {
                                if (!collision[x + i, y + rectHeight] || visited[x + i, y + rectHeight])
                                {
                                    canExpand = false;
                                    break;
                                }
                            }

                            if (canExpand) rectHeight++;
                        }

                        // Mark the visited pixels

                        for (int i = 0; i < rectHeight; i++)
                        {
                            for (int j = 0; j < rectWidth; j++)
                            {
                                visited[x + j, y + i] = true;
                            }
                        }

                        // Add the Rectangle to the discoverd collision colliders
                        rects.Add(new Rectangle(x, y, rectWidth, rectHeight));

                    }
                }
            }
            return rects;
        }
    }
}