using System;
using System.Linq;

namespace Sandbox
{
    internal class Program
    {
        private static Point ToWebMercator(double lat, double lon, int level)
        {
            lat = lat / 180 * Math.PI;
            lon = lon / 180 * Math.PI;
            return new Point
            {
                X = Math.Floor(256 / (2 * Math.PI) * Math.Pow(2, level) * (lon + Math.PI)),
                Y = Math.Floor(256 / (2 * Math.PI) * Math.Pow(2, level) *
                               (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + lat / 2))))
            };
        }

        private static void Main(string[] args)
        {
            while (true)
            {
                var s = Console.ReadLine();
                Console.Clear();
                if (s == null)
                    continue;
                double lat, lon;
                try
                {
                    var parts = s.Split(",").Select(v => v.Trim()).ToList();
                    lat = double.Parse(parts[0]);
                    lon = double.Parse(parts[1]);
                }
                catch (Exception e)
                {
                    continue;
                }

                var p = ToWebMercator(lat, lon, 17);
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 1000), Math.Floor(p.Y / 1000));
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 5000), Math.Floor(p.Y / 5000));
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 15000), Math.Floor(p.Y / 15000));
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 80000), Math.Floor(p.Y / 80000));
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 500_000), Math.Floor(p.Y / 500_000));
                Console.WriteLine("> {0} {1}", Math.Floor(p.X / 5_000_000), Math.Floor(p.Y / 5_000_000));
            }
        }

        private struct Point
        {
            public double X;
            public double Y;
        }

        private struct LatLng
        {
            public double Latitude;
            public double Longitude;
        }

        private struct Area
        {
            public LatLng SouthWest;
            public LatLng NorthEast;
        }
    }
}