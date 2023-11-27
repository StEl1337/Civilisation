using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Civilisation.MainWindow;

namespace Civilisation
{
    public class MapSquare
    {
        public TerrainType Terrain { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int MoveCost
        {
            get
            {
                switch (Terrain)
                {
                    case TerrainType.Field:
                        return 1;
                    case TerrainType.Forest:
                        return 2;
                    case TerrainType.Water:
                        return int.MaxValue;
                    default:
                        return 0;
                }
            }
        }
    }
}
