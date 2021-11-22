using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace MapTalkie.Utils.RTEFC
{
    public class Rtefc
    {
        private readonly double _a, _threshold;
        private readonly ConcurrentDictionary<Guid, Cluster> _clusters = new();
        private readonly int _maxClusters;

        public Rtefc(double a, int maxClusters, double threshold)
        {
            _a = a;
            _maxClusters = maxClusters;
            _threshold = threshold;
        }

        public Rtefc(TimeSpan sampleTime, TimeSpan tau, int maxClusters, double threshold)
            : this(Math.Exp(-sampleTime.TotalMilliseconds / tau.TotalMilliseconds), maxClusters, threshold)
        {
        }

        public IEnumerable<Cluster> Clusters => _clusters.Values;

        public void Add(Coordinate coordinate, int value)
        {
            if (_clusters.Count == 0)
            {
                var cluster = new Cluster(coordinate, value);
                _clusters[cluster.Id] = cluster;
            }

            var closestDistance = double.MaxValue;
            Cluster closest = default!;

            foreach (var cluster in _clusters.Values)
            {
                var distance = cluster.Centroid.Distance(coordinate);
                if (closestDistance > distance)
                {
                    closestDistance = distance;
                    closest = cluster;
                }
            }

            if (closestDistance <= _threshold || _clusters.Count >= _maxClusters)
            {
                closest.Value += value;
                closest.Centroid = new Coordinate(
                    closest.Centroid.X + (1 - _a) * coordinate.X,
                    closest.Centroid.Y + (1 - _a) * coordinate.Y);
            }
            else
            {
                // novel
                var cluster = new Cluster(coordinate, value);
                _clusters[cluster.Id] = cluster;
            }
        }
    }
}