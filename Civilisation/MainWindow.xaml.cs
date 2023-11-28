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
        private Game game;

        public MainWindow()
        {
            InitializeComponent();
            game = new Game(MapImage);
            MapImage.MouseLeftButtonDown += MapImage_MouseLeftButtonDown;
            MapImage.MouseRightButtonDown += MapImage_MouseRightButtonDown;
        }

        private void GenerateMapButton_Click(object sender, RoutedEventArgs e)
        {
            int mapWidth = int.Parse(MapWidthTextBox.Text);
            int mapHeight = int.Parse(MapHeightTextBox.Text);
            game.GenerateAndDrawMap(mapWidth, mapHeight);
        }

        private void MapImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            game.HandleLeftClick(e.GetPosition(MapImage));
        }

        private void MapImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            game.HandleRightClick(e.GetPosition(MapImage));
        }

        private void ShowRouteButton_Click(object sender, RoutedEventArgs e)
        {
            game.FindAndDrawRoute();
        }
    }
}
