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
    public class MapDrawer
    {
        private Map map;
        private Image mapImage;
        private Character character; // Assuming a list of characters
        private City city;
        private Point? previousCharacterPosition = null; 
        private Point? previousCityPosition = null;

        public MapDrawer(Map map, Image mapImage, Character character, City city)
        {
            this.map = map;
            this.mapImage = mapImage;
            this.character = character;
            this.city = city;
        }

        public void DrawMap()
        {
            int cellSize = CalculateCellSize();
            mapImage.Source = new WriteableBitmap((int)mapImage.Width, (int)mapImage.Height, 96, 96, PixelFormats.Bgr32, null);
            var source = mapImage.Source as WriteableBitmap;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapSquare mapSquare = map.Squares[x, y];
                    WpfColor color = DetermineColor(mapSquare);
                    DrawCell(source, x, y, cellSize, color);
                }
            }
        }

        public void DrawCell(WriteableBitmap source, int x, int y, int cellSize, WpfColor color)
        {
            int xOffset = x * cellSize;
            int yOffset = y * cellSize;

            for (int i = 0; i < cellSize; i++)
            {
                for (int j = 0; j < cellSize; j++)
                {
                    source.WritePixels(
                        new Int32Rect(xOffset + i, yOffset + j, 1, 1),
                        new byte[] { color.B, color.G, color.R, color.A },
                        4, // stride
                        0  // offset
                    );
                }
            }
        }

        public void DrawCharacter(Point newPosition)
        {
            int cellSize = CalculateCellSize();
            var source = mapImage.Source as WriteableBitmap;

            if (!IsWater(newPosition) && !IsCity(newPosition))
            {
                if (previousCharacterPosition.HasValue)
                {
                    // Revert the previous position to its original state
                    MapSquare prevSquare = map.GetSquareAt(previousCharacterPosition.Value);
                    DrawCell(source, (int)previousCharacterPosition.Value.X, (int)previousCharacterPosition.Value.Y, cellSize, DetermineColor(prevSquare));
                }

                // Draw character at new position
                DrawCell(source, (int)newPosition.X, (int)newPosition.Y, cellSize, Colors.Red);

                // Update the previous position
                previousCharacterPosition = newPosition;
            }
        }

        public void DrawCity(Point position)
        {
            int cellSize = CalculateCellSize();
            var source = mapImage.Source as WriteableBitmap;

            if (!IsWater(position) && !IsCharacter(position))
            {
                if (previousCityPosition.HasValue)
                {
                    // Revert the previous position to its original state
                    MapSquare prevSquare = map.GetSquareAt(previousCityPosition.Value);
                    DrawCell(source, (int)previousCityPosition.Value.X, (int)previousCityPosition.Value.Y, cellSize, DetermineColor(prevSquare));
                }

                DrawCell(source, (int)position.X, (int)position.Y, cellSize, Colors.Yellow);

                // Update the previous position
                previousCityPosition = position;
            }
        }

        private bool IsWater(Point position)
        {
            return map.Squares[(int)position.X, (int)position.Y].Terrain == TerrainType.Water;
        }

        private bool IsCity(Point position)
        {
            if (city.Position == position)
                return true;
            return false;
        }

        private bool IsCharacter(Point position)
        {
            if (character.Position == position)
                return true;
            return false;
        }

        public void DrawRoute(List<MapSquare> route)
        {
            int cellSize = CalculateCellSize();
            var source = mapImage.Source as WriteableBitmap;
            int time = 0;

            foreach (MapSquare sq in route)
            {
                WpfColor color = sq.Terrain == TerrainType.Forest ? Colors.DarkOrange : Colors.Orange;
                time += sq.MoveCost;
                DrawCell(source, sq.X, sq.Y, cellSize, color);
            }

            MessageBox.Show("Time: " + time);
        }

        private int CalculateCellSize()
        {
            return Math.Min((int)mapImage.Width / map.Width, (int)mapImage.Height / map.Height);
        }

        private WpfColor DetermineColor(MapSquare mapSquare)
        {
            // Your existing color determination logic
            switch (mapSquare.Terrain)
            {
                case TerrainType.Field:
                    return Colors.Green;
                case TerrainType.Forest:
                    return Colors.DarkGreen;
                case TerrainType.Water:
                    return Colors.Blue;
                default:
                    return Colors.White;
            }
        }

        public Point GetCellPosition(Point screenPosition)
        {
            int cellSize = CalculateCellSize();
            int x = (int)(screenPosition.X / cellSize);
            int y = (int)(screenPosition.Y / cellSize);
            return new Point(x, y);
        }
    }
}
