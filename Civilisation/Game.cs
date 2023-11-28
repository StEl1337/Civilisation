using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using WpfColor = System.Windows.Media.Color;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using Point = System.Windows.Point;
using System.Collections;

namespace Civilisation
{
    public class Game
    {
        private Map map;
        private Point characterPosition;
        private Point cityPosition;
        private Image mapImage;

        public Game(Image mapImage)
        {
            this.mapImage = mapImage;
            map = new Map();
        }

        public void GenerateAndDrawMap(int width, int height)
        {
            map.Generate(width, height);
            DrawMap();
        }

        private void DrawMap()
        {
            int cellSize = Math.Min((int)mapImage.Width / map.Width, (int)mapImage.Height / map.Height);

            mapImage.Source = new WriteableBitmap((int)mapImage.Width, (int)mapImage.Height, 96, 96, PixelFormats.Bgr32, null);
            var source = mapImage.Source as WriteableBitmap;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapSquare mapSquare = map.Squares[x, y];
                    WpfColor color = DetermineColor(mapSquare);
                    int xOffset = x * cellSize;
                    int yOffset = y * cellSize;
                    DrawCell(source, xOffset, yOffset, cellSize, color);
                }
            }
        }

        private WpfColor DetermineColor(MapSquare mapSquare)
        {
            switch (mapSquare.Terrain)
            {
                case TerrainType.Field:
                    return Colors.Green;
                case TerrainType.Forest:
                    return Colors.DarkGreen;
                case TerrainType.Water:
                    return Colors.Blue;
                default:
                    return Colors.Gray;
            }
        }

        private void DrawCell(WriteableBitmap source, int x, int y, int size, WpfColor color)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    source.WritePixels(
                        new Int32Rect(x + i, y + j, 1, 1),
                        new byte[] { color.B, color.G, color.R, color.A },
                        4, // stride
                        0 // offset
                    );
                }
            }
        }

        public void HandleLeftClick(Point position)
        {
            int cellSize = Math.Min((int)mapImage.Width / map.Width, (int)mapImage.Height / map.Height);
            var source = mapImage.Source as WriteableBitmap;
            Point prevPosition = characterPosition;

            // Get new position
            characterPosition.X = (int)(position.X / cellSize);
            characterPosition.Y = (int)(position.Y / cellSize);

            // Draw character at new position  
            DrawCell(source, (int)(characterPosition.X * cellSize), (int)(characterPosition.Y * cellSize), cellSize, Colors.Red);

            // Draw previous terrain at old character position
            if (prevPosition != null)
            {
                MapSquare prevSquare = map.Squares[(int)prevPosition.X, (int)prevPosition.Y];
                DrawCell(source, (int)(prevPosition.X * cellSize), (int)(prevPosition.Y * cellSize), cellSize, DetermineColor(prevSquare));
            }
        }

        public void HandleRightClick(Point position)
        {
            int cellSize = Math.Min((int)mapImage.Width / map.Width, (int)mapImage.Height / map.Height);
            var source = mapImage.Source as WriteableBitmap;
            Point prevPosition = cityPosition;

            // Get new position
            cityPosition.X = (int)(position.X / cellSize);
            cityPosition.Y = (int)(position.Y / cellSize);

            // Draw city at new position  
            DrawCell(source, (int)(cityPosition.X * cellSize), (int)(cityPosition.Y * cellSize), cellSize, Colors.Yellow);

            // Draw previous terrain at old city position
            if (prevPosition != null)
            {
                MapSquare prevSquare = map.Squares[(int)prevPosition.X, (int)prevPosition.Y];
                DrawCell(source, (int)(prevPosition.X * cellSize), (int)(prevPosition.Y * cellSize), cellSize, DetermineColor(prevSquare));
            }
        }

        public void FindAndDrawRoute()
        {
            MapSquare startSquare = map.Squares[(int)characterPosition.X, (int)characterPosition.Y];
            MapSquare endSquare = map.Squares[(int)cityPosition.X, (int)cityPosition.Y];

            // Find route  
            List<MapSquare> route = FindShortestPath(startSquare, endSquare);

            // Draw route
            if (route != null)
            {
                DrawRoute(route);
            }
        }

        private List<MapSquare> FindShortestPath(MapSquare start, MapSquare end)
        {
            List<MapSquare> openSet = new List<MapSquare> { start };

            Dictionary<MapSquare, MapSquare> cameFrom = new Dictionary<MapSquare, MapSquare>();

            Dictionary<MapSquare, int> gScore = new Dictionary<MapSquare, int>();
            gScore[start] = 0;

            Dictionary<MapSquare, int> fScore = new Dictionary<MapSquare, int>();
            fScore[start] = GetDistance(start, end);

            while (openSet.Count > 0)
            {
                MapSquare current = GetLowestFScore(openSet, fScore);

                if (current == end)
                {
                    // Reconstruct path
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (MapSquare neighbor in map.GetNeighbors(current))
                {
                    int tempG = gScore[current] + GetDistance(current, neighbor);

                    if (!gScore.ContainsKey(neighbor) || tempG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tempG;
                        fScore[neighbor] = tempG + GetDistance(neighbor, end);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        private int GetDistance(MapSquare a, MapSquare b)
        {
            if (a.Terrain == TerrainType.Water || b.Terrain == TerrainType.Water)
                return int.MaxValue;
            // Manhattan distance
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private MapSquare GetLowestFScore(List<MapSquare> openSet, Dictionary<MapSquare, int> fScore)
        {
            MapSquare lowest = null;
            int lowestFScore = int.MaxValue;

            foreach (var sq in openSet)
            {
                if (fScore[sq] < lowestFScore)
                {
                    lowestFScore = fScore[sq];
                    lowest = sq;
                }
            }

            return lowest;
        }

        private List<MapSquare> ReconstructPath(Dictionary<MapSquare, MapSquare> cameFrom, MapSquare current)
        {
            List<MapSquare> path = new List<MapSquare>();
            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();

            return path;
        }

        private void DrawRoute(List<MapSquare> route)
        {
            int cellSize = Math.Min((int)mapImage.Width / map.Width, (int)mapImage.Height / map.Height);
            var source = mapImage.Source as WriteableBitmap;
            foreach (MapSquare sq in route)
            {
                WpfColor color = sq.Terrain == TerrainType.Forest ? Colors.DarkOrange : Colors.Orange;
                DrawCell(source, sq.X * cellSize, sq.Y * cellSize, cellSize, color);
            }
        }
    }
}