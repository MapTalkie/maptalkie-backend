using System.Collections.Generic;
using MapTalkie.Common.Utils;

namespace MapTalkie.Common.Messages.Posts
{
    public interface IGeoUpdate
    {
        IList<GeoAreaUpdate> Updates { get; }
    }

    public class GeoAreaUpdate
    {
        public AreaId Id { get; set; }
        public IList<IPostCreated> NewPosts { get; set; }
    }
}