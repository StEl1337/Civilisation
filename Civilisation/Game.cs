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
        private MapDrawer mapDrawer;
        private Character character;
        private City city;
        private RouteFinder routeFinder;

        public Game(Image mapImage)
        {
            map = new Map();
            character = new Character();
            city = new City();
            mapDrawer = new MapDrawer(map, mapImage, character, city);
            routeFinder = new RouteFinder(map);
        }

        public void GenerateAndDrawMap(int width, int height)
        {
            if (width > 110 || height > 60)
                MessageBox.Show("Map size is too big");
            else
            {
                map.Generate(width, height);
                mapDrawer.DrawMap();
            }
        }

        public void HandleLeftClick(Point position)
        {
            try
            {
                character.MoveTo(mapDrawer.GetCellPosition(position));
                mapDrawer.DrawCharacter(character.Position);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }   
        }

        public void HandleRightClick(Point position)
        {
            try
            {
                city.MoveTo(mapDrawer.GetCellPosition(position));
                mapDrawer.DrawCity(city.Position);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void FindAndDrawRoute()
        {
            try
            {
                List<MapSquare> route = routeFinder.FindShortestPath(map.GetSquareAt(character.Position), map.GetSquareAt(city.Position));
                mapDrawer.DrawRoute(route);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

}
