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
    public class RouteFinder
    {
        private Map map;

        public RouteFinder(Map map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            this.map = map;
        }

        public List<MapSquare> FindShortestPath(MapSquare start, MapSquare end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));
            if (end == null)
                throw new ArgumentNullException(nameof(end));

            try
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
            catch (Exception ex)
            {                 
                MessageBox.Show(ex.Message);
                return null;
            }    

        }

        private int GetDistance(MapSquare a, MapSquare b)
        {
            int cost = 0;

            if (a.Terrain == TerrainType.Water || b.Terrain == TerrainType.Water)
                return int.MaxValue;

            cost += Math.Abs(a.X - b.X);

            cost += Math.Abs(a.Y - b.Y);

            cost *= a.MoveCost;
            cost *= b.MoveCost;

            return cost;
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

    }
}
