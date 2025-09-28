using System.Collections.Generic;

namespace TileMap
{
    [System.Serializable]
    public class TileMapData
    {
        public int width;
        public int height;
        public List<int> tiles;
    }
}