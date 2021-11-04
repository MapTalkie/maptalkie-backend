using System;

namespace MapTalkie.Utils.MapUtils
{
    public class AreaId
    {
        public static AreaId Global = new AreaId(0, 0, 0);

        public AreaId(int level, int x, int y)
        {
            if (level < 0)
                throw new ArgumentException($"Argument \"{nameof(level)}\" is less than 0");
            Level = level;
            X = x;
            Y = y;
        }

        public int Level { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public override string ToString()
        {
            return $"AREA({Level}:{X}:{Y})";
        }
    }
}