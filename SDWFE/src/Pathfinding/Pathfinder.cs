using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

namespace SDWFE.Pathfinding;

public class Pathfinder
{
    private readonly HitboxManager _hitboxManager;
    private readonly int _gridSize;
    private readonly object? _ignoreOwner;

    public Pathfinder(HitboxManager hitboxManager, int gridSize = 16, object? ignoreOwner = null)
    {
        _hitboxManager = hitboxManager;
        _gridSize = gridSize;
        _ignoreOwner = ignoreOwner;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end, HitboxLayer layer, int maxDistance)
    {
        // Convert to grid coords
        Point startGrid = WorldToGrid(start);
        Point endGrid = WorldToGrid(end);
        
        // A*
        var openSet = new PriorityQueue<PathNode>();
        var closedSet = new HashSet<Point>();
        var cameFrom = new Dictionary<Point, Point>();

        var gScore = new Dictionary<Point, float>();
        var fScore = new Dictionary<Point, float>();

        var startNode = new PathNode(startGrid, 0, Heuristic(startGrid, endGrid));
        openSet.Enqueue(startNode);
        gScore[startGrid] = 0;
        fScore[startGrid] = startNode.F;

        int iterations = 0;
        const int maxIterations = 1000;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            PathNode current = openSet.Dequeue();

            if (current.Position.Equals(endGrid))
            {
                return ReconstructPath(cameFrom, current.Position);
            }

            closedSet.Add(current.Position);

            foreach (Point neighbor in GetNeighbors(current.Position))
            {
                if (closedSet.Contains(neighbor))
                    continue;
                
                // Check if neighbor is walkable
                Vector2 neighborWorld = GridToWorld(neighbor);
                Rectangle neighborBounds = new Rectangle(
                    (int)neighborWorld.X - _gridSize / 2,
                    (int)neighborWorld.Y - _gridSize / 2,
                    _gridSize,
                    _gridSize
                );

                if (_hitboxManager.CheckStaticCollision(new FloatRect(neighborBounds), layer, _ignoreOwner))
                    continue;
                
                // check max distance
                if (Vector2.Distance(start, neighborWorld) > maxDistance)
                    continue;

                float tentativeGScore = gScore.GetValueOrDefault(current.Position, float.MaxValue) +
                                        Vector2.Distance(GridToWorld(current.Position), neighborWorld);

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current.Position;
                    gScore[neighbor] = tentativeGScore;
                    float f = tentativeGScore + Heuristic(neighbor, endGrid);
                    fScore[neighbor] = f;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(new PathNode(neighbor, tentativeGScore, f));
                }
            }
        }

        return new List<Vector2>();
    }

    private Point WorldToGrid(Vector2 worldPos)
    {
        return new Point(
            (int)Math.Round(worldPos.X / _gridSize),    
            (int)Math.Round(worldPos.Y / _gridSize)
        );
    }

    private Vector2 GridToWorld(Point gridPos)
    {
        return new Vector2(gridPos.X * _gridSize, gridPos.Y * _gridSize);
    }

    private float Heuristic(Point a, Point b)
    {
        // Euclidean distance
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return (float)Math.Sqrt(dx * dx + dy * dy) * _gridSize;
    }

    private IEnumerable<Point> GetNeighbors(Point pos)
    {
        yield return new Point(pos.X + 1, pos.Y);     // Right
        yield return new Point(pos.X - 1, pos.Y);     // Left
        yield return new Point(pos.X, pos.Y + 1);     // Down
        yield return new Point(pos.X, pos.Y - 1);     // Up
        yield return new Point(pos.X + 1, pos.Y + 1); // Down-Right
        yield return new Point(pos.X - 1, pos.Y + 1); // Down-Left
        yield return new Point(pos.X + 1, pos.Y - 1); // Up-Right
        yield return new Point(pos.X - 1, pos.Y - 1); // Up-Left
    }

    private List<Vector2> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        var path = new List<Vector2>();
        var gridPath = new List<Point> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            gridPath.Add(current);
        }

        gridPath.Reverse();

        // Convert grid path to world coordinates and simplify
        path = SimplifyPath(gridPath);

        return path;
    }

    private List<Vector2> SimplifyPath(List<Point> gridPath)
    {
        if (gridPath.Count <= 2)
            return gridPath.Select(GridToWorld).ToList();

        var simplified = new List<Vector2> { GridToWorld(gridPath[0]) };

        for (int i = 1; i < gridPath.Count - 1; i++)
        {
            Point prev = gridPath[i - 1];
            Point curr = gridPath[i];
            Point next = gridPath[i + 1];

            // Check if direction changes
            int dx1 = curr.X - prev.X;
            int dy1 = curr.Y - prev.Y;
            int dx2 = next.X - curr.X;
            int dy2 = next.Y - curr.Y;

            if (dx1 != dx2 || dy1 != dy2)
            {
                simplified.Add(GridToWorld(curr));
            }
        }

        simplified.Add(GridToWorld(gridPath[^1]));
        return simplified;
    }
    
    private class PathNode : IComparable<PathNode>
    {
        public Point Position { get; }
        public float G { get; }
        public float F { get; }

        public PathNode(Point position, float g, float f)
        {
            Position = position;
            G = g;
            F = f;
        }

        public int CompareTo(PathNode? other)
        {
            if (other == null) return 1;
            return F.CompareTo(other.F);
        }
    }

    private class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _items = new();

        public int Count => _items.Count;

        public void Enqueue(T item)
        {
            _items.Add(item);
            _items.Sort();
        }

        public T Dequeue()
        {
            T item = _items[0];
            _items.RemoveAt(0);
            return item;
        }

        public bool Contains(Point position)
        {
            if (typeof(T) == typeof(PathNode))
            {
                return _items.Cast<PathNode>().Any(n => n.Position.Equals(position));
            }

            return false;
        }
    }
}