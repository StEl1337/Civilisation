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

    public partial class MainWindow : Window
    {
        private int mapWidth;
        private int mapHeight;
        private MapSquare[,] map;
        private Point characterPosition;
        private Point cityPosition;

        public MainWindow()
        {
            InitializeComponent();

            MapImage.MouseLeftButtonDown += MapImage_MouseLeftButtonDown;
            MapImage.MouseRightButtonDown += MapImage_MouseRightButtonDown;
        }

        private void GenerateMapButton_Click(object sender, RoutedEventArgs e)
        {
            mapWidth = int.Parse(MapWidthTextBox.Text);
            mapHeight = int.Parse(MapHeightTextBox.Text);

            GenerateMap();
            DrawMap();
        }

        private void GenerateMap()
        {
            map = new MapSquare[mapWidth, mapHeight];

            var rnd = new Random();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var rand = rnd.Next(100);
                    TerrainType terrain;

                    if (rand < 40)
                    {
                        terrain = TerrainType.Field;
                    }
                    else if (rand < 70)
                    {
                        terrain = TerrainType.Forest;
                    }
                    else
                    {
                        terrain = TerrainType.Water;
                    }

                    map[x, y] = new MapSquare
                    {
                        Terrain = terrain,
                        X = x,
                        Y = y
                    };
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

        private void DrawMap()
        {
            int cellSize = Math.Min((int)MapImage.Width / mapWidth, (int)MapImage.Height / mapHeight);

            MapImage.Source = new WriteableBitmap((int)MapImage.Width, (int)MapImage.Height, 96, 96, PixelFormats.Bgr32, null);
            var source = MapImage.Source as WriteableBitmap;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {

                    MapSquare mapSquare = map[x, y];

                    WpfColor color = DetermineColor(mapSquare);

                    int xOffset = x * cellSize;
                    int yOffset = y * cellSize;

                    DrawCell(source, xOffset, yOffset, cellSize, color);
                }
            }
            //source.Invalidate();
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

        private void MapImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int cellSize = Math.Min((int)MapImage.Width / mapWidth, (int)MapImage.Height / mapHeight);
            var source = MapImage.Source as WriteableBitmap;
            Point prevPosition = characterPosition;

            // Get new position
            Point clickPosition = e.GetPosition(MapImage);
            characterPosition.X = (int)(clickPosition.X / cellSize);
            characterPosition.Y = (int)(clickPosition.Y / cellSize);

            // Draw character at new position  
            DrawCell(source, (int)(characterPosition.X * cellSize), (int)(characterPosition.Y * cellSize), cellSize, Colors.Red);

            // Draw previous terrain at old character position
            if (prevPosition != null)
            {
                MapSquare prevSquare = map[(int)prevPosition.X, (int)prevPosition.Y];
                DrawCell(source, (int)(prevPosition.X * cellSize), (int)(prevPosition.Y * cellSize), cellSize, DetermineColor(prevSquare));
            }
        }

        private void MapImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int cellSize = Math.Min((int)MapImage.Width / mapWidth, (int)MapImage.Height / mapHeight);
            var source = MapImage.Source as WriteableBitmap;
            Point prevPosition = cityPosition;

            // Get new position
            Point clickPosition = e.GetPosition(MapImage);
            cityPosition.X = (int)(clickPosition.X / cellSize);
            cityPosition.Y = (int)(clickPosition.Y / cellSize);

            // Draw character at new position  
            DrawCell(source, (int)(cityPosition.X * cellSize), (int)(cityPosition.Y * cellSize), cellSize, Colors.Yellow);

            // Draw previous terrain at old character position
            if (prevPosition != null)
            {
                MapSquare prevSquare = map[(int)prevPosition.X, (int)prevPosition.Y];
                DrawCell(source, (int)(prevPosition.X * cellSize), (int)(prevPosition.Y * cellSize), cellSize, DetermineColor(prevSquare));
            }
        }

        private void ShowRouteButton_Click(object sender, RoutedEventArgs e)
        {
            FindAndDrawRoute();
        }

        private void FindAndDrawRoute()
        {
            MapSquare startSquare = map[(int)characterPosition.X, (int)characterPosition.Y];
            MapSquare endSquare = map[(int)cityPosition.X, (int)cityPosition.Y];

            // Find route  
            List<MapSquare> route = FindShortestPath(startSquare, endSquare);

            // Draw route
            DrawRoute(route);
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

                foreach (MapSquare neighbor in GetNeighbors(current))
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

        private List<MapSquare> GetNeighbors(MapSquare sq)
        {
            List<MapSquare> neighbors = new List<MapSquare>();

            int x = sq.X;
            int y = sq.Y;

            // Check all 8 directions
            if (x > 0 && map[sq.X - 1, sq.Y].Terrain != TerrainType.Water) neighbors.Add(map[x - 1, y]);
            if (x < mapWidth - 1 && map[sq.X + 1, sq.Y].Terrain != TerrainType.Water) neighbors.Add(map[x + 1, y]);

            if (y > 0 && map[sq.X, sq.Y - 1].Terrain != TerrainType.Water) neighbors.Add(map[x, y - 1]);
            if (y < mapHeight - 1 && map[sq.X, sq.Y + 1].Terrain != TerrainType.Water) neighbors.Add(map[x, y + 1]);

            return neighbors;
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
            int time = 0;
            foreach (MapSquare sq in route)
            {
                int cellSize = Math.Min((int)MapImage.Width / mapWidth, (int)MapImage.Height / mapHeight);
                var source = MapImage.Source as WriteableBitmap;
                time += sq.MoveCost;
                WpfColor color = sq.Terrain == TerrainType.Forest ? Colors.DarkOrange : Colors.Orange;
                DrawCell(source, sq.X * cellSize, sq.Y * cellSize, cellSize, color);
            }
            Time.Content = time;
        }
    }
}
