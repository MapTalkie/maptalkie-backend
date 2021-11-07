namespace MapTalkie.Utils.MapUtils
{
    public struct MapZoneDescriptor
    {
        public int Level;
        public int CellX;
        public int CellY;
        public int Index;

        public string ToIdentifier() => $"{Level}:{Index}";
    }
}