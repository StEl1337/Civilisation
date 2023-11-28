using System;
using System.Collections.Generic;

namespace Civilisation
{
    public class Map
    {
        public MapSquare[,] Squares { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void Generate(int width, int height)
        {
            Width = width;
            Height = height;
            Squares = new MapSquare[width, height];

            var rnd = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var rand = rnd.Next(100);
                    TerrainType terrain = DetermineTerrainType(rand);
                    Squares[x, y] = new MapSquare
                    {
                        Terrain = terrain,
                        X = x,
                        Y = y
                    };
                }
            }
        }

        private TerrainType DetermineTerrainType(int rand)
        {
            if (rand < 40) return TerrainType.Field;
            else if (rand < 70) return TerrainType.Forest;
            else return TerrainType.Water;
        }

        public List<MapSquare> GetNeighbors(MapSquare sq)
        {
            List<MapSquare> neighbors = new List<MapSquare>();

            int x = sq.X;
            int y = sq.Y;

            // Check all 8 directions
            if (x > 0 && Squares[sq.X - 1, sq.Y].Terrain != TerrainType.Water) neighbors.Add(Squares[x - 1, y]);
            if (x < Width - 1 && Squares[sq.X + 1, sq.Y].Terrain != TerrainType.Water) neighbors.Add(Squares[x + 1, y]);

            if (y > 0 && Squares[sq.X, sq.Y - 1].Terrain != TerrainType.Water) neighbors.Add(Squares[x, y - 1]);
            if (y < Height - 1 && Squares[sq.X, sq.Y + 1].Terrain != TerrainType.Water) neighbors.Add(Squares[x, y + 1]);

            return neighbors;
        }
    }
}
