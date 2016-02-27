
namespace MetroTileEditor
{
    public struct Index
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int z { get; private set; }
        
        public Index Zero { get { return new Index(0, 0, 0); } }

        public Index (int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Index Left()
        {
            return new Index(x - 1, y, z);
        }

        public Index Right()
        {
            return new Index(x + 1, y, z);
        }

        public Index Down()
        {
            return new Index(x, y - 1, z);
        }

        public Index Up()
        {
            return new Index(x, y + 1, z);
        }

        public Index Forward()
        {
            return new Index(x, y, z + 1);
        }

        public Index Backward()
        {
            return new Index(x, y, z - 1);
        }
    }
}