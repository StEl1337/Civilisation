using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using WpfColor = System.Windows.Media.Color;

namespace Civilisation
{

    public partial class MainWindow : Window
    {
        private int mapWidth;
        private int mapHeight;
        private MapSquare[,] map;

        public MainWindow()
        {
            InitializeComponent();
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
                    var terrainType = (TerrainType)rnd.Next(3);
                    map[x, y] = new MapSquare
                    {
                        Terrain = terrainType,
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
    }
}
