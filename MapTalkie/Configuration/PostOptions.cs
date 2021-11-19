using System;

namespace MapTalkie.Configuration
{
    public class PostOptions
    {
        public int PopularPostsCachedTopCount { get; set; } = 100;
        public TimeSpan PopularPostsCacheUpdateEvery { get; set; } = TimeSpan.FromMinutes(10);
    }
}